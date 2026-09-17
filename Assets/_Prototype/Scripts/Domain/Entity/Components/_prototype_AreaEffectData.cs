using System;
using System.Collections.Generic;

namespace TDG0407._prototype
{
    [Serializable]
    public abstract class _prototype_AreaEffectData : _prototype_EntityData
    {
        public HashSet<_prototype_Point> areaPoints = new();
        public List<_prototype_EntityAction> onTriggerActions = new();
        
        [NonSerialized]
        public _prototype_EntityData caster;
        
        public _prototype_Side side = _prototype_Side.None;
        public bool triggerOncePerEntity = false;

        public override bool IsSolid => false;

        protected _prototype_AreaEffectData() : base() { }

        protected _prototype_AreaEffectData(
            string name,
            _prototype_BoundedValue<int> health,
            _prototype_BoundedValue<int> stamina,
            _prototype_Point point,
            _prototype_MovementType movementType,
            HashSet<_prototype_Point> areaPoints,
            List<_prototype_EntityAction> onTriggerActions,
            _prototype_EntityData caster,
            _prototype_Side side) : base(name, health, stamina, point, movementType)
        {
            this.areaPoints = areaPoints != null ? new HashSet<_prototype_Point>(areaPoints) : new HashSet<_prototype_Point>();
            this.onTriggerActions = onTriggerActions ?? new List<_prototype_EntityAction>();
            this.caster = caster;
            this.side = side;
        }

        protected _prototype_AreaEffectData(_prototype_AreaEffectData other) : base(other)
        {
            this.areaPoints = new HashSet<_prototype_Point>(other.areaPoints);
            this.onTriggerActions = other.onTriggerActions;
            this.caster = other.caster;
            this.side = other.side;
        }
    }
}
