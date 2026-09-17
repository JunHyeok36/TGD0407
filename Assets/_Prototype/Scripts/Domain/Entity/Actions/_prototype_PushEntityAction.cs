using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace TDG0407._prototype
{
    [Serializable]
    public class _prototype_PushEntityAction : _prototype_EntityAction
    {
        [Header("Push Settings")]
        [Tooltip("밀쳐낼 거리 (타일 수)")]
        public int distance = 1;

        [Tooltip("스킬 공격 조준 방향(Attack Direction) 사용 여부")]
        public bool useAttackDirection = true;

        [Tooltip("useAttackDirection이 false이거나 방향 정보가 없을 때 사용할 고정 방향")]
        public _prototype_Point fixedDirection = new(0, 1);

        [Header("Collision Effects")]
        [SerializeReference, SubclassSelector] public List<_prototype_EntityAction> onCollisionActions = new();

        public override async UniTask ExecuteAction(
            _prototype_EntityData source,
            IEnumerable<_prototype_EntityData> targets,
            _prototype_IActionParams @params)
        {
            if (distance <= 0 || targets == null) return;

            var validTargets = targets
                .Where(t => t != null && (includeSelf || t != source) && (source == null || !source.IsSameSide(t)) && t.health.Current > 0)
                .ToList();

            if (validTargets.Count == 0) return;

            // 1. 밀쳐낼 기준 방향 도출
            _prototype_Point defaultPushDir = GetDefaultDirection(source, @params);

            // 2. 공격 방향 기준 먼 곳에 위치한 엔티티부터 순차 이동하도록 정렬
            // (동일 선상의 앞선 엔티티가 먼저 이동해야 뒤 엔티티가 경로 방해를 받지 않음)
            var sortedTargets = validTargets
                .OrderByDescending(t => t.point.x * defaultPushDir.x + t.point.y * defaultPushDir.y)
                .ToList();

            List<UniTask> pushTasks = new();
            foreach (var target in sortedTargets)
            {
                _prototype_Point dir = defaultPushDir;
                if (dir == _prototype_Point.zero && source != null)
                {
                    // 방향 정보가 전혀 없는 경우 시전자로부터 멀어지는 방향 폴백
                    int dx = target.point.x - source.point.x;
                    int dy = target.point.y - source.point.y;
                    dir = new _prototype_Point(dx != 0 ? (int)Mathf.Sign(dx) : 0, dy != 0 ? (int)Mathf.Sign(dy) : 0);
                }

                if (dir != _prototype_Point.zero)
                {
                    pushTasks.Add(ApplyPush(source, target, dir, @params));
                }
            }

            if (pushTasks.Count > 0)
            {
                await UniTask.WhenAll(pushTasks);
            }
        }

        public static bool CheckKnockbackResist(_prototype_EntityData target)
        {
            if (target == null) return false;

            // 1. 장애물(Obstacle) 면역 검사
            if (target is _prototype_ObstacleData obstacle && obstacle.isKnockbackImmune)
                return true;

            // 2. 생명체(LifeData) 면역/저항 검사
            if (target is _prototype_LifeData life)
            {
                // SuperArmor 상태이상 보유 시 100% 면역
                if (life.HasStatusEffect(_prototype_StatusType.SuperArmor))
                    return true;

                int resist = life.lifeStat.knockbackResist;
                // 센티널 값: 99999 이상일 경우 100% 면역
                if (resist >= 99999)
                    return true;

                // 음수 및 일반 계산: 0 이하이면 0% (미저항), 양수이면 resist / (resist + 500f)
                if (resist > 0)
                {
                    float prob = (float)resist / (resist + 500f);
                    return UnityEngine.Random.value < prob;
                }
            }

            return false;
        }

        private _prototype_Point GetDefaultDirection(_prototype_EntityData source, _prototype_IActionParams @params)
        {
            if (useAttackDirection && @params is _prototype_CardActionParams cardParams && cardParams.Direction != _prototype_Point.zero)
            {
                return cardParams.Direction;
            }

            if (fixedDirection != _prototype_Point.zero)
            {
                return new _prototype_Point(
                    Mathf.Clamp(fixedDirection.x, -1, 1),
                    Mathf.Clamp(fixedDirection.y, -1, 1)
                );
            }

            return _prototype_Point.zero;
        }

        private async UniTask ApplyPush(
            _prototype_EntityData source,
            _prototype_EntityData target,
            _prototype_Point dir,
            _prototype_IActionParams @params)
        {
            var targetPointView = _prototype_GridManager.Instance.GetPointView(target.point);
            if (targetPointView == null) return;

            var targetView = targetPointView.PlacedEntityViews.Find(v => v.EntityData == target);
            if (targetView == null) return;

            // 넉백 저항 검사
            if (CheckKnockbackResist(target))
            {
                if (target is _prototype_LifeData life)
                {
                    _prototype_FloatingText.SpawnResistText(targetView);
                }
                return;
            }

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

            // 위치 이동 처리 (넉백/밀쳐짐 시 바라보는 방향 유지)
            if (currentPointView != targetPointView)
            {
                await _prototype_InteractionManager.MoveEntity(targetView, targetPointView, currentPointView, rotateTowardsDestination: false);
            }

            // 충돌 처리 (벽이나 장애물에 막힌 경우 연출)
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
                            await action.ExecuteAction(source, new[] { target }, @params);
                        }
                    }
                }
            }
        }
    }
}
