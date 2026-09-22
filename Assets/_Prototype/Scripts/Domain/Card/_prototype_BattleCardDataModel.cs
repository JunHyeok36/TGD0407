using System.Collections.Generic;
using UnityEngine;

namespace TDG0407._prototype
{
    [CreateAssetMenu(fileName = "BattleCard_", menuName = "_Prototype/Battle Card Model")]
    public class _prototype_BattleCardDataModel : _prototype_CardDataModel
    {
        public _prototype_CardType cardType;
        public _prototype_CostValue costValue;
        public _prototype_BoundedValue<byte> coolTicks = new(0, 3);
        [SerializeReference, SubclassSelector] public _prototype_ICastRangeSelector castRange;
        [SerializeReference, SubclassSelector] public _prototype_ITargetRangeSelector targetRange;
        [Tooltip("시전자가 밀리거나 이동할 때 공격 영역 재계산 방식 (FollowCaster: 시전자 추종 및 방향 유지, FixedGround: 바닥 좌표 고정)")]
        public _prototype_TargetAnchorType targetAnchorType = _prototype_TargetAnchorType.FollowCaster;
        [SerializeReference, SubclassSelector] public List<_prototype_EntityAction> actionList;

        public override _prototype_CardData CreateCardData()
        {
            return new _prototype_BattleCardData(
                id,
                cardType,
                costValue,
                coolTicks,
                castRange,
                targetRange,
                actionList,
                targetAnchorType,
                descriptionLocalizationKey
            );
        }
    }
}
