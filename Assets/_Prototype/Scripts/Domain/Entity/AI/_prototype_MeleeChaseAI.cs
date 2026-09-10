using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using DG.Tweening;

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
                mistakeRate = this.mistakeRate,
                _currentIdleTicks = 0
            };
        }

        public override async UniTask<_prototype_TickIntent> PlanAction(_prototype_EntityView entityView)
        {
            var intent = new _prototype_TickIntent
            {
                TargetsPlayer = false,
                Execute = async () => { await ExecuteAction(entityView); }
            };

            if (_currentIdleTicks > 0) return intent;

            var playerView = _prototype_PlayerController.Instance.ControlledEntityView;
            if (playerView == null || playerView.EntityData.health.Current <= 0) return intent;

            if (hasPlannedIntent && plannedCard != null)
            {
                var targetPoints = plannedCard.targetRange != null 
                    ? plannedCard.targetRange.GetValidTargetPoints(entityView.Point, plannedTarget) 
                    : new List<_prototype_Point> { plannedTarget };
                
                if (targetPoints.Contains(playerView.Point))
                {
                    intent.TargetsPlayer = true;
                }
            }

            return intent;
        }

        public override void EvaluateIntent(_prototype_EntityView entityView)
        {
            hasPlannedIntent = false;
            plannedCard = null;
            plannedTarget = entityView.Point;

            _prototype_GridVisualManager.Instance.ClearAllHazards(entityView);

            if (_currentIdleTicks > 0) return;

            var playerView = _prototype_PlayerController.Instance.ControlledEntityView;
            if (playerView == null || playerView.EntityData.health.Current <= 0) return;

            _prototype_Point playerCurrentPoint = playerView.Point;
            _prototype_Point myPoint = entityView.Point;

            var lifeData = entityView.EntityData as _prototype_LifeData;
            if (lifeData != null && lifeData.cardDeck != null)
            {
                _prototype_CardData cardToPlay = null;
                foreach (var card in lifeData.cardDeck.handedCardDatas)
                {
                    if (card.currentCoolTicks <= 0)
                    {
                        if (CanAffordCard(lifeData, card, allowMistake: true))
                        {
                            bool inRange = false;
                            if (card.castRange != null)
                            {
                                var validCastPoints = card.castRange.GetValidCastPoints(myPoint);
                                if (validCastPoints.Contains(playerCurrentPoint))
                                {
                                    inRange = true;
                                }
                            }
                            else
                            {
                                if (Math.Abs(myPoint.x - playerCurrentPoint.x) + Math.Abs(myPoint.y - playerCurrentPoint.y) == 1)
                                {
                                    inRange = true;
                                }
                            }

                            if (inRange)
                            {
                                cardToPlay = card;
                                break;
                            }
                        }
                    }
                }

                if (cardToPlay != null)
                {
                    plannedCard = cardToPlay;
                    plannedTarget = playerCurrentPoint;
                    hasPlannedIntent = true;

                    bool showsHazard = false;
                    foreach (var action in plannedCard.actionList)
                    {
                        if (action is _prototype_DamageEntityAction) showsHazard = true;
                        if (action is _prototype_SpawnTargetedProjectileEntityAction ||
                            action is _prototype_SpawnDirectionalProjectileEntityAction ||
                            action is _prototype_ShootLaserEntityAction)
                        {
                            showsHazard = false;
                            break;
                        }
                    }
                    
                    if (showsHazard)
                    {
                        if (plannedCard.targetRange != null)
                        {
                            var targetPoints = plannedCard.targetRange.GetValidTargetPoints(myPoint, plannedTarget);
                            foreach (var pt in targetPoints)
                            {
                                _prototype_GridVisualManager.Instance.ShowHazard(pt, entityView);
                            }
                        }
                        else
                        {
                            _prototype_GridVisualManager.Instance.ShowHazard(plannedTarget, entityView);
                        }
                    }
                }
            }
        }

        public override async UniTask ExecuteAction(_prototype_EntityView entityView)
        {
            if (_currentIdleTicks > 0)
            {
                _currentIdleTicks--;
                return;
            }

            var playerView = _prototype_PlayerController.Instance.ControlledEntityView;
            if (playerView == null || playerView.EntityData.health.Current <= 0) return;

            _prototype_GridVisualManager.Instance.ClearAllHazards(entityView);

            if (await HandleStatusEffectsOverride(entityView))
            {
                hasPlannedIntent = false;
                EvaluateIntent(entityView);
                return;
            }

            _prototype_Point playerCurrentPoint = playerView.Point;
            _prototype_Point targetPoint = playerCurrentPoint;
            _prototype_Point myPoint = entityView.Point;
            int distToPlayer = Math.Max(Math.Abs(myPoint.x - playerCurrentPoint.x), Math.Abs(myPoint.y - playerCurrentPoint.y));

            var lifeData = entityView.EntityData as _prototype_LifeData;
            if (lifeData == null) return;

            if (lifeData.cardDeck != null)
            {
                int maxHandSize = lifeData.lifeStat.handCardSlotCount;
                if (maxHandSize <= 0) maxHandSize = 3;
                if (lifeData.cardDeck.handedCardDatas.Count < maxHandSize)
                {
                    lifeData.cardDeck.DrawCards(maxHandSize - lifeData.cardDeck.handedCardDatas.Count, maxHandSize);
                }
            }

            bool hasActed = false;

            if (hasPlannedIntent && plannedCard != null)
            {
                if (lifeData.HasStatusEffect(_prototype_StatusType.Silence))
                {
                    // Can't cast
                }
                else if (!CanAffordCard(lifeData, plannedCard, allowMistake: true))
                {
                    // 녹다운 방지 또는 자원 부족으로 시전 취소 -> 휴식/추적으로 전환
                    hasPlannedIntent = false;
                    plannedCard = null;
                }
                else
                {
                    var burning = lifeData.GetStatusEffect(_prototype_StatusType.Burning);
                    bool destroyed = false;
                    if (burning != null)
                    {
                        float destroyProb = burning.value / (burning.value + 200f);
                        if (UnityEngine.Random.value < destroyProb) destroyed = true;
                    }

                    var cost = plannedCard.costValue;
                    if (cost != null)
                    {
                        int amount = (int)cost.value;
                        if (cost.costType == _prototype_CostType.FixedStamina) lifeData.stamina.Current -= amount;
                        else if (cost.costType == _prototype_CostType.FixedHealth) lifeData.health.Current -= amount;
                    }

                    lifeData.cardDeck.handedCardDatas.Remove(plannedCard);
                    if (destroyed) lifeData.cardDeck.destroyedCardDatas.Add(plannedCard);
                    else lifeData.cardDeck.discardedCardDatas.Add(plannedCard);

                    if (!destroyed && plannedCard.actionList != null)
                    {
                        List<_prototype_Point> targetRange = plannedCard.targetRange != null 
                            ? plannedCard.targetRange.GetValidTargetPoints(myPoint, plannedTarget) 
                            : new List<_prototype_Point> { plannedTarget };

                        List<_prototype_EntityData> targets = new();
                        foreach (var pt in targetRange)
                        {
                            var pointView = _prototype_GridManager.Instance.GetPointView(pt);
                            if (pointView != null)
                            {
                                bool found = false;
                                foreach (var ev in pointView.PlacedEntityViews)
                                {
                                    targets.Add(ev.EntityData);
                                    found = true;
                                }
                                if (!found && plannedCard.targetRange != null && plannedCard.targetRange.IncludeEmptyPoints)
                                    targets.Add(new _prototype_EmptyPointData(pt));
                            }
                        }

                        foreach (var action in plannedCard.actionList)
                        {
                            var filteredTargets = targets;
                            if (!action.includeSelf) filteredTargets = targets.FindAll(t => t != entityView.EntityData);
                            await action.ExecuteAction(entityView.EntityData, filteredTargets, null);
                        }
                    }
                }

                hasActed = true;
                _currentIdleTicks = UnityEngine.Random.Range(minIdleTicks, maxIdleTicks + 1);
            }
            else
            {
                int safeDistance = 4;
                bool shouldRest = false;

                if (lifeData.stamina.Current < lifeData.stamina.Max && distToPlayer >= safeDistance)
                {
                    // 거리가 멀면 SP가 꽉 찰 때까지 휴식
                    shouldRest = true;
                }
                else
                {
                    // 거리가 가깝거나 SP가 꽉 찼다면, 카드를 쓰기 위해 최소한의 SP가 부족한지 확인
                    if (lifeData.cardDeck != null)
                    {
                        int minSpNeeded = 999;
                        foreach (var card in lifeData.cardDeck.handedCardDatas)
                        {
                            if (card.currentCoolTicks <= 0) // 쿨타임이 지난 카드만 고려
                            {
                                int sp = GetCardStaminaCost(lifeData, card);
                                if (sp > 0) minSpNeeded = Math.Min(minSpNeeded, sp);
                                else minSpNeeded = 0;
                            }
                        }

                        bool isRestMistake = RollMistake();
                        int safeSpNeeded = isRestMistake ? minSpNeeded : minSpNeeded + 1;

                        if (minSpNeeded > 0 && minSpNeeded != 999 && lifeData.stamina.Current < safeSpNeeded)
                        {
                            shouldRest = true; // 가장 싼 카드라도 안전하게 쓰기 위해 휴식
                        }
                    }
                }

                // 수집된 위험 타일 목록 작성
                HashSet<_prototype_Point> hazardousPoints = new HashSet<_prototype_Point>();
                var allProjectiles = UnityEngine.Object.FindObjectsByType<_prototype_ProjectileView>();
                foreach (var proj in allProjectiles)
                {
                    if (proj.Data != null && lifeData != null && proj.Data.side != lifeData.side) // 다른 편의 투사체라면
                    {
                        foreach (var p in proj.GetPlannedPathPoints())
                        {
                            hazardousPoints.Add(p);
                        }
                    }
                }

                if (shouldRest && hazardousPoints.Contains(myPoint))
                {
                    // 휴식해야 할 타이밍이지만 현재 위치가 위험하다면 휴식을 취소하고 움직인다
                    shouldRest = false;
                }

                if (!shouldRest)
                {
                    // 카드를 사용할 수 없으나 이동은 가능한 상태라면 플레이어 방향으로 이동 시도
                    List<_prototype_PointView> path = _prototype_GridManager.Instance.FindPath(myPoint, targetPoint, entityView.EntityData, false, true, hazardousPoints);
                    if (path != null && path.Count > 0)
                    {
                        _prototype_PointView nextStep = path[0];
                        if (nextStep.CanPlaceEntity(entityView.EntityData))
                        {
                            _prototype_PointView currentPointView = _prototype_GridManager.Instance.GetPointView(myPoint);
                            if (currentPointView != null)
                            {
                                await _prototype_InteractionManager.MoveEntity(entityView, currentPointView, nextStep);
                                hasActed = true;
                            }
                        }
                    }

                    if (!hasActed && hazardousPoints.Contains(myPoint))
                    {
                        // Escape fallback
                        _prototype_Point[] dirs = new _prototype_Point[] {
                            new _prototype_Point(0, 1), new _prototype_Point(1, 0),
                            new _prototype_Point(0, -1), new _prototype_Point(-1, 0),
                            new _prototype_Point(1, 1), new _prototype_Point(1, -1),
                            new _prototype_Point(-1, 1), new _prototype_Point(-1, -1)
                        };

                        foreach (var d in dirs)
                        {
                            _prototype_Point p = myPoint + d;
                            var pv = _prototype_GridManager.Instance.GetPointView(p);
                            if (pv != null && pv.CanPlaceEntity(entityView.EntityData) && !hazardousPoints.Contains(p))
                            {
                                await _prototype_InteractionManager.MoveEntity(entityView, _prototype_GridManager.Instance.GetPointView(myPoint), pv);
                                hasActed = true;
                                break;
                            }
                        }
                    }
                }

                if (!hasActed)
                {
                    // 이동도 못하고 공격도 못하면 제자리에서 대기(휴식)하여 스테미나를 회복
                    if (lifeData != null)
                    {
                        lifeData.stamina.Current = Mathf.Min(lifeData.stamina.Max, lifeData.stamina.Current + lifeData.lifeStat.staminaRecoverAmount);
                        
                        // 시각적 효과 (통통 튀기)
                        entityView.transform.DOPunchScale(new Vector3(0.1f, -0.1f, 0), 0.2f, 1, 0f);

                        // 가장 필요 없는(쿨타임이 가장 긴) 카드를 버려서 슬롯을 하나 비워줌으로써 다음 턴에 새 카드를 뽑을 수 있게 함
                        if (lifeData.cardDeck.handedCardDatas.Count > 0)
                        {
                            _prototype_CardData worstCard = lifeData.cardDeck.handedCardDatas[0];
                            foreach (var c in lifeData.cardDeck.handedCardDatas)
                            {
                                if (c.currentCoolTicks > worstCard.currentCoolTicks)
                                    worstCard = c;
                            }
                            lifeData.cardDeck.handedCardDatas.Remove(worstCard);
                            lifeData.cardDeck.discardedCardDatas.Add(worstCard);
                        }
                    }
                    hasActed = true;
                }
            }

            // 행동을 마쳤다면 다음 유휴 시간 굴리기
            if (hasActed)
            {
                _currentIdleTicks = UnityEngine.Random.Range(minIdleTicks, maxIdleTicks + 1);
            }

            // Next turn intent evaluation
            EvaluateIntent(entityView);
        }
    }
}
