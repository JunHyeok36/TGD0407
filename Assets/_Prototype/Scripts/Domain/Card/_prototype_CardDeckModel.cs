using System.Collections.Generic;
using UnityEngine;

namespace TDG0407._prototype
{
    
    [CreateAssetMenu(fileName = "_prototype_CardDeckModel", menuName = "_Prototype/CardDeck Model")]
    public class _prototype_CardDeckModel : ScriptableObject
    {
        
        public List<_prototype_CardDataModel> allCardDatas = new();
        public List<_prototype_CardDataModel> remainedCardDatas = new();
        public List<_prototype_CardDataModel> handedCardDatas = new();
        public List<_prototype_CardDataModel> discardedCardDatas = new();
        public List<_prototype_CardDataModel> destroyedCardDatas = new();

        public _prototype_CardDeck CreateCardDeck()
        {
            List<_prototype_CardData> allCardDatas = new();
            foreach (var cardData in this.allCardDatas) allCardDatas.Add(cardData.CreateCardData());
            List<_prototype_CardData> remainedCardDatas = new();
            foreach (var cardData in this.remainedCardDatas) remainedCardDatas.Add(cardData.CreateCardData());
            List<_prototype_CardData> handedCardDatas = new();
            foreach (var cardData in this.handedCardDatas) handedCardDatas.Add(cardData.CreateCardData());
            List<_prototype_CardData> discardedCardDatas = new();
            foreach (var cardData in this.discardedCardDatas) discardedCardDatas.Add(cardData.CreateCardData());
            List<_prototype_CardData> destroyedCardDatas = new();
            foreach (var cardData in this.destroyedCardDatas) destroyedCardDatas.Add(cardData.CreateCardData());


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