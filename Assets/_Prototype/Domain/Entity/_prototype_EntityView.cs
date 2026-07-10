using UnityEngine;

namespace TDG0407._prototype
{ 

    public abstract class _prototype_EntityView : MonoBehaviour
    {

        [Header("References")]
        [SerializeField] private _prototype_EntityDataModel entityDataModel;
        
        
        protected _prototype_EntityData entityData;

        public _prototype_Point Point { get { return entityData.point; } }

        public virtual void Initialize()
        {
            if (entityDataModel == null) throw new System.Exception("Entity Data Model is not assigned.");

            if (entityDataModel is _prototype_LifeDataModel lifeDataModel)
            {
                entityData = lifeDataModel.CreateLifeData();
            }
            else if (entityDataModel is _prototype_ObstacleDataModel obstacleDataModel)
            {
                entityData = obstacleDataModel.CreateObstacleData();
            }
            else
            {
                throw new System.Exception("Unsupported Entity Data Model type.");
            }
        }

        public virtual void MoveTo(_prototype_PointView targetPointView)
        {
            transform.SetParent(targetPointView.transform, false);
            transform.localPosition = Vector3.zero;
            
            entityData.point = targetPointView.Point;
        }

    }

}