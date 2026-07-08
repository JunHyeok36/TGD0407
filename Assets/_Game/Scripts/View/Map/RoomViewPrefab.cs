using System.Collections.Generic;
using UnityEngine;

namespace TDG0407.View.Map
{

    using Domain.Map;
    using View.Entities;
    
    public class RoomViewPrefab : MonoBehaviour
    {
        #region Fields

        public List<PointView> pointViews;
        public List<EntityView> entityViews;

        #endregion
        #if UNITY_EDITOR
        #region Unity Methods

        [ContextMenu("Set All Views")]
        public void SetAllViews()
        {
            SetPointViews();
            SetEntityViews();
        }

        [ContextMenu("Set Point Views")]
        public void SetPointViews()
        {
            this.pointViews.Clear();
            PointView[] pointViews = GetComponentsInChildren<PointView>(true);
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

                this.pointViews.Add(pointView);
            }

            var sortedPointViews = new List<PointView>(this.pointViews);
            sortedPointViews.Sort((x, y) => x.Point.X != y.Point.X ? x.Point.X.CompareTo(y.Point.X) : x.Point.Y.CompareTo(y.Point.Y));

            this.pointViews = sortedPointViews;
        }

        [ContextMenu("Set Entity Views")]
        public void SetEntityViews()
        {
            this.entityViews.Clear();
            EntityView[] entityViews = GetComponentsInChildren<EntityView>(true);
            for (int i = 0; i < entityViews.Length; i++)
            {
                EntityView entityView = entityViews[i];
                this.entityViews.Add(entityView);
            }
        }
        #endregion
        #endif
    }

}