using System.Collections.Generic;
using UnityEngine;

namespace TDG0407._prototype
{

    [CreateAssetMenu(fileName = "_prototype_LifeDataModel", menuName = "_Prototype/LifeData Model")]
    public class _prototype_LifeDataModel : _prototype_EntityDataModel
    {

        public _prototype_CardDeckModel cardDeck = null;
        
        public _prototype_LifeData CreateLifeData()
        {
            return new()
            {
                ename = ename,
                health = health,
                stamina = stamina,
                point = point,
                cardDeck = cardDeck.CreateCardDeck()
            };
        }

    }

}
