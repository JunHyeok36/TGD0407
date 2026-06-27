using System;
using System.Collections.Generic;

namespace TDG0407.Domain.Map
{

    using Core.Grid;

    /// <summary>
    /// 레벨의 상태를 나타냅니다.
    /// </summary>
    [Serializable]
    public class LevelState
    {
        #region Fields
    
        public int? levelInstanceId = null;
        public string levelId = null;
        public LevelScale scale = LevelScale.Medium;
        public Point size;
        public readonly Dictionary<Point, RoomState> roomStates = new();

        public readonly Point startPoint = Point.zero;
        public readonly Point endPoint = Point.zero;

        public int nextRoomInstanceId;

        #endregion
        #region Properties

        public RoomState this[Point position]
        {
            get
            {
                if (roomStates.TryGetValue(position, out var roomState))
                    return roomState;
                return null;
            }
        }

        #endregion
        #region Constructors

        public LevelState(int levelInstanceId, string levelId, LevelScale scale, Point size, Dictionary<Point, RoomState> roomStates, Point startPoint, Point endPoint, int nextRoomInstanceId)
        {
            this.levelInstanceId = levelInstanceId;
            this.levelId = levelId;
            this.scale = scale;
            this.size = size;
            this.roomStates = roomStates;
            this.startPoint = startPoint;
            this.endPoint = endPoint;
            this.nextRoomInstanceId = nextRoomInstanceId;
        }

        #endregion
        #region Methods

        public void Initialize()
        {
            levelInstanceId = null;
            nextRoomInstanceId = 0;
        }

        public int PublishRoomInstanceId()
        {
            int currentId = nextRoomInstanceId;
            nextRoomInstanceId++;
            return currentId;
        }

        #endregion
    }

}