namespace Chess.Core
{
	public static class BitBoardUtility
	{
		public const ulong FileA = 0x101010101010101;


		public const ulong Rank1 = 0b11111111;
		public const ulong Rank2 = Rank1 << 8;
		public const ulong Rank3 = Rank2 << 8;
		public const ulong Rank4 = Rank3 << 8;
		public const ulong Rank5 = Rank4 << 8;
		public const ulong Rank6 = Rank5 << 8;
		public const ulong Rank7 = Rank6 << 8;
		public const ulong Rank8 = Rank7 << 8;

		public const ulong notAFile = ~FileA;
		public const ulong notHFile = ~(FileA << 7);

		private const ulong deBruijn64 = 0x37E84A99DAE458F;
		private static readonly int[] deBruijnTable =
		{
			0, 1, 17, 2, 18, 50, 3, 57,
			47, 19, 22, 51, 29, 4, 33, 58,
			15, 48, 20, 27, 25, 23, 52, 41,
			54, 30, 38, 5, 43, 34, 59, 8,
			63, 16, 49, 56, 46, 21, 28, 32,
			14, 26, 24, 40, 53, 37, 42, 7,
			62, 55, 45, 31, 13, 39, 36, 6,
			61, 44, 12, 35, 60, 11, 10, 9
		};

		public static readonly ulong[] KnightAttacks;
		public static readonly ulong[] KingMoves;
		public static readonly ulong[] WhitePawnAttacks;
		public static readonly ulong[] BlackPawnAttacks;

		// Get index of least significant set bit in given 64bit value. Also clears the bit to zero.
		public static int PopLSB(ref ulong b)
		{
			int i = deBruijnTable[((ulong)((long)b & -(long)b) * deBruijn64) >> 58];
			b = b & (b - 1);
			return i;
		}

		public static void SetSquare(ref ulong bitboard, int squareIndex)
		{
			bitboard |= 1ul << squareIndex;
		}

		public static void ClearSquare(ref ulong bitboard, int squareIndex)
		{
			// b&=b-1?
			bitboard &= ~(1ul << squareIndex);
		}


		public static void ToggleSquare(ref ulong bitboard, int squareIndex)
		{
			bitboard ^= 1ul << squareIndex;
		}

		public static void ToggleSquares(ref ulong bitboard, int squareA, int squareB)
		{
			bitboard ^= (1ul << squareA | 1ul << squareB);
		}

		public static bool ContainsSquare(ulong bitboard, int square)
		{
			return ((bitboard >> square) & 1) != 0;
		}

		public static ulong PawnAttacks(ulong pawnBitboard, bool isWhite)
		{
			// Pawn attacks are calculated like so: (example given with white to move)

			// The first half of the attacks are calculated by shifting all pawns north-east: northEastAttacks = pawnBitboard << 9
			// Note that pawns on the h file will be wrapped around to the a file, so then mask out the a file: northEastAttacks &= notAFile
			// (Any pawns that were originally on the a file will have been shifted to the b file, so a file should be empty).

			// The other half of the attacks are calculated by shifting all pawns north-west. This time the h file must be masked out.
			// Combine the two halves to get a bitboard with all the pawn attacks: northEastAttacks | northWestAttacks

			if (isWhite)
			{
				return ((pawnBitboard << 9) & notAFile) | ((pawnBitboard << 7) & notHFile);
			}

			return ((pawnBitboard >> 7) & notAFile) | ((pawnBitboard >> 9) & notHFile);
		}


		public static ulong Shift(ulong bitboard, int numSquaresToShift)
		{
			if (numSquaresToShift > 0)
			{
				return bitboard << numSquaresToShift;
			}
			else
			{
				return bitboard >> -numSquaresToShift;
			}

		}

		public static ulong ProtectedPawns(ulong pawns, bool isWhite)
		{
			ulong attacks = PawnAttacks(pawns, isWhite);
			return attacks & pawns;
		}

		public static ulong LockedPawns(ulong whitePawns, ulong blackPawns)
		{
			ulong pushUp = whitePawns << 8;
			ulong pushDown = blackPawns >> 8;
			return (whitePawns & pushDown) | (blackPawns & pushUp);
		}


		static BitBoardUtility()
		{
			KnightAttacks = new ulong[64];
			KingMoves = new ulong[64];
			WhitePawnAttacks = new ulong[64];
			BlackPawnAttacks = new ulong[64];

			(int x, int y)[] orthoDir = { (-1, 0), (0, 1), (1, 0), (0, -1) };
			(int x, int y)[] diagDir = { (-1, -1), (-1, 1), (1, 1), (1, -1) };
			(int x, int y)[] knightJumps = { (-2, -1), (-2, 1), (-1, 2), (1, 2), (2, 1), (2, -1), (1, -2), (-1, -2) };

			for (int y = 0; y < 8; y++)
			{
				for (int x = 0; x < 8; x++)
				{
					ProcessSquare(x, y);
				}
			}

			void ProcessSquare(int x, int y)
			{
				int squareIndex = y * 8 + x;

				for (int dirIndex = 0; dirIndex < 4; dirIndex++)
				{
					// Orthogonal and diagonal directions
					for (int dst = 1; dst < 8; dst++)
					{
						int orthoX = x + orthoDir[dirIndex].x * dst;
						int orthoY = y + orthoDir[dirIndex].y * dst;
						int diagX = x + diagDir[dirIndex].x * dst;
						int diagY = y + diagDir[dirIndex].y * dst;

						if (ValidSquareIndex(orthoX, orthoY, out int orthoTargetIndex))
						{
							if (dst == 1)
							{
								KingMoves[squareIndex] |= 1ul << orthoTargetIndex;
							}
						}

						if (ValidSquareIndex(diagX, diagY, out int diagTargetIndex))
						{
							if (dst == 1)
							{
								KingMoves[squareIndex] |= 1ul << diagTargetIndex;
							}
						}
					}

					// Knight jumps
					for (int i = 0; i < knightJumps.Length; i++)
					{
						int knightX = x + knightJumps[i].x;
						int knightY = y + knightJumps[i].y;
						if (ValidSquareIndex(knightX, knightY, out int knightTargetSquare))
						{
							KnightAttacks[squareIndex] |= 1ul << knightTargetSquare;
						}
					}

					// Pawn attacks

					if (ValidSquareIndex(x + 1, y + 1, out int whitePawnRight))
					{
						WhitePawnAttacks[squareIndex] |= 1ul << whitePawnRight;
					}
					if (ValidSquareIndex(x - 1, y + 1, out int whitePawnLeft))
					{
						WhitePawnAttacks[squareIndex] |= 1ul << whitePawnLeft;
					}


					if (ValidSquareIndex(x + 1, y - 1, out int blackPawnAttackRight))
					{
						BlackPawnAttacks[squareIndex] |= 1ul << blackPawnAttackRight;
					}
					if (ValidSquareIndex(x - 1, y - 1, out int blackPawnAttackLeft))
					{
						BlackPawnAttacks[squareIndex] |= 1ul << blackPawnAttackLeft;
					}


				}
			}

			bool ValidSquareIndex(int x, int y, out int index)
			{
				index = y * 8 + x;
				return x >= 0 && x < 8 && y >= 0 && y < 8;
			}
		}


	}
}