using System.Collections.Generic;

namespace Chess.Core
{
	// Represents the current state of the board during a game.
	// The state includes things such as: positions of all pieces, side to move,
	// castling rights, en-passant square, etc. Some extra information is included
	// as well to help with evaluation and move generation.

	// The initial state of the board can be set from a FEN string, and moves are
	// subsequently made (or undone) using the MakeMove and UnmakeMove functions.

	public sealed class Board
	{

		public ulong ZobristKey => currentGameState.zobristKey;
		public const int WhiteIndex = 0;
		public const int BlackIndex = 1;

		// Side to move info
		public bool IsWhiteToMove;
		public int MoveColour => IsWhiteToMove ? Piece.White : Piece.Black;
		public int OpponentColour => IsWhiteToMove ? Piece.Black : Piece.White;
		public int MoveColourIndex => IsWhiteToMove ? WhiteIndex : BlackIndex;
		public int OpponentColourIndex => IsWhiteToMove ? BlackIndex : WhiteIndex;

		// Stores piece code for each square on the board
		public int[] Square;

		// Piece lists
		public PieceList[] rooks;
		public PieceList[] bishops;
		public PieceList[] queens;
		public PieceList[] knights;
		public PieceList[] pawns;
		PieceList[] pieceLists;
		// Square index of white and black king
		public int[] KingSquare;

		// --- Bitboards ---
		// Bitboard for each piece type and colour (white pawns, white knights, ... black pawns, etc.)
		public ulong[] pieceBitboards;
		// Bitboards for all pieces of either colour (all white pieces, all black pieces)
		public ulong[] colourBitboards;
		public ulong allPiecesBitboard;
		public ulong FriendlyOrthogonalSliders;
		public ulong FriendlyDiagonalSliders;
		public ulong EnemyOrthogonalSliders;
		public ulong EnemyDiagonalSliders;

		// Total plies (half-moves) played in game
		public int plyCount;

		// List of (hashed) positions since last pawn move or capture (for detecting 3-fold repetition)
		public Stack<ulong> RepetitionPositionHistory;

		Stack<GameState> gameStateHistory;
		public GameState currentGameState;

		public List<Move> AllGameMoves;

		// piece count excluding pawns and kings
		public int totalPieceCountWithoutPawnsAndKings;
		bool cachedInCheckValue;
		bool hasCachedInCheckValue;

		// Update piece lists / bitboards based on given move info.
		// Note that this does not account for the following things, which must be handled separately:
		// 1. Removal of a captured piece
		// 2. Movement of rook when castling
		// 3. Removal of pawn from 1st/8th rank during pawn promotion
		// 4. Addition of promoted piece during pawn promotion
		void MovePiece(int piece, int startSquare, int targetSquare)
		{
			BitBoardUtility.ToggleSquares(ref pieceBitboards[piece], startSquare, targetSquare);
			BitBoardUtility.ToggleSquares(ref colourBitboards[MoveColourIndex], startSquare, targetSquare);

			pieceLists[piece].MovePiece(startSquare, targetSquare);
			Square[startSquare] = Piece.None;
			Square[targetSquare] = piece;
		}

