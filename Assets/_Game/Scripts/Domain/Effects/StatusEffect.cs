using System;

namespace TDG0407.Domain.Effects
{
    
    /// <summary>
    /// Entity에 적용되는 효과를 나타냅니다.
    /// </summary>
    [Serializable]
    public abstract class StatusEffect
    {
        #region Fields

        public string effectId = null;
        public Coefficients coefficients = new();
        public TickDuration? duration = null;
        public int? performedEntityInstanceId = null;

        #endregion
        #region Properties

        public bool IsExpired => duration != null && duration.Value.IsExpired == true;

        #endregion
        #region Constructors

        public StatusEffect(string effectId, Coefficients coefficients, TickDurationType durationType, int durationTicks = -1, int? performedEntityInstanceId = null)
        {
            this.effectId = effectId;
            this.coefficients = coefficients;
            this.duration = new TickDuration(durationType, durationTicks);
            this.performedEntityInstanceId = performedEntityInstanceId;
        }

        public StatusEffect(StatusEffect other)
        {
            this.effectId = other.effectId;
            this.coefficients = other.coefficients.Clone();
            if(other.duration != null) this.duration = other.duration.Value.Clone();
            this.performedEntityInstanceId = other.performedEntityInstanceId;
        }

        #endregion
        #region Methods

        public abstract StatusEffect Clone();

        #endregion
        
    }

}