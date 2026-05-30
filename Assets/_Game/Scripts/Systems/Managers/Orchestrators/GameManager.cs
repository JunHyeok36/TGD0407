namespace TDG0407.Systems.Managers
{
    using Domain.Entities;
    using Systems.Data;

    /// <summary>
    /// 게임의 전반적인 시스템을 관리하는 클래스입니다.
    /// </summary>
    public static class GameManager
    {
        #region Fields


        #endregion
        #region Methods

        public static void Initialize()
        {
            UserDataManager.Initialize();
        }

        #endregion
    }
}