using System;
using System.Collections.Generic;

namespace TDG0407.Domain.Map
{

    using Core.Grid;
    using Domain.Entities;

    /// <summary>
    /// 레벨의 각 방 상태를 나타냅니다.
    /// </summary>
    [Serializable]
    public class RoomState
    {
        #region Fields

        public int? roomInstanceId = null;
        public string roomId = null;
        public Point[] position = { Point.zero };
        public RoomScale scale = RoomScale.Single;
        public Point size = new(7, 7); // This room has (-size.x / 2, -size.y / 2) ~ (size.x / 2, size.y / 2) area.
        public RoomType type = RoomType.NULL;
        public Dictionary<Point, PointState> pointStates = new();
        public Dictionary<Point, WarpPointState> warpPointStates = new();
        public Point[] unavailablePoints = null;
        
        public int? nextPointInstanceId = null;

        #endregion
        #region Properties

        public List<EntityState> this[Point position]
        {
            get
            {
                if (pointStates.TryGetValue(position, out var pointState))
                    return pointState.placedEntities;
                return null;
            }
        }
        
        public int PlacedEntityCount
        {
            get
            {
                int count = 0;
                foreach (var pointState in pointStates.Values)
                    count += pointState.placedEntities?.Count ?? 0;
                return count;
            }
        }

        #endregion
        #region Constructors

        public RoomState(
            int roomInstanceId,
            string roomId,
            Point[] position,
            RoomScale scale,
            Point size,
            RoomType type,
            Dictionary<Point, PointState> pointStates = null,
            Dictionary<Point, WarpPointState> warpPointStates = null,
            Point[] unavailablePoints = null,
            int nextPointInstanceId = 0)
        {
            this.roomInstanceId = roomInstanceId;
            this.roomId = roomId;
            this.position = position ?? throw new ArgumentNullException(nameof(position));
            this.scale = scale;
            this.size = size;
            this.type = type;
            this.pointStates = pointStates ?? new();
            this.warpPointStates = warpPointStates ?? new();
            this.unavailablePoints = unavailablePoints;
            this.nextPointInstanceId = nextPointInstanceId;
        }

        #endregion
        #region Methods

        public void Initialize()
        {
            roomInstanceId = null;
            nextPointInstanceId = 0;
        }

        /// <summary>
        /// 두 방이 워프 포인트를 통해 연결되어 있는지 확인합니다.
        /// </summary>
        public bool IsLinkedWith(RoomState other)
        {
            foreach (var warpPointState in warpPointStates.Values)
            {
                if (warpPointState.target.roomInstanceId == other.roomInstanceId)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 워프 포인트를 추가합니다.
        /// </summary>
        /// <param name="position">워프 포인트의 위치</param>
        /// <param name="warpPointState">워프 포인트 상태</param>
        public void AddWarpPoint(Point position, WarpPointState warpPointState)
        {
            if (warpPointStates.ContainsKey(position))
                throw new InvalidOperationException($"Warp point already exists at position {position}.");
            warpPointStates[position] = warpPointState;
        }
        
        #endregion
    }
    
}