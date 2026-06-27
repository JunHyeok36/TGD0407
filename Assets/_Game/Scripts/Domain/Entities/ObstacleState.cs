using System;
using System.Collections.Generic;

namespace TDG0407.Domain.Entities
{

    using Core.Grid;

    /// <summary>
    /// 상호작용 없는 일반 비생명체의 상태입니다.
    /// </summary>
    [Serializable]
    public class ObstacleState : EntityState
    {
        #region Constructors

        public ObstacleState(
            string entityId,
            int entityInstanceId,
            Point pos,
            int health,
            int stamina,
            IEnumerable<Shield> shields = null) 
            : base(entityId, entityInstanceId, pos, health, stamina, shields) { }

        #endregion
        #region Methods

        public override void Initialize()
        {
            base.Initialize();
        }

        #endregion
    }
    
}