namespace ChessLogic
{
    public abstract class Piece
    {
        public abstract PieceType Type { get; }
        public abstract Player Color { get; }
        public bool HasMoved { get; set; } = false; //for king castling
        public abstract Piece Copy();
        public abstract IEnumerable<Move> GetMoves(Position from, Board board); //list of moves that can be done from current position

        protected IEnumerable<Position> MovePositionsInDir(Position from, Board board, Direction dir) //all reachable positions in one direction
        { 
            for (Position pos = from + dir; Board.IsInside(pos); pos += dir) 
            {
                if (board.IsEmpty(pos))
                {
                    yield return pos; 
                    continue;
                }
                Piece piece = board[pos];
                if (piece.Color != Color) yield return pos; //continue enumerable loop  
                yield break; //exit enumerable loop
            }
        }
        protected IEnumerable<Position> MovePositionsInDirs(Position from, Board board, Direction[] dirs) //all reachable positions many directions
        {
           return dirs.SelectMany(dir => MovePositionsInDir(from, board, dir)); //flattens the matrix to array also
        }

        public virtual bool CanCaptureOpponentKing(Position from, Board board)
        {
            return GetMoves(from, board).Any(move =>
            {
                Piece piece = board[move.ToPos];
                return piece != null && piece.Type == PieceType.King;
            });
        }
    }
}
