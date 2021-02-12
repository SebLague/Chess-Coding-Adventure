using Chess.Game;
using UnityEngine;

namespace Chess.Testing {
	public class BitBoardDebugViewer : MonoBehaviour {
		public Color colour;
		public bool show;
		public bool showCustomBitboard;
		public ulong customBitboard;

		Material[, ] squareMaterials;
		GameManager gameManager;
		ulong lastBitboard;
		bool settingsChanged;
		/*
				void Start () {
					CreateBoardUI ();
					gameManager = FindObjectOfType<GameManager> ();
				}


				void Update () {

					var m = new MoveGenerator ();
					m.GenerateMoves ();

					ulong bitboard = (show) ? PrecomputedMoveData.queenMoves[Board.queens[0][0]] : 0ul;
					if (showCustomBitboard) {
						bitboard = customBitboard;
					}
					if (lastBitboard != bitboard || settingsChanged) {
						settingsChanged = false;
						lastBitboard = bitboard;
						UpdateSquares (bitboard);
					}

				}

				void UpdateSquares (ulong bitboard) {
					for (int rank = 0; rank < 8; rank++) {
						for (int file = 0; file < 8; file++) {
							int square = BoardRepresentation.IndexFromCoord (file, rank);
							bool active = BitBoardUtility.ContainsSquare (bitboard, square);
							squareMaterials[file, rank].color = (active) ? colour : Color.clear;
						}
					}
				}

				void CreateBoardUI () {
					Shader squareShader = Shader.Find ("Unlit/ColorAlpha");
					squareMaterials = new Material[8, 8];

					for (int rank = 0; rank < 8; rank++) {
						for (int file = 0; file < 8; file++) {
							// Create square
							Transform square = GameObject.CreatePrimitive (PrimitiveType.Quad).transform;
							square.parent = transform;
							square.name = BoardRepresentation.SquareNameFromCoordinate (file, rank);
							square.position = Chess.Game.BoardUI.PositionFromCoord (file, rank, -1);
							Material squareMaterial = new Material (squareShader);
							square.GetComponent<MeshRenderer> ().material = squareMaterial;
							squareMaterials[file, rank] = squareMaterial;
						}
					}

					UpdateSquares (0);
				}

				void OnValidate () {
					settingsChanged = true;
				}
				*/

	}
}