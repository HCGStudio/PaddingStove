using System.Text.Json.Serialization;

namespace HCGStudio.PaddingStove.Hosting.External;

[JsonConverter(typeof(JsonStringEnumConverter<GameState>))]
internal enum GameState
{
    [JsonStringEnumMemberName("running")] Running,
    [JsonStringEnumMemberName("idle")] Idle
}

[JsonDerivedType(typeof(KeepAliveEvent), "keepAlive")]
[JsonDerivedType(typeof(BoardUpdateEvent), "boardUpdate")]
internal interface ISseEvent;

internal readonly record struct KeepAliveEvent : ISseEvent;

internal readonly record struct DeckContent(string Id, int Count);

internal readonly record struct BoardCounter(string Type, int Count);

internal sealed record BoardUpdateEvent(
    GameState State,
    IReadOnlyList<DeckContent> PlayerDeck,
    IReadOnlyList<DeckContent> OpponentDeck,
    IReadOnlyList<BoardCounter> Counters) : ISseEvent;