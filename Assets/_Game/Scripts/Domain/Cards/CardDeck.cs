using System;
using System.Collections.Generic;
using UnityEngine;

namespace TDG0407.Domain.Cards
{
    using Domain.Exceptions;
    using Domain.Interfaces;

    /// <summary>
    /// 카드 리스트를 관리합니다.
    /// </summary>
    [Serializable]
    public class CardDeck : IDataValidatable, IOnTick
    {
        #region Fields

        [SerializeField] private List<CardState> _allCards = new();

        private readonly List<CardState> _remainedCards = new();
        private CardSlot[] _handCardSlots = null;
        private readonly List<CardState> _discardedCards = new();

        #endregion
        #region Properties

        public IReadOnlyList<CardState> AllCards { get => _allCards; }
        public IReadOnlyList<CardState> RemainedCards { get => _remainedCards; }
        public CardSlot[] HandCardSlots { get => _handCardSlots; }
        public IReadOnlyList<CardState> DiscardedCards { get => _discardedCards; }

        #endregion
        #region Constructors

        public CardDeck(byte cardSlotCount, IEnumerable<CardState> allCards)
        {
            _allCards.AddRange(allCards);
            _handCardSlots = new CardSlot[cardSlotCount];
            for(byte i = 0; i < cardSlotCount; i++)
                _handCardSlots[i].Initialize(i);
        }

        public CardDeck(CardDeck other)
        {
            foreach(var card in other._allCards)
                _allCards.Add(card.Clone());
            foreach(var card in other._remainedCards)
                _remainedCards.Add(card.Clone());
            _handCardSlots = new CardSlot[other._handCardSlots.Length];
            for(byte i = 0; i < other._handCardSlots.Length; i++)
                _handCardSlots[i] = other._handCardSlots[i].Clone();
            foreach(var card in other._discardedCards)
                _discardedCards.Add(card.Clone());
        }

        #endregion
        #region Methods

        public void ValidateData()
        {
            if(_allCards.Count == 0) throw new DataValidityViolationException("At least one card needs to have in All Cards.");
            if(_handCardSlots == null || _handCardSlots.Length == 0) throw new DataValidityViolationException("At least one card slot needs.");
        }

        public CardState PopCardInHand(int index)
        {
            if(index < 0 || _handCardSlots.Length <= index) return null;

            CardState targetCard = _handCardSlots[index].PopCard();
            if(targetCard == null) return null;

            return targetCard;
        }

        public CardDeck Clone()
        {
            return new CardDeck(this);
        }

        #endregion
        #region EventHandlers

        public void OnTick()
        {
            foreach(var cardSlot in _handCardSlots)
                cardSlot.OnTick();
        }

        #endregion
    }

}