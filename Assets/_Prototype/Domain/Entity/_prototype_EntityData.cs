using Cysharp.Threading.Tasks;
using System;
using UnityEngine.Localization.SmartFormat.Utilities;

namespace TDG0407._prototype
{

    [Serializable]
    public abstract class _prototype_EntityData
    {
        public string ename = "entity";
        public _prototype_BoundedValue<int> health = new(0, 100);
        public _prototype_BoundedValue<int> stamina = new(0, 10);
        public _prototype_Point point = _prototype_Point.zero;

        public _prototype_EntityData() { }
        public _prototype_EntityData(
            string name,
            _prototype_BoundedValue<int> health,
            _prototype_BoundedValue<int> stamina,
            _prototype_Point point)
        {
            this.ename = name;
            this.health = health;
            this.stamina = stamina;
            this.point = point;
        }
        public _prototype_EntityData(_prototype_EntityData other)
        {
            this.ename = other.ename;
            this.health = other.health.Clone();
            this.stamina = other.stamina.Clone();
            this.point = other.point;
        }

        public virtual UniTask<int> TakeDamage(_prototype_DamageContext context)
        {
            health.Current -= context.modifiedDamage;
            // Armor
            return UniTask.FromResult(health.Current);
        }
    }

}
