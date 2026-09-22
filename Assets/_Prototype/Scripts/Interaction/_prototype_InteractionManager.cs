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
            if (context == null || context.target == null) return;
            if (context.target.IsDead) return;
            if (context.source != null && context.source != context.target && context.source.IsSameSide(context.target)) return;

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

            if (context.target != null && context.modifiedDamage > 0)
            {
                var targetPointView = _prototype_GridManager.Instance?.GetPointView(context.target.point);
                if (targetPointView != null)
                {
                    var targetView = targetPointView.PlacedEntityViews.FirstOrDefault(v => v.EntityData == context.target);
                    if (targetView != null)
                    {
                        await targetView.PlayHitAnimation();
                    }
                }
            }

            if (context.modifiedDamage > 0)
            {
                _prototype_EventBus.Fire(new EntityDamagedEvent(
                    context.target,
                    context.source,
                    context.modifiedDamage,
                    context.damageType,
                    context.target.point,
                    context
                ));

                if (context.target.IsDead && context.target is not _prototype_LifeData)
                {
                    _prototype_EventBus.Fire(new EntityDiedEvent(context.target, context.source));
                }

                if (context.target.TryGetComponent<_prototype_TrapComponentData>(out var trap) && trap.triggerOnDamage && !trap.isDisarmed)
                {
                    await trap.TriggerTrap(context.source);
                }
            }
        }

        public static async UniTask MoveEntity(
            _prototype_EntityView entityView, 
            _prototype_PointView fromPointView, 
            _prototype_PointView toPointView, 
            bool rotateTowardsDestination = true)
        {
            if (entityView == null) throw new Exception("entityView is null.");
            if (fromPointView == null) throw new Exception("fromPointView is null.");
            if (toPointView == null) throw new Exception("toPointView is null.");
            if (!fromPointView.Point.Equals(entityView.EntityData.point)) throw new Exception($"Entity {entityView.name} is not at the fromPoint {fromPointView.Point}.");
            if (!toPointView.IsTraversable(entityView.EntityData.movementType)) throw new Exception($"Cannot move entity to point {toPointView.Point}. Terrain is blocked.");
            if (!toPointView.CanPlaceEntity(entityView.EntityData))
                throw new Exception($"Cannot move entity to point {toPointView.Point}. Point is occupied.");

            fromPointView.RemoveEntity(entityView);
            await toPointView.PlaceEntity(entityView, animate: true, rotateTowardsDestination: rotateTowardsDestination);

            _prototype_EventBus.Fire(new EntityMovedEvent(entityView.EntityData, fromPointView.Point, toPointView.Point));

            if (entityView.EntityData is _prototype_LifeData lifeData)
            {
                var traps = new List<_prototype_TrapComponentData>();
                foreach (var v in toPointView.PlacedEntityViews)
                {
                    if (v != null && v.EntityData != null && v.EntityData.TryGetComponent<_prototype_TrapComponentData>(out var t))
                    {
                        if (!t.isDisarmed && t.triggerOnStep)
                        {
                            if (lifeData.heightBounds.Overlaps(v.EntityData.heightBounds))
                            {
                                traps.Add(t);
                            }
                        }
                    }
                }

                foreach (var trap in traps)
                {
                    await trap.TriggerTrap(lifeData);
                }

                var projectiles = toPointView.PlacedEntityViews
                    .Where(v => v.EntityData is _prototype_ProjectileData)
                    .ToList();

                foreach (var projView in projectiles)
                {
                    if (projView.EntityData is _prototype_ProjectileData projData)
                    {
                        if (lifeData.side != _prototype_Side.None && lifeData.side == projData.side) continue;
                        if (!lifeData.heightBounds.Overlaps(projData.heightBounds)) continue;

                        if (projView is _prototype_ProjectileView pv)
                        {
                            await pv.HitTarget(lifeData);
                        }
                    }
                }

                var areaEffects = toPointView.PlacedEntityViews
                    .OfType<_prototype_AreaEffectView>()
                    .ToList();

                foreach (var aev in areaEffects)
                {
                    if (aev != null && aev.EntityData != null && lifeData.heightBounds.Overlaps(aev.EntityData.heightBounds))
                    {
                        await aev.OnEntityEntered(lifeData);
                    }
                }
            }
        }

    }

}