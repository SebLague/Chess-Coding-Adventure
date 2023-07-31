using UnityEngine;

namespace Chess.Testing.Versus
{
	[System.Serializable]
	public class VersusMessage
	{

		public enum MessageType { None, RegisterPlayer, MakeMove, NewGame }
		public MessageType messageType;

		// Register player:
		public string playerName;

		// Make Move:
		public string moveName;
		public int iterativeDeepeningDepth;

		// New Game:
		public string startFen;
		public bool playingAsWhite;
		public int maxThinkTimeMs;

		public static VersusMessage CreateRegisterPlayerMessage(string playerName)
		{
			VersusMessage message = new VersusMessage()
			{
				messageType = MessageType.RegisterPlayer,
				playerName = playerName
			};

			return message;
		}

		public static VersusMessage CreateNewGameMessage(string fen, bool white, int maxThinkTimeMs)
		{
			VersusMessage message = new VersusMessage()
			{
				messageType = MessageType.NewGame,
				startFen = fen,
				playingAsWhite = white,
				maxThinkTimeMs = maxThinkTimeMs
			};

			return message;
		}

		public static VersusMessage CreateMoveMessage(string moveString)
		{
			VersusMessage message = new VersusMessage()
			{
				messageType = MessageType.MakeMove,
				moveName = moveString
			};

			return message;
		}

		public string ToJsonString()
		{
			return JsonUtility.ToJson(this);
		}

		public static VersusMessage CreateFromJson(string json)
		{
			return JsonUtility.FromJson<VersusMessage>(json);
		}
	}
}