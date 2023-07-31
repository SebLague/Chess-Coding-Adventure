namespace Chess.Core
{
	public static class MoveUtility
	{
		// Converts a moveName into internal move representation
		// Name is expected in format: "e2e4"
		// Promotions can be written with or without equals sign, for example: "e7e8=q" or "e7e8q"
		public static Move MoveFromName(string moveName, Board board)
		{
			int startSquare = BoardHelper.SquareIndexFromName(moveName.Substring(0, 2));
			int targetSquare = BoardHelper.SquareIndexFromName(moveName.Substring(2, 2));

			int movedPieceType = Piece.PieceType(board.Square[startSquare]);
			Coord startCoord = new Coord(startSquare);
			Coord targetCoord = new Coord(targetSquare);

			// Figure out move flag
			int flag = Move.NoFlag;

			if (movedPieceType == Piece.Pawn)
			{
				// Promotion
				if (moveName.Length > 4)
				{
					flag = moveName[^1] switch
					{
						'q' => Move.PromoteToQueenFlag,
						'r' => Move.PromoteToRookFlag,
						'n' => Move.PromoteToKnightFlag,
						'b' => Move.PromoteToBishopFlag,
						_ => Move.NoFlag
					};
				}
				// Double pawn push
				else if (System.Math.Abs(targetCoord.rankIndex - startCoord.rankIndex) == 2)
				{
					flag = Move.PawnTwoUpFlag;
				}
				// En-passant
				else if (startCoord.fileIndex != targetCoord.fileIndex && board.Square[targetSquare] == Piece.None)
				{
					flag = Move.EnPassantCaptureFlag;
				}
			}
			else if (movedPieceType == Piece.King)
			{
				if (System.Math.Abs(startCoord.fileIndex - targetCoord.fileIndex) > 1)
				{
					flag = Move.CastleFlag;
				}
			}

			return new Move(startSquare, targetSquare, flag);
		}

		// Get algebraic name of move (with promotion specified)
		// Examples: "e2e4", "e7e8=q"
		public static string NameFromMove(Move move)
		{
			string startSquareName = BoardHelper.SquareNameFromIndex(move.StartSquare);
			string endSquareName = BoardHelper.SquareNameFromIndex(move.TargetSquare);
			string moveName = startSquareName + endSquareName;
			if (move.IsPromotion)
			{
				switch (move.MoveFlag)
				{
					case Move.PromoteToRookFlag:
						moveName += "=r";
						break;
					case Move.PromoteToKnightFlag:
						moveName += "=n";
						break;
					case Move.PromoteToBishopFlag:
						moveName += "=b";
						break;
					case Move.PromoteToQueenFlag:
						moveName += "=q";
						break;
				}
			}
			return moveName;
		}
	}
}