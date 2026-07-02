using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

namespace TDG0407.View.Map
{

    using Domain.Archive;
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
        public LevelState LevelState { get => _levelState; }
        public IReadOnlyList<RoomView> RoomViews { get => _roomViews; }

        #endregion
        #region Methods

        public async UniTask Initialize(LevelState levelState)
        {
            _levelState = levelState;
            _roomViews.Clear();
            foreach (var roomState in levelState.roomStates.Values)
            {
                RoomDocument roomDocument = ArchiveManager.levelCollection.GetLevelDocument(levelState.levelId).roomCollection.GetRoomDocument(roomState.roomId);
                RoomView roomView = await roomDocument.InstantiateRoomView(roomState, transform);
                _roomViews.Add(roomView);
            }
        }

        #endregion
    }

}