		// Make a move on the board
		// The inSearch parameter controls whether this move should be recorded in the game history.
		// (for detecting three-fold repetition)
		public void MakeMove(Move move, bool inSearch = false)
		{
			// Get info about move
			int startSquare = move.StartSquare;
			int targetSquare = move.TargetSquare;
			int moveFlag = move.MoveFlag;
			bool isPromotion = move.IsPromotion;
			bool isEnPassant = moveFlag is Move.EnPassantCaptureFlag;

			int movedPiece = Square[startSquare];
			int movedPieceType = Piece.PieceType(movedPiece);
			int capturedPiece = isEnPassant ? Piece.MakePiece(Piece.Pawn, OpponentColour) : Square[targetSquare];
			int capturedPieceType = Piece.PieceType(capturedPiece);

			int prevCastleState = currentGameState.castlingRights;
			int prevEnPassantFile = currentGameState.enPassantFile;
			ulong newZobristKey = currentGameState.zobristKey;
			int newCastlingRights = currentGameState.castlingRights;
			int newEnPassantFile = 0;

			// Update bitboard of moved piece (pawn promotion is a special case and is corrected later)
			MovePiece(movedPiece, startSquare, targetSquare);

			// Handle captures
			if (capturedPieceType != Piece.None)
			{
				int captureSquare = targetSquare;

				if (isEnPassant)
				{
					captureSquare = targetSquare + (IsWhiteToMove ? -8 : 8);
					Square[captureSquare] = Piece.None;
				}
				if (capturedPieceType != Piece.Pawn)
				{
					totalPieceCountWithoutPawnsAndKings--;
				}

				// Remove captured piece from bitboards/piece list
				pieceLists[capturedPiece].RemovePieceAtSquare(captureSquare);
				BitBoardUtility.ToggleSquare(ref pieceBitboards[capturedPiece], captureSquare);
				BitBoardUtility.ToggleSquare(ref colourBitboards[OpponentColourIndex], captureSquare);
				newZobristKey ^= Zobrist.piecesArray[capturedPiece, captureSquare];
			}

			// Handle king
			if (movedPieceType == Piece.King)
			{
				KingSquare[MoveColourIndex] = targetSquare;
				newCastlingRights &= (IsWhiteToMove) ? 0b1100 : 0b0011;

				// Handle castling
				if (moveFlag == Move.CastleFlag)
				{
					int rookPiece = Piece.MakePiece(Piece.Rook, MoveColour);
					bool kingside = targetSquare == BoardHelper.g1 || targetSquare == BoardHelper.g8;
					int castlingRookFromIndex = (kingside) ? targetSquare + 1 : targetSquare - 2;
					int castlingRookToIndex = (kingside) ? targetSquare - 1 : targetSquare + 1;

					// Update rook position
					BitBoardUtility.ToggleSquares(ref pieceBitboards[rookPiece], castlingRookFromIndex, castlingRookToIndex);
					BitBoardUtility.ToggleSquares(ref colourBitboards[MoveColourIndex], castlingRookFromIndex, castlingRookToIndex);
					pieceLists[rookPiece].MovePiece(castlingRookFromIndex, castlingRookToIndex);
					Square[castlingRookFromIndex] = Piece.None;
					Square[castlingRookToIndex] = Piece.Rook | MoveColour;

					newZobristKey ^= Zobrist.piecesArray[rookPiece, castlingRookFromIndex];
					newZobristKey ^= Zobrist.piecesArray[rookPiece, castlingRookToIndex];
				}
			}

			// Handle promotion
			if (isPromotion)
			{
				totalPieceCountWithoutPawnsAndKings++;
				int promotionPieceType = moveFlag switch
				{
					Move.PromoteToQueenFlag => Piece.Queen,
					Move.PromoteToRookFlag => Piece.Rook,
					Move.PromoteToKnightFlag => Piece.Knight,
					Move.PromoteToBishopFlag => Piece.Bishop,
					_ => 0
				};

				int promotionPiece = Piece.MakePiece(promotionPieceType, MoveColour);

				// Remove pawn from promotion square and add promoted piece instead
				BitBoardUtility.ToggleSquare(ref pieceBitboards[movedPiece], targetSquare);
				BitBoardUtility.ToggleSquare(ref pieceBitboards[promotionPiece], targetSquare);
				pieceLists[movedPiece].RemovePieceAtSquare(targetSquare);
				pieceLists[promotionPiece].AddPieceAtSquare(targetSquare);
				Square[targetSquare] = promotionPiece;
			}

			// Pawn has moved two forwards, mark file with en-passant flag
			if (moveFlag == Move.PawnTwoUpFlag)
			{
				int file = BoardHelper.FileIndex(startSquare) + 1;
				newEnPassantFile = file;
				newZobristKey ^= Zobrist.enPassantFile[file];
			}

			// Update castling rights
			if (prevCastleState != 0)
			{
				// Any piece moving to/from rook square removes castling right for that side
				if (targetSquare == BoardHelper.h1 || startSquare == BoardHelper.h1)
				{
					newCastlingRights &= GameState.ClearWhiteKingsideMask;
				}
				else if (targetSquare == BoardHelper.a1 || startSquare == BoardHelper.a1)
				{
					newCastlingRights &= GameState.ClearWhiteQueensideMask;
				}
				if (targetSquare == BoardHelper.h8 || startSquare == BoardHelper.h8)
				{
					newCastlingRights &= GameState.ClearBlackKingsideMask;
				}
				else if (targetSquare == BoardHelper.a8 || startSquare == BoardHelper.a8)
				{
					newCastlingRights &= GameState.ClearBlackQueensideMask;
				}
			}

			// Update zobrist key with new piece position and side to move
			newZobristKey ^= Zobrist.sideToMove;
			newZobristKey ^= Zobrist.piecesArray[movedPiece, startSquare];
			newZobristKey ^= Zobrist.piecesArray[Square[targetSquare], targetSquare];
			newZobristKey ^= Zobrist.enPassantFile[prevEnPassantFile];

			if (newCastlingRights != prevCastleState)
			{
				newZobristKey ^= Zobrist.castlingRights[prevCastleState]; // remove old castling rights state
				newZobristKey ^= Zobrist.castlingRights[newCastlingRights]; // add new castling rights state
			}

			// Change side to move
			IsWhiteToMove = !IsWhiteToMove;

			plyCount++;
			int newFiftyMoveCounter = currentGameState.fiftyMoveCounter + 1;

			// Update extra bitboards
			allPiecesBitboard = colourBitboards[WhiteIndex] | colourBitboards[BlackIndex];
			UpdateSliderBitboards();

			// Pawn moves and captures reset the fifty move counter and clear 3-fold repetition history
			if (!inSearch && (movedPieceType == Piece.Pawn || capturedPieceType != Piece.None))
			{
				RepetitionPositionHistory.Clear();
				newFiftyMoveCounter = 0;
			}

			GameState newState = new(capturedPieceType, newEnPassantFile, newCastlingRights, newFiftyMoveCounter, newZobristKey);
			gameStateHistory.Push(newState);
			currentGameState = newState;
			hasCachedInCheckValue = false;

			if (!inSearch)
			{
				RepetitionPositionHistory.Push(newState.zobristKey);
				AllGameMoves.Add(move);
			}
		}

