namespace Chess.Game
{
	using UnityEngine;
	using Chess.Core;

	[CreateAssetMenu(menuName = "AI/Settings")]
	public class AISettings : ScriptableObject
	{

		public event System.Action requestCancelSearch;

		[Header("Search")]
		public SearchSettings.SearchMode mode;
		public int searchTimeMillis = 1000;
		public int fixedSearchDepth;
		public bool runOnMainThread;

		[Header("Book")]
		public bool useBook;
		public TextAsset book;
		public int maxBookPly = 16;
		public int bookMoveDelayMs = 200;
		public int bookMoveDelayRandomExtraMs = 0;

		[Header("Diagnostics")]
		public Searcher.SearchDiagnostics diagnostics;

		public void RequestCancelSearch()
		{
			requestCancelSearch?.Invoke();
		}
	}
}