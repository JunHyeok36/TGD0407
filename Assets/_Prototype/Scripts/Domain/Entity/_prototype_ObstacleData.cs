using System;

namespace TDG0407._prototype
{

    [Serializable]
    public class _prototype_ObstacleData : _prototype_EntityData, _prototype_IInteractable
    {

        public bool isKnockbackImmune = true;
        public bool isDestructible = true;

        public override bool IsSolid => TryGetComponent<_prototype_TrapComponentData>(out var trap) ? trap.isSolidObstacle : true;

        public _prototype_ObstacleData() : base() { }
        public _prototype_ObstacleData(
            string name,
            _prototype_BoundedValue<int> health,
            _prototype_BoundedValue<int> stamina,
            _prototype_Point point,
            bool isKnockbackImmune = true,
            bool isDestructible = true)
            : base(name, health, stamina, point)
        {
            this.isKnockbackImmune = isKnockbackImmune;
            this.isDestructible = isDestructible;
        }

        public _prototype_ObstacleData(_prototype_ObstacleData other)
            : base(other)
        {
            this.isKnockbackImmune = other.isKnockbackImmune;
            this.isDestructible = other.isDestructible;
        }

        public override Cysharp.Threading.Tasks.UniTask<int> TakeDamage(_prototype_DamageContext context)
        {
            if (!isDestructible)
            {
                context.modifiedDamage = 0;
                return Cysharp.Threading.Tasks.UniTask.FromResult(health.Current);
            }
            return base.TakeDamage(context);
        }

        public async Cysharp.Threading.Tasks.UniTask Interact(_prototype_EntityData caster, string interactionKey)
        {
            if (string.Equals(interactionKey, "Break", StringComparison.OrdinalIgnoreCase))
            {
                if (isDestructible)
                {
                    await TakeDamage(new _prototype_DamageContext(caster, this, _prototype_DamageType.Physical, 9999, 9999));
                }
            }
        }

    }

}
