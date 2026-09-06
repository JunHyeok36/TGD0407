using System;
using System.Collections.Generic;

namespace TDG0407._prototype
{
    [Serializable]
    public class _prototype_ProjectileData : _prototype_EntityData
    {
        public _prototype_Point direction = _prototype_Point.zero;
        public int speed = 1; // cells per tick
        public _prototype_Side side = _prototype_Side.None;
        
        [NonSerialized] public _prototype_EntityData shooter;
        public List<_prototype_CardAction> onHitActions = new();

        public float homingAnglePerStep = 0f;
        [NonSerialized] public _prototype_EntityData homingTarget;
        public float currentFloatAngle = float.NaN;

        // 새로운 투사체 옵션들
        public bool isTracking = false;
        public bool stopAtTargetPoint = false;
        public _prototype_Point fixedTargetPoint = _prototype_Point.zero;
        public int maxTravelDistance = -1; // -1 이면 무제한
        public int traveledDistance = 0;

        public _prototype_ProjectileData() : base() { }

        public _prototype_ProjectileData(
            string name,
            _prototype_BoundedValue<int> health,
            _prototype_BoundedValue<int> stamina,
            _prototype_Point point,
            _prototype_MovementType movementType,
            _prototype_Point direction,
            int speed,
            _prototype_Side side,
            _prototype_EntityData shooter,
            List<_prototype_CardAction> onHitActions,
            float homingAnglePerStep,
            _prototype_EntityData homingTarget,
            bool isTracking,
            bool stopAtTargetPoint,
            _prototype_Point fixedTargetPoint,
            int maxTravelDistance) : base(name, health, stamina, point, movementType)
        {
            this.direction = direction;
            this.speed = speed;
            this.side = side;
            this.shooter = shooter;
            this.onHitActions = onHitActions ?? new();
            this.homingAnglePerStep = homingAnglePerStep;
            this.homingTarget = homingTarget;
            this.isTracking = isTracking;
            this.stopAtTargetPoint = stopAtTargetPoint;
            this.fixedTargetPoint = fixedTargetPoint;
            this.maxTravelDistance = maxTravelDistance;
        }

        public _prototype_ProjectileData(_prototype_ProjectileData other) : base(other)
        {
            this.direction = other.direction;
            this.speed = other.speed;
            this.side = other.side;
            this.shooter = other.shooter;
            this.onHitActions = other.onHitActions;
            this.homingAnglePerStep = other.homingAnglePerStep;
            this.homingTarget = other.homingTarget;
            this.isTracking = other.isTracking;
            this.stopAtTargetPoint = other.stopAtTargetPoint;
            this.fixedTargetPoint = other.fixedTargetPoint;
            this.maxTravelDistance = other.maxTravelDistance;
            this.traveledDistance = other.traveledDistance;
        }
    }
}
