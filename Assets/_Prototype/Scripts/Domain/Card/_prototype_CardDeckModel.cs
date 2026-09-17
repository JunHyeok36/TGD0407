using System;
using System.Collections.Generic;
using UnityEngine;

namespace TDG0407._prototype
{
    
    [Serializable]
    public class _prototype_CardDeckModel
    {
        
        public List<_prototype_CardDataModel> allCardDatas = new();
        public List<_prototype_CardDataModel> remainedCardDatas = new();
        public List<_prototype_CardDataModel> handedCardDatas = new();
        public List<_prototype_CardDataModel> discardedCardDatas = new();
        public List<_prototype_CardDataModel> destroyedCardDatas = new();

        public _prototype_CardDeck CreateCardDeck()
        {
            List<_prototype_CardData> allCardDatas = new();
            foreach (var cardData in this.allCardDatas) if (cardData != null) allCardDatas.Add(cardData.CreateCardData());
            List<_prototype_CardData> remainedCardDatas = new();
            foreach (var cardData in this.remainedCardDatas) if (cardData != null) remainedCardDatas.Add(cardData.CreateCardData());
            List<_prototype_CardData> handedCardDatas = new();
            foreach (var cardData in this.handedCardDatas) if (cardData != null) handedCardDatas.Add(cardData.CreateCardData());
            List<_prototype_CardData> discardedCardDatas = new();
            foreach (var cardData in this.discardedCardDatas) if (cardData != null) discardedCardDatas.Add(cardData.CreateCardData());
            List<_prototype_CardData> destroyedCardDatas = new();
            foreach (var cardData in this.destroyedCardDatas) if (cardData != null) destroyedCardDatas.Add(cardData.CreateCardData());


            var deck = new _prototype_CardDeck
            {
                allCardDatas = allCardDatas,
                remainedCardDatas = remainedCardDatas,
                handedCardDatas = handedCardDatas,
                discardedCardDatas = discardedCardDatas,
                destroyedCardDatas = destroyedCardDatas
            };
            deck.InitializeDeck();
            return deck;
        }

    }

}