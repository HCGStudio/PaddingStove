namespace HCGStudio.PaddingStove.Core;

public interface IGameBoard : IAsyncDisposable
{
    GameBoardStatus Status { get; }

    event Action<GameBoardStatus>? OnStatusChanged;
}

public enum GameState
{
    Running,
    Idle
}

public readonly record struct DeckContent(string Id, int Count);

public readonly record struct BoardCounter(string Type, int Count);

public sealed record GameBoardStatus(
    GameState State,
    IReadOnlyList<DeckContent> PlayerDeck,
    IReadOnlyList<DeckContent> OpponentDeck,
    IReadOnlyList<BoardCounter> Counters);