using System.Collections.Generic;
using UnityEngine;

namespace TDG0407._prototype
{

    [CreateAssetMenu(fileName = "_prototype_LifeDataModel", menuName = "_Prototype/LifeData Model")]
    public class _prototype_LifeDataModel : _prototype_EntityDataModel
    {

        public _prototype_LifeStat lifeStat = new();
        public _prototype_CardDeckModel cardDeck = null;
        public _prototype_InventoryDataModel inventoryModel = null;
        [SerializeReference, SubclassSelector] public _prototype_EnemyAILogic aiLogic = new _prototype_MeleeChaseAI();
        
        public _prototype_LifeData CreateLifeData()
        {
            return new()
            {
                ename = ename,
                health = health.Clone(),
                stamina = stamina.Clone(),
                point = point,
                size = size,
                side = _prototype_Side.None,
                lifeStat = new _prototype_LifeStat(lifeStat),
                cardDeck = cardDeck != null ? cardDeck.CreateCardDeck() : new(),
                inventory = inventoryModel != null ? inventoryModel.CreateInventoryData() : null,
                aiLogic = aiLogic?.Clone(),
                statusEffects = new(),
                movementType = movementType,
                heightBounds = heightBounds,
            };
        }


        public override _prototype_EntityData CreateData(_prototype_EntityData source = null)
        {
            var data = CreateLifeData();
            PopulateComponents(data);
            return data;
        }

    }

}
