using UnityEngine;
using System.Text;

namespace TDG0407.TEST
{

    using Core.Utils;
    using Domain.Map;
    using Systems;

    public class MapGeneratorTester : MonoBehaviour
    {
        [ContextMenuItem("Generate New World Seed", nameof(GenerateNewWorldSeed))]
        [SerializeField] private int _worldSeed;
        [ContextMenuItem("Generate Map", nameof(Test))]
        [SerializeField] private WorldState _worldState;

        void Start()
        {
            Test();
        }

        public void GenerateNewWorldSeed()
        {
            _worldSeed = SeedParser.NewIntSeed();
        }

        public void Test()
        {
            _worldState = MapGenerator.GenerateWorld(_worldSeed);

            if (_worldState == null)
            {
                Debug.LogWarning("World generation failed: WorldState is null.");
                return;
            }

            StringBuilder sb = new();
            sb.AppendLine("[WorldState Dump]");
            sb.AppendLine($"worldSeed: {_worldState.worldSeed.ToString("X8")}");
            sb.AppendLine($"proceduralSeed: {_worldState.proceduralSeed.ToString("X8")}");
            sb.AppendLine($"hasLifeState: {_worldState.lifeState != null}");
            sb.AppendLine($"hasPlayerPosition: {_worldState.playerPosition != null}");

            if (_worldState.mapState == null)
            {
                sb.AppendLine("mapState: null");
                Debug.Log(sb.ToString());
                return;
            }

            sb.AppendLine($"levelCount: {_worldState.mapState.levelStates.Count}");

            for (int i = 0; i < _worldState.mapState.levelStates.Count; i++)
            {
                LevelState level = _worldState.mapState.levelStates[i];
                if (level == null)
                {
                    sb.AppendLine($"  Level[{i}]: null");
                    continue;
                }

                int roomCount = level.roomStates?.Count ?? 0;
                sb.AppendLine($"  Level[{i}] id={level.levelId}, instanceId={level.levelInstanceId}, roomCount={roomCount}");

                if (level.roomStates == null) continue;

                foreach (var roomPair in level.roomStates)
                {
                    var roomPos = roomPair.Key;
                    RoomState room = roomPair.Value;

                    if (room == null)
                    {
                        sb.AppendLine($"    Room@({roomPos.X},{roomPos.Y}): null");
                        continue;
                    }

                    int placedEntityCount = room.placed_entities?.Count ?? 0;
                    sb.AppendLine(
                        $"    Room@({roomPos.X},{roomPos.Y}) id={room.roomId}, instanceId={room.roomInstanceId}, type={room.type}, size=({room.size.X},{room.size.Y}), entities={placedEntityCount}");
                }
            }

            Debug.Log(sb.ToString());
        }
    }
    
}