using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Chess.Testing {
	public static class PerftUtility {

		// Move name matching stockfish command line output format (for perft comparison)
		public static string MoveName (Move move) {
			string from = BoardRepresentation.SquareNameFromIndex (move.StartSquare);
			string to = BoardRepresentation.SquareNameFromIndex (move.TargetSquare);
			string promotion = "";
			int specialMoveFlag = move.MoveFlag;

			switch (specialMoveFlag) {
				case Move.Flag.PromoteToRook:
					promotion += "r";
					break;
				case Move.Flag.PromoteToKnight:
					promotion += "n";
					break;
				case Move.Flag.PromoteToBishop:
					promotion += "b";
					break;
				case Move.Flag.PromoteToQueen:
					promotion += "q";
					break;
			}

			return from + to + promotion;
		}
	}
}