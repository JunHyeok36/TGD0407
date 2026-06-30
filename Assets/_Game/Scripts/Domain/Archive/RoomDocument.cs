using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace TDG0407.Domain.Archive
{

    using Core.Grid;
    using Domain.Entities;
    using Domain.Map;
    using View.Map;
    
    [CreateAssetMenu(fileName = "RoomDocument", menuName = "Archive/Map/RoomDocument", order = 5)]
    public sealed class RoomDocument : ScriptableObject
    {
        #region Fields

        public AssetReferenceGameObject prefab;

        public string id = string.Empty;
        public RoomScale scale = RoomScale.Single;
        public Point size = new(7, 7); 
        public RoomType type = RoomType.NULL;

        public Point[] unavailablePoints;
        public Point[] warpablePoints;

        public ushort appearanceWeight = 10000; // 0 ~ 65535

        #endregion
        #region Methods

        public async Task<RoomView> InstantiateRoomView(
            int roomInstanceId, 
            Point[] position, 
            Dictionary<Point, WarpPointState> warpPointStates)
        {
            var handle = prefab.InstantiateAsync();
            var roomViewObject = await handle.Task;
            if(roomViewObject == null)
            {
                Debug.LogError($"Failed to instantiate RoomView prefab for room ID '{id}'.");
                return null;
            }
            if (!roomViewObject.TryGetComponent<RoomView>(out var roomView))
            {
                Debug.LogError($"RoomView prefab for room ID '{id}' is missing the RoomView component.");
                return null;
            }

            foreach (var warpPointState in warpPointStates)
            {
                if (!warpablePoints.Contains(warpPointState.Key))
                    warpPointStates.Remove(warpPointState.Key);
            }

            roomView.Initialize(new RoomState(
                roomInstanceId: roomInstanceId,
                roomId: id,
                position: position,
                scale: scale,
                size: size,
                type: type,
                pointStates: null,
                warpPointStates: warpPointStates,
                unavailablePoints: unavailablePoints
            ));

            return roomView;
            // Destory with 'Addressables.ReleaseInstance(roomView.gameObject)' when the room is no longer needed.
        }
        
        #endregion
        #if UNITY_EDITOR
        #region DEV Methods

        [ContextMenu("InitializeRoomDocument")]
        public async void DEV_InitializeRoomDocument()
        {
            if (prefab == null)
                Debug.LogWarning("RoomDocument prefab reference is not set.");

            RoomView roomView = await InstantiateRoomView(0, new Point[] { new(0, 0) }, new Dictionary<Point, WarpPointState>());
            List<PointView> pointViews = roomView.PointViews;

            // minX, minY, maxX, maxY를 구함
            Point minPoint = new(int.MaxValue, int.MaxValue);
            Point maxPoint = new(int.MinValue, int.MinValue);
            foreach (PointView pointView in pointViews)
            {
                Point pos = pointView.Point;
                if (pos.X < minPoint.X) minPoint.X = pos.X;
                if (pos.Y < minPoint.Y) minPoint.Y = pos.Y;
                if (pos.X > maxPoint.X) maxPoint.X = pos.X;
                if (pos.Y > maxPoint.Y) maxPoint.Y = pos.Y;
            }
            this.size.X = maxPoint.X - minPoint.X + 1;
            this.size.Y = maxPoint.Y - minPoint.Y + 1;

            List<Point> unavailablePoints = new();
            for (int y = minPoint.Y; y <= maxPoint.Y; y++)
            {
                for (int x = minPoint.X; x <= maxPoint.X; x++)
                {
                    Point pos = new(x, y);
                    if (!pointViews.Any(pv => pv.Point.Equals(pos)))
                        unavailablePoints.Add(pos);
                }
            }
            this.unavailablePoints = unavailablePoints.ToArray();

            Addressables.ReleaseInstance(roomView.gameObject);
        }

        #endregion
        #endif
    }

}
