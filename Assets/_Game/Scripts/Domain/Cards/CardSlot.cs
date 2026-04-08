using System;

namespace TDG0407.Domain.Cards
{
    
    using Core.Value;
    using Domain.Interfaces;

    /// <summary>
    /// 카드를 관리하는 슬롯을 나타냅니다.
    /// </summary>
    [Serializable]
    public class CardSlot : IOnTick
    {
        #region Fields

        private byte _index = 0;
        private CardData _placedCard = null;
        private BoundedValue<int> _remainingTicks = null;

        #endregion
        #region Properties

        public byte Index { get => _index; }
        public CardData PlacedCard { get => _placedCard; }
        public BoundedValue<int> RemainingTicks { get => _remainingTicks; }

        public bool IsAvailable => _remainingTicks.IsMinimum; 

        #endregion
        #region Methods

        public void Initialize(byte index) 
        {
            _index = index;
            _placedCard = null;
            _remainingTicks = null;
        }
        public void RegisterCard(CardData cardData)
        {
            _placedCard = cardData;
            _remainingTicks = new(0, cardData.coolTicks);
        }
        public CardData PopCard()
        {
            if(IsAvailable == false) return null;

            CardData placedCard = _placedCard;
            Initialize(_index);
            return placedCard;
        }

        #endregion
        #region EventHandlers

        public void OnTick() => _remainingTicks.Current--;

        #endregion
    }

}