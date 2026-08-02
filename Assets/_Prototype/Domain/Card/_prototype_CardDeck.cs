using System;
using System.Collections.Generic;

namespace TDG0407._prototype
{

    [Serializable]
    public class _prototype_CardDeck
    {
        
        public List<_prototype_CardData> allCardDatas = new();
        public List<_prototype_CardData> remainedCardDatas = new();
        public List<_prototype_CardData> handedCardDatas = new();
        public List<_prototype_CardData> discardedCardDatas = new();
        public List<_prototype_CardData> destroyedCardDatas = new();

        public _prototype_CardDeck() { }

        public _prototype_CardDeck(List<_prototype_CardData> allCardDatas)
        {
            this.allCardDatas = allCardDatas;
            this.remainedCardDatas = new();
            this.handedCardDatas = new();
            this.discardedCardDatas = new();
            this.destroyedCardDatas = new();
        }

        public _prototype_CardDeck(_prototype_CardDeck other)
        {
            this.allCardDatas = new List<_prototype_CardData>();
            foreach (var cardData in other.allCardDatas) this.allCardDatas.Add(new(cardData));
            this.remainedCardDatas = new List<_prototype_CardData>();
            foreach (var cardData in other.remainedCardDatas) this.remainedCardDatas.Add(new(cardData));
            this.handedCardDatas = new List<_prototype_CardData>();
            foreach (var cardData in other.handedCardDatas) this.handedCardDatas.Add(new(cardData));
            this.discardedCardDatas = new List<_prototype_CardData>();
            foreach (var cardData in other.discardedCardDatas) this.discardedCardDatas.Add(new(cardData));
            this.destroyedCardDatas = new List<_prototype_CardData>();
            foreach (var cardData in other.destroyedCardDatas) this.destroyedCardDatas.Add(new(cardData));
        }

    }

}