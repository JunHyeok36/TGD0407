using UnityEngine;
using System;

namespace TDG0407._prototype
{
    [Serializable]
    public class _prototype_TrapComponentDataModel : _prototype_EntityComponentDataModel
    {
        [Header("Trap Settings")]
        public _prototype_TrapType trapType = _prototype_TrapType.Spike;
        public int triggerDamage = 10;
        public bool isSolidObstacle = false;
        public bool triggerOnStep = true;
        public bool triggerOnDamage = false;
        public bool stopsMovement = false;
        public bool isConsumedOnTrigger = true;

        [Header("Status Effect")]
        public _prototype_StatusType statusEffectToApply = _prototype_StatusType.None;
        public int statusEffectDuration = 1;

        [Header("Explosive / Hazard Settings")]
        public int explosionRadius = 1;
        public int knockbackDistance = 1;
        public int hazardDurationTicks = 2;

        public override _prototype_EntityComponentData CreateComponentData()
        {
            return new _prototype_TrapComponentData
            {
                trapType = trapType,
                triggerDamage = triggerDamage,
                isSolidObstacle = isSolidObstacle,
                isDisarmed = false,
                triggerOnStep = triggerOnStep,
                triggerOnDamage = triggerOnDamage,
                stopsMovement = stopsMovement,
                isConsumedOnTrigger = isConsumedOnTrigger,
                statusEffectToApply = statusEffectToApply,
                statusEffectDuration = statusEffectDuration,
                explosionRadius = explosionRadius,
                knockbackDistance = knockbackDistance,
                hazardDurationTicks = hazardDurationTicks
            };
        }
    }
}
