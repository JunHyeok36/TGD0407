using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace TDG0407._prototype
{
    public enum _prototype_KnockbackDirectionType
    {
        AwayFromSource,
        FixedDirection
    }

    [Serializable]
    public class _prototype_KnockbackCardAction : _prototype_CardAction
    {
        [Header("Knockback Settings")]
        public _prototype_KnockbackDirectionType directionMode = _prototype_KnockbackDirectionType.AwayFromSource;
        public _prototype_Point fixedDirection = new(0, 1);
        public int distance = 1;

        [Header("Collision Effects")]
        [SerializeReference, SubclassSelector] public List<_prototype_CardAction> onCollisionActions = new();

        public override async UniTask ExecuteCardAction(
            _prototype_EntityData source,
            IEnumerable<_prototype_EntityData> targets,
            _prototype_ICardActionParams @params)
        {
            if (distance <= 0) return;

            List<UniTask> knockbackTasks = new();

            foreach (var target in targets)
            {
                if (target == null) continue;
                if (!includeSelf && target == source) continue;

                knockbackTasks.Add(ApplyKnockback(source, target, @params));
            }

            if (knockbackTasks.Count > 0)
            {
                await UniTask.WhenAll(knockbackTasks);
            }
        }

        private async UniTask ApplyKnockback(
            _prototype_EntityData source,
            _prototype_EntityData target,
            _prototype_ICardActionParams @params)
        {
            var targetPointView = _prototype_GridManager.Instance.GetPointView(target.point);
            if (targetPointView == null) return;

            var targetView = targetPointView.PlacedEntityViews.Find(v => v.EntityData == target);
            if (targetView == null) return;

            _prototype_Point dir = GetDirection(source, target);
            if (dir == _prototype_Point.zero) return;

            _prototype_Point currentPoint = target.point;
            _prototype_PointView currentPointView = targetPointView;
            bool collided = false;

            for (int step = 0; step < distance; step++)
            {
                _prototype_Point nextPoint = currentPoint + dir;

                if (!_prototype_GridManager.Instance.IsWithinBounds(nextPoint))
                {
                    collided = true;
                    break;
                }

                var nextPointView = _prototype_GridManager.Instance.GetPointView(nextPoint);
                if (nextPointView == null)
                {
                    collided = true;
                    break;
                }

                // 지형 통과 및 엔티티 점유 가능 여부 검사
                if (!nextPointView.IsTraversable(target.movementType) || !nextPointView.CanPlaceEntity(target))
                {
                    collided = true;
                    break;
                }

                currentPoint = nextPoint;
                currentPointView = nextPointView;
            }

            // 실제 위치 이동이 발생한 경우 이동 애니메이션 적용
            if (currentPointView != targetPointView)
            {
                await _prototype_InteractionManager.MoveEntity(targetView, targetPointView, currentPointView);
            }

            // 충돌이 발생한 경우 연출 및 onCollisionActions 실행
            if (collided)
            {
                Vector3 punchDir = new Vector3(dir.x, 0, dir.y).normalized;
                var shakeTask = targetView.transform.DOShakePosition(0.2f, 0.25f, 10, 90f, false, true).AsyncWaitForCompletion();
                var punchTask = targetView.transform.DOPunchPosition(punchDir * 0.2f, 0.2f, 1, 0).AsyncWaitForCompletion();
                await UniTask.WhenAll(shakeTask.AsUniTask(), punchTask.AsUniTask());

                if (onCollisionActions != null && onCollisionActions.Count > 0)
                {
                    foreach (var action in onCollisionActions)
                    {
                        if (action != null)
                        {
                            await action.ExecuteCardAction(source, new[] { target }, @params);
                        }
                    }
                }
            }
        }

        private _prototype_Point GetDirection(_prototype_EntityData source, _prototype_EntityData target)
        {
            if (directionMode == _prototype_KnockbackDirectionType.FixedDirection)
            {
                int dx = Mathf.Clamp(fixedDirection.x, -1, 1);
                int dy = Mathf.Clamp(fixedDirection.y, -1, 1);
                return new _prototype_Point(dx, dy);
            }
            else // AwayFromSource
            {
                if (source == null) return _prototype_Point.zero;

                int dx = target.point.x - source.point.x;
                int dy = target.point.y - source.point.y;

                if (dx == 0 && dy == 0) return _prototype_Point.zero;

                // 8방향 또는 주요 축 방향으로 정규화 (-1, 0, 1)
                int normX = dx != 0 ? (int)Mathf.Sign(dx) : 0;
                int normY = dy != 0 ? (int)Mathf.Sign(dy) : 0;

                return new _prototype_Point(normX, normY);
            }
        }
    }
}
