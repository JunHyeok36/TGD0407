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
    
        [NonSerialized] public int? roomInstanceId = null;
        public Point position = Point.zero;

        #endregion
        #region Constructors

        public RoomPoint() { }
        public RoomPoint(int? roomInstanceId, Point position)
        {
            this.roomInstanceId = roomInstanceId;
            this.position = position;
        }

        #endregion
        #region Operators
        
        public static implicit operator Point(RoomPoint roomPoint) => roomPoint.position;

        #endregion
    }

}