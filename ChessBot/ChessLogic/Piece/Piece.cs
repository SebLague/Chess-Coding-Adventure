namespace ChessLogic
{
    public abstract class Piece
    {
        public abstract PieceType Type { get; }
        public abstract Player Color { get; }
        public bool HasMoved { get; set; } = false;
        public abstract Piece Copy();

        public abstract IEnumerable<Move> GetMoves(Position From, Board board);
        protected IEnumerable<Position> MovePositionInDir(Position from, Board board, Direction dir)
        {
            for (Position pos = from + dir; Board.IsInside(pos); pos += dir)
            {
                if (board.IsEmpty(pos))
                {
                    yield return pos;
                    continue;
                }

                Piece piece = board[pos];
                if (piece.Color != Color)
                {
                    yield return pos;
                }

                yield break;
            }
        }

        protected IEnumerable<Position> MovePositionInDir(Position from, Board board, Direction[] dirs)
        {
            return dirs.SelectMany(dir => MovePositionInDir(from, board, dir));
        }
    }
}
