using System.Text.RegularExpressions;
using System.Threading.Channels;
using ExhaustiveMatching;
using HearthDb;
using HearthDb.Deckstrings;

namespace HCGStudio.PaddingStove.Core;

internal sealed partial class GameBoard : IGameBoard
{
    private readonly Task _logTask;
    private readonly Task _stateHandlerTask;
    private readonly StreamReader _logStream;
    private readonly Channel<IBoardChange> _boardChangeChannel = Channel.CreateUnbounded<IBoardChange>();
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly Dictionary<string, int> PlayerDeck = new();
    private readonly Dictionary<string, int> OpponentDeck = new();

    private static readonly GameBoardStatus IdleStatus = new(GameState.Idle, [], [], []);

    private string NextDeckString = string.Empty;

    public GameBoard(StreamReader logStream)
    {
        _logStream = logStream;
        _logTask = WatchLog(_cancellationTokenSource.Token);
        _stateHandlerTask = ProcessGameState(_cancellationTokenSource.Token);
    }

    public GameBoardStatus Status { get; private set; } = IdleStatus;

    public event Action<GameBoardStatus>? OnStatusChanged;

    private async ValueTask<string> NextFitLineAsync()
    {
        while (true)
        {
            var line = await _logStream.ReadLineAsync();
            Ensure.NotNull(line);
            if (line.Contains("hearthstone"))
                return line;
        }
    }

