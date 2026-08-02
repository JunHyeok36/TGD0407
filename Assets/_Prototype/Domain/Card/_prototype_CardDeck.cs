using System;
using System.Collections.Generic;
using UnityEngine;

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

        public void InitializeRuntimeState(bool reshuffle = true)
        {
            if (allCardDatas == null) allCardDatas = new List<_prototype_CardData>();
            if (remainedCardDatas == null) remainedCardDatas = new List<_prototype_CardData>();
            if (handedCardDatas == null) handedCardDatas = new List<_prototype_CardData>();
            if (discardedCardDatas == null) discardedCardDatas = new List<_prototype_CardData>();
            if (destroyedCardDatas == null) destroyedCardDatas = new List<_prototype_CardData>();

            if (reshuffle || remainedCardDatas.Count == 0)
            {
                remainedCardDatas.Clear();
                discardedCardDatas.Clear();
                destroyedCardDatas.Clear();
                handedCardDatas.Clear();
                foreach (var card in allCardDatas)
                {
                    var runtimeCard = card == null ? null : new _prototype_CardData(card);
                    if (runtimeCard != null)
                    {
                        runtimeCard.coolTicks.Current = runtimeCard.coolTicks.Min;
                        remainedCardDatas.Add(runtimeCard);
                    }
                }
                Shuffle(remainedCardDatas);
            }
        }

        public List<_prototype_CardData> Draw(int drawCount)
        {
            List<_prototype_CardData> drawnCards = new();
            if (drawCount <= 0) return drawnCards;

            for (int i = 0; i < drawCount; i++)
            {
                if (remainedCardDatas.Count == 0)
                {
                    if (discardedCardDatas.Count == 0) break;
                    remainedCardDatas.AddRange(discardedCardDatas);
                    discardedCardDatas.Clear();
                    Shuffle(remainedCardDatas);
                }

                _prototype_CardData card = remainedCardDatas[0];
                remainedCardDatas.RemoveAt(0);
                handedCardDatas.Add(card);
                drawnCards.Add(card);
            }

            return drawnCards;
        }

        public bool TryDiscardFromHand(_prototype_CardData card)
        {
            if (card == null) return false;
            if (!handedCardDatas.Remove(card)) return false;

            discardedCardDatas.Add(card);
            return true;
        }

        public void TickCooldowns()
        {
            TickListCooldown(allCardDatas);
            TickListCooldown(remainedCardDatas);
            TickListCooldown(handedCardDatas);
            TickListCooldown(discardedCardDatas);
        }

        private static void TickListCooldown(List<_prototype_CardData> cards)
        {
            if (cards == null) return;
            for (int i = 0; i < cards.Count; i++)
            {
                cards[i]?.TickCooldown();
            }
        }

        private static void Shuffle(List<_prototype_CardData> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int randomIndex = UnityEngine.Random.Range(0, i + 1);
                _prototype_CardData temp = list[i];
                list[i] = list[randomIndex];
                list[randomIndex] = temp;
            }
        }

    }

}