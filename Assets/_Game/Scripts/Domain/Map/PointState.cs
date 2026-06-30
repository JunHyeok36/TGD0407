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

        public int? pointInstanceId = null;
        public Point position;
        [SerializeReference] public List<EntityState> placedEntities = new(1);

        public bool isAvailable = true;

        #endregion
        #region Properties

        public List<EntityState> PlacedEntities { get => placedEntities; }

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

        #endregion
    }

}