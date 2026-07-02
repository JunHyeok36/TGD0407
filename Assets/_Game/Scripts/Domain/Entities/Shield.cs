using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TDG0407.Domain.Entities
{

    using Domain.Interfaces;

    /// <summary>
    /// 쉴드에 대한 데이터입니다.
    /// </summary>
    [Serializable]
    public class Shield : IDataValidatable, IOnTick
    {
        #region Fields

        public int durability = 0;
        public TickDuration duration = new();
        public int? performedEntityInstanceId = null;

        #endregion
        #region Properties

        public bool IsExpired => durability == 0 || duration.IsExpired == true;

        #endregion
        #region Constructors

        public Shield(int durability, TickDurationType durationType, int durationTicks = -1, int? performedEntityInstanceId = null)
        {
            this.durability = durability;
            this.duration = new TickDuration(durationType, durationTicks);
            this.performedEntityInstanceId = performedEntityInstanceId;
        }

        public Shield(Shield other)
        {
            this.durability = other.durability;
            this.duration = other.duration.Clone();
            this.performedEntityInstanceId = other.performedEntityInstanceId;
        }

        #endregion
        #region Methods

        public void ValidateData()
        {
            
        }

        public Shield Clone()
        {
            return new Shield(this);
        }

        #endregion
        #region EventHandlers

        public void OnTick()
        {
            if(IsExpired == false)
                duration.OnTick();
        }

        #endregion
    }

}