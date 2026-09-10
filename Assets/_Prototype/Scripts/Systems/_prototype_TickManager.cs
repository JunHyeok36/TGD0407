using Cysharp.Threading.Tasks;
using System;

namespace TDG0407._prototype
{
    /// <summary>
    /// Static Facade for ITickManager, maintaining full backward compatibility
    /// while delegating to the instantiated _prototype_TickManagerService.
    /// </summary>
    public static class _prototype_TickManager
    {
        public static ITickManager Instance => _prototype_TickManagerService.Instance;

        public static int CurrentTick => Instance.CurrentTick;
        public static bool IsTickProcessing => Instance.IsTickProcessing;

        public static UniTask AdvanceTick(Func<UniTask> playerAction) => Instance.AdvanceTick(playerAction);

        public static void RegisterPreTick(Func<UniTask> callback) => Instance.RegisterPreTick(callback);
        public static void UnregisterPreTick(Func<UniTask> callback) => Instance.UnregisterPreTick(callback);

        public static void RegisterTick(TickActionPlanner callback) => Instance.RegisterTick(callback);
        public static void UnregisterTick(TickActionPlanner callback) => Instance.UnregisterTick(callback);

        public static void RegisterPostTick(Func<UniTask> callback) => Instance.RegisterPostTick(callback);
        public static void UnregisterPostTick(Func<UniTask> callback) => Instance.UnregisterPostTick(callback);

        public static void Initialize() => Instance.Reset();
    }
}
