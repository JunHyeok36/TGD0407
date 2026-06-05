namespace TDG0407.Systems.Managers
{

    using Domain.Map;

    /// <summary>
    /// 월드 시스템을 관리하는 클래스입니다.
    /// </summary>
    public static class WorldManager
    {
        #region Fields

        private static WorldState _worldState = null;

        #endregion
        #region Properties

        public static WorldState CurrentWorldState { get => _worldState; }
        public static MapState CurrentMapState { get => _worldState.mapState; }

        #endregion
        #region Methods

        public static void Initialize(WorldState worldState)
        {
            _worldState = worldState;
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

        #endregion
    }
}