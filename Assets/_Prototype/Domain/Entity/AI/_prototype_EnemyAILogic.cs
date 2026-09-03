using System;
using Cysharp.Threading.Tasks;

namespace TDG0407._prototype
{
    [Serializable]
    public abstract class _prototype_EnemyAILogic
    {
        public virtual UniTask ExecuteAction(_prototype_EntityView entityView) { return UniTask.CompletedTask; }
        public abstract UniTask<_prototype_TickIntent> PlanAction(_prototype_EntityView entityView);
        public abstract _prototype_EnemyAILogic Clone();
    }
}
