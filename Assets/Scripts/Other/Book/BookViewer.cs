namespace Chess {
	using System.Collections.Generic;
	using System.Collections;
	using Chess.Game;
	using UnityEngine;

	public class BookViewer : MonoBehaviour {
		public TextAsset bookFile;
		public float arrowWidth = 0.1f;
		public float arrowHeadSize = 0.1f;
		public Material arrowMaterial;
		public Color mostCommonCol;
		public Color rarestCol;
		BoardUI boardUI;

		Book book;
		Player player;

		List<GameObject> arrowObjects;
		int arrowIndex;
		Board board;
		Stack<Move> moves;

		void Start () {
			moves = new Stack<Move> ();
			arrowObjects = new List<GameObject> ();
			board = new Board ();
			var sw = System.Diagnostics.Stopwatch.StartNew ();
			book = BookCreator.LoadBookFromFile (bookFile);
			Debug.Log ("Book loaded: " + sw.ElapsedMilliseconds + " ms.");

			board.LoadStartPosition ();
			boardUI = FindObjectOfType<BoardUI> ();
			boardUI.UpdatePosition (board);

			player = new HumanPlayer (board);
			player.onMoveChosen += OnMoveChosen;
			player.NotifyTurnToMove ();
			DrawBookMoves ();
		}

		void Update () {
			if (Input.GetKeyDown (KeyCode.U)) {
				if (moves.Count > 0) {
					var move = moves.Pop ();
					board.UnmakeMove (move);
					if (moves.Count > 0) {
						boardUI.OnMoveMade (board, moves.Peek ());
					} else {
						boardUI.UpdatePosition (board);
						boardUI.ResetSquareColours (false);
					}

				}
			}
			DrawBookMoves ();
			player.Update ();
		}

		void DrawBookMoves () {
			ClearArrows ();
			arrowIndex = 0;

			if (book.HasPosition (board.ZobristKey)) {
				BookPosition bookPosition = book.GetBookPosition (board.ZobristKey);
				int mostPlayed = 0;
				int leastPlayed = int.MaxValue;
				foreach (var moveInfo in bookPosition.numTimesMovePlayed) {
					int numTimesPlayed = moveInfo.Value;
					mostPlayed = System.Math.Max (mostPlayed, numTimesPlayed);
					leastPlayed = System.Math.Min (leastPlayed, numTimesPlayed);
				}

				foreach (var moveInfo in bookPosition.numTimesMovePlayed) {
					Move move = new Move (moveInfo.Key);
					int numTimesPlayed = moveInfo.Value;
					Vector2 startPos = boardUI.PositionFromCoord (BoardRepresentation.CoordFromIndex (move.StartSquare));
					Vector2 endPos = boardUI.PositionFromCoord (BoardRepresentation.CoordFromIndex (move.TargetSquare));
					float t = Mathf.InverseLerp (leastPlayed, mostPlayed, numTimesPlayed);
					if (mostPlayed == leastPlayed) {
						t = 1;
					}

					Color col = Color.Lerp (rarestCol, mostCommonCol, t);
					DrawArrow2D (startPos, endPos, arrowWidth, arrowHeadSize, col, zPos: -1 - t);
				}
			}
		}

		void OnMoveChosen (Move move) {
			board.MakeMove (move);
			moves.Push (move);
			boardUI.OnMoveMade (board, move);
			DrawBookMoves ();
		}

		/// Draw a 2D arrow (on xy plane)
		void DrawArrow2D (Vector2 start, Vector2 end, float lineWidth, float headSize, Color color, bool flatHead = true, float zPos = 0) {
			if (arrowIndex >= arrowObjects.Count) {
				GameObject arrowObject = new GameObject ("Arrow");
				arrowObject.transform.parent = transform;

				var renderer = arrowObject.AddComponent<MeshRenderer> ();
				var filter = arrowObject.AddComponent<MeshFilter> ();
				renderer.material = arrowMaterial;
				filter.mesh = new Mesh ();
				arrowObjects.Add (arrowObject);
			}

			arrowObjects[arrowIndex].transform.position = new Vector3 (0, 0, zPos);
			arrowObjects[arrowIndex].SetActive (true);
			arrowObjects[arrowIndex].GetComponent<MeshRenderer> ().material.color = color;
			Mesh mesh = arrowObjects[arrowIndex].GetComponent<MeshFilter> ().mesh;
			ArrowMesh.CreateArrowMesh (ref mesh, start, end, lineWidth, headSize, flatHead);
			arrowIndex++;
		}

		void ClearArrows () {
			for (int i = 0; i < arrowObjects.Count; i++) {
				arrowObjects[i].SetActive (false);
			}
		}

	}
}