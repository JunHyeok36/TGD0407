using System;
using System.Collections.Generic;
using UnityEngine;

namespace TDG0407._prototype
{

    [CreateAssetMenu(fileName = "_prototype_CardDataModel", menuName = "_Prototype/CardData Model")]
    public class _prototype_CardDataModel : ScriptableObject
    {

        public _prototype_CostValue costValue;
        public _prototype_BoundedValue<byte> coolTicks = new(0, 3);
        [SerializeReference, SubclassSelector] public _prototype_ICastRangeSelector castRange;
        [SerializeReference, SubclassSelector] public _prototype_ITargetRangeSelector targetRange;
        [SerializeReference, SubclassSelector] public List<_prototype_ICardAction> actionList;

    }

}