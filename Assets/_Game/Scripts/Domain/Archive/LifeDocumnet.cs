using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

namespace TDG0407.Domain.Archive
{

    using Core.Grid;
    using Domain.Cards;
    using Domain.Entities;
    using Domain.Effects;
    using View.Entities;
    using View.Map;
    
    [CreateAssetMenu(fileName = "LifeDocument", menuName = "Archive/Entities/LifeDocument" , order = 1)]
    public sealed class LifeDocument : EntityDocument
    {
        #region Fields

        public Stat stat = new();
        public CardDeck deck = null;
        public List<StatusEffect> statusEffects = new();

        #endregion
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

            entityView.Initialize(new LifeState(
                entityInstanceId: entityInstanceId,
                entityId: this.id,
                position: pointView.Point,
                health: this.health,    
                stamina: this.stamina,
                stat: this.stat.Clone(),
                deck: this.deck.Clone(),
                statusEffects: statusEffects.ConvertAll(se => se.Clone()),
                shields: this.shields.ConvertAll(s => s.Clone())
            ), pointView.State);
            
            return entityView;
        }

        public override EntityState CreateEntityState()
        {
            return new LifeState(
                entityInstanceId: -1,
                entityId: this.id,
                position: Point.zero,
                health: this.health,    
                stamina: this.stamina,
                stat: this.stat.Clone(),
                deck: this.deck.Clone(),
                statusEffects: statusEffects.ConvertAll(se => se.Clone()),
                shields: this.shields.ConvertAll(s => s.Clone())
            );
        }

        #endregion
    }

}