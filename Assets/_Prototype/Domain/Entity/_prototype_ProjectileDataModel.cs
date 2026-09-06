using UnityEngine;
using System.Collections.Generic;

namespace TDG0407._prototype
{
    [CreateAssetMenu(fileName = "_prototype_ProjectileDataModel", menuName = "_Prototype/ProjectileData Model")]
    public class _prototype_ProjectileDataModel : _prototype_EntityDataModel
    {
        public _prototype_ProjectileData CreateProjectileData(
            _prototype_Side side, _prototype_Point direction, int speed, _prototype_EntityData shooter, List<_prototype_CardAction> onHitActions, float homingAnglePerStep = 0f, _prototype_EntityData homingTarget = null,
            bool isTracking = false, bool stopAtTargetPoint = false, _prototype_Point fixedTargetPoint = default, int maxTravelDistance = -1)
        {
            return new _prototype_ProjectileData(
                ename,
                new _prototype_BoundedValue<int>(health.Min, health.Max, health.Current),
                new _prototype_BoundedValue<int>(stamina.Min, stamina.Max, stamina.Current),
                point,
                this.movementType,
                direction,
                speed,
                side,
                shooter,
                onHitActions,
                homingAnglePerStep,
                homingTarget,
                isTracking,
                stopAtTargetPoint,
                fixedTargetPoint,
                maxTravelDistance
            );
        }
    }
}