		// Undo a move previously made on the board
		public void UnmakeMove(Move move, bool inSearch = false)
		{
			// Swap colour to move
			IsWhiteToMove = !IsWhiteToMove;

			bool undoingWhiteMove = IsWhiteToMove;

			// Get move info
			int movedFrom = move.StartSquare;
			int movedTo = move.TargetSquare;
			int moveFlag = move.MoveFlag;

			bool undoingEnPassant = moveFlag == Move.EnPassantCaptureFlag;
			bool undoingPromotion = move.IsPromotion;
			bool undoingCapture = currentGameState.capturedPieceType != Piece.None;

			int movedPiece = undoingPromotion ? Piece.MakePiece(Piece.Pawn, MoveColour) : Square[movedTo];
			int movedPieceType = Piece.PieceType(movedPiece);
			int capturedPieceType = currentGameState.capturedPieceType;

			// If undoing promotion, then remove piece from promotion square and replace with pawn
			if (undoingPromotion)
			{
				int promotedPiece = Square[movedTo];
				int pawnPiece = Piece.MakePiece(Piece.Pawn, MoveColour);
				totalPieceCountWithoutPawnsAndKings--;

				pieceLists[promotedPiece].RemovePieceAtSquare(movedTo);
				pieceLists[movedPiece].AddPieceAtSquare(movedTo);
				BitBoardUtility.ToggleSquare(ref pieceBitboards[promotedPiece], movedTo);
				BitBoardUtility.ToggleSquare(ref pieceBitboards[pawnPiece], movedTo);
			}

			MovePiece(movedPiece, movedTo, movedFrom);

			// Undo capture
			if (undoingCapture)
			{
				int captureSquare = movedTo;
				int capturedPiece = Piece.MakePiece(capturedPieceType, OpponentColour);

				if (undoingEnPassant)
				{
					captureSquare = movedTo + ((undoingWhiteMove) ? -8 : 8);
				}
				if (capturedPieceType != Piece.Pawn)
				{
					totalPieceCountWithoutPawnsAndKings++;
				}

				// Add back captured piece
				BitBoardUtility.ToggleSquare(ref pieceBitboards[capturedPiece], captureSquare);
				BitBoardUtility.ToggleSquare(ref colourBitboards[OpponentColourIndex], captureSquare);
				pieceLists[capturedPiece].AddPieceAtSquare(captureSquare);
				Square[captureSquare] = capturedPiece;
			}


			// Update king
			if (movedPieceType is Piece.King)
			{
				KingSquare[MoveColourIndex] = movedFrom;

				// Undo castling
				if (moveFlag is Move.CastleFlag)
				{
					int rookPiece = Piece.MakePiece(Piece.Rook, MoveColour);
					bool kingside = movedTo == BoardHelper.g1 || movedTo == BoardHelper.g8;
					int rookSquareBeforeCastling = kingside ? movedTo + 1 : movedTo - 2;
					int rookSquareAfterCastling = kingside ? movedTo - 1 : movedTo + 1;

					// Undo castling by returning rook to original square
					BitBoardUtility.ToggleSquares(ref pieceBitboards[rookPiece], rookSquareAfterCastling, rookSquareBeforeCastling);
					BitBoardUtility.ToggleSquares(ref colourBitboards[MoveColourIndex], rookSquareAfterCastling, rookSquareBeforeCastling);
					Square[rookSquareAfterCastling] = Piece.None;
					Square[rookSquareBeforeCastling] = rookPiece;
					pieceLists[rookPiece].MovePiece(rookSquareAfterCastling, rookSquareBeforeCastling);
				}
			}

			allPiecesBitboard = colourBitboards[WhiteIndex] | colourBitboards[BlackIndex];
			UpdateSliderBitboards();

			if (!inSearch && RepetitionPositionHistory.Count > 0)
			{
				RepetitionPositionHistory.Pop();
			}
			if (!inSearch)
			{
				AllGameMoves.RemoveAt(AllGameMoves.Count - 1);
			}

			// Go back to previous state
			gameStateHistory.Pop();
			currentGameState = gameStateHistory.Peek();
			plyCount--;
			hasCachedInCheckValue = false;
		}

