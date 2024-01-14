using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessLogic
{
    public class GameState
    {
        public Board Board { get; }
        public Player CurrentPlayer { get; private set; }
        public Result Result { get; private set; } = null;
        private readonly ChessBot chessBot;

        public int noCaptureOrPawnMoves = 0;
        private string stateString;

        private List<GameState> gameStates = new List<GameState>();
        private List<Move> moves = new List<Move>();

        private readonly Dictionary<string, int> stateHistory = new Dictionary<string, int>();
        public GameState(Player player, Board board, ChessBot chessBot)
        {
            CurrentPlayer = player;
            Board = board;
            this.chessBot = chessBot;

            stateString = new PosString(CurrentPlayer, board).ToString();
            stateHistory[stateString] = 1;
        }

        public void MakeBotMove()
        {
            // Use the chess bot to predict a move
            float[] move = chessBot.PredictMove(stateString);

            // Convert the predicted move to a Move object
            Move predictedMove = chessBot.ConvertPredictedMoveToMove(move);

            // Execute the predicted move
            MakeMove(predictedMove);
            // Store the game state and move
            gameStates.Add(this);
            moves.Add(predictedMove);
            chessBot.neuralNetwork.Train(gameStates, moves);
        }

        public float[] PredictMove(GameState gameState)
        {
            // Get the current game state as a string
            string stateString = gameState.stateString;

            // Use the chess bot to predict a move
            float[] move = chessBot.PredictMove(stateString);

            // Return the predicted move
            return move;
        }

        public IEnumerable<Move> LegalMoveForPiece(Position pos)
        {
            if (Board.IsEmpty(pos) || Board[pos].Color != CurrentPlayer)
            {
                return Enumerable.Empty<Move>();
            }

            Piece piece = Board[pos];
            IEnumerable<Move> moveCandidates = piece.GetMoves(pos, Board);
            return moveCandidates.Where(move => move.IsLegal(Board));
        }
        public void MakeMove(Move move)
        {
            Board.SetPawnSkipPosition(CurrentPlayer, null);
            bool captureOrPawn = move.Execute(Board);

            if (captureOrPawn)
            {
                noCaptureOrPawnMoves = 0;
                stateHistory.Clear();
            }
            else
            {
                noCaptureOrPawnMoves++; 
            }
            CurrentPlayer = CurrentPlayer.Opponent();
            UpdateStateString();
            CheckForGameOver();

        }
           // Getting ALL moves
        public IEnumerable<Move> AllLegalMovesFor(Player player)
        {
            IEnumerable<Move> moveCandidates = Board.PiecePositionsFor(player).SelectMany(pos =>
            {
                Piece piece = Board[pos];
                return piece.GetMoves(pos, Board);
            });

            return moveCandidates.Where(move => move.IsLegal(Board));
        }

        private void CheckForGameOver()
        {
            if (!AllLegalMovesFor(CurrentPlayer).Any())
            {
                if (Board.IsInCheck(CurrentPlayer))
                {
                    Result = Result.Win(CurrentPlayer.Opponent()); 
                }
                else
                {
                    Result = Result.Draw(EndReason.Stalemate);
                }
            }
            else if (Board.InsufficientMaterial())
            {
                Result = Result.Draw(EndReason.InsufficientMaterial);
            }
            else if (FiftyMoveRule())
            {
                Result = Result.Draw(EndReason.FiftyMoveRule);
            }
            else if (Repetition())
            {
                Result = Result.Draw(EndReason.ThreefoldRepetition);
            }
        }

        public bool IsGameOver()
        {
            return Result != null;
        }

        private bool FiftyMoveRule()
        {
            int fullMoves = noCaptureOrPawnMoves / 2;
            return fullMoves == 50;
        }

        private void UpdateStateString()
        {
            stateString = new PosString(CurrentPlayer, Board).ToString();

            if (!stateHistory.ContainsKey(stateString))
            {
                stateHistory[stateString] = 1;
            }
            else
            {
                stateHistory[stateString]++;
            }
        }
        private bool Repetition()
        {
            return stateHistory[stateString] == 3;
        }
        public void SelfPlay()
        {
            while (!IsGameOver())
            {
                MakeBotMove();
                chessBot.neuralNetwork.Train(gameStates, moves);
            }

        }


    }
}
