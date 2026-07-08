using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

namespace TDG0407.Systems.Managers
{

    using Domain.Archive;
    using Domain.Map;
    using View.Map;

    /// <summary>
    /// 레벨 상태를 기반으로 RoomView 생성 흐름을 조율합니다.
    /// </summary>
    public static class LevelViewPresenter
    {
        #region Methods

        public static async UniTask BuildRoomViews(LevelState levelState, Transform parent, List<RoomView> outputRoomViews)
        {
            if (levelState == null)
                return;
            if (outputRoomViews == null)
                return;

            outputRoomViews.Clear();

            if (!ArchiveManager.levelCollection.TryGetLevelDocument(levelState.levelId, out LevelDocument levelDocument))
            {
                Debug.LogWarning($"LevelDocument not found for levelId '{levelState.levelId}'.");
                return;
            }

            foreach (var roomState in levelState.roomStates.Values)
            {
                if (roomState == null || string.IsNullOrWhiteSpace(roomState.roomId))
                    continue;

                if (!levelDocument.roomCollection.TryGetRoomDocument(roomState.roomId, out RoomDocument roomDocument))
                {
                    Debug.LogWarning($"RoomDocument not found for roomId '{roomState.roomId}'.");
                    continue;
                }

                RoomView roomView = await roomDocument.InstantiateRoomView(roomState, parent);
                if (roomView != null)
                    outputRoomViews.Add(roomView);
            }
        }

        #endregion
    }

}
