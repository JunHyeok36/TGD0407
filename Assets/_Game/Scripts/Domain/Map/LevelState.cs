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
    
        public string levelId = null;
        public int levelInstanceId = -1;
        public readonly Dictionary<Point, RoomState> roomStates = new();

        #endregion
        #region Constructors

        public LevelState() { }

        #endregion
        #region Methods

        public void Initialize()
        {
        }

        #endregion
    }

}