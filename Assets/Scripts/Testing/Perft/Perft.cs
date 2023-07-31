using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chess.Core;

namespace Chess.Testing
{
	public class Perft : MonoBehaviour
	{
		public enum Mode
		{
			RunFullSuite,
			RunFastSuite
		}

		[Header("Settings")]
		public Mode mode;
		public bool enableTimingStats;

		[Header("Test Suites")]
		public TextAsset fullPerftSuite;
		public TextAsset fastPerftSuite;

		[Header("Display")]
		public Color testNumCol;
		public Color passCol;
		public Color failCol;
		public Color timeCol;
		public Color fenCol;

		[Header("References")]
		public TMPro.TMP_Text logUI;

		MoveGenerator moveGenerator;
		Board board;

		// Timers
		System.Diagnostics.Stopwatch makeMoveTimer;
		System.Diagnostics.Stopwatch unmakeMoveTimer;
		System.Diagnostics.Stopwatch moveGenTimer;

		void Start()
		{
			board = new Board();
			moveGenerator = new MoveGenerator();
			logUI.text = "";

			switch (mode)
			{
				case Mode.RunFullSuite:
					StartCoroutine(RunSuite(fullPerftSuite));
					break;
				case Mode.RunFastSuite:
					StartCoroutine(RunSuite(fastPerftSuite));
					break;
			}
		}

		IEnumerator RunSuite(TextAsset perftSuite)
		{
			Test[] tests = GetSuiteTests(perftSuite);

			ClearLog();
			string testSuiteName = perftSuite.text.Split('\n')[0].Trim();
			LogMessage(SetTextColour($"<b>Running {testSuiteName} ({tests.Length} tests):</b>\n", Color.white));

			int suiteTime = 0;
			int numFailed = 0;

			makeMoveTimer = new System.Diagnostics.Stopwatch();
			unmakeMoveTimer = new System.Diagnostics.Stopwatch();
			moveGenTimer = new System.Diagnostics.Stopwatch();

			for (int i = 0; i < tests.Length; i++)
			{
				yield return new WaitForEndOfFrame();
				Test test = tests[i];

				(ulong numNodes, int timeMs) = RunTest(test.fen, test.depth);

				bool success = numNodes == test.expectedNodeCount;
				numFailed += success ? 0 : 1;
				suiteTime += timeMs;

				string testLog = "Test " + SetTextColour(i + 1, testNumCol) + ": ";
				testLog += $"{numNodes} nodes\t{SetTextColour(timeMs, timeCol)} ms.";
				testLog += "\t" + SetTextColour(success ? "Passed" : "Failed", success ? passCol : failCol);
				testLog += "\t FEN: " + SetTextColour(test.fen, fenCol);
				LogMessage(testLog);
			}

			string suiteLog = numFailed == 0 ? SetTextColour("Suite passed", passCol) : SetTextColour("Suite failed", failCol);
			suiteLog += ". Total time: " + SetTextColour(suiteTime, timeCol) + " ms.";
			LogMessage("\n<b>" + suiteLog + "</b>");

			if (enableTimingStats)
			{
				LogMessage("Timing breakdown: (note that enabling this adds some overhead)");
				LogMessage($"Make move: {makeMoveTimer.ElapsedMilliseconds} ms");
				LogMessage($"Unmake move: {unmakeMoveTimer.ElapsedMilliseconds} ms");
				LogMessage($"MoveGen: {moveGenTimer.ElapsedMilliseconds} ms");
			}
		}

		(ulong nodeCount, int timeMS) RunTest(string fen, int depth)
		{
			board.LoadPosition(fen);

			var sw = System.Diagnostics.Stopwatch.StartNew();
			ulong numNodes = SearchWithTimingStatsStackTest(depth);
			sw.Stop();

			return (numNodes, (int)sw.ElapsedMilliseconds);
		}

		ulong SearchWithTimingStatsStackTest(int depth)
		{
			if (enableTimingStats) moveGenTimer.Start();

			System.Span<Move> moves = stackalloc Move[MoveGenerator.MaxMoves];
			moves = moveGenerator.GenerateMoves(board, moves);

			if (enableTimingStats) moveGenTimer.Stop();

			ulong numLocalNodes = 0;

			if (depth == 1)
			{
				return (ulong)moves.Length;
			}

			for (int i = 0; i < moves.Length; i++)
			{
				if (enableTimingStats) makeMoveTimer.Start();
				board.MakeMove(moves[i]);
				if (enableTimingStats) makeMoveTimer.Stop();
				ulong numNodesFromThisPosition = SearchWithTimingStatsStackTest(depth - 1);
				numLocalNodes += numNodesFromThisPosition;
				if (enableTimingStats) unmakeMoveTimer.Start();
				board.UnmakeMove(moves[i]);
				if (enableTimingStats) unmakeMoveTimer.Stop();
			}
			return numLocalNodes;
		}

		void LogMessage(string message)
		{
			Debug.Log(message.Replace('\t', ' '));
			if (Application.isPlaying)
			{
				logUI.text += message + "\n";
			}
		}

		void ClearLog()
		{
			if (Application.isPlaying)
			{
				logUI.text = "";
			}
		}

		public Test[] GetSuiteTests(TextAsset suiteFile)
		{
			var testList = new List<Test>();

			string suiteText = suiteFile.text;
			suiteText = suiteText.Split('{')[1].Split('}')[0];
			string[] testStrings = suiteText.Split('\n');

			for (int i = 0; i < testStrings.Length; i++)
			{
				string testString = testStrings[i].Trim();
				string[] sections = testString.Split(',');
				if (sections.Length == 3)
				{
					var test = new Test() { depth = int.Parse(sections[0]), expectedNodeCount = ulong.Parse(sections[1]), fen = sections[2] };
					testList.Add(test);
				}
			}
			return testList.ToArray();
		}

		string SetTextColour(int value, Color col)
		{
			return SetTextColour(value + "", col);
		}

		string SetTextColour(string text, Color col)
		{
			return $"<color=#{ColorUtility.ToHtmlStringRGB(col)}>{text}</color>";
		}

		[System.Serializable]
		public struct Test
		{
			public string fen;
			public int depth;
			public ulong expectedNodeCount;
		}
	}
}