using System;

namespace TDG0407.Domain.Entities
{

    using Core.Value;
    using Domain.Exceptions;
    using Domain.Interfaces;

    /// <summary>
    /// 쉴드에 대한 데이터입니다.
    /// </summary>
    [Serializable]
    public class Shield : IDataValidatable, IOnTick
    {
        #region Fields

        public readonly BoundedValue<int> durability = new(0, 0);
        public readonly BoundedValue<int> lastingTicks = new(0, 0);
        public EntityData performer = null;

        #endregion
        #region Properties

        public bool IsAvailable => durability.IsMinimum == false && (lastingTicks != null && lastingTicks.IsMinimum == false);

        #endregion
        #region Constructors

        public Shield(int durability, int lastingTicks = 0, EntityData performer = null)
        {
            Initialize(durability, lastingTicks, performer);
        }

        #endregion
        #region Methods

        public void Initialize(int durability, int lastingTicks = 0, EntityData performer = null)
        {
            this.durability.Current = this.durability.Max = durability;
            this.lastingTicks.Current = this.lastingTicks.Max = lastingTicks;
            this.performer = performer;
        }

        public void ValidateData()
        {
            if(durability.Max < 0) throw new DataValidityViolationException("Invalid value assigned to Shield Max Durability.");
            if(durability.Min != 0) throw new DataValidityViolationException("Invalid value assigned to Shield Min Durability.");
        }

        #endregion
        #region EventHandlers

        public void OnTick()
        {
            if(lastingTicks != null && lastingTicks.IsMinimum == false)
                lastingTicks.Current--;
        }

        #endregion
    }

}