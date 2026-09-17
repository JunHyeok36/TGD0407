using UnityEngine;
using System.Collections.Generic;

namespace TDG0407._prototype
{
    [System.Serializable]
    public class _prototype_CardProviderComponentDataModel : _prototype_EntityComponentDataModel
    {
        public List<_prototype_InteractionCardDataModel> providedCards = new();

        public override _prototype_EntityComponentData CreateComponentData()
        {
            var component = new _prototype_CardProviderComponentData();
            foreach (var cardModel in providedCards)
            {
                if (cardModel != null)
                {
                    component.providedCards.Add(cardModel.CreateCardData());
                }
            }
            return component;
        }
    }
}

