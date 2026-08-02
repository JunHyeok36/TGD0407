using System;
using System.Collections.Generic;
using UnityEngine;

namespace TDG0407._prototype
{

    [Serializable]
    public class _prototype_CardData
    {
        public string id;
        public _prototype_CardType cardType;
        public _prototype_CostValue costValue;
        public _prototype_BoundedValue<byte> coolTicks = new(0, 3, 0);
        public _prototype_ICastRangeSelector castRange;
        public _prototype_ITargetRangeSelector targetRange;
        public List<_prototype_ICardAction> actionList;

        public _prototype_CardData(
            string id,
            _prototype_CardType cardType,
            _prototype_CostValue costValue, 
            _prototype_BoundedValue<byte> coolTicks, 
            _prototype_ICastRangeSelector castRange,
            _prototype_ITargetRangeSelector targetRange,
            List<_prototype_ICardAction> actionList
            )
        {
            this.id = id;
            this.cardType = cardType;
            this.costValue = costValue;
            this.coolTicks = coolTicks == null ? new _prototype_BoundedValue<byte>(0, 3, 0) : coolTicks.Clone();
            this.castRange = castRange;
            this.targetRange = targetRange;
            this.actionList = actionList == null ? new List<_prototype_ICardAction>() : new List<_prototype_ICardAction>(actionList);
        }

        public _prototype_CardData(_prototype_CardData other)
        {
            this.id = other.id;
            this.cardType = other.cardType;
            this.costValue = other.costValue == null
                ? new _prototype_CostValue(_prototype_CostType.None, 0f)
                : new _prototype_CostValue(other.costValue.costType, other.costValue.value);
            this.coolTicks = other.coolTicks == null
                ? new _prototype_BoundedValue<byte>(0, 3, 0)
                : other.coolTicks.Clone();
            this.castRange = other.castRange;
            this.targetRange = other.targetRange;
            this.actionList = other.actionList == null ? new List<_prototype_ICardAction>() : new List<_prototype_ICardAction>(other.actionList);
        }

        public bool IsCoolingDown => coolTicks != null && coolTicks.Current > 0;

        public bool IsCastableAt(_prototype_Point casterPoint, _prototype_Point castPoint)
        {
            if (castRange == null) return false;
            List<_prototype_Point> validCastPoints = castRange.GetValidCastPoints(casterPoint);
            return validCastPoints != null && validCastPoints.Contains(castPoint);
        }

        public void SetOnCooldown()
        {
            if (coolTicks == null) return;
            coolTicks.Current = coolTicks.Max;
        }

        public void TickCooldown()
        {
            if (coolTicks == null || coolTicks.Current <= 0) return;
            coolTicks.Current = (byte)Mathf.Max(coolTicks.Min, coolTicks.Current - 1);
        }

    }

}