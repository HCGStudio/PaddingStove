using System.Text.Json;
using HCGStudio.PaddingStove.Core;
using Microsoft.AspNetCore.Mvc;
using Nito.AsyncEx;

namespace HCGStudio.PaddingStove.Hosting.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class TrackerController(ILogger<TrackerController> logger, IGameBoardFactory gameBoardFactory) : ControllerBase
{
    private static readonly ReadOnlyMemory<byte> SseBegin = "data: "u8.ToArray();
    private static readonly ReadOnlyMemory<byte> SseEnd = "\n\n"u8.ToArray();
    private static readonly External.KeepAliveEvent KeepAliveEvent = new();
    
    private readonly AsyncLock _mutex = new();

    [HttpGet("{id}")]
    public async Task GetAsync([FromRoute] string id, CancellationToken cancellationToken)
    {
        Response.ContentType = "text/event-stream";
        
        var client = gameBoardFactory.Create(id);
        client.OnStatusChanged += OnStatusUpdated;
        try
        {
            await SendSSEEventAsync(ToExternal(client.Status), cancellationToken);
            while (true)
            {
                await Task.Delay(5000, cancellationToken);
                await SendSSEEventAsync(KeepAliveEvent, cancellationToken);
            }
        }
        finally
        {
            client.OnStatusChanged -= OnStatusUpdated;
        }
    }

    private async ValueTask SendSSEEventAsync(External.ISseEvent data, CancellationToken cancellationToken = default)
    {
        using var _ = await _mutex.LockAsync(cancellationToken);
        await Response.Body.WriteAsync(SseBegin, cancellationToken);
        await Response.Body.WriteAsync(
            JsonSerializer.SerializeToUtf8Bytes(data, JsonContext.Default.ISseEvent),
            cancellationToken);
        await Response.Body.WriteAsync(SseEnd, cancellationToken);
        await Response.Body.FlushAsync(cancellationToken);
    }
    
    private async void OnStatusUpdated(GameBoardStatus status)
    {
        try
        {
            await SendSSEEventAsync(ToExternal(status));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to send event");
        }
    }
    
    private static External.BoardUpdateEvent ToExternal(GameBoardStatus status) =>
        new(
            (External.GameState) status.State,
            status.PlayerDeck.Select(ToExternal).ToArray(),
            status.OpponentDeck.Select(ToExternal).ToArray(),
            status.Counters.Select(ToExternal).ToArray());

    private static External.DeckContent ToExternal(DeckContent content) =>
        new(content.Id, content.Count);

    private static External.BoardCounter ToExternal(BoardCounter counter) =>
        new(counter.Type, counter.Count);
}