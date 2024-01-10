using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ChessLogic;

namespace ChessInterface
{
    public static class Images
    {

        private static readonly Dictionary<PieceType, ImageSource> blackSources = new()
        {
            {PieceType.Pawn, LoadImage("Assets/b_pawn_png_shadow_128px.png") },
            {PieceType.Rook, LoadImage("Assets/b_rook_png_shadow_128px.png")},
            {PieceType.Bishop, LoadImage("Assets/b_bishop_png_shadow_128px.png")},
            {PieceType.Knight, LoadImage("Assets/b_knight_png_shadow_128px.png")},
            {PieceType.Queen, LoadImage("Assets/b_queen_png_shadow_128px.png")},
            {PieceType.King, LoadImage("Assets/b_king_png_shadow_128px.png")}
        };
        private static readonly Dictionary<PieceType, ImageSource> whiteSources = new()
        {
            {PieceType.Pawn, LoadImage("Assets/w_pawn_png_shadow_128px.png") },
            {PieceType.Rook, LoadImage("Assets/w_rook_png_shadow_128px.png")},
            {PieceType.Bishop, LoadImage("Assets/w_bishop_png_shadow_128px.png")},
            {PieceType.Knight, LoadImage("Assets/w_knight_png_shadow_128px.png")},
            {PieceType.Queen, LoadImage("Assets/w_queen_png_shadow_128px.png")},
            {PieceType.King, LoadImage("Assets/w_king_png_shadow_128px.png")}
        };

        private static ImageSource LoadImage(string filepath)
        {
            try
            {
                return new BitmapImage(new Uri(filepath, UriKind.Relative));
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Failed to load image at '{filepath}': {ex.Message}");
                return null;
            }
        }

        public static ImageSource GetImage(Player color, PieceType type)
        {
            return color switch
            {
                Player.White => whiteSources[type],
                Player.Black => blackSources[type],
                _ => null
            };
        }
        public static ImageSource GetImage(Piece piece)
        {
            if (piece == null)
            {
                return null;
            }
            return GetImage(piece.Color, piece.Type);
        }
    }
}
