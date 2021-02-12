namespace Chess {
	using System.Collections.Generic;
	using System.Collections;
	using System.IO;
	using UnityEngine;

	public class BookCreator : MonoBehaviour {

		public int maxPlyToRecord;

		public int minMovePlayCount = 10;

		public TextAsset gamesFile;
		public TextAsset bookFile;
		public bool append;

		void Start () {

		}

		[ContextMenu ("Create Book")]
		void CreateBook () {
			var sw = System.Diagnostics.Stopwatch.StartNew ();
			Book book = new Book ();

			var reader = new StringReader (gamesFile.text);
			string pgn;
			Board board = new Board ();
			while (!string.IsNullOrEmpty (pgn = reader.ReadLine ())) {

				Move[] moves = PGNLoader.MovesFromPGN (pgn, maxPlyCount : maxPlyToRecord);
				board.LoadStartPosition ();

				for (int i = 0; i < moves.Length; i++) {
					book.Add (board.ZobristKey, moves[i]);
					board.MakeMove (moves[i]);
				}
			}

			string bookString = "";

			foreach (var bookPositionsByZobristKey in book.bookPositions) {
				ulong key = bookPositionsByZobristKey.Key;
				BookPosition bookPosition = bookPositionsByZobristKey.Value;
				string line = key + ":";

				bool isFirstMoveEntry = true;
				foreach (var moveCountByMove in bookPosition.numTimesMovePlayed) {
					ushort moveValue = moveCountByMove.Key;
					int moveCount = moveCountByMove.Value;
					if (moveCount >= minMovePlayCount) {
						if (isFirstMoveEntry) {
							isFirstMoveEntry = false;
						} else {
							line += ",";
						}
						line += $" {moveValue} ({moveCount})";
					}
				}

				bool hasRecordedAnyMoves = !isFirstMoveEntry;
				if (hasRecordedAnyMoves) {
					bookString += line + System.Environment.NewLine;
				}
			}

			//string s = fastJSON.JSON.ToJSON (book);
			FileWriter.WriteToTextAsset_EditorOnly (bookFile, bookString, append);
			Debug.Log ("Created book: " + sw.ElapsedMilliseconds + " ms.");

			//Book loadedBook = fastJSON.JSON.ToObject<Book> (s);
		}

		public static Book LoadBookFromFile (TextAsset bookFile) {
			Book book = new Book ();
			var reader = new StringReader (bookFile.text);

			string line;
			while (!string.IsNullOrEmpty (line = reader.ReadLine ())) {
				ulong positionKey = ulong.Parse (line.Split (':') [0]);
				string[] moveInfoStrings = line.Split (':') [1].Trim ().Split (',');

				for (int i = 0; i < moveInfoStrings.Length; i++) {
					string moveInfoString = moveInfoStrings[i].Trim ();
					if (!string.IsNullOrEmpty (moveInfoString)) {

						ushort moveValue = ushort.Parse (moveInfoString.Split (' ') [0]);
						string numTimesPlayedString = moveInfoString.Split (' ') [1].Replace ("(", "").Replace (")", "");
						int numTimesPlayed = int.Parse (numTimesPlayedString);
						book.Add (positionKey, new Move (moveValue), numTimesPlayed);

					}
				}
			}

			return book;
		}

	}
}