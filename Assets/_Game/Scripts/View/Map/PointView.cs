using UnityEngine;

namespace TDG0407.View.Map
{

    using Core.Grid;

    /// <summary>
    /// 맵 상의 하나의 점을 시각적으로 표현하는 뷰입니다.
    /// </summary>
    public class PointView : MonoBehaviour
    {
        #region Fields

        private Point _point;

        #endregion
        #region Properties

        public Point Point { get => _point; }

        #endregion
        #region Methods

        public void Initialize(Point point)
        {
            _point = point;
        }

        #endregion
    }
}