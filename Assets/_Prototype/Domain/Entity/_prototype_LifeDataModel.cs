using System.Collections.Generic;
using UnityEngine;

namespace TDG0407._prototype
{

    [CreateAssetMenu(fileName = "_prototype_LifeDataModel", menuName = "_Prototype/LifeData Model")]
    public class _prototype_LifeDataModel : _prototype_EntityDataModel
    {

        public _prototype_CardDeckModel cardDeck = null;
        [SerializeReference, SubclassSelector] public _prototype_EnemyAILogic aiLogic = new _prototype_MeleeChaseAI();
        
        public _prototype_LifeData CreateLifeData()
        {
            return new()
            {
                ename = ename,
                health = health.Clone(),
                stamina = stamina.Clone(),
                point = point,
                cardDeck = cardDeck?.CreateCardDeck(),
                aiLogic = aiLogic?.Clone()
            };
        }

    }

}
