namespace TDG0407.Core.Managers
{

    public class SettingsManager
    {
        public bool IsMusicOn { get; set; } = true;
        public bool IsSoundEffectsOn { get; set; } = true;
        public float MusicVolume { get; set; } = 1.0f;
        public float SoundEffectsVolume { get; set; } = 1.0f;

        // You can add methods to save/load settings from PlayerPrefs or a file if needed
    }
    
}