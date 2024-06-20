namespace ChessLogic
{
    public class Direction
    {
        public static Direction North = new Direction(-1, 0); //move up on 1
        public static Direction South = new Direction(1, 0); //move bottom on 1
        public readonly static Direction East = new Direction(0, 1); //move right on 1
        public readonly static Direction West = new Direction(0, -1); //move left on 1
        public readonly static Direction NorthEast = North + East;
        public readonly static Direction NorthWest = North + West;
        public readonly static Direction SouthEast = South + East;
        public readonly static Direction SouthWest = South + West;
        public int RowDelta { get; }
        public int ColumnDelta { get; }

        public Direction(int rowDelta, int columnDelta) {
            RowDelta = rowDelta;
            ColumnDelta = columnDelta;
        }
        public static Direction operator +(Direction dir1, Direction dir2)
        {
            return new Direction(dir1.RowDelta + dir2.RowDelta, dir1.ColumnDelta + dir2.ColumnDelta);
        }

        public static Direction operator *(int scalar, Direction dir)
        {
            return new Direction(scalar * dir.RowDelta, scalar * dir.ColumnDelta);
        }
    }
}
