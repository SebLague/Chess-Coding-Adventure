using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chess.Game
{
	public class ClockManager : MonoBehaviour
	{

		public event System.Action<bool> ClockTimeout;

		bool whiteToPlay;
		int increment;

		[Header("References")]
		[SerializeField] Clock clockWhite;
		[SerializeField] Clock clockBlack;
		[SerializeField] TMPro.TMP_Text timeControlLabel;


		void Start()
		{
			clockWhite.TimedOut += () => ClockTimeout?.Invoke(true);
			clockBlack.TimedOut += () => ClockTimeout?.Invoke(true);
		}

		void Update()
		{

		}

		public void StartClocks(bool whiteToPlay, int minutesBase, int incrementSeconds)
		{
			this.whiteToPlay = whiteToPlay;
			this.increment = incrementSeconds;
			timeControlLabel.text = $"Time Control: {minutesBase} + {incrementSeconds}";

			clockWhite.SetTime(minutesBase * 60);
			clockBlack.SetTime(minutesBase * 60);
			(whiteToPlay ? clockWhite : clockBlack).SetPaused(false);
		}

		public void ToggleClock()
		{
			whiteToPlay = !whiteToPlay;
			(whiteToPlay ? clockWhite : clockBlack).SetPaused(false);
			(whiteToPlay ? clockBlack : clockWhite).SetPaused(true);
			(whiteToPlay ? clockBlack : clockWhite).AddTime(increment);
		}

		public void StopClocks()
		{
			clockWhite.SetPaused(true);
			clockBlack.SetPaused(true);
		}
	}
}