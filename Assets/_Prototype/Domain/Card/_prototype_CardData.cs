using System;
using System.Collections.Generic;

namespace TDG0407._prototype
{

    [Serializable]
    public class _prototype_CardData
    {

        public _prototype_CostValue costValue;
        public _prototype_BoundedValue<byte> coolTicks = new(0, 3);
        public _prototype_ICastRangeSelector castRange;
        public _prototype_ITargetRangeSelector targetRange;
        public List<_prototype_ICardAction> actionList;

        public _prototype_CardData(
            _prototype_CostValue costValue, 
            _prototype_BoundedValue<byte> coolTicks, 
            _prototype_ICastRangeSelector castRange,
            _prototype_ITargetRangeSelector targetRange,
            List<_prototype_ICardAction> actionList
            )
        {
            this.costValue = costValue;
            this.coolTicks = coolTicks;
            this.castRange = castRange;
            this.targetRange = targetRange;
            this.actionList = actionList;
        }

        public _prototype_CardData(_prototype_CardData other)
        {
            this.costValue = other.costValue;
            this.coolTicks = other.coolTicks;
            this.castRange = other.castRange;
            this.targetRange = other.targetRange;
            this.actionList = other.actionList;
        }

    }

}