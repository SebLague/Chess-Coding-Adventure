namespace ChessLogic
{
    public enum Player
    {
        None,
        White,
        Black
    }

    public static class PlayerExtention
    {
        public static Player Opponent(this Player player)
        {
            return player switch
            {
                Player.White => Player.Black,
                Player.Black => Player.White,
                _ => Player.None,
            };
        }
    }
}
