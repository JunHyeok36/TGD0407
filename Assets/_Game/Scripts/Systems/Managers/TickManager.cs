using System.Numerics;

namespace TDG0407.Systems.Managers
{

    using Systems.Signals;

    /// <summary>
    /// 게임 내에서 Tick 단위로 발생하는 이벤트를 관리하는 매니저입니다.
    /// </summary>
    public static class TickManager
    {
        #region Fields

        private static BigInteger tickCount = 0;

        #endregion
        #region Properties

        public static ref BigInteger TickCount => ref tickCount;

        #endregion
        #region Methods

        public static void Initialize()
        {
            tickCount = 0;
        }

        public static void Tick()
        { 
            tickCount++;
            SignalHub.Publish<TickSignal>(new());
        }

        #endregion
    }

}