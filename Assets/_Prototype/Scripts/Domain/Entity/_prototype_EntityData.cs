using Cysharp.Threading.Tasks;
using System;

namespace TDG0407._prototype
{

    [Serializable]
    public abstract class _prototype_EntityData
    {
        public string ename = "entity";
        public _prototype_BoundedValue<int> health = new(0, 100);
        public _prototype_BoundedValue<int> stamina = new(0, 10);
        public _prototype_Point point = _prototype_Point.zero;
        public _prototype_MovementType movementType = _prototype_MovementType.Ground;

        [UnityEngine.SerializeReference, SubclassSelector]
        public System.Collections.Generic.List<_prototype_EntityComponentData> components = new();

        public _prototype_EntityData() { }
        public _prototype_EntityData(
            string name,
            _prototype_BoundedValue<int> health,
            _prototype_BoundedValue<int> stamina,
            _prototype_Point point,
            _prototype_MovementType movementType = _prototype_MovementType.Ground)
        {
            this.ename = name;
            this.health = health;
            this.stamina = stamina;
            this.point = point;
            this.movementType = movementType;
        }
        public _prototype_EntityData(_prototype_EntityData other)
        {
            this.ename = other.ename;
            this.health = other.health.Clone();
            this.stamina = other.stamina.Clone();
            this.point = other.point;
            this.movementType = other.movementType;
            if (other.components != null)
            {
                this.components = new System.Collections.Generic.List<_prototype_EntityComponentData>();
                foreach(var c in other.components)
                {
                    if (c != null) this.components.Add(c.Clone());
                }
            }
        }

        public virtual UniTask<int> TakeDamage(_prototype_DamageContext context)
        {
            health.Current -= context.modifiedDamage;
            // Armor
            return UniTask.FromResult(health.Current);
        }
    }

}
