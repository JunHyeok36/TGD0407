using Cysharp.Threading.Tasks;

namespace TDG0407._prototype
{
    public interface _prototype_IChanneledOperation
    {
        string OperationName { get; }
        int TotalTicks { get; }
        int ElapsedTicks { get; }
        bool IsComplete { get; }

        UniTask OnChannelStart(_prototype_EntityView caster);
        UniTask OnChannelTick(_prototype_EntityView caster);
        UniTask OnChannelComplete(_prototype_EntityView caster);
        void OnChannelCancelled(_prototype_EntityView caster, string reason);
    }
}
