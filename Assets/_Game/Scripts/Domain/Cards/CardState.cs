using System;

namespace TDG0407.Domain.Cards
{

    using Core.Grid;
    using Core.Value;
    using Domain.Exceptions;

    /// <summary>
    /// 카드의 상태 및 판정 데이터를 나타냅니다.
    /// </summary>
    [Serializable]
    public class CardState
    {
        #region Fields
        
        public string cardId = null;
        public int cardInstanceId = -1;
        public CostType costType = CostType.None;
        public float costValue = .0f;
        public CardType cardType = CardType.NULL;
        public readonly Coefficients coefficients = new();
        public byte coolTicks = 0;
        public RangeType rangeType = RangeType.NULL;
        public PointArea rangeValue = new();
        public readonly BoundedValue<byte> reinfocedCount = new(0, 0, 0);

        #endregion
        #region Methods

        public void ValidateData()
        {
            if(string.IsNullOrEmpty(cardId)) throw new DataValidityViolationException("Invalid value assigned to Card ID.");
            if(cardType == CardType.NULL) throw new DataValidityViolationException("Invalid value assigned to Card Type.");
            switch(rangeType)
            {
                case RangeType.Default:
                    if(rangeValue.Count == 0) throw new DataValidityViolationException("At least one Point needs to be assigned in Default Range Type."); 
                    break;
                case RangeType.Polygon:
                    if(rangeValue.Count < 3) throw new DataValidityViolationException("At least three Point needs to be assigned in Polygon Range Type."); 
                    break;
            }
        }

        #endregion
    }

}