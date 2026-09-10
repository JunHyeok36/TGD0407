using UnityEngine;
using System;

namespace TDG0407._prototype
{
    [CreateAssetMenu(fileName = "New Laser Projectile Data Model", menuName = "TDG0407/Prototype/Laser Projectile Data Model")]
    public class _prototype_LaserProjectileDataModel : _prototype_EntityDataModel
    {
        [Header("Laser Projectile Specific")]
        public int durationTicks = 1;
        public int damage = 10;
        public _prototype_Side side = _prototype_Side.None;

        public _prototype_LaserProjectileData CreateLaserProjectileData()
        {
            var clone = new _prototype_LaserProjectileData(
                ename,
                new _prototype_BoundedValue<int>(0, health.Max, health.Current),
                new _prototype_BoundedValue<int>(0, stamina.Max, stamina.Current),
                new _prototype_Point(0, 0),
                movementType,
                durationTicks,
                damage,
                side
            );
            return clone;
        }
    }
}
