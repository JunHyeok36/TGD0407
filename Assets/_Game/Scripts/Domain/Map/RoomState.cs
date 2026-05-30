using System;
using System.Collections.Generic;

namespace TDG0407.Domain.Map
{

    using Core.Grid;
    using Domain.Entities;

    /// <summary>
    /// 레벨 내 하나의 방 상태를 나타냅니다.
    /// </summary>
    [Serializable]
    public class RoomState
    {
        #region Fields

        public int roomInstanceId = -1;
        public string roomId = null;
        public Point size = new(5, 5); // This room has (-size.x / 2, -size.y / 2) ~ (size.x / 2, size.y / 2) area.
        public RoomType type = RoomType.NULL;
        public readonly Dictionary<Point, EntityState> placed_entities = new();

        #endregion
        #region Constructors

        public RoomState(int roomInstanceId, string roomId, Point size, RoomType type, Dictionary<Point, EntityState> placed_entities)
        {
            this.roomInstanceId = roomInstanceId;
            this.roomId = roomId;
            this.size = size;
            this.type = type;
            this.placed_entities = placed_entities;
        }

        #endregion
        #region Methods

        public void Initialize()
        {
            
        }
        
        #endregion
        #region Operators

        public EntityState this[Point position]
        {
            get
            {
                if (placed_entities.TryGetValue(position, out var entity))
                    return entity;
                return null;
            }
        }

        #endregion
    }
    
}