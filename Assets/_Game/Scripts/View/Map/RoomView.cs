using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TDG0407.View.Map
{
    using Core.Grid;
    using Domain.Archive;
    using Domain.Entities;
    using Domain.Map;
    using View.Entities;

    /// <summary>
    /// 방 상태를 기반으로 맵을 시각적으로 표현하는 뷰입니다.
    /// </summary>
    public class RoomView : MonoBehaviour
    {
        #region Fields

        private RoomState _state = null;
        [SerializeField] private List<PointView> _pointViews = new();
        [SerializeField] private List<EntityView> _entityViews = new();

        #endregion
        #region Properties

        public int? RoomInstanceId { get => _state?.roomInstanceId; }
        public string RoomId { get => _state?.roomId; }
        public RoomState State { get => _state; }
        public List<PointView> PointViews { get => _pointViews; }

        #endregion
        #region Methods

        public async UniTask Initialize(RoomState roomState)
        {
            _state = roomState;

            if (_state != null)
            {
                _pointViews.Clear();
                PointView[] pointViews = transform.GetComponentsInChildren<PointView>();
                foreach (var pointView in pointViews)
                {
                    pointView.State.placedEntities = new(1);
                    _pointViews.Add(pointView);
                }

                _entityViews.Clear();
                EntityView[] entityViews = transform.GetComponentsInChildren<EntityView>();
                foreach (var entityView in entityViews)
                    DestroyImmediate(entityView.gameObject);

                if (_state.pointStates == null)
                {
                    Dictionary<Point, PointState> pointStates = new();
                    foreach (var pointView in _pointViews)
                    {
                        pointStates[pointView.Point] = pointView.State 
                            ?? throw new Exception($"PointView {pointView.Point} has a null State.");
                    }
                    _state.pointStates = pointStates;
                }

                Vector2 center = _state.position.Aggregate(new Vector2(0, 0), (acc, p) => acc + new Vector2(p.X, p.Y)) / _state.position.Length;
                transform.localPosition = new Vector3(center.x * 14, 0, center.y * 14);
                
                foreach (var entityState in _state.PlacedEntities)
                {
                    PointState pointState = _state.pointStates[entityState.position];
                    if (ArchiveManager.levelCollection == null)
                    {
                        Debug.LogWarning("LevelCollection is not initialized.");
                        continue;
                    }
                    if (!ArchiveManager.levelCollection.TryGetLevelDocument(_state.levelId, out LevelDocument levelDocument))
                    {
                        Debug.LogWarning($"LevelDocument not found for levelId '{_state.levelId}'.");
                        continue;
                    }
                    if (levelDocument.entityCollection == null)
                    {
                        Debug.LogWarning($"EntityCollection is missing on level '{_state.levelId}'.");
                        continue;
                    }
                    if (!levelDocument.entityCollection.TryGetEntityDocument(entityState.entityId, out EntityDocument entityDocument))
                    {
                        Debug.LogWarning($"EntityDocument not found for entityId '{entityState.entityId}'.");
                        continue;
                    }

                    EntityView entityView = await entityDocument.InstantiateEntityView(_state.levelId, entityState.entityInstanceId.Value, GetPointView(pointState.position));
                    _entityViews.Add(entityView);
                }
            }
            else
            {
                _pointViews.Clear();
                PointView[] pointViews = transform.GetComponentsInChildren<PointView>();
                for (int i = 0; i < pointViews.Length; i++)
                {
                    PointView pointView = pointViews[i];

                    PointState pointState = pointView.State;
                    pointState.pointInstanceId = i;
                    pointState.position = new((int)pointView.transform.localPosition.x, (int)pointView.transform.localPosition.z);
                    pointView.Initialize(pointState);

                    _pointViews.Add(pointView);
                }

                _entityViews.Clear();
                EntityView[] entityViews = transform.GetComponentsInChildren<EntityView>();
                foreach (var entityView in entityViews)
                {
                    EntityState entityState = entityView.State;
                    if (entityView.transform.parent.TryGetComponent(out PointView parentPointView))
                    {
                        entityView.Initialize(entityState, parentPointView.State);
                    }
                    else
                    {
                        throw new Exception($"EntityView '{entityView.name}' does not have a PointView as its parent.");
                    }
                    _entityViews.Add(entityView);
                }
            }
        }

        public PointView GetPointView(Point point)
        {
            foreach (var pointView in _pointViews)
            {
                if (pointView.Point.Equals(point))
                    return pointView;
            }
            return null;
        }

        public void TransposeRoomViews()
        {
            foreach (var pointView in _pointViews)
            {
                pointView.transform.localPosition = new Vector3(
                    pointView.transform.localPosition.z,
                    0,
                    pointView.transform.localPosition.x
                );

                PointState pointState = pointView.State;
                pointState.position = new Point(pointState.position.Y, pointState.position.X);
            }

            if (_state != null)
            {
                Dictionary<Point, PointState> pointStates = new();
                for (int i = 0; i < _pointViews.Count; i++)
                {
                    PointView pointView = _pointViews[i];
                    pointStates[pointView.Point] = pointView.State 
                        ?? throw new System.Exception($"PointView at index {i} has a null State.");
                }
                _state.pointStates = pointStates;
            }
        }
        public void RotateRoomViews(byte degrees)
        {
            if (degrees % 90 != 0)
                throw new System.ArgumentException("Rotation degrees must be a multiple of 90.");

            int rotations = (degrees / 90) % 4;
            for (int i = 0; i < rotations; i++)
            {
                foreach (var pointView in _pointViews)
                {
                    pointView.transform.localPosition = new Vector3(
                        -pointView.transform.localPosition.z,
                        0,
                        pointView.transform.localPosition.x
                    );

                    PointState pointState = pointView.State;
                    pointState.position = new Point(-pointState.position.Y, pointState.position.X);
                }
            }

            if (_state != null)
            {
                Dictionary<Point, PointState> pointStates = new();
                for (int i = 0; i < _pointViews.Count; i++)
                {
                    PointView pointView = _pointViews[i];
                    pointStates[pointView.Point] = pointView.State 
                        ?? throw new System.Exception($"PointView at index {i} has a null State.");
                }
                _state.pointStates = pointStates;
            }
        }

        #endregion
        #if UNITY_EDITOR
        #region DEV Methods

        [ContextMenu("InitializePointViews")]
        public void DEV_InitializePointViews()
        {
            _pointViews.Clear();
            PointView[] pointViews = transform.GetComponentsInChildren<PointView>();
            for (int i = 0; i < pointViews.Length; i++)
            {
                PointView pointView = pointViews[i];
                pointView.name = $"P({pointView.Point.X},{pointView.Point.Y})";
                pointView.transform.localPosition = new Vector3(
                    Mathf.RoundToInt(pointView.transform.localPosition.x), 
                    0, 
                    Mathf.RoundToInt(pointView.transform.localPosition.z)
                );

                PointState pointState = pointView.State;
                pointState.pointInstanceId = i;
                pointState.position = new((int)pointView.transform.localPosition.x, (int)pointView.transform.localPosition.z);
                pointView.Initialize(pointState);

                _pointViews.Add(pointView);
            }

            var sortedPointViews = new List<PointView>(_pointViews);
            sortedPointViews.Sort((x, y) => x.Point.X != y.Point.X ? x.Point.X.CompareTo(y.Point.X) : x.Point.Y.CompareTo(y.Point.Y));

            for (int i = 0; i < sortedPointViews.Count; i++)
            {
                PointView pointView = sortedPointViews[i];
                pointView.transform.SetSiblingIndex(i);
            }

            _pointViews = sortedPointViews;
        }

        [ContextMenu("InitializeEntityViews")]
        public void DEV_InitializeEntityViews()
        {
            _entityViews.Clear();
            EntityView[] entityViews = transform.GetComponentsInChildren<EntityView>();
            foreach (var entityView in entityViews)
            {
                EntityState entityState = entityView.State;
                if (entityView.transform.parent.TryGetComponent(out PointView parentPointView))
                {
                    entityView.Initialize(entityState, parentPointView.State);
                }
                else
                {
                    throw new System.Exception($"EntityView '{entityView.name}' does not have a PointView as its parent.");
                }
                _entityViews.Add(entityView);
            }
        }

        [ContextMenu("TransposeRoomViews")]
        public void DEV_TransposeRoomViews()
        {
            TransposeRoomViews();

            DEV_InitializePointViews();
        }

        [ContextMenu("RotateRoomViews90DegreesAntiClockwise")]
        public void DEV_RotateRoomViews90DegreesAntiClockwise()
        {
            RotateRoomViews(90);

            DEV_InitializePointViews();
        }

        #endregion
        #endif
    }
}