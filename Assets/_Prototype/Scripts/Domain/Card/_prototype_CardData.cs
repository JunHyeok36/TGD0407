using System;

namespace TDG0407._prototype
{
    [Serializable]
    public abstract class _prototype_CardData
    {
        public string id;
        public string descriptionLocalizationKey;

        [NonSerialized]
        public _prototype_EntityData sourceProvider;

        public _prototype_CardData() { }

        public _prototype_CardData(string id, string descriptionLocalizationKey = "")
        {
            this.id = id;
            this.descriptionLocalizationKey = descriptionLocalizationKey;
        }

        public _prototype_CardData(_prototype_CardData other)
        {
            if (other == null) return;
            this.id = other.id;
            this.descriptionLocalizationKey = other.descriptionLocalizationKey;
            this.sourceProvider = other.sourceProvider;
        }

        public virtual bool IsVisible(_prototype_ConditionContext context) => true;

        public virtual bool IsEmpowered(_prototype_LifeData caster) => false;

        public abstract _prototype_CardData Clone();
    }
}
