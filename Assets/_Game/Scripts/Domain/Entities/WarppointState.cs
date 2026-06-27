using System;
using System.Collections.Generic;

namespace TDG0407.Domain.Entities
{

    using Core.Grid;

    /// <summary>
    /// 워프 포인트의 상태입니다.
    /// </summary>
    [Serializable]
    public class WarpPointState : ObstacleState
    {
        #region Fields

        public readonly LevelPoint target = new();

        #endregion
        #region Constructors

        public WarpPointState(
            string entityId,
            int entityInstanceId,
            Point pos,
            LevelPoint target,
            int health = 1,
            int stamina = 1,
            IEnumerable<Shield> shields = null)
            : base(entityId, entityInstanceId, pos, health, stamina, shields)
        {
            this.target = target;
        }

        #endregion
        #region Methods

        public override void Initialize()
        {
            base.Initialize();
        }

        #endregion
    }
    
}