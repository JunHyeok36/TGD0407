using UnityEngine;

namespace TDG0407.Systems.Data
{

    using Domain.Interfaces;
    using Systems.Setting;

    /// <summary>
    /// 게임 설정 데이터를 관리하는 클래스입니다.
    /// </summary>
    public class SettingData : IUserData
    {
        #region Fields

        public readonly ResolutionSetting resolution;
        public readonly QualitySetting quality;
        public readonly AudioSetting audio;
        public SystemLanguage? language;
    
        #endregion
        #region Methods

        public void ValidateData()
        {
            resolution.ValidateData();
            quality.ValidateData();
            audio.ValidateData();
            if (language.HasValue) language = SystemLanguage.English;
        }

        #endregion
    }

}