using System;
using System.Collections.Generic;

namespace TDG0407._prototype
{
    [Serializable]
    public class _prototype_BattleCardData : _prototype_CardData
    {
        public _prototype_CardType cardType;
        public _prototype_CostValue costValue;
        public _prototype_BoundedValue<byte> coolTicks = new(0, 3);
        public int currentCoolTicks = 0;
        public _prototype_ICastRangeSelector castRange;
        public _prototype_ITargetRangeSelector targetRange;
        public _prototype_TargetAnchorType targetAnchorType = _prototype_TargetAnchorType.FollowCaster;
        public List<_prototype_EntityAction> actionList;

        public _prototype_BattleCardData() : base() { }

        public _prototype_BattleCardData(
            string id,
            string description,
            _prototype_CardType cardType,
            _prototype_CostValue costValue,
            _prototype_BoundedValue<byte> coolTicks,
            _prototype_ICastRangeSelector castRange,
            _prototype_ITargetRangeSelector targetRange,
            List<_prototype_EntityAction> actionList,
            _prototype_TargetAnchorType targetAnchorType = _prototype_TargetAnchorType.FollowCaster
        ) : base(id, description)
        {
            this.cardType = cardType;
            this.costValue = costValue;
            this.coolTicks = coolTicks;
            this.castRange = castRange;
            this.targetRange = targetRange;
            this.actionList = actionList;
            this.targetAnchorType = targetAnchorType;
        }

        public _prototype_BattleCardData(_prototype_BattleCardData other) : base(other)
        {
            if (other == null) return;
            this.cardType = other.cardType;
            this.costValue = other.costValue;
            this.coolTicks = other.coolTicks;
            this.currentCoolTicks = other.currentCoolTicks;
            this.castRange = other.castRange;
            this.targetRange = other.targetRange;
            this.actionList = other.actionList;
            this.targetAnchorType = other.targetAnchorType;
        }

        public override _prototype_CardData Clone()
        {
            return new _prototype_BattleCardData(this);
        }
    }
}
