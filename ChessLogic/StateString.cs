using System.Text;

namespace ChessLogic
{
    public class StateString
    {
        private readonly StringBuilder sb = new StringBuilder();
        public StateString(Player currentPlayer, Board board) 
        {
            AddPiecePlacement(board);
            sb.Append(' ');
            AddCurrentPlayer(currentPlayer);
            sb.Append(' ');
            AddCastlingRights(board);
            sb.Append(' ');
            AddEnPassant(board, currentPlayer);
        }

        public static string RotateStateString(string stateString)
        {

            string playerAndCastling = stateString.Substring(stateString.IndexOf(' ') + 1, stateString.LastIndexOf(' ') - (stateString.IndexOf(' ') + 1));
            string fullReversedString = new string(stateString.Reverse().ToArray());

            int firstSpace = fullReversedString.IndexOf(' ');
            int lastSpace = fullReversedString.LastIndexOf(' ');

            string reversedPiecePlacementString = fullReversedString.Substring(lastSpace + 1, fullReversedString.Length - (lastSpace + 1));
            string reversedEnPassantSquare = new string(fullReversedString.Substring(0, firstSpace).Reverse().ToArray());
            string convertedEnPassantSquare = reversedEnPassantSquare;
            if (reversedEnPassantSquare != "-") convertedEnPassantSquare = Math.Abs(reversedEnPassantSquare[0] - '0' - 8) + "" + Math.Abs(reversedEnPassantSquare[1] - '0' - 8);

            return reversedPiecePlacementString + ' ' + playerAndCastling + ' ' + convertedEnPassantSquare;
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
                _ => ' ',
            };
            //UPPERCASE - white, lowercase - black
            if (piece.Color == Player.White) return char.ToUpper(c);
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
            if (empty > 0) sb.Append(empty);
        }

        private void AddPiecePlacement(Board board)
        {
            for (int r = 0; r < 8; r++)
            {
                if (r != 0) sb.Append('|');
                AddRowData(board, r);
            }
        }
        private void AddCurrentPlayer(Player currentPlayer)
        {
            if (currentPlayer == Player.White) sb.Append('w');
            else sb.Append('b');
        }
        private void AddCastlingRights(Board board)
        {
            bool castleWKS = board.CastleRightKS(Player.White, board.isRotated);
            bool castleWQS = board.CastleRightQS(Player.White, board.isRotated);
            bool castleBKS = board.CastleRightKS(Player.Black, board.isRotated);
            bool castleBQS = board.CastleRightQS(Player.Black, board.isRotated);

            if (!(castleWKS || castleWQS || castleBKS || castleBQS)) 
            {
                sb.Append('-');
                return;
            }
            if (castleWKS) sb.Append('K');
            if (castleWQS) sb.Append('Q');
            if (castleBKS) sb.Append('k');
            if (castleBQS) sb.Append('q');
        }
        private void AddEnPassant(Board board, Player currentPlayer) 
        {
            if (!board.CanCaptureEnPassant(currentPlayer, board.isRotated))
            {
                sb.Append('-');
                return;
            }
            Position pos = board.GetPawnSkipPosition(currentPlayer.Opponent());
            //Pos = (rank, file), rank == row but from 1 to 8 (starting from the old row 7), file == column but from A to H (starting from old column 0)
            /* char file = (char)('a' + pos.Column);*/

            //but to make rotate workable i've changed file from 1 to 8 (starting from the old column 0) 
            int file = 8 - pos.Column;
            int rank = 8 - pos.Row;
            sb.Append(file);
            sb.Append(rank);
        }
    }
}
