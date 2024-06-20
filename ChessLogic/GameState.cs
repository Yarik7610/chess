namespace ChessLogic
{
    public class GameState
    {
        public Board Board { get; set; }
        public Player CurrentPlayer { get; private set; }
        public Result Result { get; private set; } = null;

        public int noCaptureOrPawnMoves = 0;
        private string stateString;
        public Dictionary<string, int> stateHistory = new Dictionary<string, int>();


        public bool withTime = false;
        public bool blackMoved  = false;
        public bool whiteMoved  = false;
        public int whiteLeftTime = 5;
        public int blackLeftTime = 5;
        
        public GameState(Player player, Board board)
        {
            Board = board;
            CurrentPlayer = player;
            stateString = new StateString(CurrentPlayer, board).ToString();
            stateHistory[stateString] = 1;
        }

        public Dictionary<string, int> ConvertStateHistory()
        {
            Dictionary<string, int> newStateHistory = new Dictionary<string, int>();
            foreach (var stateString in stateHistory.Keys) {
                string rotatedString = StateString.RotateStateString(stateString);
                newStateHistory[rotatedString] = stateHistory[stateString];
            }
            return newStateHistory;
        }

        public IEnumerable<Move> LegalMovesForPiece(Position pos)
        {
            //Board[pos].Color != CurrentPlayer if we take a figure that doesn't have its turn
            if (Board.IsEmpty(pos) || Board[pos].Color != CurrentPlayer) return Enumerable.Empty<Move>();
            Piece piece = Board[pos];
            IEnumerable<Move> moveCandidates =  piece.GetMoves(pos, Board);
            return moveCandidates.Where(move => move.IsLegal(Board));
        }

        public bool MakeMove(Move move) 
        {
            if (CurrentPlayer == Player.White && !whiteMoved) whiteMoved = true;
            else if (CurrentPlayer == Player.Black && !blackMoved) blackMoved = true;

            Board.SetPawnSkipPosition(CurrentPlayer, null);
            bool captureOrPawnMove = move.Execute(Board);
            if (captureOrPawnMove)
            {
                noCaptureOrPawnMoves = 0;
                stateHistory.Clear();
            }
            else noCaptureOrPawnMoves++;

            CurrentPlayer = CurrentPlayer.Opponent();
            UpdateStateString();
            CheckForGameOver();
            if (move.Type == MoveType.Normal)
            {
                NormalMove normalMove = (NormalMove)move;
                return normalMove.didCaptureFigure;
            }
            else return false; //dummy
        }
        public IEnumerable<Move> AllLegalMovesFor(Player player)
        {
            IEnumerable<Move> moveCandidates = Board.PiecePositionsFor(player).SelectMany(pos =>
            {
                Piece piece = Board[pos];
                return piece.GetMoves(pos, Board);
            });
            return moveCandidates.Where(move => move.IsLegal(Board));
        }
        public void CheckForGameOver()
        {

            if (!AllLegalMovesFor(CurrentPlayer).Any())
            {
                if (Board.IsInCheck(CurrentPlayer))
                {
                    Result = Result.Win(CurrentPlayer.Opponent(), EndReason.Checkmate);
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
            else if (FiftyMoveRool())
            {
                Result = Result.Draw(EndReason.FiftyMoveRule);
            }
            else if (whiteLeftTime == 0)
            {
                Result = Result.Win(Player.Black, EndReason.Time);
            }
            else if (blackLeftTime == 0)
            {
                Result = Result.Win(Player.White, EndReason.Time);
            }
            else if (ThreefoldRepetition())
            {
                Result = Result.Draw(EndReason.ThreefoldRepetition);
            }
        }

        public bool IsGameOver()
        {
            return Result != null;
        }

        private bool FiftyMoveRool()
        {
            int fullMoves = noCaptureOrPawnMoves / 2;
            return fullMoves == 50;
        }
        private void UpdateStateString()
        {
            stateString = new StateString(CurrentPlayer, Board).ToString();
            if (!stateHistory.ContainsKey(stateString)) stateHistory[stateString] = 1;
            else stateHistory[stateString]++;
        }
        private bool ThreefoldRepetition()
        {
            return stateHistory[stateString] == 3;
        }
    }
}
