using System;
using Cysharp.Threading.Tasks;

namespace TDG0407._prototype
{
    public delegate UniTask<_prototype_TickIntent> TickActionPlanner();

    public interface ITickManager
    {
        int CurrentTick { get; }
        bool IsTickProcessing { get; }

        UniTask AdvanceTick(Func<UniTask> playerAction);

        void RegisterPreTick(Func<UniTask> callback);
        void UnregisterPreTick(Func<UniTask> callback);

        void RegisterTick(TickActionPlanner callback);
        void UnregisterTick(TickActionPlanner callback);

        void RegisterPostTick(Func<UniTask> callback);
        void UnregisterPostTick(Func<UniTask> callback);

        void Reset();
    }
}
