using System;

namespace TDG0407._prototype
{
    [Serializable]
    public class _prototype_StatusEffect
    {
        public _prototype_StatusType type;
        public int durationTicks;
        public float value;
        public _prototype_EntityData sourceEntity;
        public _prototype_EntityData targetEntity;
        public int appliedTick;

        public _prototype_StatusEffect(
            _prototype_StatusType type,
            int durationTicks,
            _prototype_EntityData sourceEntity = null,
            _prototype_EntityData targetEntity = null,
            float value = 0f
            )
        {
            this.type = type;
            this.durationTicks = durationTicks;
            this.sourceEntity = sourceEntity;
            this.targetEntity = targetEntity;
            this.value = value;
            this.appliedTick = _prototype_TickManager.CurrentTick;
        }
    }
}
