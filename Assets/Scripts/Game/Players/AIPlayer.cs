namespace Chess.Players
{
	using System.Threading.Tasks;
	using System.Threading;
	using Chess.Core;
	using Chess.Game;
	using UnityEngine;

	public class AIPlayer : Player
	{

		public Searcher search;
		AISettings settings;
		bool moveFound;
		Move move;
		public Board board;
		CancellationTokenSource cancelSearchTimer;
		System.Random rng;

		OpeningBook book;

		public AIPlayer(Board board, AISettings settings)
		{
			this.settings = settings;
			this.board = board;
			rng = new System.Random();
			settings.requestCancelSearch += TimeOutThreadedSearch;

			search = new Searcher(board, CreateSearchSettings(settings));
			search.onSearchComplete += OnSearchComplete;
			search.searchDiagnostics = new Searcher.SearchDiagnostics();

			if (settings.useBook)
			{
				book = new OpeningBook(settings.book.text);
			}
		}

		// Update running on Unity main thread. This is used to return the chosen move so as
		// not to end up on a different thread and unable to interface with Unity stuff.
		public override void Update()
		{

			if (moveFound)
			{
				settings.diagnostics = search.searchDiagnostics;
				moveFound = false;
				ChoseMove(move);
			}

			settings.diagnostics = search.searchDiagnostics;

		}

		public void AbortSearch()
		{
			search.EndSearch();
		}

		public override void NotifyTurnToMove()
		{

			search.searchDiagnostics.isBook = false;
			moveFound = false;

			Move bookMove = Move.NullMove;
			if (settings.useBook && board.plyCount <= settings.maxBookPly)
			{
				if (book.TryGetBookMove(FenUtility.CurrentFen(board), out string moveString))
				{
					bookMove = MoveUtility.MoveFromName(moveString, board);
					//Debug.Log("Book move: " + moveString);
				}
				else
				{
					//Debug.Log("Failed to find pos " + FenUtility.CurrentFen(board));
				}
			}

			if (bookMove.IsNull)
			{
				if (settings.runOnMainThread)
				{
					StartSearch();
				}
				else
				{
					StartThreadedSearch();

				}
			}
			else
			{

				search.searchDiagnostics.isBook = true;
				search.searchDiagnostics.moveVal = Chess.PGNCreator.NotationFromMove(FenUtility.CurrentFen(board), bookMove);
				settings.diagnostics = search.searchDiagnostics;

				int waitTime = (int)(settings.bookMoveDelayMs + settings.bookMoveDelayRandomExtraMs * rng.NextDouble());
				Task.Delay(waitTime).ContinueWith((t) => PlayBookMove(bookMove));

			}
		}

		void StartSearch()
		{
			search.StartSearch();
			moveFound = true;
		}

		void StartThreadedSearch()
		{
			Task.Factory.StartNew(() => search.StartSearch(), TaskCreationOptions.LongRunning);

			if (settings.mode != SearchSettings.SearchMode.FixedDepth)
			{
				cancelSearchTimer = new CancellationTokenSource();
				Task.Delay(settings.searchTimeMillis, cancelSearchTimer.Token).ContinueWith((t) => TimeOutThreadedSearch());
			}

		}

		// Note: called outside of Unity main thread
		void TimeOutThreadedSearch()
		{
			if (cancelSearchTimer == null || !cancelSearchTimer.IsCancellationRequested)
			{
				search.EndSearch();
			}
		}

		void PlayBookMove(Move bookMove)
		{
			this.move = bookMove;
			moveFound = true;
		}

		SearchSettings CreateSearchSettings(AISettings aISettings)
		{
			return new SearchSettings();
		}
		void OnSearchComplete(Move move)
		{
			// Cancel search timer in case search finished before timer ran out (can happen when a mate is found)
			cancelSearchTimer?.Cancel();
			moveFound = true;
			this.move = move;
		}
	}
}