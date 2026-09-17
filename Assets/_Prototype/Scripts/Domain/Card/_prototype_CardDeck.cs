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
            foreach (var cardData in other.allCardDatas) if (cardData != null) this.allCardDatas.Add(cardData.Clone());
            this.remainedCardDatas = new List<_prototype_CardData>();
            foreach (var cardData in other.remainedCardDatas) if (cardData != null) this.remainedCardDatas.Add(cardData.Clone());
            this.handedCardDatas = new List<_prototype_CardData>();
            foreach (var cardData in other.handedCardDatas) if (cardData != null) this.handedCardDatas.Add(cardData.Clone());
            this.discardedCardDatas = new List<_prototype_CardData>();
            foreach (var cardData in other.discardedCardDatas) if (cardData != null) this.discardedCardDatas.Add(cardData.Clone());
            this.destroyedCardDatas = new List<_prototype_CardData>();
            foreach (var cardData in other.destroyedCardDatas) if (cardData != null) this.destroyedCardDatas.Add(cardData.Clone());
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

        public void DrawCards(int count, int maxHandSize = 999)
        {
            for (int i = 0; i < count; i++)
            {
                if (handedCardDatas.Count >= maxHandSize) return;

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

                    if (drawnCard != null)
                    {
                        if (drawnCard is _prototype_BattleCardData battleCard)
                        {
                            battleCard.currentCoolTicks = battleCard.coolTicks != null ? battleCard.coolTicks.Current : 0;
                        }
                        handedCardDatas.Add(drawnCard);
                    }
                }
            }
        }

        public void DiscardHand()
        {
            discardedCardDatas.AddRange(handedCardDatas);
            handedCardDatas.Clear();
        }

        public _prototype_CardData DiscardHighestCooldownCard()
        {
            if (handedCardDatas.Count == 0) return null;

            _prototype_CardData worstCard = handedCardDatas[0];
            int maxCool = worstCard is _prototype_BattleCardData wbc ? wbc.currentCoolTicks : 0;
            foreach (var c in handedCardDatas)
            {
                int cCool = c is _prototype_BattleCardData cbc ? cbc.currentCoolTicks : 0;
                if (cCool > maxCool)
                {
                    worstCard = c;
                    maxCool = cCool;
                }
            }
            handedCardDatas.Remove(worstCard);
            discardedCardDatas.Add(worstCard);
            return worstCard;
        }

    }

}