using UnityEngine;

namespace TDG0407._prototype
{
    public struct StatusDisplayData
    {
        public string id;
        public string displayName;
        public string symbolChar;
        public Sprite icon;
        public Color themeColor;
        public string description;

        // Visual info
        public int stackCount;
        public int durationTicks;
        public bool isForever;
    }
}
