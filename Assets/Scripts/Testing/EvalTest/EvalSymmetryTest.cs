using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chess.Core;

namespace Chess.Testing
{
	public class EvalSymmetryTest : MonoBehaviour
	{
		public TextAsset positionFile;
		public int searchPly = 2;

		public string progress;
		MoveGenerator moveGen;
		Evaluation evaluator;
		Board board;
		Board flippedBoard;
		bool failed;

		void Start()
		{
			moveGen = new MoveGenerator();
			board = new Board();
			flippedBoard = new Board();
			evaluator = new Evaluation();

			StartCoroutine(RunTests());
		}

		IEnumerator RunTests()
		{
			string[] fens = positionFile.text.Split('\n');

			for (int i = 0; i < fens.Length; i++)
			{
				//Debug.Log(fens[i]);
				//Debug.Log(FenUtility.FlipFen(fens[i]));
				progress = $"Processing {i + 1}/{fens.Length}";
				Test(fens[i]);
				yield return null;
			}
			progress += " (Done)";
			Debug.Log("Done");
		}

		void Test(string fen)
		{
			board.LoadPosition(fen);
			flippedBoard.LoadPosition(FenUtility.FlipFen(fen));
			Search(searchPly);

		}

		void Search(int plyRemaining, string n = "")
		{
			if (failed)
			{
				return;
			}

			int eval = evaluator.Evaluate(board);
			int eval2 = evaluator.Evaluate(flippedBoard);
			if (eval != eval2)
			{
				Debug.Log("Eval not symmetric");
				failed = true;
			}

			if (plyRemaining <= 0)
			{
				return;
			}

			var moves = moveGen.GenerateMoves(board);
			foreach (var move in moves)
			{
				Coord startCoord = BoardHelper.CoordFromIndex(move.StartSquare);
				Coord targetCoord = BoardHelper.CoordFromIndex(move.TargetSquare);
				Coord flippedStartCoord = new Coord(startCoord.fileIndex, 7 - startCoord.rankIndex);
				Coord flippedTargetCoord = new Coord(targetCoord.fileIndex, 7 - targetCoord.rankIndex);

				Move flippedMove = new Move(flippedStartCoord.SquareIndex, flippedTargetCoord.SquareIndex, move.MoveFlag);
				board.MakeMove(move, true);
				flippedBoard.MakeMove(flippedMove, true);
				Search(plyRemaining - 1, MoveUtility.NameFromMove(move));
				board.UnmakeMove(move, true);
				flippedBoard.UnmakeMove(flippedMove, true);
			}
		}
	}
}