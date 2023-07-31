namespace Chess.Core
{
	public readonly struct SearchSettings
	{
		public enum SearchMode
		{
			IterativeDeepening,
			FixedDepth
		}

		public readonly SearchMode Mode;
		public readonly int FixedSearchDepth;

		public SearchSettings(SearchMode mode, int fixedSearchDepth)
		{
			Mode = mode;
			FixedSearchDepth = fixedSearchDepth;
		}
	}
}