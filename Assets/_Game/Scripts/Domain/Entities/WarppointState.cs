using System;
using System.Collections.Generic;

namespace TDG0407.Domain.Entities
{

    using Core.Grid;
    using Core.Value;

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

        public WarpPointState() : base() {}
        public WarpPointState(
            int entityInstanceId,
            string entityId,
            Point position,
            LevelPoint target,
            BoundedValue<int> health,
            BoundedValue<int> stamina,
            IEnumerable<Shield> shields = null)
            : base(entityInstanceId, entityId, position, health, stamina, shields)
        {
            this.target = target;
        }

        public WarpPointState(WarpPointState other)
            : base(
                other.entityInstanceId ?? throw new InvalidOperationException("EntityInstanceId is null."),
                other.entityId,
                other.position,
                other.health,
                other.stamina,
                other.shields)
        {
            this.target = other.target.Clone();
        }

        #endregion
        #region Methods

        public override void Initialize()
        {
            base.Initialize();
        }

        public override EntityState Clone()
        {
            return new WarpPointState(this);
        }

        #endregion
    }
    
}