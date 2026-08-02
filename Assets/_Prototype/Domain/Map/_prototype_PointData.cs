using System;
using System.Collections.Generic;

namespace TDG0407._prototype
{
    
    [Serializable]
    public class _prototype_PointData
    {
        
        [ReadOnly] public _prototype_Point point;
        [ReadOnly] public List<_prototype_EntityData> placedEntityDatas;
        public bool isPlaceable = true;
        public bool isHoverable = true;

        public _prototype_PointData() {}

        public _prototype_PointData(_prototype_Point point, bool isPlaceable = true, bool isHoverable = true)
        {
            this.point = point;
            this.placedEntityDatas = new(1);
            this.isPlaceable = isPlaceable;
            this.isHoverable = isHoverable;
        }

        public _prototype_PointData(_prototype_PointData other)
        {
            this.point = other.point;
            this.placedEntityDatas = new(other.placedEntityDatas.Count);
            foreach (var entityData in other.placedEntityDatas)
            {
                if (entityData is _prototype_LifeData lifeData)
                {
                    this.placedEntityDatas.Add(lifeData);
                }
                else if (entityData is _prototype_ObstacleData obstacleData)
                {
                    this.placedEntityDatas.Add(obstacleData);
                }
            }
            this.isPlaceable = other.isPlaceable;
            this.isHoverable = other.isHoverable;
        }

    }

}