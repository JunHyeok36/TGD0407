using System;
using TDG0407.Domain;

namespace TDG0407._prototype
{
    [Serializable]
    public class _prototype_StatusEffect
    {
        public _prototype_StatusType type;
        public TickDuration duration;
        public float value;
        public _prototype_EntityData sourceEntity;
        public _prototype_EntityData targetEntity;
        public int appliedTick;

        /// <summary>
        /// 상태효과 만료 여부를 반환합니다. Forever 타입은 영구 지속이므로 false입니다.
        /// </summary>
        public bool IsExpired => duration.IsExpired;

        /// <summary>
        /// 하위 호환을 위한 프로퍼티. Forever 타입인 경우 int.MaxValue 반환.
        /// </summary>
        public int durationTicks
        {
            get
            {
                if (duration.TickDurationType == TickDurationType.Forever) return int.MaxValue;
                return duration.Value != null ? duration.Value.Current : 0;
            }
            set
            {
                if (duration.TickDurationType == TickDurationType.Forever) return;
                if (duration.Value != null)
                {
                    duration.Value.Current = value;
                }
                else
                {
                    duration = new TickDuration(TickDurationType.TickBased, value);
                }
            }
        }

        public _prototype_StatusEffect(
            _prototype_StatusType type,
            TickDuration duration,
            _prototype_EntityData sourceEntity = null,
            _prototype_EntityData targetEntity = null,
            float value = 0f
            )
        {
            this.type = type;
            this.duration = duration;
            this.sourceEntity = sourceEntity;
            this.targetEntity = targetEntity;
            this.value = value;
            this.appliedTick = _prototype_TickManager.CurrentTick;
        }

        public _prototype_StatusEffect(
            _prototype_StatusType type,
            int durationTicks,
            _prototype_EntityData sourceEntity = null,
            _prototype_EntityData targetEntity = null,
            float value = 0f
            ) : this(
                type,
                durationTicks == int.MaxValue ? new TickDuration(TickDurationType.Forever) : new TickDuration(TickDurationType.TickBased, durationTicks),
                sourceEntity,
                targetEntity,
                value)
        {
        }
    }
}

