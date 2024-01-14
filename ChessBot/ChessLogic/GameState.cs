using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace ChessLogic
{
    public class GameState
    {
        public Board Board { get; }
        public Player CurrentPlayer { get; private set; }
        public Result Result { get; private set; } = null;
        private ChessBot bot;

        public int noCaptureOrPawnMoves = 0;
        private string stateString;

        private readonly Dictionary<string, int> stateHistory = new Dictionary<string, int>();
        public GameState(Player player, Board board, ChessBot bot)
        {
            CurrentPlayer = player;
            Board = board;
            this.bot = bot;

            stateString = new PosString(CurrentPlayer, board).ToString();
            stateHistory[stateString] = 1;
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

        public Move MakeBotMove()
        {
            float output = bot.FeedForward(FenToArray(stateString))[0];
            Trace.WriteLine(output);
            Move[] legalMoveFor = AllLegalMovesFor(CurrentPlayer).ToArray();
            output *= legalMoveFor.Length - 1;

            output = MathF.Round(output);
            MakeMove(legalMoveFor[(int)output]);
            return legalMoveFor[(int)output];
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

        private float GetPieceValue(char piece)
        {
            switch (Char.ToUpper(piece))
            {
                case 'P': return 1;
                case 'R': return 2;
                case 'N': return 3;
                case 'B': return 4;
                case 'Q': return 5;
                case 'K': return 6;
                default: throw new ArgumentException("Invalid piece character");
            }
        }

        public float[] FenToArray(string fen)
        {
            // Split the FEN string into its constituent parts
            string[] parts = fen.Split(' ')[0].Split('/');

            // Initialize an empty array to hold the board representation
            float[] board = new float[8 * 8];


            // Iterate over each row of the board
            for (int i = 0; i < 8; i++)
            {
                int currentSquare = 0;
                // Iterate over each character in the row
                foreach (char c in parts[i])
                {
                    // If the character is a number, skip that many spaces
                    if (Char.IsDigit(c))
                    {
                        int numSpaces = Int32.Parse(c.ToString());
                        for (int j = 0; j < numSpaces; j++)
                        {
                            board[(i * 8) + j] = 0;
                            currentSquare++;
                        }
                    }
                    // Otherwise, map the piece to an integer and add it to the board
                    else
                    {
                        float pieceValue = GetPieceValue(c);
                        board[(i * 8) + currentSquare] = pieceValue;
                        currentSquare++;
                    }
                }
            }
            return board;
        }
    }
}
