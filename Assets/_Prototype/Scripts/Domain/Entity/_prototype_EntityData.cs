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
        public _prototype_Point size = new _prototype_Point(1, 1);
        public _prototype_MovementType movementType = _prototype_MovementType.Ground;
        public _prototype_Bounds<float> heightBounds = new(0.0f, 1.8f);

        public virtual bool IsSolid => true;

        [UnityEngine.SerializeReference, SubclassSelector]
        public System.Collections.Generic.List<_prototype_EntityComponentData> components = new();

        public _prototype_EntityData() { }
        public _prototype_EntityData(
            string name,
            _prototype_BoundedValue<int> health,
            _prototype_BoundedValue<int> stamina,
            _prototype_Point point,
            _prototype_MovementType movementType = _prototype_MovementType.Ground,
            _prototype_Bounds<float> heightBounds = default)
        {
            this.ename = name;
            this.health = health;
            this.stamina = stamina;
            this.point = point;
            this.size = new _prototype_Point(1, 1);
            this.movementType = movementType;
            this.heightBounds = (heightBounds.Min == 0f && heightBounds.Max == 0f) ? new _prototype_Bounds<float>(0.0f, 1.8f) : heightBounds;
        }
        public _prototype_EntityData(_prototype_EntityData other)
        {
            this.ename = other.ename;
            this.health = other.health.Clone();
            this.stamina = other.stamina.Clone();
            this.point = other.point;
            this.size = other.size;
            this.movementType = other.movementType;
            this.heightBounds = other.heightBounds;
            if (other.components != null)
            {
                this.components = new System.Collections.Generic.List<_prototype_EntityComponentData>();
                foreach(var c in other.components)
                {
                    if (c != null) this.components.Add(c.Clone());
                }
            }
        }

        public virtual bool IsDead => health.Current <= 0;

        public event Action OnDied;

        public virtual void Die(_prototype_EntityData killer = null)
        {
            health.Current = 0;
            OnDied?.Invoke();
            _prototype_EventBus.Fire(new EntityDiedEvent(this, killer));
        }

        public virtual UniTask<int> TakeDamage(_prototype_DamageContext context)
        {
            health.Current -= context.modifiedDamage;
            if (health.Current <= 0)
            {
                Die(context.source);
            }
            // Armor
            return UniTask.FromResult(health.Current);
        }

        public bool TryGetComponent<T>(out T component) where T : _prototype_EntityComponentData
        {
            if (components != null)
            {
                foreach (var c in components)
                {
                    if (c is T tComp)
                    {
                        component = tComp;
                        return true;
                    }
                }
            }
            component = null;
            return false;
        }

        public T GetComponent<T>() where T : _prototype_EntityComponentData
        {
            TryGetComponent<T>(out var comp);
            return comp;
        }
    }

}
