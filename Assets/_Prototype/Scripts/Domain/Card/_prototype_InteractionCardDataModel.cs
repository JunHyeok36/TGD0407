using System.Collections.Generic;
using UnityEngine;

namespace TDG0407._prototype
{
    [CreateAssetMenu(fileName = "InteractionCard_", menuName = "_Prototype/Interaction Card Model")]
    public class _prototype_InteractionCardDataModel : _prototype_CardDataModel
    {
        [Tooltip("상호작용 식별 키 (예: Disarm, Detonate, TurnOn, TurnOff, Break 등)")]
        public string interactionKey;

        [SerializeReference, SubclassSelector]
        public List<_prototype_Condition> visibilityConditions = new();

        public override _prototype_CardData CreateCardData()
        {
            return new _prototype_InteractionCardData(
                id,
                interactionKey,
                visibilityConditions,
                descriptionLocalizationKey
            );
        }
    }
}
