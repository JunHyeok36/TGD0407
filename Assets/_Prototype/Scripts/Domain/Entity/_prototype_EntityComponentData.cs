using System;
using Cysharp.Threading.Tasks;

namespace TDG0407._prototype
{
    [Serializable]
    public abstract class _prototype_EntityComponentData
    {
        public virtual void Initialize(_prototype_EntityView owner) { }
        public virtual UniTask OnTick(_prototype_EntityView owner) { return UniTask.CompletedTask; }
        
        public abstract _prototype_EntityComponentData Clone();
    }
}
