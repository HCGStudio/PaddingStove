using System.Collections.Concurrent;
using HCGStudio.PaddingStove.Core.Native;

namespace HCGStudio.PaddingStove.Core;

internal class GameBoardFactory : IGameBoardFactory
{
    private readonly ConcurrentDictionary<string, IGameBoard> _boards = new();

    public IGameBoard Create(string id)
    {
        return _boards.GetOrAdd(id, CreateNewBoardInternal);
    }

    private static IGameBoard CreateNewBoardInternal(string id)
    {
        using var connection = new DeviceConnector(id);
        var client = connection.ConnectLockDownClient();
        var service = client.StartService("com.apple.syslog_relay");
        var serviceClient = service.CreatServiceClient();
        return new GameBoard(new(serviceClient));
    }
}