using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Chess {
	using Chess.Game;

	public class GameViewer : MonoBehaviour {
		[Multiline]
		public string pgn;

		Move[] gameMoves;
		int moveIndex;
		BoardUI boardUI;
		Board board;

		void Start () {
			gameMoves = PGNLoader.MovesFromPGN (pgn);
			board = new Board();
			board.LoadStartPosition ();
			boardUI = FindObjectOfType<BoardUI> ();
			boardUI.UpdatePosition (board);
		}

		void Update () {
			if (Input.GetKeyDown (KeyCode.Space)) {
				if (moveIndex < gameMoves.Length) {
					board.MakeMove (gameMoves[moveIndex]);
					boardUI.OnMoveMade (board,gameMoves[moveIndex]);
					moveIndex++;
				}
			}
		}
	}
}