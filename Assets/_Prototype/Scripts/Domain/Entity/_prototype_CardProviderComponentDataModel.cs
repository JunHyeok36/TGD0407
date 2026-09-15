using UnityEngine;
using System.Collections.Generic;

namespace TDG0407._prototype
{
    [System.Serializable]
    public class _prototype_ProvidedCardWrapper
    {
        public _prototype_CardDataModel cardModel;
        
        [SerializeReference, SubclassSelector]
        public List<_prototype_Condition> visibilityConditions = new();
    }

    [System.Serializable]
    public class _prototype_CardProviderComponentDataModel : _prototype_EntityComponentDataModel
    {
        public List<_prototype_ProvidedCardWrapper> providedCards = new();

        public override _prototype_EntityComponentData CreateComponentData()
        {
            var component = new _prototype_CardProviderComponentData();
            foreach (var wrapper in providedCards)
            {
                if (wrapper != null && wrapper.cardModel != null)
                {
                    var cardData = wrapper.cardModel.CreateCardData();
                    if (wrapper.visibilityConditions != null)
                    {
                        cardData.visibilityConditions = new List<_prototype_Condition>(wrapper.visibilityConditions);
                    }
                    component.providedCards.Add(cardData);
                }
            }
            return component;
        }
    }
}
