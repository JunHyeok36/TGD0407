using System.Collections.Generic;
using UnityEngine;

namespace TDG0407.View.Map
{

    using Core.Grid;
    using Domain.Entities;
    using Domain.Map;
    using View.Entities;

    /// <summary>
    /// 맵 상의 하나의 점을 시각적으로 표현하는 뷰입니다.
    /// </summary>
    public class PointView : MonoBehaviour
    {
        #region Fields

        [SerializeField] private PointState _state;

        #endregion
        #region Properties

        public PointState State { get => _state; set => _state = value; }
        public Point Point { get => _state.position; }

        #endregion
        #region Methods

        public void Initialize(PointState state)
        {
            _state = state;
        }

        #endregion
        #if UNITY_EDITOR
        #region DEV Methods

        [ContextMenu("GetChildEntityStates")]
        public void DEV_GetChildEntityStates()
        {
            EntityView[] entityViews = transform.GetComponentsInChildren<EntityView>();
            List<EntityState> entityStates = new();
            foreach (EntityView entityView in entityViews)
                entityStates.Add(entityView.State);
            _state.placedEntities = entityStates;
        }

        #endregion
        #endif
    }
}