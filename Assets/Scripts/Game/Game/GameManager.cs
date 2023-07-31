using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Chess.Core;
using Chess.UI;
using Chess.Players;
using UnityEngine.InputSystem;

namespace Chess.Game
{
	public class GameManager : MonoBehaviour
	{

		public event System.Action onPositionLoaded;
		public event System.Action<Move> onMoveMade;

		public enum PlayerType { Human, AI }

		[Header("Start Position")]
		public bool loadCustomPosition;
		public string customPosition = "1rbq1r1k/2pp2pp/p1n3p1/2b1p3/R3P3/1BP2N2/1P3PPP/1NBQ1RK1 w - - 0 1";

		[Header("Players")]
		public PlayerType whitePlayerType;
		public PlayerType blackPlayerType;

		[Header("Time")]
		public bool useClocks;

		public int minutesBase;
		public int incrementSeconds;

		[Header("Audio")]
		public bool useSoundEffects;

		[Header("References")]
		public ClockManager clockManager;
		public AISettings aiSettings;
		public TMPro.TMP_Text resultUI;
		public AudioClip moveSfx;
		public AudioClip captureSfx;

		[Header("Debug")]
		public string currentFen;
		public ulong zobristDebug;

		// Internal stuff
		GameResult.Result gameResult;

		Player whitePlayer;
		Player blackPlayer;
		Player playerToMove;
		BoardUI boardUI;
		AudioSource audioSource;

		public Board board { get; private set; }
		Board searchBoard; // Duplicate version of board used for ai search

		void Start()
		{
			clockManager.gameObject.SetActive(useClocks);
			Application.targetFrameRate = 120;

			boardUI = FindObjectOfType<BoardUI>();
			board = new Board();
			searchBoard = new Board();
			aiSettings.diagnostics = new Searcher.SearchDiagnostics();
			audioSource = GetComponent<AudioSource>();

			NewGame(whitePlayerType, blackPlayerType);

		}

		void Update()
		{
			HandleInput();
			UpdateGame();
			UpdateDebugInfo();
		}

		void UpdateGame()
		{
			if (gameResult == GameResult.Result.Playing)
			{
				playerToMove.Update();
			}

		}

		void UpdateDebugInfo()
		{
			zobristDebug = board.currentGameState.zobristKey;
			ulong generatedKey = Zobrist.CalculateZobristKey(board);
			if (generatedKey != zobristDebug)
			{
				Debug.Log("Key Error: incremental: " + zobristDebug + "  gen: " + generatedKey);
			}

		}

		void HandleInput()
		{
			Keyboard keyboard = Keyboard.current;

			if (keyboard[Key.U].wasPressedThisFrame)
			{
				if (board.AllGameMoves.Count > 0)
				{
					Move moveToUndo = board.AllGameMoves[^1];
					board.UnmakeMove(moveToUndo);
					searchBoard.UnmakeMove(moveToUndo);
					boardUI.UpdatePosition(board);
					boardUI.ResetSquareColours();
					boardUI.HighlightLastMadeMoveSquares(board);

					PlayMoveSound(moveToUndo);

					gameResult = GameResult.GetGameState(board);
					PrintGameResult(gameResult);
				}
			}

			if (keyboard[Key.E].wasPressedThisFrame)
			{
				ExportGame();
			}


			if (keyboard[Key.N].wasPressedThisFrame)
			{
				Debug.Log("Make Null Move");
				board.MakeNullMove();
			}


		}

		void OnMoveChosen(Move move)
		{
			PlayMoveSound(move);

			bool animateMove = playerToMove is AIPlayer;
			board.MakeMove(move);
			searchBoard.MakeMove(move);

			currentFen = FenUtility.CurrentFen(board);
			onMoveMade?.Invoke(move);
			boardUI.UpdatePosition(board, move, animateMove);

			if (useClocks)
			{
				clockManager.ToggleClock();
			}

			NotifyPlayerToMove();
		}

		public void NewGame(bool humanPlaysWhite)
		{
			boardUI.SetPerspective(humanPlaysWhite);
			NewGame((humanPlaysWhite) ? PlayerType.Human : PlayerType.AI, (humanPlaysWhite) ? PlayerType.AI : PlayerType.Human);
		}

		public void NewComputerVersusComputerGame()
		{
			boardUI.SetPerspective(true);
			NewGame(PlayerType.AI, PlayerType.AI);
		}

