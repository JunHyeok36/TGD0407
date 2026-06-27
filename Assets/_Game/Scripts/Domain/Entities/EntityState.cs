using System;
using System.Collections.Generic;

namespace TDG0407.Domain.Entities
{
    
    using Core.Value;
    using Core.Grid;

    /// <summary>
    /// 레벨의 모든 Point 위에 존재할 수 있는 공통 Entity 상태입니다.
    /// </summary>
    [Serializable]
    public abstract class EntityState
    {
        #region Fields

        public string entityId = null;
        public int? entityInstanceId = null;
        public Point pos = Point.zero;
        public readonly BoundedValue<int> health = new(0, 50);
        public readonly BoundedValue<int> stamina = new(0, 10);
        public readonly Queue<Shield> shields = new();
        
        #endregion
        #region Constructors

        protected EntityState(
            string entityId,
            int entityInstanceId,
            Point pos,
            int health,
            int stamina,
            IEnumerable<Shield> shields = null)
        {
            this.entityId = entityId;
            this.entityInstanceId = entityInstanceId;
            this.pos = pos;
            this.health = new BoundedValue<int>(0, health);
            this.stamina = new BoundedValue<int>(0, stamina);

            if (shields != null)
            {
                foreach (Shield shield in shields)
                    this.shields.Enqueue(shield);
            }   
        }

        #endregion
        #region Methods

        public virtual void Initialize()
        {
            
        }

        #endregion
        
    }

}