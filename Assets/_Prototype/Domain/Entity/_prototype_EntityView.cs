using UnityEngine;

namespace TDG0407._prototype
{ 

    public abstract class _prototype_EntityView : MonoBehaviour
    {

        protected _prototype_EntityData entityData;

        public virtual void Initialize(_prototype_EntityData entityData)
        {
            this.entityData = entityData;
        }

    }

}