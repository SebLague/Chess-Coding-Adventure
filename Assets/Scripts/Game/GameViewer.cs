using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chess.Core;
using UnityEngine.InputSystem;

namespace Chess.UI
{


	public class GameViewer : MonoBehaviour
	{
		[Multiline]
		public string pgn;

		Move[] gameMoves;
		int moveIndex;
		BoardUI boardUI;
		Board board;

		void Start()
		{
			gameMoves = PGNLoader.MovesFromPGN(pgn);
			board = new Board();
			board.LoadStartPosition();
			boardUI = FindObjectOfType<BoardUI>();
			boardUI.UpdatePosition(board);
		}

		void Update()
		{
			if (Keyboard.current[Key.Space].wasPressedThisFrame)
			{
				if (moveIndex < gameMoves.Length)
				{
					board.MakeMove(gameMoves[moveIndex]);
					boardUI.UpdatePosition(board, gameMoves[moveIndex]);
					moveIndex++;
				}
			}
		}
	}
}