    private async Task WatchLog(CancellationToken cancellationToken)
    {
        while (!_logStream.EndOfStream)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var line = await NextFitLineAsync();
            await ParseZoneChangeAsync(line);
            await ParseDeckChangeAsync(line);
            await ParseGameStartAsync(line);
            await ParseGameEndAsync(line);
        }
    }

    private async Task ProcessGameState(CancellationToken cancellationToken)
    {
        await foreach (var state in _boardChangeChannel.Reader.ReadAllAsync(cancellationToken))
        {
            switch (state)
            {
                case DeckChange deckChange:
                    NextDeckString = deckChange.DeckString;
                    break;
                case GameEndChange _:
                    UpdateAndChangeStatus(IdleStatus);
                    PlayerDeck.Clear();
                    OpponentDeck.Clear();
                    break;
                case GameStartChange _:
                    try
                    {
                        var deck = DeckSerializer.Deserialize(NextDeckString);
                        foreach (var (dbfId, count) in deck.CardDbfIds)
                        {
                            if (Cards.CollectibleByDbfId.TryGetValue(dbfId, out var card))
                            {
                                PlayerDeck[card.Id] = count;
                            }
                        }
                        
                        var playerDeck = PlayerDeck
                            .Select(kv => new DeckContent(kv.Key, kv.Value))
                            .ToArray();
                        UpdateAndChangeStatus(new(GameState.Running, playerDeck, [], []));
                    }
                    catch (Exception)
                    {
                        // Deck can't be deserialized, falling back to no deck mode.
                        UpdateAndChangeStatus(new(GameState.Running, [], [], []));
                    }

                    break;
                case ZoneChange zoneChange:
                    HandleZoneChange(zoneChange);
                    break;
                default:
                    throw ExhaustiveMatch.Failed(state);
            }
        }
    }

    private void UpdateAndChangeStatus(GameBoardStatus status)
    {
        Status = status;
        OnStatusChanged?.Invoke(status);
    }

    private async ValueTask ParseZoneChangeAsync(string input)
    {
        var match = ZoneChangeRegex().Match(input);
        if (match.Success)
        {
            var change = new ZoneChange(
                int.Parse(match.Groups[2].Value),
                match.Groups[3].Value,
                int.Parse(match.Groups[4].Value),
                ToTeam(match.Groups[5].Value),
                ToZone(match.Groups[6].Value),
                ToTeam(match.Groups[7].Value),
                ToZone(match.Groups[8].Value));
            Console.WriteLine(input);
            Console.WriteLine(change);
            await _boardChangeChannel.Writer.WriteAsync(change);
        }
    }

    private async ValueTask ParseDeckChangeAsync(string input)
    {
        if (input.Contains("Finding Game With Deck"))
        {
            await NextFitLineAsync();
            await NextFitLineAsync();
            var lineWithDeck = await NextFitLineAsync();
            var deckString = DeckStartRegex().Match(lineWithDeck).Groups[1].Value;
            await _boardChangeChannel.Writer.WriteAsync(new DeckChange(deckString));
        }
    }

    private async ValueTask ParseGameStartAsync(string input)
    {
        if (GameStartRegex().IsMatch(input))
            await _boardChangeChannel.Writer.WriteAsync(new GameStartChange());
    }

    private async ValueTask ParseGameEndAsync(string input)
    {
        if (GameEndRegex().IsMatch(input))
            await _boardChangeChannel.Writer.WriteAsync(new GameEndChange());
    }

    private void HandleZoneChange(ZoneChange change)
    {
        if (change.CardId == string.Empty)
            return;

        switch (change)
        {
            case { FromTeam: Team.Opposing, FromZone: Zone.Hand or Zone.Deck, ToZone: Zone.Play }:
                OpponentDeck[change.CardId] = OpponentDeck.GetValueOrDefault(change.CardId) + 1;
                break;
            case { FromTeam: Team.Friendly, FromZone: Zone.Deck }:
                PlayerDeck[change.CardId] = PlayerDeck.GetValueOrDefault(change.CardId) - 1;
                break;
            case { ToTeam: Team.Friendly, ToZone: Zone.Deck }:
                PlayerDeck[change.CardId] = PlayerDeck.GetValueOrDefault(change.CardId) + 1;
                break;
            case { ToTeam: Team.Opposing, ToZone: Zone.Deck }:
                OpponentDeck[change.CardId] = OpponentDeck.GetValueOrDefault(change.CardId) + 1;
                break;
        }

        UpdateAndChangeStatus(new(
            GameState.Running,
            PlayerDeck.Select(kv => new DeckContent(kv.Key, kv.Value)).ToArray(),
            OpponentDeck.Select(kv => new DeckContent(kv.Key, kv.Value)).ToArray(),
            []));
    }
    
    private static Team ToTeam(string team) =>
        team switch
        {
            "FRIENDLY" => Team.Friendly,
            "OPPOSING" => Team.Opposing,
            _ => Team.Unknown
        };


    private static Zone ToZone(string zone) =>
        zone switch
        {
            "PLAY (Hero)" => Zone.Hero,
            "PLAY (Hero Power)" => Zone.HeroPower,
            "HAND" => Zone.Hand,
            "DECK" => Zone.Deck,
            "PLAY" => Zone.Play,
            _ => Zone.Unknown
        };

    [GeneratedRegex(
        @"\[Zone\] ZoneChangeList\.ProcessChanges\(\) - id=\d* local=.* \[entityName=(.*) id=(\d*) zone=.* zonePos=\d* cardId=(.*) player=(\d)\] zone from ?(FRIENDLY|OPPOSING)? ?(.*)? -> ?(FRIENDLY|OPPOSING)? ?(.*)?")]
    private static partial Regex ZoneChangeRegex();

    [GeneratedRegex(@"\[Decks\] ([A-Za-z0-9+/]+=*)")]
    private static partial Regex DeckStartRegex();

    [GeneratedRegex(@"\[Power\] GameState\.DebugPrintPower\(\) -\s+CREATE_GAME")]
    private static partial Regex GameStartRegex();

    [GeneratedRegex(
        @"\[Power\] GameState\.DebugPrintPower\(\) - TAG_CHANGE Entity=(.*) tag=PLAYSTATE value=(WON|TIED)")]
    private static partial Regex GameEndRegex();

    private readonly record struct CardStore(Card Card, int Count);

    private enum Team
    {
        Unknown,
        Friendly,
        Opposing
    }

    private enum Zone
    {
        Unknown,
        Hero,
        HeroPower,
        Hand,
        Deck,
        Play
    }

    [Closed(
        typeof(ZoneChange),
        typeof(DeckChange),
        typeof(GameStartChange),
        typeof(GameEndChange))]
    private interface IBoardChange;

    private sealed record ZoneChange(
        int EntityId,
        string CardId,
        int PlayerId,
        Team FromTeam,
        Zone FromZone,
        Team ToTeam,
        Zone ToZone) : IBoardChange;

    private readonly record struct DeckChange(string DeckString) : IBoardChange;

    private readonly record struct GameStartChange : IBoardChange;

    private readonly record struct GameEndChange : IBoardChange;

    public async ValueTask DisposeAsync()
    {
        await _cancellationTokenSource.CancelAsync();
        try
        {
            await Task.WhenAll(_logTask, _stateHandlerTask);
        }
        catch (TaskCanceledException)
        {
        }

        _logStream.Dispose();
    }
}