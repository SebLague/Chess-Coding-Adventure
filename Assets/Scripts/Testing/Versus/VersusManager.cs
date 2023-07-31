using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using TMPro;
using UnityEngine;
using Chess.Core;
using Chess.UI;
using UnityEngine.UI;
using Chess.Game;

namespace Chess.Testing.Versus
{
	public class VersusManager : TCPServer
	{
		[Header("Settings")]
		public int maxThinkTimeMillis = 1000;
		public int maxGameLength = 100;
		public TextAsset openingPositions;
		public bool bothSidesPlayPosition = true;

		[Header("References")]
		public TMP_Text infoDisplay;
		public TMP_Text whiteNameUI;
		public TMP_Text blackNameUI;
		public TMP_Text debugInfo;
		public TMP_InputField thinkTimeInputField;
		public Button startButton;

		Player playerA;
		Player playerB;
		Board board;
		BoardUI boardUI;
		List<Move> gameMoves;
		string gameStartFen;
		int gameIndex;
		string[] startFens;

		string sessionSavePath;

		void Start()
		{
			Screen.SetResolution(1280, 720, FullScreenMode.Windowed);
			QualitySettings.vSyncCount = 1;
			startFens = openingPositions.text.Split('\n');
			gameIndex = -1;
			UpdateInfoDisplay();

			boardUI = FindObjectOfType<BoardUI>();
			board = new Board();
			SetUpUI();

			StartServer();
		}

		void SetUpUI()
		{
			startButton.interactable = false;
			startButton.onClick.AddListener(StartMatch);
			thinkTimeInputField.text = maxThinkTimeMillis + "";
			thinkTimeInputField.onEndEdit.AddListener((s) => UpdateSettingsFromUI());

		}

		void UpdateSettingsFromUI()
		{
			if (int.TryParse(thinkTimeInputField.text, out int result))
			{
				maxThinkTimeMillis = result;
			}
			else
			{
				thinkTimeInputField.text = maxThinkTimeMillis + "";
			}

			UpdateInfoDisplay();
		}

		void StartMatch()
		{
			StartNewGame();
			startButton.interactable = false;
		}



		protected override void MessageReceived(TcpClient client, string message)
		{
			// Parse message
			VersusMessage versusMessage = VersusMessage.CreateFromJson(message);
			switch (versusMessage.messageType)
			{
				case VersusMessage.MessageType.RegisterPlayer:
					RegisterPlayer(client, versusMessage.playerName);
					break;
				case VersusMessage.MessageType.MakeMove:
					MoveReceived(client, versusMessage);
					break;
			}
		}

		void MoveReceived(TcpClient client, VersusMessage message)
		{
			Player playerWhoMadeMove = (board.IsWhiteToMove) ? whitePlayer : blackPlayer;
			playerWhoMadeMove.numMoves++;
			playerWhoMadeMove.iterativeDeepeningDepthSum += Mathf.Min(message.iterativeDeepeningDepth, 15);//clamp outliers

			Move move = MoveUtility.MoveFromName(message.moveName, board);
			board.MakeMove(move);
			boardUI.UpdatePosition(board, move);
			gameMoves.Add(move);



			GameResult.Result result = GameResult.GetGameState(board);
			if (gameMoves.Count / 2 >= maxGameLength)
			{
				result = GameResult.Result.DrawByArbiter;
			}

			if (result == GameResult.Result.Playing && gameMoves.Count / 2 < maxGameLength)
			{
				Player playerToMove = (board.IsWhiteToMove) ? whitePlayer : blackPlayer;
				string notifyMessage = VersusMessage.CreateMoveMessage(message.moveName).ToJsonString();
				SendMessageToClient(playerToMove.client, notifyMessage);
			}
			else
			{
				OnGameOver(result);
			}
		}

