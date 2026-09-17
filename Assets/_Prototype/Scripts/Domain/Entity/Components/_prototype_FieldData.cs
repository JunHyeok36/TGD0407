using System;
using System.Collections.Generic;

namespace TDG0407._prototype
{
    [Serializable]
    public class _prototype_FieldData : _prototype_AreaEffectData
    {
        public int durationTicks;

        public _prototype_FieldData() : base() { }

        public _prototype_FieldData(
            string name,
            _prototype_BoundedValue<int> health,
            _prototype_BoundedValue<int> stamina,
            _prototype_Point point,
            _prototype_MovementType movementType,
            HashSet<_prototype_Point> areaPoints,
            List<_prototype_EntityAction> onTriggerActions,
            _prototype_EntityData caster,
            _prototype_Side side,
            int durationTicks) : base(name, health, stamina, point, movementType, areaPoints, onTriggerActions, caster, side)
        {
            this.durationTicks = durationTicks;
        }

        public _prototype_FieldData(_prototype_FieldData other) : base(other)
        {
            this.durationTicks = other.durationTicks;
        }
    }
}
