using System;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

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

            if (context.source != null && context.target != null && context.modifiedDamage > 0)
            {
                var sourcePointView = _prototype_GridManager.Instance.GetPointView(context.source.point);
                var targetPointView = _prototype_GridManager.Instance.GetPointView(context.target.point);
                
                if (sourcePointView != null && targetPointView != null)
                {
                    var sourceView = sourcePointView.PlacedEntityViews.FirstOrDefault(v => v.EntityData == context.source);
                    var targetView = targetPointView.PlacedEntityViews.FirstOrDefault(v => v.EntityData == context.target);

                    if (sourceView != null && targetView != null)
                    {
                        Vector3 dir = (targetView.transform.position - sourceView.transform.position).normalized;
                        // 약간 튀어오르듯(Bump) 펀치 애니메이션 재생 (공격 모션)
                        var attackAnim = sourceView.transform.DOPunchPosition(dir * 0.3f, 0.2f, 1, 0).AsyncWaitForCompletion();
                        
                        // 타겟도 맞은 흔들림(Shake) (피격 모션)
                        var hitAnim = targetView.transform.DOShakePosition(0.2f, 0.2f, 10, 90, false, true).AsyncWaitForCompletion();
                        
                        // 둘 다 완전히 끝날 때까지 대기
                        await UniTask.WhenAll(attackAnim.AsUniTask(), hitAnim.AsUniTask());
                    }
                }
            }

            if (context.modifiedDamage > 0 && !(context.target is _prototype_ObstacleData))
            {
                var pointView = _prototype_GridManager.Instance.GetPointView(context.target.point);
                if (pointView != null)
                {
                    GameObject prefab = Resources.Load<GameObject>("DamageTextPrefab");
                    if (prefab != null)
                    {
                        GameObject dmgObj = UnityEngine.Object.Instantiate(prefab);
                        dmgObj.transform.position = pointView.transform.position + Vector3.up * 1.5f;
                        var dmgText = dmgObj.GetComponent<_prototype_DamageText>();
                        if (dmgText == null) dmgText = dmgObj.AddComponent<_prototype_DamageText>();
                        dmgText.Initialize(context.modifiedDamage, Color.red);
                    }
                    else
                    {
                        Debug.LogError("DamageTextPrefab not found in Resources!");
                    }
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
            if (entityView.EntityData.movementType == _prototype_MovementType.Ground && toPointView.PlacedEntityViews.Count > 0)
                throw new Exception($"Cannot move Ground entity to point {toPointView.Point}. Point is occupied.");

            fromPointView.RemoveEntity(entityView);
            await toPointView.PlaceEntity(entityView);
        }

    }

}