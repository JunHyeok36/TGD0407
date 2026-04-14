using System;
using System.Collections.Generic;

namespace TDG0407.Domain.Map
{

    /// <summary>
    /// 맵의 상태를 나타냅니다.
    /// </summary>
    [Serializable]
    public class MapState 
    {
        #region Fields
        
        public int randomSeed = -1;
        public readonly List<LevelState> levelStates = new();

        #endregion
        #region Methods

        public void Initialize()
        {
            levelStates.Clear();
        }

        #endregion
    }

}