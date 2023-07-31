﻿namespace Chess.Core
{
	public class Evaluation
	{

		public const int PawnValue = 100;
		public const int KnightValue = 300;
		public const int BishopValue = 320;
		public const int RookValue = 500;
		public const int QueenValue = 900;

		static readonly int[] passedPawnBonuses = { 0, 120, 80, 50, 30, 15, 15 };
		static readonly int[] isolatedPawnPenaltyByCount = { 0, -10, -25, -50, -75, -75, -75, -75, -75 };

		const float endgameMaterialStart = RookValue * 2 + BishopValue + KnightValue;
		Board board;

		public EvaluationData whiteEval;
		public EvaluationData blackEval;

		// Performs static evaluation of the current position.
		// The position is assumed to be 'quiet', i.e no captures are available that could drastically affect the evaluation.
		// The score that's returned is given from the perspective of whoever's turn it is to move.
		// So a positive score means the player who's turn it is to move has an advantage, while a negative score indicates a disadvantage.
		public int Evaluate(Board board)
		{
			this.board = board;
			whiteEval = new EvaluationData();
			blackEval = new EvaluationData();

			MaterialInfo whiteMaterial = GetMaterialInfo(Board.WhiteIndex);
			MaterialInfo blackMaterial = GetMaterialInfo(Board.BlackIndex);

			// Score based on number (and type) of pieces on board
			whiteEval.materialScore = whiteMaterial.materialScore;
			blackEval.materialScore = blackMaterial.materialScore;
			// Score based on positions of pieces
			whiteEval.pieceSquareScore = EvaluatePieceSquareTables(true, blackMaterial.endgameT);
			blackEval.pieceSquareScore = EvaluatePieceSquareTables(false, whiteMaterial.endgameT);
			// Encourage using own king to push enemy king to edge of board in winning endgame
			whiteEval.mopUpScore = MopUpEval(true, whiteMaterial, blackMaterial);
			blackEval.mopUpScore = MopUpEval(false, blackMaterial, whiteMaterial);


			whiteEval.pawnScore = EvaluatePawns(Board.WhiteIndex);
			blackEval.pawnScore = EvaluatePawns(Board.BlackIndex);

			int perspective = board.IsWhiteToMove ? 1 : -1;
			int eval = whiteEval.Sum() - blackEval.Sum();
			return eval * perspective;
		}

		public int EvaluatePawns(int colourIndex)
		{
			PieceList pawns = board.pawns[colourIndex];
			bool isWhite = colourIndex == Board.WhiteIndex;
			ulong opponentPawns = board.pieceBitboards[Piece.MakePiece(Piece.Pawn, isWhite ? Piece.Black : Piece.White)];
			ulong friendlyPawns = board.pieceBitboards[Piece.MakePiece(Piece.Pawn, isWhite ? Piece.White : Piece.Black)];
			ulong[] masks = isWhite ? Bits.WhitePassedPawnMask : Bits.BlackPassedPawnMask;
			int bonus = 0;
			int numIsolatedPawns = 0;

			//Debug.Log((isWhite ? "Black" : "White") + " has no pieces: " + opponentHasNoPieces);

			for (int i = 0; i < pawns.Count; i++)
			{
				int square = pawns[i];
				ulong passedMask = masks[square];
				// Is passed pawn
				if ((opponentPawns & passedMask) == 0)
				{
					int rank = BoardHelper.RankIndex(square);
					int numSquaresFromPromotion = isWhite ? 7 - rank : rank;
					bonus += passedPawnBonuses[numSquaresFromPromotion];
				}

				// Is isolated pawn
				if ((friendlyPawns & Bits.AdjacentFileMasks[BoardHelper.FileIndex(square)]) == 0)
				{
					numIsolatedPawns++;
				}
			}

			return bonus + isolatedPawnPenaltyByCount[numIsolatedPawns];
		}



		float EndgamePhaseWeight(int materialCountWithoutPawns)
		{
			const float multiplier = 1 / endgameMaterialStart;
			return 1 - System.Math.Min(1, materialCountWithoutPawns * multiplier);
		}

		// As game transitions to endgame, and if up material, then encourage moving king closer to opponent king
		int MopUpEval(bool isWhite, MaterialInfo myMaterial, MaterialInfo enemyMaterial)
		{
			if (myMaterial.materialScore > enemyMaterial.materialScore + PawnValue * 2 && enemyMaterial.endgameT > 0)
			{
				int mopUpScore = 0;
				int friendlyIndex = isWhite ? Board.WhiteIndex : Board.BlackIndex;
				int opponentIndex = isWhite ? Board.BlackIndex : Board.WhiteIndex;

				int friendlyKingSquare = board.KingSquare[friendlyIndex];
				int opponentKingSquare = board.KingSquare[opponentIndex];
				// Encourage moving king closer to opponent king
				mopUpScore += (14 - PrecomputedMoveData.OrthogonalDistance[friendlyKingSquare, opponentKingSquare]) * 4;
				// Encourage pushing opponent king to edge of board
				mopUpScore += PrecomputedMoveData.CentreManhattanDistance[opponentKingSquare] * 10;
				return (int)(mopUpScore * enemyMaterial.endgameT);
			}
			return 0;
		}

		int CountMaterial(int colourIndex)
		{
			int material = 0;
			material += board.pawns[colourIndex].Count * PawnValue;
			material += board.knights[colourIndex].Count * KnightValue;
			material += board.bishops[colourIndex].Count * BishopValue;
			material += board.rooks[colourIndex].Count * RookValue;
			material += board.queens[colourIndex].Count * QueenValue;

			return material;
		}

		int EvaluatePieceSquareTables(bool isWhite, float endgameT)
		{
			int value = 0;
			int colourIndex = isWhite ? Board.WhiteIndex : Board.BlackIndex;
			//value += EvaluatePieceSquareTable(PieceSquareTable.Pawns, board.pawns[colourIndex], isWhite);
			value += EvaluatePieceSquareTable(PieceSquareTable.Rooks, board.rooks[colourIndex], isWhite);
			value += EvaluatePieceSquareTable(PieceSquareTable.Knights, board.knights[colourIndex], isWhite);
			value += EvaluatePieceSquareTable(PieceSquareTable.Bishops, board.bishops[colourIndex], isWhite);
			value += EvaluatePieceSquareTable(PieceSquareTable.Queens, board.queens[colourIndex], isWhite);

			int pawnEarly = EvaluatePieceSquareTable(PieceSquareTable.Pawns, board.pawns[colourIndex], isWhite);
			int pawnLate = EvaluatePieceSquareTable(PieceSquareTable.PawnsEnd, board.pawns[colourIndex], isWhite);
			value += (int)(pawnEarly * (1 - endgameT));
			value += (int)(pawnLate * endgameT);

			int kingEarlyPhase = PieceSquareTable.Read(PieceSquareTable.KingStart, board.KingSquare[colourIndex], isWhite);
			value += (int)(kingEarlyPhase * (1 - endgameT));
			int kingLatePhase = PieceSquareTable.Read(PieceSquareTable.KingEnd, board.KingSquare[colourIndex], isWhite);
			value += (int)(kingLatePhase * (endgameT));

			return value;
		}

		static int EvaluatePieceSquareTable(int[] table, PieceList pieceList, bool isWhite)
		{
			int value = 0;
			for (int i = 0; i < pieceList.Count; i++)
			{
				value += PieceSquareTable.Read(table, pieceList[i], isWhite);
			}
			return value;
		}

		public struct EvaluationData
		{
			public int materialScore;
			public int mopUpScore;
			public int pieceSquareScore;
			public int pawnScore;

			public int Sum()
			{
				return materialScore + mopUpScore + pieceSquareScore + pawnScore;
			}
		}

		MaterialInfo GetMaterialInfo(int colourIndex)
		{
			int numPawns = board.pawns[colourIndex].Count;
			int numKnights = board.knights[colourIndex].Count;
			int numBishops = board.bishops[colourIndex].Count;
			int numRooks = board.rooks[colourIndex].Count;
			int numQueens = board.queens[colourIndex].Count;

			bool isWhite = colourIndex == Board.WhiteIndex;
			ulong myPawns = board.pieceBitboards[Piece.MakePiece(Piece.Pawn, isWhite)];
			ulong enemyPawns = board.pieceBitboards[Piece.MakePiece(Piece.Pawn, !isWhite)];

			return new MaterialInfo(numPawns, numKnights, numBishops, numQueens, numRooks, myPawns, enemyPawns);
		}

		public readonly struct MaterialInfo
		{
			public readonly int materialScore;
			public readonly int numPawns;
			public readonly int numMajors;
			public readonly int numMinors;
			public readonly int numBishops;
			public readonly int numQueens;
			public readonly int numRooks;

			public readonly ulong pawns;
			public readonly ulong enemyPawns;

			public readonly float endgameT;

			public MaterialInfo(int numPawns, int numKnights, int numBishops, int numQueens, int numRooks, ulong myPawns, ulong enemyPawns)
			{
				this.numPawns = numPawns;
				this.numBishops = numBishops;
				this.numQueens = numQueens;
				this.numRooks = numRooks;
				this.pawns = myPawns;
				this.enemyPawns = enemyPawns;

				numMajors = numRooks + numQueens;
				numMinors = numBishops + numKnights;

				materialScore = 0;
				materialScore += numPawns * PawnValue;
				materialScore += numKnights * KnightValue;
				materialScore += numBishops * BishopValue;
				materialScore += numRooks * RookValue;
				materialScore += numQueens * QueenValue;

				// Endgame Transition (0->1)
				const int queenEndgameWeight = 45;
				const int rookEndgameWeight = 20;
				const int bishopEndgameWeight = 10;
				const int knightEndgameWeight = 10;

				const int endgameStartWeight = 2 * rookEndgameWeight + 2 * bishopEndgameWeight + 2 * knightEndgameWeight + queenEndgameWeight;
				int endgameWeightSum = numQueens * queenEndgameWeight + numRooks * rookEndgameWeight + numBishops * bishopEndgameWeight + numKnights * knightEndgameWeight;
				endgameT = 1 - System.Math.Min(1, endgameWeightSum / (float)endgameStartWeight);
			}
		}
	}
}