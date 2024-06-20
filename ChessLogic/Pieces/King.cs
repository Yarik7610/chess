namespace ChessLogic
{
    public class King : Piece
    {
        public override PieceType Type => PieceType.King;
        public override Player Color { get; }

        private static readonly Direction[] dirs = new Direction[] 
        {
            Direction.North,
            Direction.South,
            Direction.West,
            Direction.East,
            Direction.SouthEast,
            Direction.SouthWest,
            Direction.NorthEast,
            Direction.NorthWest,
        };
        public King(Player color)
        {
            Color = color;
        }

        private static bool AllEmpty(IEnumerable<Position> positions, Board board)
        {
            return positions.All(pos => board.IsEmpty(pos));
        }
        private static bool IsUnmovedRook(Position pos, Board board)
        {
            if (board.IsEmpty(pos)) return false;
            Piece piece = board[pos];
            return piece.Type == PieceType.Rook && !piece.HasMoved;
        }
        private bool CanCastleKingSide(Position from, Board board)
        {
            if (HasMoved) return false;
            if (!board.isRotated)
            {
                Position rookPos = new Position(from.Row, 7);
                Position[] betweenPositions = new Position[] { new(from.Row, 5), new(from.Row, 6) };
                return IsUnmovedRook(rookPos, board) && AllEmpty(betweenPositions, board);
            }
            else
            {
                Position rookPos = new Position(from.Row, 0);
                Position[] betweenPositions = new Position[] { new(from.Row, 1), new(from.Row, 2)};
                return IsUnmovedRook(rookPos, board) && AllEmpty(betweenPositions, board);
            }
          
        }
        private bool CanCastleQueenSide(Position from, Board board)
        {
            if (HasMoved) return false;
            if (!board.isRotated)
            {
                Position rookPos = new Position(from.Row, 0);
                Position[] betweenPositions = new Position[] { new(from.Row, 1), new(from.Row, 2), new(from.Row, 3) };
                return IsUnmovedRook(rookPos, board) && AllEmpty(betweenPositions, board);
            }
            else
            {
                Position rookPos = new Position(from.Row, 7);
                Position[] betweenPositions = new Position[] { new(from.Row, 4), new(from.Row, 5), new(from.Row, 6) };
                return IsUnmovedRook(rookPos, board) && AllEmpty(betweenPositions, board);
            }
        }
        public override Piece Copy()
        {
            King copy = new King(Color);
            copy.HasMoved = HasMoved;
            return copy;
        }
        private IEnumerable<Position> MovePositions(Position from, Board board) 
        {
            foreach (Direction dir in dirs) 
            {
                Position to = from + dir;
                if (!Board.IsInside(to)) continue;
                if (board.IsEmpty(to) || board[to].Color != Color)
                {
                    yield return to;
                }
            }
        }

        public override IEnumerable<Move> GetMoves(Position from, Board board)
        {
            foreach (Position to in MovePositions(from, board)) yield return new NormalMove(from, to);
            if (CanCastleKingSide(from, board)) yield return new Castle(MoveType.CastleKS, from, board);
            if (CanCastleQueenSide(from, board)) yield return new Castle(MoveType.CastleQS, from, board);
        }

        public override bool CanCaptureOpponentKing(Position from, Board board)
        {
            //without castling cases???
            return MovePositions(from, board).Any(to =>
            {
                Piece piece = board[to];
                return piece != null && piece.Type == PieceType.King;
            });
        }
      
    }
}
