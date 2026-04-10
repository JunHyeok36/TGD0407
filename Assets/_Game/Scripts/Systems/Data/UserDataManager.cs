using System;

namespace TDG0407.Systems.Data
{
    using Domain.Entities;

    /// <summary>
    /// 유저 데이터를 관리하는 클래스입니다.
    /// </summary>
    public static class UserDataManager
    {
        #region Fields

        private static UserData userData = new();

        #endregion
        #region Properties

        public static UserData UserData => userData;

        #endregion
        #region Methods

        public static void Initialize()
        {
            userData = new UserData();
        }

        public static void ValidateUserData()
        {
            userData.ValidateData();
        }

        #endregion
    }
}