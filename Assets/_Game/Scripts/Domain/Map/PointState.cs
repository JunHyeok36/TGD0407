using System;
using System.Collections.Generic;
using UnityEngine;

namespace TDG0407.Domain.Map
{

    using Core.Grid;
    using Domain.Entities;

    /// <summary>
    /// 방의 각 포인트 상태를 나타냅니다.
    /// </summary>
    [Serializable]
    public class PointState
    {
        #region Fields

        [NonSerialized] public int? pointInstanceId = null;
        public Point position;
        public List<EntityState> placedEntities = new(1);

        public bool isAvailable = true;

        #endregion
        #region Constructors

        public PointState(
            int pointInstanceId,
            Point position, 
            bool isAvailable = true)
        {
            this.pointInstanceId = pointInstanceId;
            this.position = position;
            this.placedEntities = new(1);
            this.isAvailable = isAvailable;
        }

        public PointState(PointState other)
        {
            this.pointInstanceId = other.pointInstanceId;
            this.position = other.position;
            foreach (var entity in other.placedEntities)
                this.placedEntities.Add(entity.Clone());
            this.isAvailable = other.isAvailable;
        }

        #endregion
        #region Methods

        public PointState Clone()
        {
            return new PointState(this);
        }

        #endregion
    }

}