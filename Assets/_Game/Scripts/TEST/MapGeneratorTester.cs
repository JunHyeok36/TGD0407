using Cysharp.Threading.Tasks;
using System.Text;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TDG0407.TEST
{

    using Core.Grid;
    using Core.Utils;
    using Domain.Archive;
    using Domain.Map;
    using Systems.Generators;
    using Systems.Managers;

    public class MapGeneratorTester : MonoBehaviour
    {
        [ContextMenuItem("Generate New World Seed", nameof(GenerateNewWorldSeed))]
        [SerializeField] private int _worldSeed;
        [ContextMenuItem("Generate Map", nameof(Test))]
        private WorldState _worldState = null;

        async UniTaskVoid Start()
        {
            await ArchiveManager.Initialize();
            Test().Forget();
        }

        public void GenerateNewWorldSeed()
        {
            _worldSeed = SeedParser.NewIntSeed();
        }

        public async UniTaskVoid Test()
        {
            await GenerateNewWorldState();
            
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

        [ContextMenu("Generate New WorldState")]
        public async UniTask GenerateNewWorldState()
        {
            _worldState = await MapGenerator.GenerateWorld(_worldSeed);
        }

        [ContextMenu("Build LevelViews")]
        public async UniTask BuildLevelViews()
        {
            await WorldManager.Initialize(_worldState);
        }

        [ContextMenu("Set Next LevelViews")]
        public async UniTask SetNextLevelViews()
        {
            await WorldManager.SetNextLevelView();
        }
    }
}