using System;

namespace TDG0407._prototype
{
    [Serializable]
    public class _prototype_StatusEffect
    {
        public _prototype_StatusType type;
        public int durationTicks;

        public _prototype_StatusEffect(_prototype_StatusType type, int durationTicks)
        {
            this.type = type;
            this.durationTicks = durationTicks;
        }
    }
}
