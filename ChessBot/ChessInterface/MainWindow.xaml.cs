using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using ChessLogic;

namespace ChessInterface
{
    
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Image[,] pieceImages = new Image[8, 8];
        private readonly Rectangle[,] highlights = new Rectangle[8, 8];
        private readonly Dictionary<Position, Move> moveCache = new Dictionary<Position, Move>();
        
        
        private ChessLogic.GameState gameState;
        private Position selectedPos = null;
        private EvolutionarySystem evo;
        private FenSave fenSave = new FenSave();
        private int blackOrWhite;
        public MainWindow()
        {
            InitializeComponent();
            InitializeBoard();
            evo = new(100, 100);
            HumanVSHuman();
        }


        private void InitializeBoard()
        {
            for (int r = 0; r < 8; r++)
            {
                for(int c = 0; c < 8; c++)
                {
                    Image image = new Image();
                    pieceImages[r, c] = image;
                    PieceGrid.Children.Add(image);

                    Rectangle highlight = new Rectangle();
                    highlights[r, c] = highlight;
                    BoardGrid.Children.OfType<UniformGrid>().FirstOrDefault(ug => ug.Name == "HighlightGrid").Children.Add(highlight);
                }
            }
        }

        private void DrawBoard(Board board)
        {
            for (int r = 0; r < 8; r++)
            {
                for (int c = 0; c < 8; c++)
                {
                    Piece piece = board[r, c];
                    if (piece != null)
                    {
                        pieceImages[r, c].Source = Images.GetImage(piece);
                    }
                    else
                    {
                        pieceImages[r, c].Source = null;
                    }
                }
            }
        }
        private void CachMoves(IEnumerable<Move> moves)
        {
            moveCache.Clear();

            foreach (Move move in moves)
            {
                moveCache[move.ToPos] = move;
            }
        }

        private void ShowHighlights()
        {
            Color color = Color.FromArgb(150, 137, 207, 240);

            foreach (Position to in moveCache.Keys)
            {
                highlights[to.Row, to.Column].Fill = new SolidColorBrush(color);
            }
        }

        private void HideHighlights()
        {
            foreach (Position to in moveCache.Keys)
            {
                highlights[to.Row, to.Column].Fill = Brushes.Transparent;
            }
        }
        private Position ToSquarePosition(Point point)
        {
            double squareSize = BoardGrid.ActualWidth / 8;
            int row = (int)(point.Y / squareSize);
            int col = (int)(point.X / squareSize);
            return new Position(row, col);
        }

        private void OnFromPositionSelected(Position pos)
        {
            IEnumerable<Move> moves = gameState.LegalMoveForPiece(pos);

            if (moves.Any())
            {
                selectedPos = pos;
                CachMoves(moves);
                ShowHighlights();
            }
        }
        private void OnToPositionSelected(Position pos)
        {
            selectedPos = null;
            HideHighlights();

            if (moveCache.TryGetValue(pos, out Move move))
            {
                if (move.Type == MoveType.PawnPromotion)
                {
                    HandlePromotion(move.FromPos, move.ToPos);
                }
                else
                {
                    HandleMove(move);
                }

            }
        }

        private void HandlePromotion(Position from, Position to)
        {
            pieceImages[to.Row, to.Column].Source = Images.GetImage(gameState.CurrentPlayer, PieceType.Pawn);
            pieceImages[from.Row, from.Column].Source = null;

            PromotionMenu proMenu = new PromotionMenu(gameState.CurrentPlayer);
            MenuContainer.Content = proMenu;

            proMenu.PieceSelected += type =>
            {
                MenuContainer.Content = null;
                Move promMove = new PawnPromotion(from, to, type);
                HandleMove(promMove);

            };
        }
        private void HandleMove(Move move)
        {
            gameState.MakeMove(move);
            DrawBoard(gameState.Board);

            if (gameState.IsGameOver())
            {
                Restart();
            }
        }
        public void BoardGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point point = e.GetPosition(BoardGrid);
            Position pos = ToSquarePosition(point);

