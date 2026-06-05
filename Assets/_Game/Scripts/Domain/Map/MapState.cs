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
        public int? nextLevelInstanceId = null;
        public int? nextRoomInstanceId = null;
        public int? nextEntityInstanceId = null;

        #endregion
        #region Constructors

        public MapState(IEnumerable<LevelState> levelStates, int nextLevelInstanceId, int nextRoomInstanceId, int nextEntityInstanceId) 
        {
            this.levelStates.AddRange(levelStates);
            this.nextLevelInstanceId = nextLevelInstanceId;
            this.nextRoomInstanceId = nextRoomInstanceId;
            this.nextEntityInstanceId = nextEntityInstanceId;
        }

        #endregion
        #region Methods

        public void Initialize()
        {
            levelStates.Clear();
            nextLevelInstanceId = 0;
            nextRoomInstanceId = 0;
        }

        #endregion
    }

}