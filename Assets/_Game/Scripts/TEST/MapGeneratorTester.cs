using UnityEngine;
using System.Text;

namespace TDG0407.TEST
{

    using Core.Grid;
    using Core.Utils;
    using Domain.Archive;
    using Domain.Map;
    using Systems.Generators;

    public class MapGeneratorTester : MonoBehaviour
    {
        [ContextMenuItem("Generate New World Seed", nameof(GenerateNewWorldSeed))]
        [SerializeField] private int _worldSeed;
        [ContextMenuItem("Generate Map", nameof(Test))]
        [SerializeField] private WorldState _worldState;

        void Start()
        {
            ArchiveManager.Initialize();
            Test();
        }

        public void GenerateNewWorldSeed()
        {
            _worldSeed = SeedParser.NewIntSeed();
        }

        public void Test()
        {
            _worldState = MapGenerator.GenerateWorld(_worldSeed);
            
            for (int i = 0; i < _worldState.mapState.levelStates.Count; i++)
            {
                LevelState level = _worldState.mapState.levelStates[i];
                Debug.Log($"Level[{i}] id={level.levelId}, instanceId={level.levelInstanceId}, scale={level.scale}, size=({level.size.X},{level.size.Y}), roomCount={level.roomStates.Count}");
                // Graphical representation of the rooms in the level
                int halfWidth = level.size.X / 2, 
                    halfHeight = level.size.Y / 2;
                StringBuilder strBuilder = new();
                strBuilder.AppendLine();
                for (int y = -halfHeight; y <= halfHeight; y++)
                {
                    for (int x = -halfWidth; x <= halfWidth; x++)
                    {
                        Point pos = new(x, y);
                        if (level.roomStates.TryGetValue(pos, out _))
                        {
                            if (pos == level.startPoint) strBuilder.Append("◯");
                            else if (pos == level.endPoint) strBuilder.Append("✕");
                            else strBuilder.Append("■");
                        }
                        else strBuilder.Append("□");
                    }
                    strBuilder.AppendLine();
                }
                Debug.Log(strBuilder.ToString());
    
                // Log each room's details
                foreach (var roomPair in level.roomStates)
                {
                    var roomPos = roomPair.Key;
                    RoomState room = roomPair.Value;
                    Debug.Log($"Room@({roomPos.X},{roomPos.Y}) id={room.roomId}, instanceId={room.roomInstanceId}, type={room.type}, scale={room.scale}, size=({room.size.X},{room.size.Y}), entities={room.PlacedEntityCount}");
                }
            }
        }
    }
    
}