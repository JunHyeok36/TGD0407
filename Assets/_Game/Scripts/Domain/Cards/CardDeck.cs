using System;
using System.Collections.Generic;

namespace TDG0407.Domain.Cards
{
    using System.Linq;
    using Domain.Exceptions;
    using Domain.Interfaces;
    using UnityEngine.Localization.SmartFormat.Utilities;

    /// <summary>
    /// 카드 리스트를 관리합니다.
    /// </summary>
    [Serializable]
    public class CardDeck : IDataValidatable, IOnTick
    {
        #region Fields

        private readonly List<CardData> _allCards = new();

        private readonly List<CardData> _remainedCards = new();
        private CardSlot[] _handCardSlots = null;
        private readonly List<CardData> _discardedCards = new();


        #endregion
        #region Properties

        public List<CardData> AllCards { get => _allCards; }
        public List<CardData> RemainedCards { get => _remainedCards; }
        public CardSlot[] HandCardSlots { get => _handCardSlots; }
        public List<CardData> DiscardedCards { get => _discardedCards; }

        #endregion
        #region Constructors

        #endregion
        #region Methods

        public void Initialize(byte cardSlotCount) // TODO : compelete this function with corrective parameters.
        {
            _handCardSlots = new CardSlot[cardSlotCount];
            for(byte i = 0; i < cardSlotCount; i++)
                _handCardSlots[i].Initialize(i);
        }

        public void ValidateData()
        {
            if(_allCards.Count == 0) throw new DataValidityViolationException("At least one card needs to have in All Cards.");
            if(_handCardSlots == null || _handCardSlots.Length == 0) throw new DataValidityViolationException("At least one card slot needs.");
        }

        public void UseCardInHands(int index)
        {
            if(index < 0 || _handCardSlots.Length <= index) return;

            CardData targetCard = _handCardSlots[index].PopCard();
            if(targetCard == null) return;

            // TODO : continue
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