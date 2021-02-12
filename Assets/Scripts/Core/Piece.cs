namespace Chess {
	public static class Piece {

		public const int None = 0;
		public const int King = 1;
		public const int Pawn = 2;
		public const int Knight = 3;
		public const int Bishop = 5;
		public const int Rook = 6;
		public const int Queen = 7;

		public const int White = 8;
		public const int Black = 16;

		const int typeMask = 0b00111;
		const int blackMask = 0b10000;
		const int whiteMask = 0b01000;
		const int colourMask = whiteMask | blackMask;

		public static bool IsColour (int piece, int colour) {
			return (piece & colourMask) == colour;
		}

		public static int Colour (int piece) {
			return piece & colourMask;
		}

		public static int PieceType (int piece) {
			return piece & typeMask;
		}

		public static bool IsRookOrQueen (int piece) {
			return (piece & 0b110) == 0b110;
		}

		public static bool IsBishopOrQueen (int piece) {
			return (piece & 0b101) == 0b101;
		}

		public static bool IsSlidingPiece (int piece) {
			return (piece & 0b100) != 0;
		}
	}
}