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

        private RoomState _roomState = null;
        private readonly List<PointView> _pointViews = new();

        #endregion
        #region Properties

        public int? RoomInstanceId { get => _roomState?.roomInstanceId; }

        #endregion
        #region Methods

        public void Initialize(RoomState roomState)
        {
            _roomState = roomState;
        }

        #endregion
    }
}