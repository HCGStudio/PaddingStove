using Xunit;

namespace HCGStudio.PaddingStove.Core.Tests;

public class GameBoardRegexTests
{
    [Fact]
    public void GameStartRegex_MatchesCreateGameLine()
    {
        const string line =
            "Mar 15 12:00:00 hearthstone[123] <Notice>: D 12:00:00.000 [Power] GameState.DebugPrintPower() -     CREATE_GAME";
        Assert.Matches(GameBoard.GameStartRegex(), line);
    }

    [Fact]
    public void GameStartRegex_DoesNotMatchUnrelated()
    {
        const string line = "Mar 15 12:00:00 hearthstone[123] <Notice>: nothing here";
        Assert.DoesNotMatch(GameBoard.GameStartRegex(), line);
    }

    [Theory]
    [InlineData("WON")]
    [InlineData("TIED")]
    public void GameEndRegex_MatchesPlayStateTagChange(string outcome)
    {
        var line =
            $"hearthstone [Power] GameState.DebugPrintPower() - TAG_CHANGE Entity=Player1 tag=PLAYSTATE value={outcome}";
        var match = GameBoard.GameEndRegex().Match(line);
        Assert.True(match.Success);
        Assert.Equal(outcome, match.Groups[2].Value);
    }

    [Fact]
    public void GameEndRegex_DoesNotMatchOtherPlayStates()
    {
        const string line =
            "hearthstone [Power] GameState.DebugPrintPower() - TAG_CHANGE Entity=Player1 tag=PLAYSTATE value=PLAYING";
        Assert.DoesNotMatch(GameBoard.GameEndRegex(), line);
    }

    [Fact]
    public void DeckStartRegex_ExtractsBase64DeckString()
    {
        const string line = "hearthstone [Decks] AAEBAa0GBgi+Aa+9AvLOAg==";
        var match = GameBoard.DeckStartRegex().Match(line);
        Assert.True(match.Success);
        Assert.Equal("AAEBAa0GBgi+Aa+9AvLOAg==", match.Groups[1].Value);
    }

    [Fact]
    public void ZoneChangeRegex_ParsesFriendlyDeckToHand()
    {
        const string line =
            "hearthstone [Zone] ZoneChangeList.ProcessChanges() - id=1 local=False [entityName=Frostbolt id=42 zone=DECK zonePos=0 cardId=CS2_024 player=1] zone from FRIENDLY DECK -> FRIENDLY HAND";
        var match = GameBoard.ZoneChangeRegex().Match(line);
        Assert.True(match.Success);
        Assert.Equal("Frostbolt", match.Groups[1].Value);
        Assert.Equal("42", match.Groups[2].Value);
        Assert.Equal("CS2_024", match.Groups[3].Value);
        Assert.Equal("1", match.Groups[4].Value);
        Assert.Equal("FRIENDLY", match.Groups[5].Value);
        Assert.Equal("DECK", match.Groups[6].Value);
        Assert.Equal("FRIENDLY", match.Groups[7].Value);
        Assert.Equal("HAND", match.Groups[8].Value);
    }

    [Fact]
    public void ZoneChangeRegex_ParsesOpposingHandToPlay()
    {
        const string line =
            "hearthstone [Zone] ZoneChangeList.ProcessChanges() - id=2 local=False [entityName=Fireball id=99 zone=PLAY zonePos=1 cardId=CS2_029 player=2] zone from OPPOSING HAND -> OPPOSING PLAY";
        var match = GameBoard.ZoneChangeRegex().Match(line);
        Assert.True(match.Success);
        Assert.Equal("OPPOSING", match.Groups[5].Value);
        Assert.Equal("HAND", match.Groups[6].Value);
        Assert.Equal("OPPOSING", match.Groups[7].Value);
        Assert.Equal("PLAY", match.Groups[8].Value);
    }

    [Fact]
    public void ZoneChangeRegex_ParsesEmptyFromZone()
    {
        const string line =
            "hearthstone [Zone] ZoneChangeList.ProcessChanges() - id=3 local=False [entityName=GameEntity id=1 zone=PLAY zonePos=0 cardId= player=0] zone from  -> ";
        Assert.Matches(GameBoard.ZoneChangeRegex(), line);
    }
}
