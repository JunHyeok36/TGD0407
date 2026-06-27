namespace TDG0407.Core.Grid
{

    /// <summary>
    /// 방향을 나타냅니다.
    /// </summary>
    public enum Direction : sbyte
    {
        NULL = -1,
        Up = 0,
        UpRight = 1,
        Right = 2,
        DownRight = 3,
        Down = 4,
        DownLeft = 5,
        Left = 6,
        UpLeft = 7,
    }

    public static class DirectionExtensions
    {
        
        public static Point ToPoint(this Direction direction)
        {
            return direction switch
            {
                Direction.Up => new Point(0, 1),
                Direction.UpRight => new Point(1, 1),
                Direction.Right => new Point(1, 0),
                Direction.DownRight => new Point(1, -1),
                Direction.Down => new Point(0, -1),
                Direction.DownLeft => new Point(-1, -1),
                Direction.Left => new Point(-1, 0),
                Direction.UpLeft => new Point(-1, 1),
                _ => new Point(0, 0),
            };
        }

    }

}