using System;
using TDG0407.Domain;
using UnityEngine;

namespace TDG0407._prototype
{
    /// <summary>
    /// 엔티티의 보호막 데이터. 개별 인스턴스마다 수치와 지속시간(틱)을 가집니다.
    /// </summary>
    [Serializable]
    public class _prototype_ShieldData
    {
        public int amount;
        /// <summary>
        /// 지속 시간 (TickDuration).
        /// </summary>
        public TickDuration duration;

        public bool IsExpired => amount <= 0 || duration.IsExpired;

        /// <summary>
        /// 하위 호환을 위한 프로퍼티 (-1이면 영구 보호막).
        /// </summary>
        public int durationTicks
        {
            get
            {
                if (duration.TickDurationType == TickDurationType.Forever) return -1;
                return duration.Value != null ? duration.Value.Current : 0;
            }
            set
            {
                if (value < 0) duration = new TickDuration(TickDurationType.Forever);
                else if (duration.Value != null) duration.Value.Current = value;
                else duration = new TickDuration(TickDurationType.TickBased, value);
            }
        }

        public _prototype_ShieldData(int amount, TickDuration duration)
        {
            this.amount = Mathf.Max(0, amount);
            this.duration = duration;
        }

        public _prototype_ShieldData(int amount, int durationTicks = -1)
            : this(amount, durationTicks < 0 ? new TickDuration(TickDurationType.Forever) : new TickDuration(TickDurationType.TickBased, durationTicks))
        {
        }

        public _prototype_ShieldData(_prototype_ShieldData other)
        {
            if (other == null) return;
            this.amount = other.amount;
            this.duration = other.duration.Clone();
        }

        public _prototype_ShieldData Clone() => new _prototype_ShieldData(this);
    }


    /// <summary>
    /// 엔티티의 보호막 수치가 변경되었을 때(획득, 만료, 피격 흡수 등) 발생하는 이벤트.
    /// </summary>
    public struct EntityShieldChangedEvent
    {
        public _prototype_LifeData Entity { get; }
        public int CurrentShield { get; }

        public EntityShieldChangedEvent(_prototype_LifeData entity, int currentShield)
        {
            Entity = entity;
            CurrentShield = currentShield;
        }
    }
}
