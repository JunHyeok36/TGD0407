using System;
using System.Collections.Generic;

namespace TDG0407.Systems
{

    using Core.Grid;
    using Core.Utils;
    using Domain.Entities;
    using Domain.Map;

    /// <summary>
    /// 맵을 생성하는 시스템입니다.
    /// </summary>
    public static class MapGenerator
    {
        #region Methods

        public static WorldState GenerateWorld(int? worldSeed = null, int? proceduralSeed = null)
        {
            worldSeed ??= SeedParser.NewIntSeed();
            proceduralSeed ??= SeedParser.NewIntSeed();

            Random worldRandom = new(worldSeed.Value);
            MapState mapState = GenerateMap(worldRandom);
            return new WorldState(worldSeed.Value, proceduralSeed.Value, mapState, null, null);
        }
    
        private static MapState GenerateMap(Random worldRandom)
        {
            // TODO: 맵 생성 알고리즘 구현 (현재는 테스트 상태)
            int nextLevelInstanceId = 0;
            int nextRoomInstanceId = 0;

            List<LevelState> levelStates = new()
            {
                GenerateLevel(SelectTestLevelId(worldRandom), nextLevelInstanceId++, worldRandom, ref nextRoomInstanceId),
                GenerateLevel(SelectTestLevelId(worldRandom), nextLevelInstanceId++, worldRandom, ref nextRoomInstanceId),
                GenerateLevel(SelectTestLevelId(worldRandom), nextLevelInstanceId++, worldRandom, ref nextRoomInstanceId),
            };

            return new MapState(levelStates);

            static string SelectTestLevelId(Random random)
            {
                string[] testLevelIds = new[] { "TEST_LEVEL", "TSET_LEVEL" };
                return testLevelIds[random.Next(testLevelIds.Length)];
            }
        }

        private static LevelState GenerateLevel(string levelId, int levelInstanceId, Random worldRandom, ref int nextRoomInstanceId)
        {
            Dictionary<Point, RoomState> roomStates = levelId switch
            {
                "TEST_LEVEL" => GenerateRoomStates("TEST_ROOM", worldRandom, ref nextRoomInstanceId),
                "TSET_LEVEL" => GenerateRoomStates("TSET_ROOM", worldRandom, ref nextRoomInstanceId),
                _ => throw new NotImplementedException($"Level generation for levelId '{levelId}' is not implemented."),
            };
            return new LevelState(levelInstanceId, levelId, roomStates);
        }

        private static Dictionary<Point, RoomState> GenerateRoomStates(string roomId, Random worldRandom, ref int nextRoomInstanceId)
        {
            Dictionary<Point, RoomState> roomStates = new();
            
            int y_min = worldRandom.Next(-2, 0);
            int y_max = worldRandom.Next(0, 2);
            int x_min = worldRandom.Next(-2, 0);
            int x_max = worldRandom.Next(0, 2);
            for(int y = y_min; y <= y_max; y++)
            {
                for(int x = x_min; x <= x_max; x++)
                {
                    Point roomPosition = new(x, y);
                    int w = worldRandom.Next(5, 10);
                    int h = worldRandom.Next(5, 10);
                    roomStates[roomPosition] = new RoomState(
                        nextRoomInstanceId++, 
                        roomId, 
                        new Point(w, h), 
                        RoomType.Battle, 
                        new Dictionary<Point, EntityState>()
                    );
                }
            }

            return roomStates;
        }

        #endregion
    }

}