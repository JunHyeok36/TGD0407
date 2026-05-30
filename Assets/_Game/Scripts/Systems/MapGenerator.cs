using System;
using System.Collections.Generic;

namespace TDG0407.Systems
{

    using Core.Grid;
    using Domain.Entities;
    using Domain.Map;
    using Systems.Managers;

    /// <summary>
    /// 맵을 생성하는 시스템입니다.
    /// </summary>
    public static class MapGenerator
    {
        #region Methods

        public static WorldState GenerateWorld(string worldSeed = null, string proceduralSeed = null)
        {
            worldSeed ??= Guid.NewGuid().ToString();
            proceduralSeed ??= Guid.NewGuid().ToString();

            MapState mapState = GenerateMap(worldSeed);
            return new WorldState(worldSeed, proceduralSeed, mapState, null, null);
        }
    
        private static MapState GenerateMap(string worldSeed)
        {
            // TODO: 맵 생성 알고리즘 구현 (현재는 테스트 상태)
            int nextLevelInstanceId = 0;
            int nextRoomInstanceId = 0;

            List<LevelState> levelStates = new()
            {
                GenerateLevel("TEST_LEVEL_01", nextLevelInstanceId++, ref worldSeed, ref nextRoomInstanceId),
                GenerateLevel("TEST_LEVEL_02", nextLevelInstanceId++, ref worldSeed, ref nextRoomInstanceId),
                GenerateLevel("TEST_LEVEL_03", nextLevelInstanceId++, ref worldSeed, ref nextRoomInstanceId),
            };

            return new MapState(levelStates);
        }

        private static LevelState GenerateLevel(string levelId, int levelInstanceId, ref string worldSeed, ref int nextRoomInstanceId)
        {
            Dictionary<Point, RoomState> roomStates = null;
            if (levelId.Contains("TEST_LEVEL"))
            {
                roomStates = GenerateRoomStates("TEST_ROOM", ref worldSeed, ref nextRoomInstanceId);
            }
            else
            {
                throw new NotImplementedException($"Level generation for levelId '{levelId}' is not implemented.");
            }
            return new LevelState(levelInstanceId, levelId, roomStates);
        }

        private static Dictionary<Point, RoomState> GenerateRoomStates(string roomId, ref string worldSeed, ref int nextRoomInstanceId)
        {
            Dictionary<Point, RoomState> roomStates = new();
            Point roomPosition = Point.zero;

            roomStates[roomPosition] = new RoomState(
                nextRoomInstanceId++,
                roomId,
                new Point(5, 5),
                RoomType.Empty,
                new Dictionary<Point, EntityState>());

            return roomStates;
        }

        #endregion
    }

}