		// Switch side to play without making a move (NOTE: must not be in check when called)
		public void MakeNullMove()
		{
			IsWhiteToMove = !IsWhiteToMove;

			plyCount++;

			ulong newZobristKey = currentGameState.zobristKey;
			newZobristKey ^= Zobrist.sideToMove;
			newZobristKey ^= Zobrist.enPassantFile[currentGameState.enPassantFile];

			GameState newState = new(Piece.None, 0, currentGameState.castlingRights, currentGameState.fiftyMoveCounter + 1, newZobristKey);
			currentGameState = newState;
			gameStateHistory.Push(currentGameState);
			UpdateSliderBitboards();
			hasCachedInCheckValue = true;
			cachedInCheckValue = false;
		}

		public void UnmakeNullMove()
		{
			IsWhiteToMove = !IsWhiteToMove;
			plyCount--;
			gameStateHistory.Pop();
			currentGameState = gameStateHistory.Peek();
			UpdateSliderBitboards();
			hasCachedInCheckValue = true;
			cachedInCheckValue = false;
		}

		// Is current player in check?
		// Note: caches check value so calling multiple times does not require recalculating
		public bool IsInCheck()
		{
			if (hasCachedInCheckValue)
			{
				return cachedInCheckValue;
			}
			cachedInCheckValue = CalculateInCheckState();
			hasCachedInCheckValue = true;

			return cachedInCheckValue;
		}

		// Calculate in check value
		// Call IsInCheck instead for automatic caching of value
		public bool CalculateInCheckState()
		{
			int kingSquare = KingSquare[MoveColourIndex];
			ulong blockers = allPiecesBitboard;

			if (EnemyOrthogonalSliders != 0)
			{
				ulong rookAttacks = Magic.GetRookAttacks(kingSquare, blockers);
				if ((rookAttacks & EnemyOrthogonalSliders) != 0)
				{
					return true;
				}
			}
			if (EnemyDiagonalSliders != 0)
			{
				ulong bishopAttacks = Magic.GetBishopAttacks(kingSquare, blockers);
				if ((bishopAttacks & EnemyDiagonalSliders) != 0)
				{
					return true;
				}
			}

			ulong enemyKnights = pieceBitboards[Piece.MakePiece(Piece.Knight, OpponentColour)];
			if ((BitBoardUtility.KnightAttacks[kingSquare] & enemyKnights) != 0)
			{
				return true;
			}

			ulong enemyPawns = pieceBitboards[Piece.MakePiece(Piece.Pawn, OpponentColour)];
			ulong pawnAttackMask = IsWhiteToMove ? BitBoardUtility.WhitePawnAttacks[kingSquare] : BitBoardUtility.BlackPawnAttacks[kingSquare];
			if ((pawnAttackMask & enemyPawns) != 0)
			{
				return true;
			}

			return false;
		}


		// Load the starting position
		public void LoadStartPosition()
		{
			LoadPosition(FenUtility.StartPositionFEN);
		}

