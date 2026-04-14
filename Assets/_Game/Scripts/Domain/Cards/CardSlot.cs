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
        private CardState _placedCard = null;
        private readonly BoundedValue<int> _remainingTicks = new(0, 0);

        #endregion
        #region Properties

        public byte Index { get => _index; }
        public CardState PlacedCard { get => _placedCard; }
        public BoundedValue<int> RemainingTicks { get => _remainingTicks; }

        public bool IsAvailable => _remainingTicks == null || _remainingTicks.IsMinimum; 

        #endregion
        #region Methods

        public void Initialize(byte index) 
        {
            _index = index;
            _placedCard = null;
        }
        public void RegisterCard(CardState CardState)
        {
            _placedCard = CardState;
            _remainingTicks.Initalize(0, CardState.coolTicks);
        }
        public CardState PopCard()
        {
            if(IsAvailable == false) return null;

            CardState placedCard = _placedCard;
            Initialize(_index);
            return placedCard;
        }

        #endregion
        #region EventHandlers

        public void OnTick()
        {
            if(_remainingTicks != null && _remainingTicks.IsMinimum == false)
                _remainingTicks.Current--;
        }

        #endregion
    }

}