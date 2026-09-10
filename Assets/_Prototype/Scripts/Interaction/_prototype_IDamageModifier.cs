using Cysharp.Threading.Tasks;

namespace TDG0407._prototype
{
    
    public interface _prototype_IDamageModifier
    {
        
        int ExecutionOrder { get; }

        UniTask OnAttack(_prototype_DamageContext context);
        UniTask OnDefend(_prototype_DamageContext context);

    }

}