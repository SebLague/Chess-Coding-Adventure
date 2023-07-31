using static System.Math;

namespace Chess.Core
{
	// A collection of precomputed bitboards for use during movegen, search, etc.
	public static class Bits
	{
		public const ulong FileA = 0x101010101010101;

		public const ulong WhiteKingsideMask = 1ul << BoardHelper.f1 | 1ul << BoardHelper.g1;
		public const ulong BlackKingsideMask = 1ul << BoardHelper.f8 | 1ul << BoardHelper.g8;

		public const ulong WhiteQueensideMask2 = 1ul << BoardHelper.d1 | 1ul << BoardHelper.c1;
		public const ulong BlackQueensideMask2 = 1ul << BoardHelper.d8 | 1ul << BoardHelper.c8;

		public const ulong WhiteQueensideMask = WhiteQueensideMask2 | 1ul << BoardHelper.b1;
		public const ulong BlackQueensideMask = BlackQueensideMask2 | 1ul << BoardHelper.b8;

		public static readonly ulong[] WhitePassedPawnMask;
		public static readonly ulong[] BlackPassedPawnMask;

		// A pawn on 'e4' for example, is considered supported by any pawn on
		// squares: d3, d4, f3, f4
		public static readonly ulong[] WhitePawnSupportMask;
		public static readonly ulong[] BlackPawnSupportMask;

		public static readonly ulong[] FileMask;
		public static readonly ulong[] AdjacentFileMasks;

		public static readonly ulong[] KingSafetyMask;

		// Mask of 'forward' square. For example, from e4 the forward squares for white are: [e5, e6, e7, e8]
		public static readonly ulong[] WhiteForwardFileMask;
		public static readonly ulong[] BlackForwardFileMask;

		// Mask of three consecutive files centred at given file index.
		// For example, given file '3', the mask would contains files [2,3,4].
		// Note that for edge files, such as file 0, it would contain files [0,1,2]
		public static readonly ulong[] TripleFileMask;

		static Bits()
		{
			FileMask = new ulong[8];
			AdjacentFileMasks = new ulong[8];

			for (int i = 0; i < 8; i++)
			{
				FileMask[i] = FileA << i;
				ulong left = i > 0 ? FileA << (i - 1) : 0;
				ulong right = i < 7 ? FileA << (i + 1) : 0;
				AdjacentFileMasks[i] = left | right;
			}

			TripleFileMask = new ulong[8];
			for (int i = 0; i < 8; i++)
			{
				int clampedFile = System.Math.Clamp(i, 1, 6);
				TripleFileMask[i] = FileMask[clampedFile] | AdjacentFileMasks[clampedFile];
			}

			WhitePassedPawnMask = new ulong[64];
			BlackPassedPawnMask = new ulong[64];
			WhitePawnSupportMask = new ulong[64];
			BlackPawnSupportMask = new ulong[64];
			WhiteForwardFileMask = new ulong[64];
			BlackForwardFileMask = new ulong[64];

			for (int square = 0; square < 64; square++)
			{
				int file = BoardHelper.FileIndex(square);
				int rank = BoardHelper.RankIndex(square);
				ulong adjacentFiles = FileA << Max(0, file - 1) | FileA << Min(7, file + 1);
				// Passed pawn mask
				ulong whiteForwardMask = ~(ulong.MaxValue >> (64 - 8 * (rank + 1)));
				ulong blackForwardMask = ((1ul << 8 * rank) - 1);

				WhitePassedPawnMask[square] = (FileA << file | adjacentFiles) & whiteForwardMask;
				BlackPassedPawnMask[square] = (FileA << file | adjacentFiles) & blackForwardMask;
				// Pawn support mask
				ulong adjacent = (1ul << (square - 1) | 1ul << (square + 1)) & adjacentFiles;
				WhitePawnSupportMask[square] = adjacent | BitBoardUtility.Shift(adjacent, -8);
				BlackPawnSupportMask[square] = adjacent | BitBoardUtility.Shift(adjacent, +8);

				WhiteForwardFileMask[square] = whiteForwardMask & FileMask[file];
				BlackForwardFileMask[square] = blackForwardMask & FileMask[file];
			}



			KingSafetyMask = new ulong[64];
			for (int i = 0; i < 64; i++)
			{
				KingSafetyMask[i] = BitBoardUtility.KingMoves[i] | (1ul << i);
			}


		}

	}
}