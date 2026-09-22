using UnityEngine;

namespace TDG0407._prototype
{
    public abstract class _prototype_CardDataModel : ScriptableObject
    {
        public string id;
        [Tooltip("Unity Localization 테이블 키 (예: CARD_BASIC_ATTACK_DESC)")]
        public string descriptionLocalizationKey;

        public abstract _prototype_CardData CreateCardData();
    }
}