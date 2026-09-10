using System;
using System.Collections.Generic;

namespace TDG0407._prototype
{

    [Serializable]
    public class _prototype_CardData
    {

        public string id;
        public string description;
        public _prototype_CardType cardType;
        public _prototype_CostValue costValue;
        public _prototype_BoundedValue<byte> coolTicks = new(0, 3);
        public int currentCoolTicks = 0;
        public _prototype_ICastRangeSelector castRange;
        public _prototype_ITargetRangeSelector targetRange;
        public List<_prototype_EntityAction> actionList;

        public _prototype_CardData(
            string id,
            string description,
            _prototype_CardType cardType,
            _prototype_CostValue costValue, 
            _prototype_BoundedValue<byte> coolTicks, 
            _prototype_ICastRangeSelector castRange,
            _prototype_ITargetRangeSelector targetRange,
            List<_prototype_EntityAction> actionList
            )
        {
            this.id = id;
            this.description = description;
            this.cardType = cardType;
            this.costValue = costValue;
            this.coolTicks = coolTicks;
            this.castRange = castRange;
            this.targetRange = targetRange;
            this.actionList = actionList;
        }

        public _prototype_CardData(_prototype_CardData other)
        {
            this.id = other.id;
            this.description = other.description;
            this.cardType = other.cardType;
            this.costValue = other.costValue;
            this.coolTicks = other.coolTicks;
            this.castRange = other.castRange;
            this.targetRange = other.targetRange;
            this.actionList = other.actionList;
        }

        /// <summary>
        /// 탐색 모드에서 사용 가능한 카드인지 여부를 반환합니다.
        /// Utility, Communication, None 타입의 카드는 탐색 모드에서도 사용 가능합니다.
        /// </summary>
        public bool IsUsableInExploration =>
            cardType == _prototype_CardType.Utility ||
            cardType == _prototype_CardType.Communication ||
            cardType == _prototype_CardType.None;

    }

}
