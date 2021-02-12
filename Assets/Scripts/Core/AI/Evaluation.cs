using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chess {
	public class Evaluation {

		public const int pawnValue = 100;
		public const int knightValue = 300;
		public const int bishopValue = 320;
		public const int rookValue = 500;
		public const int queenValue = 900;

		const float endgameMaterialStart = rookValue * 2 + bishopValue + knightValue;
		Board board;

		// Performs static evaluation of the current position.
		// The position is assumed to be 'quiet', i.e no captures are available that could drastically affect the evaluation.
		// The score that's returned is given from the perspective of whoever's turn it is to move.
		// So a positive score means the player who's turn it is to move has an advantage, while a negative score indicates a disadvantage.
		public int Evaluate (Board board) {
			this.board = board;
			int whiteEval = 0;
			int blackEval = 0;

			int whiteMaterial = CountMaterial (Board.WhiteIndex);
			int blackMaterial = CountMaterial (Board.BlackIndex);

			int whiteMaterialWithoutPawns = whiteMaterial - board.pawns[Board.WhiteIndex].Count * pawnValue;
			int blackMaterialWithoutPawns = blackMaterial - board.pawns[Board.BlackIndex].Count * pawnValue;
			float whiteEndgamePhaseWeight = EndgamePhaseWeight (whiteMaterialWithoutPawns);
			float blackEndgamePhaseWeight = EndgamePhaseWeight (blackMaterialWithoutPawns);

			whiteEval += whiteMaterial;
			blackEval += blackMaterial;
			whiteEval += MopUpEval (Board.WhiteIndex, Board.BlackIndex, whiteMaterial, blackMaterial, blackEndgamePhaseWeight);
			blackEval += MopUpEval (Board.BlackIndex, Board.WhiteIndex, blackMaterial, whiteMaterial, whiteEndgamePhaseWeight);

			whiteEval += EvaluatePieceSquareTables (Board.WhiteIndex, blackEndgamePhaseWeight);
			blackEval += EvaluatePieceSquareTables (Board.BlackIndex, whiteEndgamePhaseWeight);

			int eval = whiteEval - blackEval;

			int perspective = (board.WhiteToMove) ? 1 : -1;
			return eval * perspective;
		}

		float EndgamePhaseWeight (int materialCountWithoutPawns) {
			const float multiplier = 1 / endgameMaterialStart;
			return 1 - System.Math.Min (1, materialCountWithoutPawns * multiplier);
		}

		int MopUpEval (int friendlyIndex, int opponentIndex, int myMaterial, int opponentMaterial, float endgameWeight) {
			int mopUpScore = 0;
			if (myMaterial > opponentMaterial + pawnValue * 2 && endgameWeight > 0) {

				int friendlyKingSquare = board.KingSquare[friendlyIndex];
				int opponentKingSquare = board.KingSquare[opponentIndex];
				mopUpScore += PrecomputedMoveData.centreManhattanDistance[opponentKingSquare] * 10;
				// use ortho dst to promote direct opposition
				mopUpScore += (14 - PrecomputedMoveData.NumRookMovesToReachSquare (friendlyKingSquare, opponentKingSquare)) * 4;

				return (int) (mopUpScore * endgameWeight);
			}
			return 0;
		}

		int CountMaterial (int colourIndex) {
			int material = 0;
			material += board.pawns[colourIndex].Count * pawnValue;
			material += board.knights[colourIndex].Count * knightValue;
			material += board.bishops[colourIndex].Count * bishopValue;
			material += board.rooks[colourIndex].Count * rookValue;
			material += board.queens[colourIndex].Count * queenValue;

			return material;
		}

		int EvaluatePieceSquareTables (int colourIndex, float endgamePhaseWeight) {
			int value = 0;
			bool isWhite = colourIndex == Board.WhiteIndex;
			value += EvaluatePieceSquareTable (PieceSquareTable.pawns, board.pawns[colourIndex], isWhite);
			value += EvaluatePieceSquareTable (PieceSquareTable.rooks, board.rooks[colourIndex], isWhite);
			value += EvaluatePieceSquareTable (PieceSquareTable.knights, board.knights[colourIndex], isWhite);
			value += EvaluatePieceSquareTable (PieceSquareTable.bishops, board.bishops[colourIndex], isWhite);
			value += EvaluatePieceSquareTable (PieceSquareTable.queens, board.queens[colourIndex], isWhite);
			int kingEarlyPhase = PieceSquareTable.Read (PieceSquareTable.kingMiddle, board.KingSquare[colourIndex], isWhite);
			value += (int) (kingEarlyPhase * (1 - endgamePhaseWeight));
			//value += PieceSquareTable.Read (PieceSquareTable.kingMiddle, board.KingSquare[colourIndex], isWhite);

			return value;
		}

		static int EvaluatePieceSquareTable (int[] table, PieceList pieceList, bool isWhite) {
			int value = 0;
			for (int i = 0; i < pieceList.Count; i++) {
				value += PieceSquareTable.Read (table, pieceList[i], isWhite);
			}
			return value;
		}
	}
}