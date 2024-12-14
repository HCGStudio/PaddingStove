namespace HCGStudio.PaddingStove.Core;

public interface IGameBoardFactory
{
    IGameBoard Create(string id);
}