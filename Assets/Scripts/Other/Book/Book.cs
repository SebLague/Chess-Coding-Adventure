namespace Chess {
	using System.Collections.Generic;
	using System.Collections;
	using UnityEngine;

	[System.Serializable]
	public class Book {

		public Dictionary<ulong, BookPosition> bookPositions;

		public Book () {
			bookPositions = new Dictionary<ulong, BookPosition> ();
		}

		public bool HasPosition (ulong positionKey) {
			return bookPositions.ContainsKey (positionKey);
		}

		public BookPosition GetBookPosition (ulong key) {
			return bookPositions[key];
		}

		public Move GetRandomBookMove (ulong key) {
			var p = bookPositions[key];
			ushort[] moves = new List<ushort> (p.numTimesMovePlayed.Keys).ToArray ();
			var prng = new System.Random ();
			ushort randomMove = moves[prng.Next (0, moves.Length)];
			return new Move (randomMove);
		}

		public Move GetRandomBookMoveWeighted (ulong key) {
			var p = bookPositions[key];
			ushort[] moves = new List<ushort> (p.numTimesMovePlayed.Keys).ToArray ();
			int[] numTimesMovePlayed = new List<int> (p.numTimesMovePlayed.Values).ToArray ();

			float[] moveWeights = new float[moves.Length];
			for (int i = 0; i < moveWeights.Length; i++) {
				moveWeights[i] = numTimesMovePlayed[i];
			}

			// Smooth weights to increase probability of rarer moves
			// (strength of 1 would make all moves equally likely)
			SmoothWeights (moveWeights, strength : 0.5f);

			float sum = 0;
			for (int i = 0; i < moveWeights.Length; i++) {
				sum += moveWeights[i];
			}

			float[] moveProbabilitiesCumul = new float[moveWeights.Length];
			float previousProbability = 0;
			for (int i = 0; i < moveWeights.Length; i++) {
				moveProbabilitiesCumul[i] = previousProbability + moveWeights[i] / sum;
				previousProbability = moveProbabilitiesCumul[i];
			}

			var prng = new System.Random ();
			float t = (float) prng.NextDouble ();

			//for (int i = 0; i < moves.Length; i++) {
			//Debug.Log ((new Move (moves[i]).Name) + "  " + moveProbabilitiesCumul[i] + "  " + t);
			//}

			for (int i = 0; i < moves.Length; i++) {
				if (t <= moveProbabilitiesCumul[i]) {
					return new Move (moves[i]);
				}
			}

			return new Move (moves[0]);
		}

		void SmoothWeights (float[] weights, float strength = 0.1f) {
			float sum = 0;
			for (int i = 0; i < weights.Length; i++) {
				sum += weights[i];
			}
			float avg = sum / weights.Length;

			for (int i = 0; i < weights.Length; i++) {
				float offsetFromAvg = avg - weights[i];
				weights[i] += offsetFromAvg * strength;
			}
		}

		public void Add (ulong positionKey, Move move) {
			if (!bookPositions.ContainsKey (positionKey)) {
				bookPositions.Add (positionKey, new BookPosition ());
			}

			bookPositions[positionKey].AddMove (move);
		}

		public void Add (ulong positionKey, Move move, int numTimesPlayed) {
			if (!bookPositions.ContainsKey (positionKey)) {
				bookPositions.Add (positionKey, new BookPosition ());
			}

			bookPositions[positionKey].AddMove (move, numTimesPlayed);
		}
	}

	[System.Serializable]
	public class BookPosition {
		public Dictionary<ushort, int> numTimesMovePlayed;

		public BookPosition () {
			numTimesMovePlayed = new Dictionary<ushort, int> ();
		}

		public void AddMove (Move move, int numTimesPlayed = 1) {
			ushort moveValue = move.Value;

			if (numTimesMovePlayed.ContainsKey (moveValue)) {
				numTimesMovePlayed[moveValue]++;
			} else {
				numTimesMovePlayed.Add (moveValue, numTimesPlayed);
			}
		}
	}

}