using System;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TDG0407._prototype
{
    
    public static class _prototype_InteractionManager
    {

        public static async UniTask<bool> Interact(
            _prototype_EntityData source,
            _prototype_InteractableData target)
        {
            if (source == null || target == null)
                return false;

            var playMode = _prototype_PlayModeManager.Instance != null
                ? _prototype_PlayModeManager.Instance.CurrentMode
                : _prototype_PlayMode.Exploration;
            var context = new _prototype_InteractionContext(source, target, playMode);

            if (!target.CanInteractWith(context, out var failureReason))
            {
                Debug.Log($"[Interactable] Interaction blocked: {failureReason}");
                return false;
            }

            if (target.interactions == null)
                return false;

            bool executed = false;
            foreach (var interaction in target.interactions)
            {
                if (interaction == null || !interaction.CanExecute(context, out _))
                    continue;

                await interaction.Execute(source, target);
                executed = true;
            }

            if (!executed)
                return false;

            target.MarkInteracted();
            _prototype_EventBus.Fire(new EntityInteractedEvent(source, target));
            return true;
        }
        
        public static async UniTask ApplyDamage(_prototype_DamageContext context)
        {
            if (context == null || context.target == null) return;
            if (context.target.health.Current <= 0) return;

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

            if (context.source != null && context.target != null && context.modifiedDamage > 0)
            {
                var sourcePointView = _prototype_GridManager.Instance?.GetPointView(context.source.point);
                var targetPointView = _prototype_GridManager.Instance?.GetPointView(context.target.point);
                
                if (sourcePointView != null && targetPointView != null)
                {
                    var sourceView = sourcePointView.PlacedEntityViews.FirstOrDefault(v => v.EntityData == context.source);
                    var targetView = targetPointView.PlacedEntityViews.FirstOrDefault(v => v.EntityData == context.target);

                    if (sourceView != null && targetView != null)
                    {
                        var hitTask = targetView.PlayHitAnimation();

                        if (sourceView is _prototype_LifeView lifeView)
                        {
                            Vector3 dir = (targetView.transform.position - sourceView.transform.position).normalized;
                            await UniTask.WhenAll(lifeView.PlayAttackAnimation(dir), hitTask);
                        }
                        else
                        {
                            await hitTask;
                        }
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

                if (context.target.health.Current <= 0)
                {
                    _prototype_EventBus.Fire(new EntityDiedEvent(context.target, context.source));
                }
            }
        }

        public static async UniTask MoveEntity(_prototype_EntityView entityView, _prototype_PointView fromPointView, _prototype_PointView toPointView)
        {
            if (entityView == null) throw new Exception("entityView is null.");
            if (fromPointView == null) throw new Exception("fromPointView is null.");
            if (toPointView == null) throw new Exception("toPointView is null.");
            if (!fromPointView.Point.Equals(entityView.EntityData.point)) throw new Exception($"Entity {entityView.name} is not at the fromPoint {fromPointView.Point}.");
            if (!toPointView.IsTraversable(entityView.EntityData.movementType)) throw new Exception($"Cannot move entity to point {toPointView.Point}. Terrain is blocked.");
            if (!toPointView.CanPlaceEntity(entityView.EntityData))
                throw new Exception($"Cannot move entity to point {toPointView.Point}. Point is occupied.");

            fromPointView.RemoveEntity(entityView);
            await toPointView.PlaceEntity(entityView);

            _prototype_EventBus.Fire(new EntityMovedEvent(entityView.EntityData, fromPointView.Point, toPointView.Point));

            if (entityView.EntityData is _prototype_LifeData lifeData)
            {
                var projectiles = toPointView.PlacedEntityViews
                    .Where(v => v.EntityData is _prototype_ProjectileData)
                    .ToList();

                foreach (var projView in projectiles)
                {
                    if (projView.EntityData is _prototype_ProjectileData projData)
                    {
                        if (lifeData.side != _prototype_Side.None && lifeData.side == projData.side) continue;

                        if (projView is _prototype_ProjectileView pv)
                        {
                            await pv.HitTarget(lifeData);
                        }
                    }
                }
            }
        }

    }

}