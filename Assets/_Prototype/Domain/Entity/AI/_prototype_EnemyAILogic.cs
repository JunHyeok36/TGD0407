using System;
using Cysharp.Threading.Tasks;

namespace TDG0407._prototype
{
    [Serializable]
    public abstract class _prototype_EnemyAILogic
    {
        public abstract UniTask ExecuteAction(_prototype_EntityView entityView);
        public abstract _prototype_EnemyAILogic Clone();
    }
}
