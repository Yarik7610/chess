namespace ChessLogic
{
    public class Board
    {
        private readonly Piece[,] pieces = new Piece[8, 8];

        private Dictionary<Player, Position> pawnSkipPositions = new Dictionary<Player, Position>()
        { 
            {Player.White, null }, 
            {Player.Black, null},
        };

        public bool isRotated = false; 


        public Piece this[int row, int col]
        {
            get { return pieces[row, col]; }
            set { pieces[row, col] = value; }
        }
        public Piece this[Position pos]
        {
            get { return pieces[pos.Row, pos.Column]; }
            set { this[pos.Row, pos.Column] = value; }
        }
        public Position GetPawnSkipPosition(Player player) 
        {
            return pawnSkipPositions[player];
        }
        public void SetPawnSkipPosition(Player player, Position pos)
        {
            pawnSkipPositions[player] = pos;
        }

        public static Board Initial() { 
            Board board = new Board();
            board.AddStartPieces();
            return board;

        }

        private void AddStartPieces()
        {
            this[0, 0] = new Rook(Player.Black);
            this[0, 1] = new Knight(Player.Black);
            this[0, 2] = new Bishop(Player.Black);
            this[0, 3] = new Queen(Player.Black);
            this[0, 4] = new King(Player.Black);
            this[0, 5] = new Bishop(Player.Black);
            this[0, 6] = new Knight(Player.Black);
            this[0, 7] = new Rook(Player.Black);

            this[7, 0] = new Rook(Player.White);
            this[7, 1] = new Knight(Player.White);
            this[7, 2] = new Bishop(Player.White);
            this[7, 3] = new Queen(Player.White);
            this[7, 4] = new King(Player.White);
            this[7, 5] = new Bishop(Player.White);
            this[7, 6] = new Knight(Player.White);
            this[7, 7] = new Rook(Player.White);

            for (int i = 0; i < 8; i++)
            {
                this[1, i] = new Pawn(Player.Black);
                this[6, i] = new Pawn(Player.White);
            }
        }

        public Board Rotate()
        {
            Board board = new Board();
            board.isRotated = !isRotated;
            Position blackSkipPosition = GetPawnSkipPosition(Player.Black);
            Position whiteSkipPosition = GetPawnSkipPosition(Player.White);

            if (blackSkipPosition != null) board.SetPawnSkipPosition(Player.Black, new Position(Math.Abs(7 - blackSkipPosition.Row), Math.Abs(7 - blackSkipPosition.Column)));
            if (whiteSkipPosition != null) board.SetPawnSkipPosition(Player.White, new Position(Math.Abs(7 - whiteSkipPosition.Row), Math.Abs(7 - whiteSkipPosition.Column)));

            for (int r = 0; r < 8; r++)
            {
                for (int c = 0; c < 8; c++)
                {
                    board[r, c] = this[Math.Abs(7 - r), Math.Abs(7 - c)];
                    if (board[r, c] != null && board[r, c].Type == PieceType.Pawn)
                    {
                        Pawn pawn = (Pawn)board[r, c];
                        pawn.SwapForwardDirection();
                    }
                }
            }
            return board;
        }
        public static bool IsInside(Position pos) //for move generation bounds
        {
            return pos.Row >= 0 && pos.Row < 8 && pos.Column >= 0 && pos.Column < 8;
        }

        public bool IsEmpty(Position pos) //for move generation bounds
        {
            return this[pos] == null;
        }

        public IEnumerable<Position> PiecePositions() //non-empty positions
        {
            for (int r = 0; r < 8; r++)
            {
                for (int c = 0; c < 8; c++)
                {
                    Position pos = new Position(r, c);
                    if (!IsEmpty(pos)) yield return pos;
                }
            }
        }
        public IEnumerable<Position> PiecePositionsFor(Player player) //non-empty positions for Black or White
        {
            return PiecePositions().Where(pos => this[pos].Color == player);
        }

        public bool IsInCheck(Player player)
        {
            return PiecePositionsFor(player.Opponent()).Any(pos =>
            {
                Piece piece = this[pos];
                return piece.CanCaptureOpponentKing(pos, this);
            });
        }

        public Board Copy()
        {
            Board copy = new Board();
            foreach(Position pos in PiecePositions())
            {
                copy[pos] = this[pos].Copy();
            }
            return copy;
        }
        public Counting CountPieces()
        {
            Counting counting = new Counting();
            foreach (Position pos in PiecePositions())
            {
                Piece piece = this[pos];
                counting.Increment(piece.Color, piece.Type);
            }
            return counting;
        }

        public bool InsufficientMaterial()
        {
            Counting counting = CountPieces();
            return IsKingVSKing(counting) || IsKingBishopVSKing(counting) || IsKingKnightVSKing(counting) || IsKingBishopVSKingBishop(counting);
        }

        private bool IsKingVSKing(Counting counting)
        {
            return counting.TotalCount == 2;
        }
        private bool IsKingBishopVSKing(Counting counting)
        {
            return counting.TotalCount == 3 && (counting.White(PieceType.Bishop) == 1 || counting.Black(PieceType.Bishop) == 1);
        }
        private bool IsKingKnightVSKing(Counting counting)
        {
            return counting.TotalCount == 3 && (counting.White(PieceType.Knight) == 1 || counting.Black(PieceType.Knight) == 1);
        }
        private bool IsKingBishopVSKingBishop(Counting counting)
        {
            if (counting.TotalCount != 4) return false;
            if (counting.White(PieceType.Bishop) != 1 || counting.Black(PieceType.Bishop) != 1) return false;
            Position wBishopPos = FindPiece(Player.White, PieceType.Bishop);
            Position bBishopPos = FindPiece(Player.Black, PieceType.Bishop);

            return wBishopPos.SquareColor() == bBishopPos.SquareColor();
        }

        private Position FindPiece(Player color, PieceType type) 
        {
            return PiecePositionsFor(color).First(pos => this[pos].Type == type);
        }

        private bool IsUnmovedKingAndRook(Position kingPos, Position rookPos)
        {
            if (IsEmpty(kingPos) || IsEmpty(rookPos)) return false;
            Piece king = this[kingPos];
            Piece rook = this[rookPos];

            return king.Type == PieceType.King && rook.Type == PieceType.Rook && !king.HasMoved && !rook.HasMoved;
        }

        public bool CastleRightKS(Player player, bool isBoardRotated)
        {
            if (!isBoardRotated)
            {
                return player switch
                {
                    Player.White => IsUnmovedKingAndRook(new Position(7, 4), new Position(7, 7)),
                    Player.Black => IsUnmovedKingAndRook(new Position(0, 4), new Position(0, 7)),
                    _ => false
                };
            }
            else
            {
                return player switch
                {
                    Player.White => IsUnmovedKingAndRook(new Position(0, 3), new Position(0, 0)),
                    Player.Black => IsUnmovedKingAndRook(new Position(7, 3), new Position(7, 0)),
                    _ => false
                };
            }
        }

        public bool CastleRightQS(Player player, bool isBoardRotated)
        {
            if (!isBoardRotated)
            {
                return player switch
                {
                    Player.White => IsUnmovedKingAndRook(new Position(7, 4), new Position(7, 0)),
                    Player.Black => IsUnmovedKingAndRook(new Position(0, 4), new Position(0, 7)),
                    _ => false
                };
            }
            else
            {
                return player switch
                {
                    Player.White => IsUnmovedKingAndRook(new Position(0, 3), new Position(0, 7)),
                    Player.Black => IsUnmovedKingAndRook(new Position(7, 3), new Position(7, 7)),
                    _ => false
                };
            }
        }

        private bool HasPawnInPosition(Player player, Position[] pawnPositions, Position skipPos) 
        { 
            foreach (Position pos in pawnPositions.Where(IsInside)) {
                Piece piece = this[pos];
                if (piece == null || piece.Color != player || piece.Type != PieceType.Pawn) continue;
                EnPassant move = new EnPassant(pos, skipPos); //check whether enpassant leads to check
                if (move.IsLegal(this)) return true;
            }
            return false;

        }
        public bool CanCaptureEnPassant(Player player, bool isBoardRotated)
        {
            Position skipPos = GetPawnSkipPosition(player.Opponent());
            if (skipPos == null) return false;

            Position[] pawnPositions;

            if (!isBoardRotated) 
            {
                pawnPositions = player switch
                {
                    Player.White => new Position[] { skipPos + Direction.SouthWest, skipPos + Direction.SouthEast },
                    Player.Black => new Position[] { skipPos + Direction.NorthWest, skipPos + Direction.NorthEast },
                    _ => Array.Empty<Position>()

                };
            }
            else
            {
                pawnPositions = player switch
                {
                    Player.White => new Position[] { skipPos + Direction.NorthWest, skipPos + Direction.NorthEast },
                    Player.Black => new Position[] { skipPos + Direction.SouthWest, skipPos + Direction.SouthEast },
                    _ => Array.Empty<Position>()

                };
            }
            return HasPawnInPosition(player, pawnPositions, skipPos);
        }

    }
}
