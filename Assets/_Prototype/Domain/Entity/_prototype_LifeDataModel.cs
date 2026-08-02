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
                health = health.Clone(),
                stamina = stamina.Clone(),
                point = point,
                cardDeck = cardDeck == null ? new _prototype_CardDeck() : cardDeck.CreateCardDeck()
            };
        }

    }

}
