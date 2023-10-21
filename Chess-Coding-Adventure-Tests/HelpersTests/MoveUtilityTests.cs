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
	
}