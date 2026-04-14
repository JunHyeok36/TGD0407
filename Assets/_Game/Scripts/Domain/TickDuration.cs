using System;

namespace TDG0407.Domain
{
    
    using Core.Value;
    using Domain.Interfaces;

    /// <summary>
    /// 효과나 카드의 지속 시간을 나타냅니다.
    /// </summary>
    [Serializable]
    public struct TickDuration : IOnTick
    {
        #region Fields

        private TickDurationType _tickDurationType;
        private readonly BoundedValue<int> _value;

        #endregion
        #region Properties

        public readonly TickDurationType TickDurationType { get => _tickDurationType; }
        public readonly BoundedValue<int> Value { get => _value; }
        public readonly bool IsExpired => _value != null && _value.IsMinimum == true;

        #endregion
        #region Constructors

        public TickDuration(TickDurationType tickDurationType, int ticks = -1)
        {
            _tickDurationType = tickDurationType;
            if(tickDurationType == TickDurationType.Forever ) _value = null;
            else _value = new BoundedValue<int>(0, ticks);
        }

        #endregion
        #region Methods



        #endregion
        #region EventHandlers

        public void OnTick()
        {
            if(IsExpired == false)
                _value.Current--;
        }

        #endregion
        #region Operators

        public static implicit operator BoundedValue<int>(TickDuration tickDuration) => tickDuration._value;

        #endregion
    }

}