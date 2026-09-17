using System;

namespace TDG0407._prototype
{
    [Serializable]
    public abstract class _prototype_CardData
    {
        public string id;
        public string description;

        [NonSerialized]
        public _prototype_EntityData sourceProvider;

        public _prototype_CardData() { }

        public _prototype_CardData(string id, string description)
        {
            this.id = id;
            this.description = description;
        }

        public _prototype_CardData(_prototype_CardData other)
        {
            if (other == null) return;
            this.id = other.id;
            this.description = other.description;
            this.sourceProvider = other.sourceProvider;
        }

        public virtual bool IsVisible(_prototype_ConditionContext context) => true;

        public abstract _prototype_CardData Clone();
    }
}
