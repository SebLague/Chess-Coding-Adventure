using Chess.Game;
using UnityEngine;
using Chess.UI;
using Chess.Core;

namespace Chess.Testing
{
	public class BitBoardDebugViewer : MonoBehaviour
	{
		public enum DisplayType
		{
			None,
			Custom,
			AllPiecesSingleSide,
			AllPiecesBothSides,
			Pawn,
			Knight,
			Bishop,
			Rook,
			King,
			Queen,
			OrthoMovesBySquare,
			DiagMovesBySquare,
			KnightMovesBySquare,
			KingsideMask,
			QueensideMask,
			ProtectedPawns,
			LockedPawns,
			MoveGenTest,
			PawnAttacks,
			Align,
			DirRay,
			OrthoSliders,
			DiagonalSliders,
			PassedPawnMask,
			PawnSupportMask,
			PawnShieldSquares,
			Temp
		}

		public DisplayType displayType;
		public bool displayWhitePieces;

		public Color colour;
		public Color colour2;
		public ulong customBitboard;
		[Range(0, 63)]
		public int square;
		[Range(0, 63)]
		public int alignSquare;

		Material[,] squareMaterials;
		GameManager gameManager;
		ulong lastBitboard;
		ulong lastBitboard2;
		bool settingsChanged;
		BoardUI boardUI;

		void Start()
		{
			gameManager = FindObjectOfType<GameManager>();
			boardUI = FindObjectOfType<BoardUI>();
			CreateBoardUI();
		}



		void Update()
		{
			Seb.Vis.Draw.Point(boardUI.PositionFromCoord(new Coord(square), -.5f), 0.2f, Color.red);
			if (displayType == DisplayType.Align)
			{
				Seb.Vis.Draw.Point(boardUI.PositionFromCoord(new Coord(alignSquare), -.5f), 0.2f, Color.red);
			}
			Board b = gameManager.board;

			//int rank = BoardHelper.RankIndex(square);
			//ulong whiteForwardMask = (ulong.MaxValue >> (64 - 8 * (rank + 1)));
			//ulong blackForwardMask = ~((1ul << 8 * rank) - 1);

			ulong bitboard = displayType switch
			{
				DisplayType.None => 0,
				DisplayType.Temp => Bits.TripleFileMask[BoardHelper.FileIndex(square)],
				DisplayType.ProtectedPawns => BitBoardUtility.ProtectedPawns(b.pieceBitboards[Piece.MakePiece(Piece.Pawn, displayWhitePieces)], displayWhitePieces),
				DisplayType.LockedPawns => BitBoardUtility.LockedPawns(b.pieceBitboards[Piece.WhitePawn], b.pieceBitboards[Piece.BlackPawn]),
				DisplayType.Custom => customBitboard,
				DisplayType.AllPiecesSingleSide => b.colourBitboards[displayWhitePieces ? Board.WhiteIndex : Board.BlackIndex],
				DisplayType.AllPiecesBothSides => b.allPiecesBitboard,
				DisplayType.Pawn => b.pieceBitboards[Piece.MakePiece(Piece.Pawn, displayWhitePieces)],
				DisplayType.Knight => b.pieceBitboards[Piece.MakePiece(Piece.Knight, displayWhitePieces)],
				DisplayType.Bishop => b.pieceBitboards[Piece.MakePiece(Piece.Bishop, displayWhitePieces)],
				DisplayType.Rook => b.pieceBitboards[Piece.MakePiece(Piece.Rook, displayWhitePieces)],
				DisplayType.Queen => b.pieceBitboards[Piece.MakePiece(Piece.Queen, displayWhitePieces)],
				DisplayType.King => b.pieceBitboards[Piece.MakePiece(Piece.King, displayWhitePieces)],
				DisplayType.KnightMovesBySquare => BitBoardUtility.KnightAttacks[square],
				DisplayType.KingsideMask => displayWhitePieces ? Bits.WhiteKingsideMask : Bits.BlackKingsideMask,
				DisplayType.QueensideMask => displayWhitePieces ? Bits.WhiteQueensideMask : Bits.BlackQueensideMask,
				DisplayType.PawnAttacks => displayWhitePieces ? BitBoardUtility.WhitePawnAttacks[square] : BitBoardUtility.BlackPawnAttacks[square],
				DisplayType.Align => PrecomputedMoveData.alignMask[square, alignSquare],
				DisplayType.DirRay => PrecomputedMoveData.dirRayMask[alignSquare, square],
				DisplayType.OrthoSliders => b.IsWhiteToMove == displayWhitePieces ? b.FriendlyOrthogonalSliders : b.EnemyOrthogonalSliders,
				DisplayType.DiagonalSliders => b.IsWhiteToMove == displayWhitePieces ? b.FriendlyDiagonalSliders : b.EnemyDiagonalSliders,
				DisplayType.PassedPawnMask => displayWhitePieces ? Bits.WhitePassedPawnMask[square] : Bits.BlackPassedPawnMask[square],
				DisplayType.PawnSupportMask => displayWhitePieces ? Bits.WhitePawnSupportMask[square] : Bits.BlackPawnSupportMask[square],
				DisplayType.PawnShieldSquares => displayWhitePieces ? SquaresToBitboard(PrecomputedEvaluationData.PawnShieldSquaresWhite[square]) : SquaresToBitboard(PrecomputedEvaluationData.PawnShieldSquaresBlack[square]),
				_ => 0
			};

			ulong bitboard2 = displayType switch
			{
				DisplayType.OrthoSliders => b.IsWhiteToMove != displayWhitePieces ? b.FriendlyOrthogonalSliders : b.EnemyOrthogonalSliders,
				DisplayType.DiagonalSliders => b.IsWhiteToMove != displayWhitePieces ? b.FriendlyDiagonalSliders : b.EnemyDiagonalSliders,
				_ => 0
			};


			if (lastBitboard != bitboard || lastBitboard2 != bitboard2 || settingsChanged)
			{
				settingsChanged = false;
				lastBitboard = bitboard;
				lastBitboard2 = bitboard2;
				UpdateSquares(bitboard, bitboard2);
			}

		}

		void UpdateSquares(ulong bitboard, ulong bitboard2)
		{
			for (int rank = 0; rank < 8; rank++)
			{
				for (int file = 0; file < 8; file++)
				{
					int square = BoardHelper.IndexFromCoord(file, rank);
					bool active = BitBoardUtility.ContainsSquare(bitboard, square);
					squareMaterials[file, rank].color = (active) ? colour : Color.clear;

					if (BitBoardUtility.ContainsSquare(bitboard2, square))
					{
						squareMaterials[file, rank].color = colour2;
					}
				}
			}
		}

		void CreateBoardUI()
		{
			Shader squareShader = Shader.Find("Unlit/ColorAlpha");
			squareMaterials = new Material[8, 8];

			for (int rank = 0; rank < 8; rank++)
			{
				for (int file = 0; file < 8; file++)
				{
					// Create square
					Transform square = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;
					square.parent = transform;
					square.name = BoardHelper.SquareNameFromCoordinate(file, rank);
					square.position = boardUI.PositionFromCoord(file, rank, -1);
					Material squareMaterial = new Material(squareShader);
					square.GetComponent<MeshRenderer>().material = squareMaterial;
					squareMaterials[file, rank] = squareMaterial;
				}
			}

			UpdateSquares(0, 0);
		}

		void OnValidate()
		{
			settingsChanged = true;
		}

		static ulong SquaresToBitboard(int[] squares)
		{
			ulong bitboard = 0;
			foreach (int square in squares)
			{
				bitboard |= 1ul << square;
			}
			return bitboard;
		}

	}
}