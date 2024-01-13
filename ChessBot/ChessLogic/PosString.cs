using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessLogic
{
    public class PosString
    {
        private readonly StringBuilder sb = new StringBuilder();
        public PosString(Player currentPlayer, Board board) 
        {
            AddPiecePlacement(board);
            sb.Append(' ');
            AddCurentPlayer(currentPlayer);
            sb.Append(' ');
            AddCastleingRights(board);
            sb.Append(' ');
            EnPassantData(board, currentPlayer);
        }

        public override string ToString()
        {
            return sb.ToString();
        }

        private static char PieceChar(Piece piece)
        {
            char c = piece.Type switch
            {
                PieceType.Pawn => 'p',
                PieceType.Knight => 'n',
                PieceType.Rook => 'r',
                PieceType.Bishop => 'b',
                PieceType.Queen => 'q',
                PieceType.King => 'k',
                _ => ' '
            };

            if (piece.Color == Player.White)
            {
                return char.ToUpper(c);
            }

            return c;
        }

        private void AddRowData(Board board, int row)
        {
            int empty = 0;

            for (int c = 0; c < 8; c++) 
            {
                if (board[row, c] == null) 
                {
                    empty++;
                    continue;
                }

                if (empty > 0)
                {
                    sb.Append(empty);
                    empty = 0;

                }

                sb.Append(PieceChar(board[row, c]));
            }

            if (empty > 0)
            {
                sb.Append(empty);
            } 
        }

        private void AddPiecePlacement(Board board)
        {
            for (int r = 0; r < 8; r++)
            {
                if (r != 0)
                {
                    sb.Append('/');
                }

                AddRowData(board, r);
            }
        }

        private void AddCurentPlayer(Player currentPlayer)
        {
            if (currentPlayer == Player.White)
            {
                sb.Append('w');
            }
            else
            {
                sb.Append('b');
            }
        }

        private void AddCastleingRights(Board board)
        {
            bool castleWKS = board.CastleRightKS(Player.White);
            bool castleWQS = board.CastleRightQS(Player.White);
            bool castleBKS = board.CastleRightKS(Player.Black);
            bool castleBQS = board.CastleRightQS(Player.Black);

            if (!(castleWKS && castleWQS && castleBKS && castleBQS))
            {
                sb.Append('-');
                return;
            }

            if (castleWKS)
            {
                sb.Append('K');
            }
            if (castleWQS)
            {
                sb.Append('Q');
            }
            if (castleBKS) 
            {
                sb.Append('k');
            }
            if (castleBQS)
            {
                sb.Append('q');
            }
        }

        private void EnPassantData(Board board, Player currentPlayer)
        {
            if (!board.CanCaptureEnPassant(currentPlayer))
            {
                sb.Append('-');
                return;
            }

            Position pos = board.GetPawnSkipPosition(currentPlayer.Opponent());
            char file = (char)('a' + pos.Column);
            int rank = 8 - pos.Row;
            sb.Append(file);
            sb.Append(rank);
        }
    }
}
