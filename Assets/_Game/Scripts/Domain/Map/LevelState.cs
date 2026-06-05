using System;
using System.Collections.Generic;

namespace TDG0407.Domain.Map
{

    using Core.Grid;

    /// <summary>
    /// 레벨의 상태를 나타냅니다.
    /// </summary>
    [Serializable]
    public class LevelState
    {
        #region Fields
    
        public int? levelInstanceId = null;
        public string levelId = null;
        public readonly Dictionary<Point, RoomState> roomStates = new();

        #endregion
        #region Constructors

        public LevelState(int levelInstanceId, string levelId, Dictionary<Point, RoomState> roomStates)
        {
            this.levelInstanceId = levelInstanceId;
            this.levelId = levelId;
            this.roomStates = roomStates;
        }

        #endregion
        #region Methods

        public void Initialize()
        {
            levelInstanceId = null;
        }

        #endregion
    }

}