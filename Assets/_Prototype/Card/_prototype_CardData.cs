using System;
using System.Collections.Generic;

namespace TDG0407._prototype
{

    [Serializable]
    public class _prototype_CardData
    {

        public _prototype_CostValue costValue;
        public _prototype_BoundedValue<byte> coolTicks = new(0, 3);
        public List<_prototype_ICardAction> actionList;
        public _prototype_IRangeSelector rangeSelector;

        public _prototype_CardData(
            _prototype_CostValue costValue, 
            _prototype_BoundedValue<byte> coolTicks, 
            List<_prototype_ICardAction> actionList, 
            _prototype_IRangeSelector rangeSelector)
        {
            this.costValue = costValue;
            this.coolTicks = coolTicks;
            this.actionList = actionList;
            this.rangeSelector = rangeSelector;
        }

        public _prototype_CardData(_prototype_CardData other)
        {
            this.costValue = other.costValue;
            this.coolTicks = other.coolTicks;
            this.actionList = other.actionList;
            this.rangeSelector = other.rangeSelector;
        }

    }

}