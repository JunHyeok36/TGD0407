using System;

namespace TDG0407.Domain.Entities
{
    using Domain.Cards;
    using Domain.Exceptions;

    /// <summary>
    /// 카드를 사용해 전투 행동을 수행하는 Entity 데이터입니다.
    /// </summary>
    [Serializable]
    public class LifeData : EntityData
    {
        #region Fields

        public CardDeck deck = new();

        #endregion
        #region Constructors

        public LifeData() : base() { }

        #endregion
        #region Methods

        public override void Initialize()
        {
            base.Initialize();
            deck ??= new CardDeck();
        }

        public override void ValidateData()
        {
            base.ValidateData();
            if(deck == null) throw new DataValidityViolationException("Card Deck cannot be null.");
            deck.ValidateData();
        }

        #endregion
    }
}