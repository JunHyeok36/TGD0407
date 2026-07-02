using System;
using System.Collections.Generic;

namespace TDG0407.Domain.Entities
{

    using Core.Grid;
    using Core.Value;

    /// <summary>
    /// 상호작용 없는 일반 비생명체의 상태입니다.
    /// </summary>
    [Serializable]
    public class ObstacleState : EntityState
    {
        #region Constructors

        public ObstacleState() : base() {}
        public ObstacleState(
            int entityInstanceId,
            string entityId,
            Point position,
            BoundedValue<int> health,
            BoundedValue<int> stamina,
            IEnumerable<Shield> shields = null) 
            : base(entityInstanceId, entityId, position, health, stamina, shields) { }

        public ObstacleState(ObstacleState other) 
            : base(
                other.entityInstanceId ?? throw new InvalidOperationException("EntityInstanceId is null."), 
                other.entityId, 
                other.position, 
                other.health, 
                other.stamina, 
                other.shields) { }

        #endregion
        #region Methods

        public override void Initialize()
        {
            base.Initialize();
        }

        public override EntityState Clone()
        {
            return new ObstacleState(this);
        }

        #endregion
    }
    
}