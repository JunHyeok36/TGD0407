using System;

namespace TDG0407.Domain.Cards
{

    using Core.Grid;
    using Core.Value;
    using Domain.Exceptions;
    using Domain.Interfaces;

    /// <summary>
    /// 카드에 대한 데이터를 나타냅니다.
    /// </summary>
    [Serializable]
    public class CardData : IDataValidatable
    {
        #region Fields
        
        public string id = null;
        public CostType costType = CostType.None;
        public float costValue = .0f;
        public CardType type = CardType.None;
        public Coefficients coefficients = new();
        public byte coolTicks = 0;
        public RangeType rangeType = RangeType.None;
        public PointArea rangeValue = new();
        public BoundedValue<byte> reinfocedCount = new(0, 0, 0);

        #endregion
        #region Methods

        public void ValidateData()
        {
            if(id == null) throw new DataValidityViolationException("Invalid value assigned to Card ID.");
            if(type == CardType.None) throw new DataValidityViolationException("Invalid value assigned to Card Type.");
            switch(rangeType)
            {
                case RangeType.Default:
                    if(rangeValue.Count == 0) throw new DataValidityViolationException("At least one Point needs to be assigned in Default Range Type."); 
                    break;
                case RangeType.Polygon:
                    if(rangeValue.Count > 2) throw new DataValidityViolationException("At least three Point needs to be assigned in Polygon Range Type."); 
                    break;
            }
        }

        #endregion
    }

}