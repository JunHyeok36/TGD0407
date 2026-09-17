using System;
using System.Collections.Generic;

namespace TDG0407._prototype
{
    [Serializable]
    public class _prototype_ExplosionData : _prototype_AreaEffectData
    {
        public _prototype_ExplosionData() : base() { }

        public _prototype_ExplosionData(
            string name,
            _prototype_BoundedValue<int> health,
            _prototype_BoundedValue<int> stamina,
            _prototype_Point point,
            _prototype_MovementType movementType,
            HashSet<_prototype_Point> areaPoints,
            List<_prototype_EntityAction> onTriggerActions,
            _prototype_EntityData caster,
            _prototype_Side side) : base(name, health, stamina, point, movementType, areaPoints, onTriggerActions, caster, side)
        {
        }

        public _prototype_ExplosionData(_prototype_ExplosionData other) : base(other)
        {
        }
    }
}
