using System.Collections.Generic;
using UnityEngine;

namespace TDG0407.View.Map
{

    using Domain.Map;

    /// <summary>
    /// 방 상태를 기반으로 맵을 시각적으로 표현하는 뷰입니다.
    /// </summary>
    public class RoomView : MonoBehaviour
    {
        #region Fields

        [SerializeField] private RoomState _state = null;
        [SerializeField] private List<PointView> _pointViews = new();

        #endregion
        #region Properties

        public int? RoomInstanceId { get => _state?.roomInstanceId; }

        #endregion
        #region Methods

        public void Initialize(RoomState roomState)
        {
            _state = roomState;

            _pointViews.Clear();
            PointView[] pointViews = transform.GetComponentsInChildren<PointView>();
            for (int i = 0; i < pointViews.Length; i++)
            {
                PointView pointView = pointViews[i];
                pointView.name = $"P({pointView.Point.X},{pointView.Point.Y})";

                PointState pointState = pointView.State;
                pointState.pointInstanceId = i;
                pointState.position = new((int)pointView.transform.localPosition.x, (int)pointView.transform.localPosition.z);
                pointView.Initialize(pointState);

                _pointViews.Add(pointView);
            }
        }

        #endregion
        #if UNITY_EDITOR
        #region DEV Methods

        [ContextMenu("InitializePointViews")]
        public void DEV_InitializePointViews()
        {
            Initialize(_state);
            
            var sortedPointViews = new List<PointView>(_pointViews);
            sortedPointViews.Sort((x, y) => x.Point.X != y.Point.X ? x.Point.X.CompareTo(y.Point.X) : x.Point.Y.CompareTo(y.Point.Y));

            for (int i = 0; i < sortedPointViews.Count; i++)
            {
                PointView pointView = sortedPointViews[i];
                pointView.transform.SetSiblingIndex(i);
                _pointViews.Add(pointView);
            }
        }

        #endregion
        #endif
    }
}