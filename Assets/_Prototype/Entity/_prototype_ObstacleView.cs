using System;

namespace TDG0407._prototype
{

    public class _prototype_ObstacleView : _prototype_EntityView
    {

        public _prototype_ObstacleData Data => entityData as _prototype_ObstacleData;

        public override void Initialize(_prototype_EntityData entityData)
        {
            if(entityData == null || entityData is not _prototype_ObstacleData)
                throw new ArgumentException("Invalid entity data for obstacle view initialization.");

            base.Initialize(entityData);
        }

    }

}
