using System;
using System.Collections.Generic;

namespace TDG0407._prototype
{
    [Serializable]
    public class _prototype_CardProviderComponentData : _prototype_EntityComponentData
    {
        [NonSerialized]
        public List<_prototype_CardData> providedCards = new();

        public override _prototype_EntityComponentData Clone()
        {
            var clone = new _prototype_CardProviderComponentData();


            if (providedCards != null)
            {
                foreach (var c in providedCards)
                {
                    if (c != null) clone.providedCards.Add(c.Clone());
                }
            }
            return clone;
        }
    }
}
