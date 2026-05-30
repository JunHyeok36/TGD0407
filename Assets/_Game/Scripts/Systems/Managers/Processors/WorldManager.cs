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

        #endregion
    }
}