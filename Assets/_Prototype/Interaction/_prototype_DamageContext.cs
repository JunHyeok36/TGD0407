namespace TDG0407._prototype
{

    public class _prototype_DamageContext
    {
        
        public _prototype_EntityData source;
        public _prototype_EntityData target;
        public int recordedTick;

        public int baseDamage;
        public int modifiedDamage;

        public bool isCriticalAvailable = true;
        public bool isAvoidable = true;
        public bool isArmorBreakable = false;

        public int? finalDamage; // if finalDamage is null, damage has not been applied yet. 

        public _prototype_DamageContext(_prototype_EntityData source, _prototype_EntityData target, int baseDamage)
        {
            this.source = source;
            this.target = target;
            this.recordedTick = _prototype_TickManager.CurrentTick;

            this.baseDamage = baseDamage;
            this.modifiedDamage = 0;

            this.finalDamage = null;
        }

    }

}
