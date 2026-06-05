using System;

namespace TDG0407.Core.Grid
{
    
    /// <summary>
    /// 특정 Level의 특정 Room 내의 특정 위치를 나타냅니다.
    /// </summary>
    [Serializable]
    public class LevelPoint : RoomPoint
    {
        #region Fields

        public int? levelInstanceId = null;

        #endregion
        #region Constructors

        public LevelPoint() : base() { }
        public LevelPoint(int? levelInstanceId, int? roomInstanceId, Point position) : base(roomInstanceId, position)
        {
            this.levelInstanceId = levelInstanceId;
        }

        #endregion
    }

}