            if (selectedPos == null)
            {
                OnFromPositionSelected(pos);
            }
            else
            {
                OnToPositionSelected(pos);
            }
        }

        public bool Restart()
        {
            HideHighlights();
            moveCache.Clear();
            MenuContainer.Content = null;
            // try getting new contenders
            ChessBot[] i = evo.GetContenders();
            if (i == null)
            {
                fenSave.AddFen("------END OF GENERATION-----");
                if (evo.EndGeneration())
                {
                    HumanVSHuman();
                    return false;
                }
                else
                {
                    BotVsBot();
                    return false;
                }
            }

            gameState = new ChessLogic.GameState(Player.White, Board.Initial(), i[0], i[1]);
            DrawBoard(gameState.Board);
            return true;
        }

        public void RestartHuman()
        {
            HideHighlights();
            moveCache.Clear();

            gameState = new ChessLogic.GameState(Player.White, Board.Initial());
            DrawBoard(gameState.Board);
        }
        public void RestartHumanVsBot()
        {
            HideHighlights();
            moveCache.Clear();
            ChessBot bot = new ChessBot();
            try
            {
                bot = FenSave.LoadFromFile<ChessBot>("C:/Chess/ChessBot/ChessLogic/Json/Parent1.json");
            }
            catch { }
            gameState = new ChessLogic.GameState(Player.White, Board.Initial(), bot, blackOrWhite);
            DrawBoard(gameState.Board);
            gameState.stateString = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq -";

        }
        private void o(object sender, KeyEventArgs e)
        {
            GameModeSelectScreen gameModeMenu = new GameModeSelectScreen(this);
            MenuContainer.Content = gameModeMenu;
        }

        public void HumanVSHuman()
        {
            RestartHuman();
            MenuContainer.Content = null;
        }


        public async void BotVsBot()
        {
            if (!Restart()) return;

            var isWhitePlaying = true;

            await Task.Run(() =>
            {
                while (!gameState.IsGameOver())
                {
                    gameState.MakeBotMove(isWhitePlaying ? Player.White : Player.Black);

                    // Update UI on the main thread
                    Dispatcher.Invoke(() =>
                    {
                        DrawBoard(gameState.Board);
                    });

                    isWhitePlaying = !isWhitePlaying;
                }

                fenSave.AddFen(gameState.stateString);
                gameState.AssignFitness();
            });

            // Restart the game or perform other actions after completion
            BotVsBot();
        }
        

        public void HumanVsBot()
        {
            Random rng = new Random(); 
            blackOrWhite = rng.Next(0, 2);
            RestartHumanVsBot();
            MenuContainer.Content = null;
            gameState.onPlayerMoved += PlayerMoved;
            if (blackOrWhite == 0)
            {
                gameState.MakeBotMove(blackOrWhite == 0 ? Player.White : Player.Black);
                DrawBoard(gameState.Board);
            }
        }

        private void PlayerMoved(object _, EventArgs e)
        {
            if (gameState.IsGameOver())
            {
                gameState.onPlayerMoved -= PlayerMoved;
                HumanVSHuman();
                return;
            }

            if (gameState.CurrentPlayer == (blackOrWhite == 0 ? Player.White : Player.Black))
            {
                gameState.MakeBotMove(blackOrWhite == 0 ? Player.White : Player.Black);
                DrawBoard(gameState.Board);
            }
        }

    }

    [Serializable]
    public class FenSave
    {
        public List<string> Fens { get; set; } = new();

        public FenSave()
        {
            try
            {
                Fens.AddRange(LoadFromFile<List<string>>("C:/Chess/ChessBot/ChessLogic/Json/Fen.json"));
            }
            catch
            {
                Trace.WriteLine("Cannot load fen from file");
                // ignored
            }
        }

        public void AddFen(string fen)
        {
            Fens.Add(fen);
            Trace.WriteLine(fen);
            SaveToFile();
        }

        private void SaveToFile()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            var json = JsonSerializer.Serialize(this, options);
            File.WriteAllText("C:/Chess/ChessBot/ChessLogic/Json/Fen.json", json);
        }

        public static T LoadFromFile<T>(string path)
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<T>(json);
        }
    }
}