		// Load custom position from fen string
		public void LoadPosition(string fen)
		{
			Initialize();
			FenUtility.PositionInfo posInfo = FenUtility.PositionFromFen(fen);

			// Load pieces into board array and piece lists
			for (int squareIndex = 0; squareIndex < 64; squareIndex++)
			{
				int piece = posInfo.squares[squareIndex];
				int pieceType = Piece.PieceType(piece);
				int colourIndex = Piece.IsWhite(piece) ? WhiteIndex : BlackIndex;
				Square[squareIndex] = piece;

				if (piece != Piece.None)
				{
					BitBoardUtility.SetSquare(ref pieceBitboards[piece], squareIndex);
					BitBoardUtility.SetSquare(ref colourBitboards[colourIndex], squareIndex);

					if (pieceType == Piece.King)
					{
						KingSquare[colourIndex] = squareIndex;
					}
					else
					{
						pieceLists[piece].AddPieceAtSquare(squareIndex);
					}
					totalPieceCountWithoutPawnsAndKings += (pieceType is Piece.Pawn or Piece.King) ? 0 : 1;
				}
			}

			// Side to move
			IsWhiteToMove = posInfo.whiteToMove;

			// Set extra bitboards
			allPiecesBitboard = colourBitboards[WhiteIndex] | colourBitboards[BlackIndex];
			UpdateSliderBitboards();

			// Create gamestate
			int whiteCastle = ((posInfo.whiteCastleKingside) ? 1 << 0 : 0) | ((posInfo.whiteCastleQueenside) ? 1 << 1 : 0);
			int blackCastle = ((posInfo.blackCastleKingside) ? 1 << 2 : 0) | ((posInfo.blackCastleQueenside) ? 1 << 3 : 0);
			int castlingRights = whiteCastle | blackCastle;

			plyCount = (posInfo.moveCount - 1) * 2 + (IsWhiteToMove ? 0 : 1);

			// Set game state (note: calculating zobrist key relies on current game state)
			currentGameState = new GameState(Piece.None, posInfo.epFile, castlingRights, posInfo.fiftyMovePlyCount, 0);
			ulong zobristKey = Zobrist.CalculateZobristKey(this);
			currentGameState = new GameState(Piece.None, posInfo.epFile, castlingRights, posInfo.fiftyMovePlyCount, zobristKey);

			RepetitionPositionHistory.Push(zobristKey);

			gameStateHistory.Push(currentGameState);
		}

		void UpdateSliderBitboards()
		{
			int friendlyRook = Piece.MakePiece(Piece.Rook, MoveColour);
			int friendlyQueen = Piece.MakePiece(Piece.Queen, MoveColour);
			int friendlyBishop = Piece.MakePiece(Piece.Bishop, MoveColour);
			FriendlyOrthogonalSliders = pieceBitboards[friendlyRook] | pieceBitboards[friendlyQueen];
			FriendlyDiagonalSliders = pieceBitboards[friendlyBishop] | pieceBitboards[friendlyQueen];

			int enemyRook = Piece.MakePiece(Piece.Rook, OpponentColour);
			int enemyQueen = Piece.MakePiece(Piece.Queen, OpponentColour);
			int enemyBishop = Piece.MakePiece(Piece.Bishop, OpponentColour);
			EnemyOrthogonalSliders = pieceBitboards[enemyRook] | pieceBitboards[enemyQueen];
			EnemyDiagonalSliders = pieceBitboards[enemyBishop] | pieceBitboards[enemyQueen];
		}

		void Initialize()
		{
			AllGameMoves = new List<Move>();
			Square = new int[64];
			KingSquare = new int[2];

			RepetitionPositionHistory = new Stack<ulong>(capacity: 64);
			gameStateHistory = new Stack<GameState>(capacity: 64);

			currentGameState = new GameState();
			plyCount = 0;

			knights = new PieceList[] { new PieceList(10), new PieceList(10) };
			pawns = new PieceList[] { new PieceList(8), new PieceList(8) };
			rooks = new PieceList[] { new PieceList(10), new PieceList(10) };
			bishops = new PieceList[] { new PieceList(10), new PieceList(10) };
			queens = new PieceList[] { new PieceList(9), new PieceList(9) };

			pieceLists = new PieceList[Piece.MaxPieceIndex + 1];
			pieceLists[Piece.WhitePawn] = pawns[WhiteIndex];
			pieceLists[Piece.WhiteKnight] = knights[WhiteIndex];
			pieceLists[Piece.WhiteBishop] = bishops[WhiteIndex];
			pieceLists[Piece.WhiteRook] = rooks[WhiteIndex];
			pieceLists[Piece.WhiteQueen] = queens[WhiteIndex];
			pieceLists[Piece.WhiteKing] = new PieceList(1);

			pieceLists[Piece.BlackPawn] = pawns[BlackIndex];
			pieceLists[Piece.BlackKnight] = knights[BlackIndex];
			pieceLists[Piece.BlackBishop] = bishops[BlackIndex];
			pieceLists[Piece.BlackRook] = rooks[BlackIndex];
			pieceLists[Piece.BlackQueen] = queens[BlackIndex];
			pieceLists[Piece.BlackKing] = new PieceList(1);

			totalPieceCountWithoutPawnsAndKings = 0;

			// Initialize bitboards
			pieceBitboards = new ulong[Piece.MaxPieceIndex + 1];
			colourBitboards = new ulong[2];
			allPiecesBitboard = 0;
		}

	}
}