using System;
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

        public static UniTask MoveEntity(_prototype_EntityView entityView, _prototype_PointView fromPointView, _prototype_PointView toPointView)
        {
            if (entityView == null) throw new Exception("entityView is null.");
            if (fromPointView == null) throw new Exception("fromPointView is null.");
            if (toPointView == null) throw new Exception("toPointView is null.");
            if (!fromPointView.Point.Equals(entityView.EntityData.point)) throw new Exception($"Entity {entityView.name} is not at the fromPoint {fromPointView.Point}.");
            if (!toPointView.IsEntityPlaceable) throw new Exception($"Cannot move entity to point {toPointView.Point}. Point is not placeable.");

            fromPointView.RemoveEntity(entityView);
            toPointView.PlaceEntity(entityView);
            return UniTask.CompletedTask;
        }

    }

}