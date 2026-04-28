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
        
        public readonly List<LevelState> levelStates = new();

        #endregion
        #region Constructors

        public MapState(IEnumerable<LevelState> levelStates) 
        {
            this.levelStates.AddRange(levelStates);
        }

        #endregion
        #region Methods

        public void Initialize()
        {
            levelStates.Clear();
        }

        #endregion
    }

}