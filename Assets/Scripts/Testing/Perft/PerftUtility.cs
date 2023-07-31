using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chess.Core;

namespace Chess.Testing
{
	public static class PerftUtility
	{

		// Move name matching stockfish command line output format (for perft comparison)
		public static string MoveName(Move move)
		{
			string from = BoardHelper.SquareNameFromIndex(move.StartSquare);
			string to = BoardHelper.SquareNameFromIndex(move.TargetSquare);
			string promotion = "";
			int specialMoveFlag = move.MoveFlag;

			switch (specialMoveFlag)
			{
				case Move.PromoteToRookFlag:
					promotion += "r";
					break;
				case Move.PromoteToKnightFlag:
					promotion += "n";
					break;
				case Move.PromoteToBishopFlag:
					promotion += "b";
					break;
				case Move.PromoteToQueenFlag:
					promotion += "q";
					break;
			}

			return from + to + promotion;
		}
	}
}