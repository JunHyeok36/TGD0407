using System;

namespace TDG0407._prototype
{
    [Serializable]
    public class _prototype_ProjectileData : _prototype_EntityData
    {
        public _prototype_Point direction = _prototype_Point.zero;
        public int speed = 1; // cells per tick
        public int damage = 10;
        public _prototype_Side side = _prototype_Side.None;

        public _prototype_ProjectileData() : base() { }

        public _prototype_ProjectileData(
            string name,
            _prototype_BoundedValue<int> health,
            _prototype_BoundedValue<int> stamina,
            _prototype_Point point,
            _prototype_MovementType movementType,
            _prototype_Point direction,
            int speed,
            int damage,
            _prototype_Side side) : base(name, health, stamina, point, movementType)
        {
            this.direction = direction;
            this.speed = speed;
            this.damage = damage;
            this.side = side;
        }

        public _prototype_ProjectileData(_prototype_ProjectileData other) : base(other)
        {
            this.direction = other.direction;
            this.speed = other.speed;
            this.damage = other.damage;
            this.side = other.side;
        }
    }
}
