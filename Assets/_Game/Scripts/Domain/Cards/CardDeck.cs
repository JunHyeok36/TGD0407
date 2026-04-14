using System;
using System.Collections.Generic;

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

        private readonly List<CardState> _allCards = new();

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