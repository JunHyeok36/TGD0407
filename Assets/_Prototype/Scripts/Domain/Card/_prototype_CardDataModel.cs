using UnityEngine;

namespace TDG0407._prototype
{
    public abstract class _prototype_CardDataModel : ScriptableObject
    {
        public string id;
        [TextArea(3, 5)] public string description;

        public abstract _prototype_CardData CreateCardData();
    }
}