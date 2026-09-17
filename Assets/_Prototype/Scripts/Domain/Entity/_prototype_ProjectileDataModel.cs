using UnityEngine;
using System.Collections.Generic;

namespace TDG0407._prototype
{
    [CreateAssetMenu(fileName = "_prototype_ProjectileDataModel", menuName = "_Prototype/ProjectileData Model")]
    public class _prototype_ProjectileDataModel : _prototype_EntityDataModel
    {
        public _prototype_ProjectileData CreateProjectileData(
            _prototype_Side side, _prototype_Point direction, int speed, _prototype_EntityData shooter, List<_prototype_EntityAction> onHitActions, float homingAnglePerStep = 0f, _prototype_EntityData homingTarget = null,
            bool isTracking = false, bool stopAtTargetPoint = false, _prototype_Point fixedTargetPoint = default, int maxTravelDistance = -1)
        {
            var data = new _prototype_ProjectileData(
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
            data.heightBounds = heightBounds;
            return data;
        }

        public _prototype_ProjectileData CreateProjectileData(_prototype_ProjectileData source)
        {
            var pd = new _prototype_ProjectileData(
                ename,
                health.Clone(),
                stamina.Clone(),
                source.point,
                movementType,
                source.direction,
                source.speed,
                source.side,
                source.shooter,
                source.onHitActions,
                source.homingAnglePerStep,
                source.homingTarget,
                source.isTracking,
                source.stopAtTargetPoint,
                source.fixedTargetPoint,
                source.maxTravelDistance)
            {
                currentFloatAngle = source.currentFloatAngle,
                traveledDistance = source.traveledDistance,
                heightBounds = source.heightBounds,
            };
            return pd;
        }

        public override _prototype_EntityData CreateData(_prototype_EntityData source = null)
        {
            if (source is not _prototype_ProjectileData projectileData)
                throw new System.ArgumentException("ProjectileData is required.", nameof(source));

            var data = CreateProjectileData(projectileData);
            PopulateComponents(data);
            return data;
        }
    }
}
