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
        public int nextLevelInstanceId;
        public int nextEntityInstanceId;

        #endregion
        #region Properties

        public RoomState TheFirstRoom => levelStates[0]?.roomStates[levelStates[0].startPoint];

        #endregion
        #region Constructors

        public MapState(IEnumerable<LevelState> levelStates, int nextLevelInstanceId, int nextEntityInstanceId)
        {
            this.levelStates.AddRange(levelStates);
            this.nextLevelInstanceId = nextLevelInstanceId;
            this.nextEntityInstanceId = nextEntityInstanceId;
        }

        #endregion
        #region Methods

        public void Initialize()
        {
            levelStates.Clear();
            nextLevelInstanceId = 0;
        }

        public int PublishLevelInstanceId()
        {
            int currentId = nextLevelInstanceId;
            nextLevelInstanceId++;
            return currentId;
        }

        public int PublishEntityInstanceId()
        {
            int currentId = nextEntityInstanceId;
            nextEntityInstanceId++;
            return currentId;
        }

        #endregion
    }

}