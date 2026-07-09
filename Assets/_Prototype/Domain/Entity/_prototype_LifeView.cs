using System;

namespace TDG0407._prototype
{

    public sealed class _prototype_LifeView : _prototype_EntityView
    {

        public _prototype_LifeData Data => entityData as _prototype_LifeData;


        public override void Initialize(_prototype_EntityData entityData)
        {
            if(entityData == null || entityData is not _prototype_LifeData)
                throw new ArgumentException("Invalid entity data for life view initialization.");

            base.Initialize(entityData);
        }

    }

}