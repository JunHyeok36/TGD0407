using System;

namespace TDG0407.Systems.Setting
{
    using Domain.Interfaces;

    /// <summary>
    /// 오디오 설정에 대해 관리합니다. 
    /// </summary>
    [Serializable]
    public class AudioSetting : IDataValidatable
    {
        #region Fields

        public float masterVolume = 0.75f;
        public float musicVolume = 0.75f;
        public float sfxVolume = 0.75f;

        #endregion
        #region Constructors

        #endregion
        #region Methods

        public void ValidateData()
        {
            masterVolume = Math.Clamp(masterVolume, 0f, 1f);
            musicVolume = Math.Clamp(musicVolume, 0f, 1f);
            sfxVolume = Math.Clamp(sfxVolume, 0f, 1f);
        }

        #endregion
    }
    
}