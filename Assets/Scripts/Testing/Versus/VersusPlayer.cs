using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using Chess.Players;
using Chess.Core;
using System.IO;
using Chess.Game;

namespace Chess.Testing.Versus
{
	public class VersusPlayer : TcpPlayer
	{

		public string playerName;
		public TMPro.TMP_Text logUI;
		public AISettings aiSettings;
		public string commitGithubLink;

		public UnityEngine.UI.Button openCommitButton;
		public bool logSearchDebug;

		Player computerPlayer;
		Board board;
		bool isConnected;
		int gameIndex;

		string searchDebugInfo;
		string logpath;

		void Start()
		{
			Screen.SetResolution(1280, 720, FullScreenMode.Windowed);
			board = new Board();
			CreatePlayer();

			if (openCommitButton != null)
			{
				openCommitButton.onClick.AddListener(OpenCommit);
			}

			if (logSearchDebug)
			{
				logpath = Seb.IOHelper.EnsureUniqueDirectoryName(Path.Combine(Application.persistentDataPath, "Search Debug Log"));
				Directory.CreateDirectory(logpath);
			}
		}

		void OpenCommit()
		{
			Application.OpenURL(commitGithubLink);
		}

		protected override void Update()
		{
			base.Update();

			if (computerPlayer != null)
			{
				computerPlayer.Update();
			}

			if (logUI != null)
			{
				logUI.text = $"Name: {playerName}\nConnected: {isConnected}";
			}
		}

		protected override void OnConnected()
		{
			base.OnConnected();

			string message = VersusMessage.CreateRegisterPlayerMessage(playerName).ToJsonString();
			SendMessageToServer(message);
			isConnected = true;
		}

		protected override void MessageReceived(TcpClient client, string message)
		{
			base.MessageReceived(client, message);
			//logUI.text += "Message received: " + message + "\n";

			// Parse message
			VersusMessage msg = VersusMessage.CreateFromJson(message);
			switch (msg.messageType)
			{
				case VersusMessage.MessageType.NewGame:
					StartNewGame(msg.startFen, msg.playingAsWhite, msg.maxThinkTimeMs);
					break;
				case VersusMessage.MessageType.MakeMove:
					OpponentMoveReceived(msg.moveName);
					break;
			}
		}

		void StartNewGame(string fen, bool playingAsWhite, int maxThinkTime)
		{
			gameIndex++;
			if (logSearchDebug)
			{
				if (!string.IsNullOrEmpty(searchDebugInfo))
				{
					Seb.IOHelper.SaveTextToFile(logpath, "Game_" + (gameIndex - 1), "txt", searchDebugInfo, true);
				}
			}
			searchDebugInfo = "Search Debug Log: " + playerName;
			board.LoadPosition(fen);
			aiSettings.searchTimeMillis = maxThinkTime;

			if (computerPlayer == null)
			{
				computerPlayer = new AIPlayer(board, aiSettings);
				computerPlayer.onMoveChosen += OnMoveChosen;
			}
			else
			{
				(computerPlayer as AIPlayer).search.ClearForNewPosition();
			}



			if (board.IsWhiteToMove == playingAsWhite)
			{
				computerPlayer.NotifyTurnToMove();
			}
		}

		void OpponentMoveReceived(string moveName)
		{
			searchDebugInfo += "\n\nOpponent Move: " + moveName + "\n\n";
			Move move = MoveUtility.MoveFromName(moveName, board);
			board.MakeMove(move);
			computerPlayer.NotifyTurnToMove();
		}

		void OnMoveChosen(Move move)
		{

			board.MakeMove(move);
			var versusMessage = VersusMessage.CreateMoveMessage(MoveUtility.NameFromMove(move));
			if (computerPlayer is AIPlayer ai)
			{
				searchDebugInfo += ai.search.debugInfo;
				versusMessage.iterativeDeepeningDepth = ai.search.CurrentDepth;
			}
			string message = versusMessage.ToJsonString();
			SendMessageToServer(message);
		}

		
	}
}