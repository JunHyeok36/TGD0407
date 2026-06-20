using System.Collections.Generic;
using UnityEngine;

namespace TDG0407.View.Map
{

    using Domain.Map;

    /// <summary>
    /// 레벨 상태를 기반으로 맵을 시각적으로 표현하는 뷰입니다.
    /// </summary>
    public class LevelView : MonoBehaviour
    {
        #region Fields

        private LevelState _levelState = null;
        private readonly List<RoomView> _roomViews = new();

        #endregion
        #region Properties

        public int? LevelInstanceId { get => _levelState?.levelInstanceId; }

        #endregion
        #region Methods

        public void Initialize(LevelState levelState)
        {
            _levelState = levelState;
            foreach (var roomState in levelState.roomStates.Values)
            {
                var roomView = Instantiate(MapPrefabLoader.LoadRoomViewAsync(roomState.roomId).Result, transform).GetComponent<RoomView>();
                roomView.Initialize(roomState);
                _roomViews.Add(roomView);
            }
        }

        #endregion
    }

}