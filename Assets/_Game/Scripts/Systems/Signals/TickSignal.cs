using System.Numerics;
using TDG0407.Systems.Managers;

namespace TDG0407.Systems.Signals
{

    /// <summary>
    /// 틱이 증가했음을 알리는 시그널입니다.
    /// </summary>
    public readonly struct TickSignal
    {
        #region Static Fields

        public static ref BigInteger TickCount => ref TickManager.TickCount;

        #endregion
    }

}