		void OnGameOver(GameResult.Result result)
		{
			SaveGamePGN();
			UpdateStats();
			UpdateInfoDisplay();
			StartNewGame();

			void SaveGamePGN()
			{
				if (string.IsNullOrEmpty(sessionSavePath))
				{
					sessionSavePath = Seb.IOHelper.EnsureUniqueDirectoryName(Path.Combine(GamesSaveFolder, "Games"));
					Directory.CreateDirectory(sessionSavePath);
				}

				string resultFolderName = result switch
				{
					GameResult.Result.WhiteIsMated => whitePlayer == playerA ? "Loss" : "Win",
					GameResult.Result.BlackIsMated => whitePlayer == playerA ? "Win" : "Loss",
					_ => "Draw"
				};

				string gameSavePath = Path.Combine(sessionSavePath, resultFolderName);
				Directory.CreateDirectory(gameSavePath);

				string pgn = PGNCreator.CreatePGN(gameMoves.ToArray(), gameStartFen, whitePlayer.name, blackPlayer.name);
				string path = Path.Combine(gameSavePath, $"Game {gameIndex}.txt");
				StreamWriter writer = new StreamWriter(path);
				writer.Write(pgn);
				writer.Close();
			}

			void UpdateStats()
			{
				// Update stats
				if (result == GameResult.Result.BlackIsMated || result == GameResult.Result.WhiteIsMated)
				{
					Player winner = (result == GameResult.Result.WhiteIsMated) ? blackPlayer : whitePlayer;
					Player loser = (result == GameResult.Result.WhiteIsMated) ? whitePlayer : blackPlayer;
					winner.numWins++;
					loser.numLosses++;
				}
				else
				{
					whitePlayer.numDraws++;
					blackPlayer.numDraws++;
				}
			}
		}

		void ReadyToStartMatch()
		{
			startButton.interactable = true;
		}

		void StartNewGame()
		{
			gameIndex++;
			int fenIndex = bothSidesPlayPosition ? gameIndex / 2 : gameIndex;

			if (fenIndex < startFens.Length)
			{
				string fen = startFens[fenIndex];


				gameStartFen = fen;
				gameMoves = new List<Move>();

				string whiteMessage = VersusMessage.CreateNewGameMessage(fen, true, maxThinkTimeMillis).ToJsonString();
				string blackMessage = VersusMessage.CreateNewGameMessage(fen, false, maxThinkTimeMillis).ToJsonString();

				SendMessageToClient(whitePlayer.client, whiteMessage);
				SendMessageToClient(blackPlayer.client, blackMessage);

				// Update UI
				board.LoadPosition(fen);
				boardUI.UpdatePosition(board);
				whiteNameUI.text = "White: " + whitePlayer.name;
				blackNameUI.text = "Black: " + blackPlayer.name;
			}

			UpdateInfoDisplay();
		}

		protected override void OnClientConnected(TcpClient client)
		{
			base.OnClientConnected(client);
			debugInfo.text += "\nClient connected";
		}

		void RegisterPlayer(TcpClient client, string playerName)
		{
			debugInfo.text += "\nRegister " + playerName;
			Player newPlayer = new Player() { name = playerName, client = client };
			if (playerA == null)
			{
				playerA = newPlayer;
			}
			else if (playerB == null)
			{
				playerB = newPlayer;
				// Both players registered, ready to start the first game!
				ReadyToStartMatch();
			}
			else
			{
				debugInfo.text += "\nUnexpected registration, already have two players. Name: " + playerName;
			}

			UpdateInfoDisplay();
		}

		void UpdateInfoDisplay()
		{
			int numGames = startFens.Length * 2;
			infoDisplay.text = $"Game number: {Mathf.Min(gameIndex + 1, numGames)} / {numGames}";

			DisplayPlayerInfo(playerA);
			DisplayPlayerInfo(playerB);

			infoDisplay.text += "\n\nSettings:";
			infoDisplay.text += $"\nMax think time: {maxThinkTimeMillis} ms";
			infoDisplay.text += $"\nMax game length: {maxGameLength} moves";

			void DisplayPlayerInfo(Player player)
			{
				if (player == null)
				{
					infoDisplay.text += "\nPlayer not connected";
				}
				else
				{
					float avgDepth = player.iterativeDeepeningDepthSum / (float)Mathf.Max(1, player.numMoves);
					infoDisplay.text += $"\n{player.name}: Wins: {player.numWins} Losses: {player.numLosses} Draws: {player.numDraws} AvgDepth: {avgDepth:0.00}";
				}
			}
		}

		public static string GamesSaveFolder => Path.Combine(Application.persistentDataPath, "Versus Games");

		Player whitePlayer => bothSidesPlayPosition ? (gameIndex % 2 == 0 ? playerA : playerB) : playerA;
		Player blackPlayer => whitePlayer == playerA ? playerB : playerA;

		public class Player
		{
			public string name;
			public TcpClient client;
			public int numWins;
			public int numLosses;
			public int numDraws;

			public int numMoves;
			public int iterativeDeepeningDepthSum;
		}
	}
}