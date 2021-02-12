namespace Chess.Game {
	using System.Collections.Generic;
	using System.Collections;
	using UnityEngine;

	public class Clock : MonoBehaviour {

		public TMPro.TMP_Text timerUI;
		public bool isTurnToMove;
		public int startSeconds;
		float secondsRemaining;
		public int lowTimeThreshold = 10;
		[Range (0, 1)]
		public float inactiveAlpha = 0.75f;
		[Range (0, 1)]
		public float decimalFontSizeMultiplier = 0.75f;
		public Color lowTimeCol;

		void Start () {
			secondsRemaining = startSeconds;
		}

		void Update () {
			if (isTurnToMove) {
				secondsRemaining -= Time.deltaTime;
				secondsRemaining = Mathf.Max (0, secondsRemaining);
			}
			int numMinutes = (int) (secondsRemaining / 60);
			int numSeconds = (int) (secondsRemaining - numMinutes * 60);

			timerUI.text = $"{numMinutes:00}:{numSeconds:00}";
			if (secondsRemaining <= lowTimeThreshold) {
				int dec = (int) ((secondsRemaining - numSeconds) * 10);
				float size = timerUI.fontSize * decimalFontSizeMultiplier;
				timerUI.text += $"<size={size}>.{dec}</size>";
			}

			var col = Color.white;
			if ((int) secondsRemaining <= lowTimeThreshold) {
				col = lowTimeCol;
			}
			if (!isTurnToMove) {
				col = new Color (col.r, col.g, col.b, inactiveAlpha);
			}
			timerUI.color = col;
		}

	}
}