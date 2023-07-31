using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chess.Core;
using Chess.UI;
using Chess.Players;
using UnityEngine.InputSystem;
using Seb.Vis;
using Chess.Game;

namespace Chess.Testing
{
	public class EvalTest : MonoBehaviour
	{
		public bool useCustomFen;
		public string fen;
		public bool runSearch;


		[Header("Static Eval Data")]
		public int staticEvalWhitePerspective;
		public int staticEvalCurrPerspective;
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


		void Start()
		{
			board = new Board();
			player = new HumanPlayer(board);
			player.onMoveChosen += OnMoveChosen;

			board.LoadPosition(useCustomFen ? fen : FenUtility.StartPositionFEN);
			boardUI.UpdatePosition(board);

			settings = ScriptableObject.Instantiate(settingsPrefab);


			RunStaticEvaluation();
			StartNewSearch();

		}

		void StartNewSearch()
		{
			if (bot != null)
			{
				bot.AbortSearch();
				bot = null;
			}
			if (runSearch)
			{

				Board searchBoard = new Board();
				searchBoard.LoadPosition(FenUtility.CurrentFen(board));
				bot = new AIPlayer(searchBoard, settings);
				infoText.text = "Searching...";
				bot.search.ClearForNewPosition();
				bot.NotifyTurnToMove();
			}
		}

		void Update()
		{

			if (Keyboard.current[Key.U].wasPressedThisFrame && board.AllGameMoves.Count > 0)
			{
				Debug.Log("Undo move");
				board.UnmakeMove(board.AllGameMoves[^1]);
				boardUI.UpdatePosition(board);
				RunStaticEvaluation();
				StartNewSearch();
			}

			player.Update();

			UpdateSearchInfo();
		}

		void OnMoveChosen(Move move)
		{
			board.MakeMove(move);
			boardUI.UpdatePosition(board, move);
			RunStaticEvaluation();
			StartNewSearch();
		}

		public void RunStaticEvaluation()
		{
			Evaluation evaluator = new Evaluation();
			int eval = evaluator.Evaluate(board);
			whiteEvalData = evaluator.whiteEval;
			blackEvalData = evaluator.blackEval;
			staticEvalWhitePerspective = MakeEvaluationFromWhitePerspective(eval);
			staticEvalCurrPerspective = eval;
		}

		int MakeEvaluationFromWhitePerspective(int eval) => board.IsWhiteToMove ? eval : -eval;

		void UpdateSearchInfo()
		{
			if (runSearch)
			{
				Move bestMoveSoFar = bot.search.BestMoveSoFar;
				Vector2 startPos = boardUI.PositionFromCoord(new Coord(bestMoveSoFar.StartSquare));
				Vector2 targetPos = boardUI.PositionFromCoord(new Coord(bestMoveSoFar.TargetSquare));
				Draw.Arrow(startPos, targetPos, 0.17f, 0.25f, 0.6f, arrowCol);

				int eval = MakeEvaluationFromWhitePerspective(bot.search.BestEvalSoFar);

				infoText.text = $"Eval: {eval}\nDepth: {bot.search.CurrentDepth}";
			}
			else
			{
				infoText.text = "";
			}
		}

		void OnDestroy()
		{
			if (bot != null)
			{
				bot.AbortSearch();
			}
		}
	}
}
