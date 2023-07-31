namespace Chess.Game
{
	using System.Collections.Generic;
	using System.Collections;
	using UnityEngine;

	public class Clock : MonoBehaviour
	{
		public event System.Action TimedOut;

		public TMPro.TMP_Text timerUI;
		bool isRunning;
		float secondsRemaining;
		public int lowTimeThreshold = 10;
		[Range(0, 1)]
		public float inactiveAlpha = 0.75f;
		[Range(0, 1)]
		public float decimalFontSizeMultiplier = 0.75f;
		public Color lowTimeCol;

		public void SetTime(int seconds) => secondsRemaining = seconds;

		public void AddTime(int seconds) => secondsRemaining += seconds;

		public void SetPaused(bool isPaused)
		{
			isRunning = !isPaused;
		}

		void Update()
		{
			if (isRunning)
			{
				secondsRemaining -= Time.deltaTime;
				secondsRemaining = Mathf.Max(0, secondsRemaining);

				if (secondsRemaining == 0)
				{
					isRunning = false;
					TimedOut?.Invoke();
				}
			}
			int numMinutes = (int)(secondsRemaining / 60);
			int numSeconds = (int)(secondsRemaining - numMinutes * 60);

			timerUI.text = $"{numMinutes:00}:{numSeconds:00}";
			if (secondsRemaining <= lowTimeThreshold)
			{
				int dec = (int)((secondsRemaining - numSeconds) * 10);
				float size = timerUI.fontSize * decimalFontSizeMultiplier;
				timerUI.text += $"<size={size}>.{dec}</size>";
			}

			var col = Color.white;
			if ((int)secondsRemaining <= lowTimeThreshold)
			{
				col = lowTimeCol;
			}
			if (!isRunning)
			{
				col = new Color(col.r, col.g, col.b, inactiveAlpha);
			}
			timerUI.color = col;
		}

	}
}