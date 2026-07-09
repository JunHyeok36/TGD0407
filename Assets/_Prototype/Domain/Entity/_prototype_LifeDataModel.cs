using System.Collections.Generic;
using UnityEngine;

namespace TDG0407._prototype
{

    [CreateAssetMenu(fileName = "_prototype_LifeDataModel", menuName = "_Prototype/LifeData Model")]
    public class _prototype_LifeDataModel : _prototype_EntityDataModel
    {

        public List<_prototype_CardData> allCardDatas;
        public List<_prototype_CardData> remainedCardDatas;
        public List<_prototype_CardData> handedCardDatas;
        public List<_prototype_CardData> discardedCardDatas;
        public List<_prototype_CardData> destroyedCardDatas;

        public _prototype_LifeData CreateLifeData()
        {
            return new()
            {
                ename = ename,
                health = health,
                stamina = stamina,
                position = position,

                allCardDatas = allCardDatas,
                remainedCardDatas = remainedCardDatas,
                handedCardDatas = handedCardDatas,
                discardedCardDatas = discardedCardDatas,
                destroyedCardDatas = destroyedCardDatas
            };
        }

    }

}
