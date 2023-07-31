using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chess.Core;

namespace Chess.Testing
{
	public class EvalDebug : MonoBehaviour
	{
		public string fen;

		void Start()
		{
			Board board = new Board();
			board.LoadPosition(fen);

			Evaluation eval = new Evaluation();
			eval.Evaluate(board);
			//Debug.Log("White endgame weight: " + eval.materialInfo[Board.WhiteIndex].endgameWeight);
			//Debug.Log("Black endgame weight: " + eval.materialInfo[Board.BlackIndex].endgameWeight);
		//	Debug.Log("White king shield: " + eval.PawnShieldEval(Board.WhiteIndex, eval.materialInfo[Board.BlackIndex].endgameWeight));
			//Debug.Log("Black king shield: " + eval.PawnShieldEval(Board.BlackIndex, eval.materialInfo[Board.WhiteIndex].endgameWeight));
		}
	}
}