		void NewGame(PlayerType whitePlayerType, PlayerType blackPlayerType)
		{
			if (loadCustomPosition)
			{
				currentFen = customPosition;
				board.LoadPosition(customPosition);
				searchBoard.LoadPosition(customPosition);
			}
			else
			{
				currentFen = FenUtility.StartPositionFEN;
				board.LoadStartPosition();
				searchBoard.LoadStartPosition();
			}
			onPositionLoaded?.Invoke();
			boardUI.UpdatePosition(board);
			boardUI.ResetSquareColours();

			CreatePlayer(ref whitePlayer, whitePlayerType);
			CreatePlayer(ref blackPlayer, blackPlayerType);

			if (useClocks)
			{
				clockManager.StartClocks(board.IsWhiteToMove, minutesBase, incrementSeconds);
				clockManager.ClockTimeout -= OnTimeout;
				clockManager.ClockTimeout += OnTimeout;
			}


			gameResult = GameResult.Result.Playing;

			NotifyPlayerToMove();

		}


		public void ExportGame()
		{
			string pgn = PGNCreator.CreatePGN(board.AllGameMoves.ToArray());
			string baseUrl = "https://www.lichess.org/paste?pgn=";
			string escapedPGN = UnityEngine.Networking.UnityWebRequest.EscapeURL(pgn);
			string url = baseUrl + escapedPGN;

			Application.OpenURL(url);
			TextEditor t = new TextEditor();
			t.text = pgn;
			t.SelectAll();
			t.Copy();
		}

		public void QuitGame()
		{
			Application.Quit();
		}

		void NotifyPlayerToMove()
		{
			gameResult = GameResult.GetGameState(board);

			if (gameResult == GameResult.Result.Playing)
			{
				playerToMove = (board.IsWhiteToMove) ? whitePlayer : blackPlayer;

				playerToMove.NotifyTurnToMove();

			}
			else
			{
				GameOver();
			}
		}

		void GameOver()
		{
			Debug.Log("Game Over " + gameResult);
			PrintGameResult(gameResult);
			clockManager.StopClocks();
		}

		void PrintGameResult(GameResult.Result result)
		{
			if (result == GameResult.Result.Playing)
			{
				resultUI.text = "";
			}
			else
			{
				string subtitleSettings = $"<color=#787878> <size=75%>";
				resultUI.text = "Game Over\n" + subtitleSettings;

				if (result is GameResult.Result.WhiteIsMated or GameResult.Result.BlackIsMated)
				{
					string winner = result == GameResult.Result.WhiteIsMated ? "Black" : "White";
					resultUI.text += $"{winner} wins by checkmate";
				}
				else if (result is GameResult.Result.WhiteTimeout or GameResult.Result.BlackTimeout)
				{
					string winner = result == GameResult.Result.WhiteTimeout ? "Black" : "White";
					resultUI.text += $"{winner} wins on time";
				}
				else if (result == GameResult.Result.FiftyMoveRule)
				{
					resultUI.text += "Draw by 50 move rule";
				}
				else if (result == GameResult.Result.Repetition)
				{
					resultUI.text += "Draw by 3-fold repetition";
				}
				else if (result == GameResult.Result.Stalemate)
				{
					resultUI.text += "Draw by stalemate";
				}
				else if (result == GameResult.Result.InsufficientMaterial)
				{
					resultUI.text += "Draw due to insufficient material";
				}
			}
		}

		void PlayMoveSound(Move moveToBePlayed)
		{
			if (useSoundEffects)
			{
				bool isCapture = board.Square[moveToBePlayed.TargetSquare] != Piece.None;
				audioSource.PlayOneShot(isCapture ? captureSfx : moveSfx);
			}
		}

		void OnTimeout(bool whiteTimedOut)
		{
			gameResult = whiteTimedOut ? GameResult.Result.WhiteTimeout : GameResult.Result.BlackTimeout;
			GameOver();
		}

		void CreatePlayer(ref Player player, PlayerType playerType)
		{
			if (player != null)
			{
				player.onMoveChosen -= OnMoveChosen;
			}

			if (playerType == PlayerType.Human)
			{
				player = new HumanPlayer(board);
			}
			else
			{
				player = new AIPlayer(searchBoard, aiSettings);
			}
			player.onMoveChosen += OnMoveChosen;
		}
	}
}