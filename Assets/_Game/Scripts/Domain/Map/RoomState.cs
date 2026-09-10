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

        [NonSerialized] public int? roomInstanceId = null;
        public string roomId = null;
        public string levelId = null;
        public Point[] position = { Point.zero };
        public RoomScale scale = RoomScale.Single;
        public Point size = new(7, 7); // This room has (-size.x / 2, -size.y / 2) ~ (size.x / 2, size.y / 2) area.
        public RoomType type = RoomType.NULL;
        public Dictionary<Point, PointState> pointStates = new();
        public Dictionary<Point, WarpPointState> warpPointStates = new();
        public Point[] unavailablePoints = null;
        
        [NonSerialized] public int? nextPointInstanceId = null;

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

        public List<EntityState> PlacedEntities
        {
            get
            {
                List<EntityState> entities = new();
                foreach (var pointState in pointStates.Values)
                    entities.AddRange(pointState.placedEntities);
                return entities;
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
            string levelId,
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
            this.levelId = levelId;
            this.position = position ?? throw new ArgumentNullException(nameof(position));
            this.scale = scale;
            this.size = size;
            this.type = type;
            this.pointStates = pointStates ?? new();
            this.warpPointStates = warpPointStates ?? new();
            this.unavailablePoints = unavailablePoints;
            this.nextPointInstanceId = nextPointInstanceId;
        }

        public RoomState(RoomState other)
        {
            this.roomInstanceId = other.roomInstanceId;
            this.roomId = other.roomId;
            this.levelId = other.levelId;
            this.position = (Point[])other.position.Clone();
            this.scale = other.scale;
            this.size = other.size;
            this.type = other.type;
            foreach (var kvp in other.pointStates)
                this.pointStates[kvp.Key] = kvp.Value.Clone();
            foreach (var kvp in other.warpPointStates)
                this.warpPointStates[kvp.Key] = (WarpPointState)kvp.Value.Clone();
            if (other.unavailablePoints != null)
                this.unavailablePoints = (Point[])other.unavailablePoints.Clone();
            this.nextPointInstanceId = other.nextPointInstanceId;
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

        public RoomState Clone()
        {
            Dictionary<Point, PointState> clonedPointStates = new();
            foreach (var kvp in pointStates)
            {
                clonedPointStates[kvp.Key] = kvp.Value.Clone();
            }

            Dictionary<Point, WarpPointState> clonedWarpPointStates = new();
            foreach (var kvp in warpPointStates)
            {
                clonedWarpPointStates[kvp.Key] = (WarpPointState)kvp.Value.Clone();
            }

            return new RoomState(
                roomInstanceId: this.roomInstanceId ?? throw new InvalidOperationException("RoomInstanceId is null."),
                roomId: this.roomId,
                levelId: this.levelId,
                position: (Point[])this.position.Clone(),
                scale: this.scale,
                size: this.size,
                type: this.type,
                pointStates: clonedPointStates,
                warpPointStates: clonedWarpPointStates,
                unavailablePoints: this.unavailablePoints != null ? (Point[])this.unavailablePoints.Clone() : null,
                nextPointInstanceId: this.nextPointInstanceId ?? 0
            );
        }

        #endregion
    }
    
}