using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chess.Core;
using Chess.UI;
using UnityEngine.InputSystem;

namespace Chess.Players
{
	public class HumanPlayer : Player
	{

		public enum InputState
		{
			None,
			PieceSelected,
			DraggingPiece
		}

		InputState currentState;

		BoardUI boardUI;
		Camera cam;
		Coord selectedPieceSquare;
		Board board;
		public HumanPlayer(Board board)
		{
			boardUI = GameObject.FindObjectOfType<BoardUI>();
			cam = Camera.main;
			this.board = board;
		}

		public override void NotifyTurnToMove()
		{

		}

		public override void Update()
		{
			HandleInput();
		}

		void HandleInput()
		{
			Mouse mouse = Mouse.current;
			Vector2 mousePos = cam.ScreenToWorldPoint(mouse.position.ReadValue());

			if (currentState == InputState.None)
			{
				HandlePieceSelection(mousePos);
			}
			else if (currentState == InputState.DraggingPiece)
			{
				HandleDragMovement(mousePos);
			}
			else if (currentState == InputState.PieceSelected)
			{
				HandlePointAndClickMovement(mousePos);
			}

			if (mouse.rightButton.wasPressedThisFrame)
			{
				CancelPieceSelection();
			}
		}

		void HandlePointAndClickMovement(Vector2 mousePos)
		{
			if (Mouse.current.leftButton.isPressed)
			{
				HandlePiecePlacement(mousePos);
			}
		}

		void HandleDragMovement(Vector2 mousePos)
		{
			boardUI.DragPiece(selectedPieceSquare, mousePos);
			// If mouse is released, then try place the piece
			if (Mouse.current.leftButton.wasReleasedThisFrame)
			{
				HandlePiecePlacement(mousePos);
			}
		}

		void HandlePiecePlacement(Vector2 mousePos)
		{
			Coord targetSquare;
			if (boardUI.TryGetCoordFromPosition(mousePos, out targetSquare))
			{
				if (targetSquare.Equals(selectedPieceSquare))
				{
					boardUI.ResetPiecePosition(selectedPieceSquare);
					if (currentState == InputState.DraggingPiece)
					{
						currentState = InputState.PieceSelected;
					}
					else
					{
						currentState = InputState.None;
						boardUI.ResetSquareColours();
						boardUI.HighlightLastMadeMoveSquares(board);
					}
				}
				else
				{
					int targetIndex = BoardHelper.IndexFromCoord(targetSquare.fileIndex, targetSquare.rankIndex);
					if (Piece.IsColour(board.Square[targetIndex], board.MoveColour) && board.Square[targetIndex] != 0)
					{
						CancelPieceSelection();
						HandlePieceSelection(mousePos);
					}
					else
					{
						TryMakeMove(selectedPieceSquare, targetSquare);
					}
				}
			}
			else
			{
				CancelPieceSelection();
			}

		}

		void CancelPieceSelection()
		{
			if (currentState != InputState.None)
			{
				currentState = InputState.None;
				boardUI.ResetSquareColours();
				boardUI.HighlightLastMadeMoveSquares(board);
				boardUI.ResetPiecePosition(selectedPieceSquare);
			}
		}

		void TryMakeMove(Coord startSquare, Coord targetSquare)
		{
			int startIndex = BoardHelper.IndexFromCoord(startSquare);
			int targetIndex = BoardHelper.IndexFromCoord(targetSquare);
			bool moveIsLegal = false;
			Move chosenMove = new Move();

			MoveGenerator moveGenerator = new MoveGenerator();
			bool wantsKnightPromotion = Keyboard.current[Key.LeftAlt].isPressed;

			var legalMoves = moveGenerator.GenerateMoves(board);
			for (int i = 0; i < legalMoves.Length; i++)
			{
				var legalMove = legalMoves[i];

				if (legalMove.StartSquare == startIndex && legalMove.TargetSquare == targetIndex)
				{
					if (legalMove.IsPromotion)
					{
						if (legalMove.MoveFlag == Move.PromoteToQueenFlag && wantsKnightPromotion)
						{
							continue;
						}
						if (legalMove.MoveFlag != Move.PromoteToQueenFlag && !wantsKnightPromotion)
						{
							continue;
						}
					}
					moveIsLegal = true;
					chosenMove = legalMove;
					//	Debug.Log (legalMove.PromotionPieceType);
					break;
				}
			}

			if (moveIsLegal)
			{
				ChoseMove(chosenMove);
				currentState = InputState.None;
			}
			else
			{
				CancelPieceSelection();
			}
		}

		void HandlePieceSelection(Vector2 mousePos)
		{
			if (Mouse.current.leftButton.wasPressedThisFrame)
			{
				if (boardUI.TryGetCoordFromPosition(mousePos, out selectedPieceSquare))
				{
					int index = BoardHelper.IndexFromCoord(selectedPieceSquare);
					// If square contains a piece, select that piece for dragging
					if (Piece.IsColour(board.Square[index], board.MoveColour))
					{
						boardUI.HighlightLegalMoves(board, selectedPieceSquare);
						boardUI.HighlightSquare(selectedPieceSquare);
						currentState = InputState.DraggingPiece;
					}
				}
			}
		}
	}
}