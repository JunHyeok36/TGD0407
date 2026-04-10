using System;
using System.Collections.Generic;

namespace TDG0407.Domain.Entities
{
    
    using Core.Value;
    using Core.Grid;
    using Domain.Exceptions;
    using Domain.Interfaces;

    /// <summary>
    /// Room 안의 모든 Point 위에 존재할 수 있는 공통 Entity 데이터입니다.
    /// </summary>
    [Serializable]
    public abstract class EntityData : IDataValidatable
    {
        #region Fields

        public string id = null;
        public Point pos = Point.zero;
        public readonly BoundedValue<int> health = new(0, 50);
        public readonly BoundedValue<int> stamina = new(0, 10);
        public readonly Queue<Shield> shields = new();
        
        #endregion
        #region Constructors

        protected EntityData() { }

        #endregion
        #region Methods

        public virtual void Initialize()
        {
            
        }

        public virtual bool UsesCards { get => false; }

        public virtual void ValidateData()
        {
            if(string.IsNullOrEmpty(id)) throw new DataValidityViolationException("Invalid value assigned to Entity ID.");
            if(health.Max < 0) throw new DataValidityViolationException("Invalid value assigned to Entity Max Health.");
            if(health.Min != 0) throw new DataValidityViolationException("Invalid value assigned to Entity Min Health.");
            if(stamina.Max < 0) throw new DataValidityViolationException("Invalid value assigned to Entity Max Stamina.");
            if(stamina.Min != 0) throw new DataValidityViolationException("Invalid value assigned to Entity Min Stamina.");
        }

        #endregion
        
    }

}