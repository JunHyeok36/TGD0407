using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TDG0407.Systems.Managers
{

    using Domain.Archive;
    using Domain.Map;
    using View.Map;

    /// <summary>
    /// 월드 시스템을 관리하는 클래스입니다.
    /// </summary>
    public static class WorldManager
    {
        #region Fields

        private static WorldState _worldState = null;
        private static (int depth, LevelView view) _currentLevel = (-1, null);

        #endregion
        #region Properties

        public static WorldState CurrentWorldState { get => _worldState; }
        public static MapState CurrentMapState { get => _worldState.mapState; }

        #endregion
        #region Methods

        public static async UniTask Initialize(WorldState worldState)
        {
            _worldState = worldState;
            _currentLevel = (-1, null);

            await SetNextLevelView();
        }

        public static LevelState GetLevelState(int levelInstanceId)
        {
            foreach (var levelState in CurrentMapState.levelStates)
            {
                if (levelState.levelInstanceId == levelInstanceId)
                    return levelState;
            }
            return null;
        }

        public static RoomState GetRoomState(int roomInstanceId)
        {
            foreach (var levelState in CurrentMapState.levelStates)
            {
                foreach (var roomState in levelState.roomStates.Values)
                {
                    if (roomState.roomInstanceId == roomInstanceId)
                        return roomState;
                }
            }
            return null;
        }

        public static async UniTask<LevelView> SetNextLevelView()
        {
            if (_currentLevel.view != null)
                GameObject.Destroy(_currentLevel.view.gameObject);
            
            if(++_currentLevel.depth < 0) 
                _currentLevel.depth = 0;
            else if(_currentLevel.depth >= CurrentMapState.levelStates.Count)
            {
                _currentLevel = (CurrentMapState.levelStates.Count - 1, null);
                return null;
            }
            GameObject levelViewObject = new("LevelView_" + _currentLevel.depth);
            levelViewObject.SetActive(false);
            _currentLevel.view = levelViewObject.AddComponent<LevelView>();
            await _currentLevel.view.Initialize(CurrentMapState.levelStates[_currentLevel.depth]);
            levelViewObject.SetActive(true);

            return _currentLevel.view;
        }

        #endregion
    }
}