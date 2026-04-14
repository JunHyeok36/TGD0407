using System;

namespace TDG0407.Core.Grid
{
    
    /// <summary>
    /// 특정 Room의 특정 위치를 나타냅니다.
    /// </summary>
    [Serializable]
    public class RoomPoint
    {
        #region Fields

        public string roomId = null;
        public Point position = Point.zero;

        #endregion
        #region Constructors

        public RoomPoint() { }
        public RoomPoint(string roomId, Point position)
        {
            this.roomId = roomId;
            this.position = position;
        }

        #endregion
        #region Operators
        
        public static implicit operator Point(RoomPoint roomPoint) => roomPoint.position;

        #endregion
    }

}