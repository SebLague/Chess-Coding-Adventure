using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chess.Core;
using Chess.Players;
using Chess.UI;
using Seb.Vis;
using UnityEngine.InputSystem;
using Chess.Game;

namespace Chess.Testing
{
	public class PVTest : MonoBehaviour
	{

		public string fen;
		public bool runSearchOnStart;

		[Header("Static Eval Data")]
		public Evaluation.EvaluationData whiteEvalData;
		public Evaluation.EvaluationData blackEvalData;

		[Header("References")]
		public BoardUI boardUI;
		public AISettings settingsPrefab;
		public Color arrowCol;
		public TMPro.TMP_Text infoText;

		AISettings settings;
		HumanPlayer player;
		AIPlayer bot;
		Board board;
		bool searchComplete;

		void Start()
		{
			board = new Board();
			player = new HumanPlayer(board);
			player.onMoveChosen += OnMoveChosen;

			board.LoadPosition(fen);
			boardUI.UpdatePosition(board);

			settings = ScriptableObject.Instantiate(settingsPrefab);
			bot = new AIPlayer(board, settings);
			bot.onMoveChosen += OnBotMoveChosen;

			RunStaticEvaluation();

			if (runSearchOnStart)
			{
				infoText.text = "Searching...";
				bot.search.ClearForNewPosition();
				bot.NotifyTurnToMove();
			}
		}



		public void RunStaticEvaluation()
		{
			Evaluation evaluator = new Evaluation();
			int eval = evaluator.Evaluate(board);
			eval = MakeEvaluationFromWhitePerspective(eval);
			whiteEvalData = evaluator.whiteEval;
			blackEvalData = evaluator.blackEval;
			Debug.Log("Static Evaluation: " + eval);
		}

		public void RunQSearchEvaluation()
		{
			Debug.Log("not impl");
			//int eval = bot.search.QuiescenceSearch(Search.negativeInfinity, Search.positiveInfinity);
			//eval = MakeEvaluationFromWhitePerspective(eval);
			//Debug.Log("Qsearch Evaluation: " + eval);
		}


		void Update()
		{

			if (Keyboard.current[Key.U].wasPressedThisFrame && board.AllGameMoves.Count > 0)
			{
				Debug.Log("Undo move");
				board.UnmakeMove(board.AllGameMoves[^1]);
				boardUI.UpdatePosition(board);
				RunStaticEvaluation();
			}

			player.Update();

			bot.Update();
	
			if (searchComplete)
			{
				TranspositionTable tt = bot.search.GetTranspositionTable();
				var ttEntry = tt.GetEntry(board.currentGameState.zobristKey);
				if (ttEntry.key == board.currentGameState.zobristKey)
				{
					Move hashMove = ttEntry.move;
					Vector2 startPos = boardUI.PositionFromCoord(new Coord(hashMove.StartSquare));
					Vector2 targetPos = boardUI.PositionFromCoord(new Coord(hashMove.TargetSquare));
					Draw.Arrow(startPos, targetPos, 0.17f, 0.25f, 0.6f, arrowCol);

					string nodeType = ttEntry.nodeType switch
					{
						TranspositionTable.Exact => "Exact",
						TranspositionTable.LowerBound => "Lower Bound",
						TranspositionTable.UpperBound => "Upper Bound",
						_ => "None"
					};

					int eval = MakeEvaluationFromWhitePerspective(ttEntry.value);
					string evalString = ttEntry.nodeType switch
					{
						TranspositionTable.LowerBound => "At least " + eval,
						TranspositionTable.UpperBound => "At most " + eval,
						_ => eval + ""
					};

					infoText.text = $"Eval: {evalString}\nDepth: {ttEntry.depth}\nType: {nodeType}";
				}
				else
				{
					infoText.text = "End of search";
				}
			}
		}

		int MakeEvaluationFromWhitePerspective(int eval) => board.IsWhiteToMove ? eval : -eval;


		void OnMoveChosen(Move move)
		{
			board.MakeMove(move);
			boardUI.UpdatePosition(board, move);
			RunStaticEvaluation();
		}

		void OnBotMoveChosen(Move move)
		{
			searchComplete = true;
			//UpdateInfo(true);
		}

	}
}