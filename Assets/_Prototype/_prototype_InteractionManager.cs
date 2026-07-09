using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace TDG0407._prototype
{
    
    public static class _prototype_InteractionManager
    {
        
        public static async UniTask ApplyDamage(_prototype_DamageContext context)
        {
            List<(_prototype_EntityData, _prototype_IDamageModifier)> modifiers = new();
            //if (context.source != null) modifiers.AddRange((context.source, context.source.GetDamageModifiers()));
            //if (context.target != null) modifiers.AddRange((context.target, context.target.GetDamageModifiers()));

            var sortedModifiers = modifiers.OrderBy(m => m.Item2.ExecutionOrder);

            foreach (var modifier in sortedModifiers)
            {
                if (context.source == modifier.Item1)
                    await modifier.Item2.OnAttack(context);
                else if (context.target == modifier.Item1)
                    await modifier.Item2.OnDefend(context);
            }

            if (context.modifiedDamage < 0)
                context.modifiedDamage = 0;

            context.finalDamage = await context.target.TakeDamage(context);
        }

    }

}