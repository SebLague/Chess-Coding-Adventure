using Chess.Core;
using CodingAdventureBot;

namespace Chess_Coding_Adventure_Tests.HelpersTests;

public class MoveUtilityTests
{

	[Fact]
	public void CanGetMoveFromPawnE2E4()
	{
		var bot = new Bot(); 
		var board = bot.board;
		var move = MoveUtility.GetMoveFromUCIName("e2e4", board);
		Assert.Equal(12, move.StartSquare);
		Assert.Equal(28, move.TargetSquare);
		Assert.Equal(Move.PawnTwoUpFlag, move.MoveFlag);
	}
	
	[Fact]
	public void CanGetMovePawnEnPassant()
	{
		var bot = new Bot();
		var board = bot.board;
		var move = MoveUtility.GetMoveFromUCIName("e2f3", board);
		Assert.Equal(12, move.StartSquare);
		Assert.Equal(21, move.TargetSquare);
		Assert.Equal(Move.EnPassantCaptureFlag, move.MoveFlag);
	}
	
	[Fact]
	public void CanPromotePawnToQueen()
	{
		var bot = new Bot();
		var board = bot.board;
		var move = MoveUtility.GetMoveFromUCIName("e7e8q", board);
		Assert.Equal(52, move.StartSquare);
		Assert.Equal(60, move.TargetSquare);
		Assert.Equal(Move.PromoteToQueenFlag, move.MoveFlag);
	}

    [Fact]
    public void GetMoveNameUCI_PawnPromotionToRook()
    {
        // Arrange
        var move = new Move(12, 28, Move.PromoteToRookFlag);

        // Act
        string moveName = MoveUtility.GetMoveNameUCI(move);

        // Assert
        Assert.Equal("e2e4r", moveName);
    }

    [Fact]
    public void GetMoveNameUCI_PawnPromotionToKnight()
    {
        // Arrange
        var move = new Move(12, 28, Move.PromoteToKnightFlag);

        // Act
        string moveName = MoveUtility.GetMoveNameUCI(move);

        // Assert
        Assert.Equal("e2e4n", moveName);
    }

    [Fact]
    public void GetMoveNameUCI_PawnPromotionToBishop()
    {
        // Arrange
        var move = new Move(12, 28, Move.PromoteToBishopFlag);

        // Act
        string moveName = MoveUtility.GetMoveNameUCI(move);

        // Assert
        Assert.Equal("e2e4b", moveName);
    }

    [Fact]
    public void GetMoveNameUCI_PawnPromotionToQueen()
    {
        // Arrange
        var move = new Move(12, 28, Move.PromoteToQueenFlag);

        // Act
        string moveName = MoveUtility.GetMoveNameUCI(move);

        // Assert
        Assert.Equal("e2e4q", moveName);
    }

    [Fact]
    public void GetMoveNameUCI_NoPromotion()
    {
        // Arrange
        var move = new Move(12, 28, Move.NoFlag);

        // Act
        string moveName = MoveUtility.GetMoveNameUCI(move);

        // Assert
        Assert.Equal("e2e4", moveName);
    }

    [Fact]
    public void GetMoveNameSAN_NullMove()
    {
        // Arrange
        var bot = new Bot();
        var board = bot.board;
        var move = new Move();

        // Act
        string moveName = MoveUtility.GetMoveNameSAN(move, board);

        // Assert
        Assert.Equal("Null", moveName);
    }

    [Fact]
    public void GetMoveNameSAN_KnightMove()
    {
        // Arrange
        var bot = new Bot();
        var board = bot.board;
        var move = MoveUtility.GetMoveFromUCIName("g1f3", board);

        // Act
        var moveName = MoveUtility.GetMoveNameSAN(move, board);

        // Assert
        Assert.Equal("Nf3", moveName);
    }



}