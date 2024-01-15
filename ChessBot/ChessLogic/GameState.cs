using Accord.Math.Distances;
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
        private Individual whiteIndividual;
        private Individual blackIndividual;

        public int noCaptureOrPawnMoves = 0;
        public string stateString;

        private readonly Dictionary<string, int> stateHistory = new Dictionary<string, int>();
        public GameState(Player player, Board board, Individual whiteIndividual, Individual blackIndividual)
        {
            CurrentPlayer = player;
            Board = board;
            this.whiteIndividual = whiteIndividual;
            this.blackIndividual = blackIndividual;
            stateString = new PosString(CurrentPlayer, board).ToString();
            stateHistory[stateString] = 1;
        }

        public GameState(Player player, Board board)
        {
            CurrentPlayer = player;
            Board = board;
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

        public Move MakeBotMove(Player player)
        {
            // selects the bot corresponding to the current player
            var currentBot = player == Player.White ? whiteIndividual.ChessBot : blackIndividual.ChessBot;

            // calculate move
            float output = currentBot.FeedForward(FenToArray(stateString))[0];
            Move[] legalMoveFor = AllLegalMovesFor(CurrentPlayer).ToArray();
            output *= legalMoveFor.Length - 1;
            output = MathF.Round(output);

            if (float.IsNaN(output))
            {
                MakeMove(legalMoveFor[0]); 
                return legalMoveFor[0];
            }
            else
            {
                MakeMove(legalMoveFor[(int)output]);
                return legalMoveFor[(int)output];
            }

            
        }



        public int CalculatePoints(Player player)
        {
            
            Piece[] pieceOnPos = Board.PieceOnBoard(player).ToArray();
            int points = 0;
            for (int i = 0; i < pieceOnPos.Length; i++)
            {
                if (pieceOnPos[i].Type == PieceType.Pawn)
                {
                    points += 1;
                }
                else if (pieceOnPos[i].Type == PieceType.Knight)
                {
                    points += 3;
                }
                else if (pieceOnPos[i].Type == PieceType.Bishop)
                {
                    points += 3;
                }
                else if (pieceOnPos[i].Type == PieceType.Rook)
                {
                    points += 5;
                }
                else if (pieceOnPos[i].Type == PieceType.Queen)
                {
                    points += 9;
                }
                else
                {
                    points += 0;
                }
            }
            return points;
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
            return stateHistory[stateString] >= 3;
        }

        private float GetPieceValue(char piece)
        {
            switch (Char.ToUpper(piece))
            {
                case 'P': return 0.1f;
                case 'R': return 0.2f;
                case 'N': return 0.3f;
                case 'B': return 0.4f;
                case 'Q': return 0.5f;
                case 'K': return 0.6f;
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

        public void AssignFitness()
        {
            // best in terms of piece count
            whiteIndividual.Fitness += CalculatePoints(Player.White);
            blackIndividual.Fitness += CalculatePoints(Player.Black);

            float multiplier = 2;

            switch (Result.Winner)
            {
                case Player.Black:
                    blackIndividual.Fitness *= multiplier;
                    break;
                case Player.White:
                    whiteIndividual.Fitness *= multiplier;
                    break;
            }

            stateHistory.Clear();
        }
    }
}
