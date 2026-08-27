using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TDG0407._prototype
{
    [Serializable]
    public class _prototype_MeleeChaseAI : _prototype_EnemyAILogic
    {
        public int attackDamage = 10;
        
        [Header("Idle Settings")]
        public int minIdleTicks = 0;
        public int maxIdleTicks = 1;

        private int _currentIdleTicks = 0;

        public override _prototype_EnemyAILogic Clone()
        {
            return new _prototype_MeleeChaseAI
            {
                attackDamage = this.attackDamage,
                minIdleTicks = this.minIdleTicks,
                maxIdleTicks = this.maxIdleTicks,
                _currentIdleTicks = 0
            };
        }

        public override async UniTask ExecuteAction(_prototype_EntityView entityView)
        {
            if (_currentIdleTicks > 0)
            {
                _currentIdleTicks--;
                return; // 이번 턴은 쉬기
            }

            var playerView = _prototype_PlayerController.Instance.ControlledEntityView;
            if (playerView == null || playerView.EntityData.health.Current <= 0) return;

            _prototype_Point playerCurrentPoint = playerView.Point;
            _prototype_Point targetPoint = _prototype_PlayerController.Instance.ControlledEntityLastPoint;
            _prototype_Point myPoint = entityView.Point;

            List<_prototype_PointView> path = _prototype_GridManager.Instance.FindPath(myPoint, targetPoint, false, true);
            
            bool hasActed = false;

            if (path != null && path.Count > 0)
            {
                _prototype_PointView nextStep = path[0];
                
                // 만약 다음 스텝이 플레이어의 현재 위치라면 공격!
                if (nextStep.Point.Equals(playerCurrentPoint))
                {
                    var damageContext = new _prototype_DamageContext(
                        entityView.EntityData, 
                        playerView.EntityData, 
                        _prototype_DamageType.Physical, 
                        attackDamage, attackDamage);
                    await _prototype_InteractionManager.ApplyDamage(damageContext);
                    hasActed = true;
                }
                // 플레이어가 아니고 빈 칸이면 이동
                else if (nextStep.IsEntityPlaceable)
                {
                    _prototype_PointView currentPointView = _prototype_GridManager.Instance.GetPointView(myPoint);
                    if (currentPointView != null)
                    {
                        await _prototype_InteractionManager.MoveEntity(entityView, currentPointView, nextStep);
                        hasActed = true;
                    }
                }
            }

            // 행동을 마쳤다면 다음 유휴 시간 굴리기
            if (hasActed)
            {
                _currentIdleTicks = UnityEngine.Random.Range(minIdleTicks, maxIdleTicks + 1);
            }
        }
    }
}
