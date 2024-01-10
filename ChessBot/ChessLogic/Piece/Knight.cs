namespace ChessLogic
{
    public class Knight : Piece
    {
        public override PieceType Type => PieceType.Knight;
        public override Player Color { get; }

        public Knight(Player color)
        {
            Color = color;
        }

        public override Piece Copy()
        {
            Knight copy = new Knight(Color);
            copy.HasMoved = HasMoved;
            return copy;
        }

        private static IEnumerable<Position> PotentialToPosition(Position from)
        {
            foreach (Direction vDir in new Direction[] {Direction.North, Direction.South })
            {
                foreach (Direction hDir in new Direction[] {Direction.West, Direction.East })
                {
                    yield return from + 2 * vDir + hDir;
                    yield return from + 2 * hDir + vDir;
                }
            }
        }

        private IEnumerable<Position> MovePostions(Position from, Board board)
        {
            return PotentialToPosition(from).Where(pos => Board.IsInside(pos) 
                && (board.IsEmpty(pos) || board[pos].Color != Color));

        }

        public override IEnumerable<Move> GetMoves(Position from, Board board)
        {
            return MovePostions(from, board).Select(to => new NormalMove(from, to));
        }
    }
}
