namespace TDG0407._prototype
{

    public class _prototype_DamageContext
    {
        
        public _prototype_EntityData source;
        public _prototype_EntityData target;
        public int recordedTick;

        public _prototype_DamageType damageType;
        public int baseDamage;
        public int modifiedDamage;

        public bool isCriticalAvailable = true;
        public bool isAvoidable = true;
        public bool isArmorBreakable = false;

        public bool isCritical = false;

        public int? finalDamage; // if finalDamage is null, damage has not been applied yet. 

        public _prototype_DamageContext(_prototype_EntityData source, _prototype_EntityData target, _prototype_DamageType damageType, int baseDamage, int modifiedDamage, bool isCritical = false)
        {
            this.source = source;
            this.target = target;
            this.recordedTick = _prototype_TickManager.CurrentTick;

            this.damageType = damageType;
            this.baseDamage = baseDamage;
            this.modifiedDamage = modifiedDamage;
            this.isCritical = isCritical;

            this.finalDamage = null;
        }

    }

}
