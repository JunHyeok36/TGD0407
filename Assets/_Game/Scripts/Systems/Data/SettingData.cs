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

        public ResolutionSetting resolution = new(1920, 1080, 60, false);
        public QualitySetting quality = new();
        public AudioSetting audio = new();
        public SystemLanguage? language;
    
        #endregion
        #region Constructors

        public SettingData()
        {
            language = SystemLanguage.English;
        }

        #endregion
        #region Methods

        public void ValidateData()
        {
            resolution ??= new ResolutionSetting(1920, 1080, 60, false);
            quality ??= new QualitySetting();
            audio ??= new AudioSetting();

            resolution.ValidateData();
            quality.ValidateData();
            audio.ValidateData();
            if (language.HasValue == false)
                language = SystemLanguage.English;
        }

        #endregion
    }

}