using UnityEngine;

namespace Chess.Game {
	[CreateAssetMenu (menuName = "Theme/Pieces")]
	public class PieceTheme : ScriptableObject {

		public PieceSprites whitePieces;
		public PieceSprites blackPieces;

		public Sprite GetPieceSprite (int piece) {
			PieceSprites pieceSprites = Piece.IsColour (piece, Piece.White) ? whitePieces : blackPieces;

			switch (Piece.PieceType (piece)) {
				case Piece.Pawn:
					return pieceSprites.pawn;
				case Piece.Rook:
					return pieceSprites.rook;
				case Piece.Knight:
					return pieceSprites.knight;
				case Piece.Bishop:
					return pieceSprites.bishop;
				case Piece.Queen:
					return pieceSprites.queen;
				case Piece.King:
					return pieceSprites.king;
				default:
					if (piece != 0) {
						Debug.Log (piece);
					}
					return null;
			}
		}

		[System.Serializable]
		public class PieceSprites {
			public Sprite pawn, rook, knight, bishop, queen, king;

			public Sprite this [int i] {
				get {
					return new Sprite[] { pawn, rook, knight, bishop, queen, king }[i];
				}
			}
		}
	}
}