using System;

namespace TDG0407.Domain.Map
{

    using Core.Grid;
    using Domain.Entities;

    /// <summary>
    /// 세계의 상태를 나타냅니다.
    /// </summary>
    [Serializable]
    public class WorldState 
    {
        #region Fields

        public int worldSeed;
        public int proceduralSeed;
        public readonly MapState mapState;

        public readonly LifeState lifeState;
        public readonly RoomPoint playerPosition;


        #endregion
        #region Constructors

        public WorldState(int worldSeed, int proceduralSeed, MapState mapState, LifeState lifeState, RoomPoint playerPosition) 
        {
            this.worldSeed = worldSeed;
            this.proceduralSeed = proceduralSeed;
            this.mapState = mapState;
            this.lifeState = lifeState; 
            this.playerPosition = playerPosition;
        }

        #endregion
    }

}