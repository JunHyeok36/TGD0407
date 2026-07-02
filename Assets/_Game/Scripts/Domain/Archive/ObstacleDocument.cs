using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TDG0407.Domain.Archive
{

    using Core.Grid;
    using Domain.Entities;
    using View.Entities;
    using View.Map;
    
    [CreateAssetMenu(fileName = "ObstacleCollection", menuName = "Archive/Entities/ObstacleDocument" , order = 2)]
    public sealed class ObstacleDocument : EntityDocument
    {
        #region Methods

        public override async UniTask<EntityView> InstantiateEntityView(
            string levelId,
            int entityInstanceId,
            PointView pointView)
        {
            var handle = prefab.InstantiateAsync(pointView.transform);
            var entityViewObject = await handle.Task;
            if(entityViewObject == null)
            {
                Debug.LogError($"Failed to instantiate EntityView prefab for entity ID '{id}'.");
                return null;
            }
            if (!entityViewObject.TryGetComponent<EntityView>(out var entityView))
            {
                Debug.LogError($"EntityView prefab for entity ID '{id}' is missing the EntityView component.");
                return null;
            }
            //entityViewObject.SetActive(false);

            entityView.Initialize(new ObstacleState(
                entityInstanceId: entityInstanceId,
                entityId: this.id,
                position: pointView.Point,
                health: this.health,    
                stamina: this.stamina,
                shields: this.shields.ConvertAll(s => s.Clone())
            ), pointView.State);
            
            return entityView;
        }

        public override EntityState CreateEntityState()
        {
            return new ObstacleState(
                entityInstanceId: -1,
                entityId: this.id,
                position: Point.zero,
                health: this.health,    
                stamina: this.stamina,
                shields: this.shields.ConvertAll(s => s.Clone())
            );
        }

        #endregion
    }

}