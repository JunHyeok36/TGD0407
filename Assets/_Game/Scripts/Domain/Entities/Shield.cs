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

        public int durability = 0;
        public readonly TickDuration? duration = new();
        public int? performedEntityInstanceId = null;

        #endregion
        #region Properties

        public bool IsExpired => durability == 0 || (duration != null && duration.Value.IsExpired == true);

        #endregion
        #region Constructors

        public Shield(int durability, TickDurationType durationType, int durationTicks = -1, int? performedEntityInstanceId = null)
        {
            this.durability = durability;
            this.duration = new TickDuration(durationType, durationTicks);
            this.performedEntityInstanceId = performedEntityInstanceId;
        }

        #endregion
        #region Methods


        public void ValidateData()
        {
            
        }

        #endregion
        #region EventHandlers

        public void OnTick()
        {
            if(IsExpired == false)
                duration.Value.OnTick();
        }

        #endregion
    }

}