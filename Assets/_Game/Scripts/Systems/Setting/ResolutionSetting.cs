using System;

namespace TDG0407.Systems.Setting
{

    using Domain.Interfaces;

    /// <summary>
    /// 해상도 설정에 대해 관리합니다. 
    /// </summary>
    [Serializable]
    public class ResolutionSetting : IDataValidatable
    {
        #region Fields

        public int width = 1920;
        public int height = 1080;
        public int refreshRate = 60;
        public bool isFullScreen = false;

        #endregion
        #region Constructors

        public ResolutionSetting(int width, int height, int refreshRate, bool isFullScreen)
        {
            this.width = width;
            this.height = height;
            this.refreshRate = refreshRate;
            this.isFullScreen = isFullScreen;
        }

        #endregion
        #region Methods

        public void ValidateData()
        {
            if (width <= 0) width = 1920;
            if (height <= 0) height = 1080;
            if (refreshRate <= 0) refreshRate = 60;
        }

        #endregion
    }
    
}