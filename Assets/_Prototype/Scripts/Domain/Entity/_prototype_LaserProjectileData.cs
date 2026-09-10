using System;

namespace TDG0407._prototype
{
    [Serializable]
    public class _prototype_LaserProjectileData : _prototype_EntityData
    {
        public int durationTicks = 1;
        public int damage = 10;
        public _prototype_Side side = _prototype_Side.None;

        public _prototype_LaserProjectileData() : base() { }

        public _prototype_LaserProjectileData(
            string name,
            _prototype_BoundedValue<int> health,
            _prototype_BoundedValue<int> stamina,
            _prototype_Point point,
            _prototype_MovementType movementType,
            int durationTicks,
            int damage,
            _prototype_Side side) : base(name, health, stamina, point, movementType)
        {
            this.durationTicks = durationTicks;
            this.damage = damage;
            this.side = side;
        }

        public _prototype_LaserProjectileData(_prototype_LaserProjectileData other) : base(other)
        {
            this.durationTicks = other.durationTicks;
            this.damage = other.damage;
            this.side = other.side;
        }
    }
}
