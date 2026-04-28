using System;
using UnityEngine;

namespace TDG0407.Systems.Data
{

    using Domain;
    using Domain.Interfaces;
    using Domain.Map;

    /// <summary>
    /// 저장/로드 대상이 되는 유저 저장 데이터 루트 클래스입니다.
    /// </summary>
    [Serializable]
    public class UserData : IDataValidatable
    {
        #region Static Fields

        public const int MAX_SAVE_SLOTS = 3;

        #endregion
        #region Fields

        public string version = null;
        public WorldState[] worldStates;
        public int nextInstanceId = 0;

        #endregion
        #region Constructors

        public UserData()
        {
            version = Application.version;
            Initialize();
        }

        #endregion
        #region Methods

        public void Initialize()
        {
            worldStates = new WorldState[MAX_SAVE_SLOTS] { null, null, null };
        }

        public void ValidateData()
        {
            if(worldStates == null) throw new Exception("Invalid value assigned to World States.");
        }

        #endregion
    }
}
