namespace Chess {
	using System.Collections.Generic;
	using System.Collections;
	using UnityEngine;

	[CreateAssetMenu (menuName = "AI/Settings")]
	public class AISettings : ScriptableObject {

		public event System.Action requestAbortSearch;

		public int depth;
		public bool useIterativeDeepening;
		public bool useTranspositionTable;

		public bool useThreading;
		public bool useFixedDepthSearch;
		public int searchTimeMillis = 1000;
		public bool endlessSearchMode;
		public bool clearTTEachMove;

		public bool useBook;
		public TextAsset book;
		public int maxBookPly = 10;
		
		public MoveGenerator.PromotionMode promotionsToSearch;

		public Search.SearchDiagnostics diagnostics;

		public void RequestAbortSearch () {
			requestAbortSearch?.Invoke ();
		}
	}
}