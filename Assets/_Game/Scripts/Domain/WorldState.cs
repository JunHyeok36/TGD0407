using System;

namespace TDG0407.Domain
{

    using Domain.Map;

    /// <summary>
    /// 세계의 상태를 나타냅니다.
    /// </summary>
    [Serializable]
    public class WorldState 
    {
        #region Fields

        public string seed = null;
        public readonly MapState mapState;

        #endregion
        #region Constructors

        public WorldState(MapState mapState, string seed = null) 
        {
            this.mapState = mapState;
            this.seed = seed ?? Guid.NewGuid().ToString();
        }

        #endregion
    }

}