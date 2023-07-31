namespace Chess.Core
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
		static readonly int[] kingPawnShieldScores = { 4, 7, 4, 3, 6, 3 };

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

			whiteEval.pawnShieldScore = KingPawnShield(Board.WhiteIndex, blackMaterial, blackEval.pieceSquareScore);
			blackEval.pawnShieldScore = KingPawnShield(Board.BlackIndex, whiteMaterial, whiteEval.pieceSquareScore);

			int perspective = board.IsWhiteToMove ? 1 : -1;
			int eval = whiteEval.Sum() - blackEval.Sum();
			return eval * perspective;
		}

		public int KingPawnShield(int colourIndex, MaterialInfo enemyMaterial, float enemyPieceSquareScore)
		{
			if (enemyMaterial.endgameT >= 1)
			{
				return 0;
			}

			int penalty = 0;

			bool isWhite = colourIndex == Board.WhiteIndex;
			int friendlyPawn = Piece.MakePiece(Piece.Pawn, isWhite);
			int kingSquare = board.KingSquare[colourIndex];
			int kingFile = BoardHelper.FileIndex(kingSquare);

			//int filePenalty = kingOpeningFilePenalty[kingFile];
			int uncastledKingPenalty = 0;

			if (kingFile <= 2 || kingFile >= 5)
			{
				//Debug.Log("King: " + kingSquare );
				int[] squares = isWhite ? PrecomputedEvaluationData.PawnShieldSquaresWhite[kingSquare] : PrecomputedEvaluationData.PawnShieldSquaresBlack[kingSquare];

				for (int i = 0; i < squares.Length / 2; i++)
				{
					int shieldSquareIndex = squares[i];
					if (board.Square[shieldSquareIndex] != friendlyPawn)
					{
						if (squares.Length > 3 && board.Square[squares[i + 3]] == friendlyPawn)
						{
							penalty += kingPawnShieldScores[i + 3];
						}
						else
						{
							penalty += kingPawnShieldScores[i];
						}
						//Debug.Log(BoardHelper.SquareNameFromIndex(shieldSquareIndex) + "  " + kingPawnShieldScores[i]);
					}
				}
				penalty *= penalty;
			}
			else
			{
				float enemyDevelopmentScore = System.Math.Clamp((enemyPieceSquareScore + 10) / 130f, 0, 1);
				uncastledKingPenalty = (int)(50 * enemyDevelopmentScore);
				//Debug.Log(isWhite + "  " + uncastledKingPenalty);
				//Debug.Log("File penalty: " + filePenalty);
			}

			int openFileAgainstKingPenalty = 0;

			if (enemyMaterial.numRooks > 1 || (enemyMaterial.numRooks > 0 && enemyMaterial.numQueens > 0))
			{

				int clampedKingFile = System.Math.Clamp(kingFile, 1, 6);
				ulong myPawns = enemyMaterial.enemyPawns;
				for (int attackFile = clampedKingFile; attackFile <= clampedKingFile + 1; attackFile++)
				{
					ulong fileMask = Bits.FileMask[attackFile];
					bool isKingFile = attackFile == kingFile;
					if ((enemyMaterial.pawns & fileMask) == 0)
					{
						openFileAgainstKingPenalty += isKingFile ? 25 : 15;
						if ((myPawns & fileMask) == 0)
						{
							openFileAgainstKingPenalty += isKingFile ? 15 : 10;
						}
					}

				}
			}

			float pawnShieldWeight = 1 - enemyMaterial.endgameT;
			if (board.Queens[1 - colourIndex].Count == 0)
			{
				pawnShieldWeight *= 0.6f;
			}

			return (int)((-penalty - uncastledKingPenalty - openFileAgainstKingPenalty) * pawnShieldWeight);
		}

		public int EvaluatePawns(int colourIndex)
		{
			PieceList pawns = board.Pawns[colourIndex];
			bool isWhite = colourIndex == Board.WhiteIndex;
			ulong opponentPawns = board.PieceBitboards[Piece.MakePiece(Piece.Pawn, isWhite ? Piece.Black : Piece.White)];
			ulong friendlyPawns = board.PieceBitboards[Piece.MakePiece(Piece.Pawn, isWhite ? Piece.White : Piece.Black)];
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
			material += board.Pawns[colourIndex].Count * PawnValue;
			material += board.Knights[colourIndex].Count * KnightValue;
			material += board.Bishops[colourIndex].Count * BishopValue;
			material += board.Rooks[colourIndex].Count * RookValue;
			material += board.Queens[colourIndex].Count * QueenValue;

			return material;
		}

		int EvaluatePieceSquareTables(bool isWhite, float endgameT)
		{
			int value = 0;
			int colourIndex = isWhite ? Board.WhiteIndex : Board.BlackIndex;
			//value += EvaluatePieceSquareTable(PieceSquareTable.Pawns, board.pawns[colourIndex], isWhite);
			value += EvaluatePieceSquareTable(PieceSquareTable.Rooks, board.Rooks[colourIndex], isWhite);
			value += EvaluatePieceSquareTable(PieceSquareTable.Knights, board.Knights[colourIndex], isWhite);
			value += EvaluatePieceSquareTable(PieceSquareTable.Bishops, board.Bishops[colourIndex], isWhite);
			value += EvaluatePieceSquareTable(PieceSquareTable.Queens, board.Queens[colourIndex], isWhite);

			int pawnEarly = EvaluatePieceSquareTable(PieceSquareTable.Pawns, board.Pawns[colourIndex], isWhite);
			int pawnLate = EvaluatePieceSquareTable(PieceSquareTable.PawnsEnd, board.Pawns[colourIndex], isWhite);
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
			public int pawnShieldScore;

			public int Sum()
			{
				return materialScore + mopUpScore + pieceSquareScore + pawnScore + pawnShieldScore;
			}
		}

		MaterialInfo GetMaterialInfo(int colourIndex)
		{
			int numPawns = board.Pawns[colourIndex].Count;
			int numKnights = board.Knights[colourIndex].Count;
			int numBishops = board.Bishops[colourIndex].Count;
			int numRooks = board.Rooks[colourIndex].Count;
			int numQueens = board.Queens[colourIndex].Count;

			bool isWhite = colourIndex == Board.WhiteIndex;
			ulong myPawns = board.PieceBitboards[Piece.MakePiece(Piece.Pawn, isWhite)];
			ulong enemyPawns = board.PieceBitboards[Piece.MakePiece(Piece.Pawn, !isWhite)];

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