using System;
using UnityEngine;

namespace TDG0407.Systems.Data
{

    using Domain.Interfaces;
    using Domain.Map;

    /// <summary>
    /// 저장/로드 대상이 되는 유저 저장 데이터 루트 클래스입니다.
    /// </summary>
    [Serializable]
    public class UserData : IDataValidatable
    {
        #region Fields

        public string version = null;
        public MapState[] mapState = new MapState[3] { null, null, null };
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
            
        }

        public void ValidateData()
        {
            if(mapState == null) throw new Exception("Invalid value assigned to Map State.");
        }

        #endregion
    }
}
