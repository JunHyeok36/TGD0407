using System;

namespace TDG0407._prototype
{

    [Serializable]
    public class _prototype_ObstacleData : _prototype_EntityData
    {

        public bool isKnockbackImmune = true;

        public _prototype_ObstacleData() : base() { }
        public _prototype_ObstacleData(
            string name,
            _prototype_BoundedValue<int> health,
            _prototype_BoundedValue<int> stamina,
            _prototype_Point point,
            bool isKnockbackImmune = true)
            : base(name, health, stamina, point)
        {
            this.isKnockbackImmune = isKnockbackImmune;
        }

        public _prototype_ObstacleData(_prototype_ObstacleData other)
            : base(other)
        {
            this.isKnockbackImmune = other.isKnockbackImmune;
        }

    }

}
