using Accord.Math.Distances;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ChessLogic
{
    [Serializable]
    public class GameState
    {
        public event EventHandler onPlayerMoved;
        public Board Board { get; }
        public Player CurrentPlayer { get; private set; }
        public Result Result { get; private set; } = null;
        private ChessBot whiteIndividual;
        private ChessBot blackIndividual;

        public int noCaptureOrPawnMoves = 0;
        public string stateString;

        Dictionary<string, OpeningBookEntry> _openingBookWhite = new Dictionary<string, OpeningBookEntry>();
        Dictionary<string, OpeningBookEntry> _openingBookBlack = new Dictionary<string, OpeningBookEntry>();

        private readonly Dictionary<string, int> stateHistory = new Dictionary<string, int>();
        public GameState(Player player, Board board, ChessBot chessbot, ChessBot blackIndividual)
        {
            CurrentPlayer = player;
            Board = board;
            this.whiteIndividual = chessbot;
            this.blackIndividual = blackIndividual;
            stateString = new PosString(CurrentPlayer, board).ToString();
            stateHistory[stateString] = 1;
            Trace.WriteLine(stateString);
            loadOpeningBook("C:/Chess/ChessBot/ChessLogic/Json/OpeningBook.json", _openingBookWhite);
            loadOpeningBook("C:/Chess/ChessBot/ChessLogic/Json/OpeningBookBlack.json", _openingBookBlack);


        }

        public GameState(Player player, Board board)
        {
            CurrentPlayer = player;
            Board = board;
            stateString = new PosString(CurrentPlayer, board).ToString();
            stateHistory[stateString] = 1;
            Trace.WriteLine(stateString);
            loadOpeningBook("C:/Chess/ChessBot/ChessLogic/Json/OpeningBook.json", _openingBookWhite);
            loadOpeningBook("C:/Chess/ChessBot/ChessLogic/Json/OpeningBookBlack.json", _openingBookBlack);
        }

        

        public GameState(Player player, Board board, ChessBot chessBot, int blackOrWhite)
        {
            CurrentPlayer = player;
            Board = board;
            if (blackOrWhite == 0)
            {
                this.whiteIndividual = chessBot;
            }
            else
            {
                this.blackIndividual = chessBot;
            }
            loadOpeningBook("C:/Chess/ChessBot/ChessLogic/Json/OpeningBook.json", _openingBookWhite);
            loadOpeningBook("C:/Chess/ChessBot/ChessLogic/Json/OpeningBookBlack.json", _openingBookBlack);
        }
        public void loadOpeningBook(string filePath, Dictionary<string, OpeningBookEntry> openingBook)
        {
            try
            {
                // Load the opening book
                string json = File.ReadAllText(filePath);
                var options = new JsonSerializerOptions();
                List<OpeningBookEntry> openingBookEntries = JsonSerializer.Deserialize<List<OpeningBookEntry>>(json, options);

                foreach (var entry in openingBookEntries)
                {
                    openingBook.Add(entry.Position, entry);
                }
            }
            catch (JsonException ex)
            {
                // Log or print exception details
                Trace.WriteLine($"JsonException: {ex.Message}");
            }
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
            HumanMoved();
            UpdateStateString();
            CheckForGameOver();
        }

        public void HumanMoved()
        {
            onPlayerMoved?.Invoke(this, EventArgs.Empty);
        }

        public Move MakeBotMove(Player player)
        {
            // selects the bot corresponding to the current player
            var currentBot = player == Player.White ? whiteIndividual : blackIndividual;

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
            else if (legalMoveFor.Length == 0)
            {
                return null;
            }
            else
            {
                UpdateStateString();
                Dictionary<string, OpeningBookEntry> currentOpeningBook =
                    player == Player.White ? _openingBookWhite : _openingBookBlack;

                if (currentOpeningBook.TryGetValue(stateString, out OpeningBookEntry bookEntry) && bookEntry.BestMove != null)
                {
                    // Access the FromPos and ToPos properties within BestMove
                    Position fromPos = FromString(bookEntry.BestMove.FromPos);
                    Position toPos = FromString(bookEntry.BestMove.ToPos);
                    // Determine the piece type at the FromPos position
                    PieceType pieceTypeTable = Board[fromPos]?.Type ?? PieceType.Pawn;

                    // Create and return a new NormalMove with the correct PieceType
                    NormalMove move = new NormalMove(fromPos, toPos);
                    MakeMove(move);
                    return move;
                }
                else
                {
                    // If it's not, make the move calculated by the bot
                    MakeMove(legalMoveFor[(int)output]);
                    return legalMoveFor[(int)output];
                }
            }
        }



        public int CalculatePoints(Player player)
        {
            Dictionary<PieceType, int> piecePoints = new Dictionary<PieceType, int>
            {
                { PieceType.Pawn, 1 },
                { PieceType.Knight, 3 },
                { PieceType.Bishop, 3 },
                { PieceType.Rook, 5 },
                { PieceType.Queen, 9 },
                // Add other piece types if needed
            };

            Piece[] pieceOnPos = Board.PieceOnBoard(player).ToArray();
            int points = 0;

            for (int i = 0; i < pieceOnPos.Length; i++)
            {
                if (piecePoints.TryGetValue(pieceOnPos[i].Type, out int pieceValue))
                {
                    points += pieceValue;
                }
            }

            return points;
        }

        // Getting ALL moves
        public IEnumerable<Move> AllLegalMovesFor(Player player)
        {
            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount // Adjust as needed
            };

            var legalMovesBag = new ConcurrentBag<Move>();

            Parallel.ForEach(Board.PiecePositionsFor(player), parallelOptions, pos =>
            {
                Piece piece = Board[pos];
                IEnumerable<Move> moves = piece.GetMoves(pos, Board);
                foreach (var move in moves.Where(move => move.IsLegal(Board)))
                {
                    legalMovesBag.Add(move);
                }
            });

            return legalMovesBag;
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
            int fullMoves = noCaptureOrPawnMoves;
            return fullMoves == 100;
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

            Trace.WriteLine(stateString);
        }
        private bool Repetition()
        {
            return stateHistory[stateString] >= 3;
        }

        private float GetPieceValue(char piece)
        {
            switch (Char.ToUpper(piece))
            {
                case 'P': return 1f;
                case 'R': return 2f;
                case 'N': return 3f;
                case 'B': return 4f;
                case 'Q': return 5f;
                case 'K': return 6f;
                default: throw new ArgumentException("Invalid piece character");
            }
        }

        public float[] FenToArray(string fen)
        {

            // Split the FEN string into its constituent parts
            string[] parts = fen.Split(' ')[0].Split('/');

            // Initialize the board array
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
                            if (currentSquare < board.Length)
                            {
                                board[(i * 8) + currentSquare] = 0;
                                currentSquare++;
                            }
                        }
                    }
                    else
                    {
                        if (currentSquare < board.Length)
                        {
                            float pieceValue = GetPieceValue(c);
                            board[(i * 8) + currentSquare] = pieceValue;
                            currentSquare++;
                        }
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

            float multiplier = 100;

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



        public class OpeningBookEntry
        {
            public string Position { get; set; }
            public MoveInfo BestMove { get; set; }
        }

        public class MoveInfo
        {
            public string FromPos { get; set; }
            public string ToPos { get; set; }

        }
        public Position FromString(string positionString)
        {
            if (positionString.Length != 2)
            {
                throw new ArgumentException("Invalid position string length");
            }

            char columnChar = positionString[0];
            int row = '8' - positionString[1];  // Adjust the row based on the mapping 1=7, 2=6, ..., 8=0
            int column = columnChar - 'a';

            return new Position(row, column);
        }

        public NormalMove ConvertMoveStringToNormalMove(JObject moveObject)
        {
            // Read the positions from the JSON
            string fromPosStr = moveObject["BestMove"]["FromPos"].ToString();
            string toPosStr = moveObject["BestMove"]["ToPos"].ToString();

            // Parse the position strings into integers
            int fromRow = int.Parse(fromPosStr[1].ToString());
            int fromColumn = Char.ToLower(fromPosStr[0]) - 'a';
            int toRow = int.Parse(toPosStr[1].ToString());
            int toColumn = Char.ToLower(toPosStr[0]) - 'a';

            // Convert the position strings to Position objects
            Position fromPos = new Position(fromRow, fromColumn);
            Position toPos = new Position(toRow, toColumn);

            // Create and return a new NormalMove
            return new NormalMove(fromPos, toPos);
        }
    }
}
