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

        public void InitializeDeck()
        {
            remainedCardDatas.Clear();
            handedCardDatas.Clear();
            discardedCardDatas.Clear();
            
            remainedCardDatas.AddRange(allCardDatas);
            ShuffleRemained();
        }

        public void ShuffleRemained()
        {
            System.Random rnd = new System.Random();
            int n = remainedCardDatas.Count;
            while (n > 1)
            {
                n--;
                int k = rnd.Next(n + 1);
                var value = remainedCardDatas[k];
                remainedCardDatas[k] = remainedCardDatas[n];
                remainedCardDatas[n] = value;
            }
        }

        public void DrawCards(int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (remainedCardDatas.Count == 0)
                {
                    if (discardedCardDatas.Count == 0) return; // No cards left to draw
                    
                    remainedCardDatas.AddRange(discardedCardDatas);
                    discardedCardDatas.Clear();
                    ShuffleRemained();
                }

                if (remainedCardDatas.Count > 0)
                {
                    var drawnCard = remainedCardDatas[0];
                    remainedCardDatas.RemoveAt(0);
                    handedCardDatas.Add(drawnCard);
                }
            }
        }

        public void DiscardHand()
        {
            discardedCardDatas.AddRange(handedCardDatas);
            handedCardDatas.Clear();
        }

    }

}