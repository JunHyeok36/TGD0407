using System;
using System.Collections.Generic;
using System.Linq;
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
            if (playerView == null || playerView.EntityData.IsDead) return intent;

            var lifeData = entityView.EntityData as _prototype_LifeData;
            if (lifeData != null && (lifeData.HasStatusEffect(_prototype_StatusType.Stun) || lifeData.HasStatusEffect(_prototype_StatusType.Silence)))
            {
                hasPlannedIntent = false;
                plannedCard = null;
                _prototype_GridVisualManager.Instance.ClearAllHazards(entityView);
                return intent;
            }

            if (hasPlannedIntent && plannedCard is _prototype_BattleCardData plannedBattleCard)
            {
                var targetPoints = plannedBattleCard.targetRange != null 
                    ? plannedBattleCard.targetRange.GetValidTargetPoints(entityView.Point, plannedTarget) 
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
            CurrentIntent = _prototype_EnemyIntent.None();

            _prototype_GridVisualManager.Instance.ClearAllHazards(entityView);

            if (_currentIdleTicks > 0) { CurrentIntent = _prototype_EnemyIntent.ForRest(); return; }

            var lifeData = entityView.EntityData as _prototype_LifeData;
            if (lifeData == null) return;

            if (lifeData.HasStatusEffect(_prototype_StatusType.Stun) || lifeData.HasStatusEffect(_prototype_StatusType.Silence))
            {
                CurrentIntent = _prototype_EnemyIntent.ForStunned();
                return;
            }

            if (lifeData.HasStatusEffect(_prototype_StatusType.Fear))
            {
                CurrentIntent = _prototype_EnemyIntent.ForFlee();
            }

            var playerView = _prototype_PlayerController.Instance.ControlledEntityView;
            if (playerView == null || playerView.EntityData.IsDead) return;

            _prototype_Point playerCurrentPoint = playerView.Point;
            _prototype_Point myPoint = entityView.Point;

            if (lifeData.cardDeck != null)
            {
                _prototype_BattleCardData cardToPlay = null;
                var lifeView = entityView as _prototype_LifeView;
                var availableCards = lifeView != null ? lifeView.GetAvailableCards() : lifeData.cardDeck.handedCardDatas;
                
                foreach (var card in availableCards)
                {
                    if (card is _prototype_BattleCardData battleCard && battleCard.currentCoolTicks <= 0)
                    {
                        if (CanAffordCard(lifeData, battleCard, allowMistake: true))
                        {
                            bool inRange = IsInRangeOfTarget(entityView, playerCurrentPoint, battleCard);
                            if (inRange)
                            {
                                cardToPlay = battleCard;
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
                    CurrentIntent = _prototype_EnemyIntent.ForAttack(cardToPlay, playerCurrentPoint, entityView.EntityData, playerView.EntityData);

                    bool showsHazard = false;
                    if (cardToPlay.actionList != null)
                    {
                        foreach (var action in cardToPlay.actionList)
                        {
                            if (action is _prototype_DamageEntityAction) showsHazard = true;
                            if (action is _prototype_SpawnTargetedProjectileEntityAction ||
                                action is _prototype_SpawnDirectionalProjectileEntityAction)
                            {
                                showsHazard = false;
                                break;
                            }
                        }
                    }
                    
                    if (showsHazard)
                    {
                        if (cardToPlay.targetRange != null)
                        {
                            var targetPoints = cardToPlay.targetRange.GetValidTargetPoints(myPoint, plannedTarget);
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

                    if (CurrentIntent != null && CurrentIntent.knockbackLandingPoint.HasValue)
                    {
                        _prototype_GridVisualManager.Instance.ShowKnockbackHazard(entityView, CurrentIntent.knockbackLandingPoint.Value);
                    }
                }
                else
                {
                    // 공격할 카드가 없으면 이동 의도
                    CurrentIntent = _prototype_EnemyIntent.ForMove();
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
            if (playerView == null || playerView.EntityData.IsDead) return;

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

            if (lifeData.HasStatusEffect(_prototype_StatusType.Stun))
            {
                hasPlannedIntent = false;
                plannedCard = null;
                return;
            }

            if (lifeData.HasStatusEffect(_prototype_StatusType.Silence))
            {
                hasPlannedIntent = false;
                plannedCard = null;
            }

            bool hasActed = false;

            if (hasPlannedIntent && plannedCard != null)
            {
                if (!CanAffordCard(lifeData, plannedCard, allowMistake: true))
                {
                    // 녹다운 방지 또는 자원 부족으로 시전 취소 -> 휴식/추적으로 전환
                    hasPlannedIntent = false;
                    plannedCard = null;
                }
                else
                {
                    if (plannedCard is _prototype_BattleCardData battleCardToPlay)
                    {
                        bool destroyed = false;
                        var burning = lifeData.GetStatusEffect(_prototype_StatusType.Burning);
                        if (burning != null)
                        {
                            float destroyProb = burning.value / (burning.value + 200f);
                            if (UnityEngine.Random.value < destroyProb) destroyed = true;
                        }

                        var cost = battleCardToPlay.costValue;
                        if (cost != null)
                        {
                            int amount = (int)cost.value;
                            if (cost.costType == _prototype_CostType.FixedStamina) lifeData.stamina.Current -= amount;
                            else if (cost.costType == _prototype_CostType.FixedHealth) lifeData.health.Current -= amount;
                        }

                        if (battleCardToPlay.sourceProvider == null)
                        {
                            lifeData.cardDeck.handedCardDatas.Remove(battleCardToPlay);
                            if (destroyed) lifeData.cardDeck.destroyedCardDatas.Add(battleCardToPlay);
                            else lifeData.cardDeck.discardedCardDatas.Add(battleCardToPlay);
                        }

                        if (!destroyed && battleCardToPlay.actionList != null)
                        {
                            List<_prototype_Point> targetRange = battleCardToPlay.targetRange != null 
                                ? battleCardToPlay.targetRange.GetValidTargetPoints(myPoint, plannedTarget) 
                                : new List<_prototype_Point> { plannedTarget };

                            List<_prototype_EntityData> targets = new();
                            foreach (var pt in targetRange)
                            {
                                var pointView = _prototype_GridManager.Instance.GetPointView(pt);
                                if (pointView != null)
                                {
                                    foreach (var ev in pointView.PlacedEntityViews)
                                    {
                                        targets.Add(ev.EntityData);
                                    }
                                }
                            }
                            targets = targets.Distinct().ToList();

                            int dx = plannedTarget.x - entityView.Point.x;
                            int dy = plannedTarget.y - entityView.Point.y;
                            int normX = dx != 0 ? (int)Mathf.Sign(dx) : 0;
                            int normY = dy != 0 ? (int)Mathf.Sign(dy) : 0;
                            _prototype_Point attackDir = new _prototype_Point(normX, normY);
                            var cardParams = new _prototype_CardActionParams(battleCardToPlay, plannedTarget, attackDir, targetRange);

                            foreach (var action in battleCardToPlay.actionList)
                            {
                                var filteredTargets = targets;
                                if (!action.includeSelf) filteredTargets = targets.FindAll(t => t != entityView.EntityData);
                                await action.ExecuteAction(entityView.EntityData, filteredTargets, cardParams);
                            }
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
                        var lifeView = entityView as _prototype_LifeView;
                        var availableCards = lifeView != null ? lifeView.GetAvailableCards() : lifeData.cardDeck.handedCardDatas;
                        foreach (var card in availableCards)
                        {
                            if (card is _prototype_BattleCardData bc && bc.currentCoolTicks <= 0) // 쿨타임이 지난 카드만 고려
                            {
                                int sp = GetCardStaminaCost(lifeData, bc);
                                if (sp > 0) minSpNeeded = Math.Min(minSpNeeded, sp);
                                else minSpNeeded = 0;
                            }
                        }

                        bool isRestMistake = RollMistake();
                        int safeSpNeeded = Math.Min(isRestMistake ? minSpNeeded : minSpNeeded + 1, lifeData.stamina.Max);

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

                // Check if already in melee range of player
                bool isAdjacentToPlayer = IsInRangeOfTarget(entityView, playerCurrentPoint, null);
                if (isAdjacentToPlayer)
                {
                    // Already in range! Do not move closer (prevents bumping/overlapping player)
                    shouldRest = true;
                }

                if (shouldRest && hazardousPoints.Contains(myPoint))
                {
                    // 휴식해야 할 타이밍이지만 현재 위치가 위험하다면 휴식을 취소하고 움직인다
                    shouldRest = false;
                }

                if (!shouldRest)
                {
                    _prototype_Point bestTargetPoint = targetPoint;
                    var entitySize = entityView.EntityData != null ? entityView.EntityData.size : _prototype_Point.one;
                    if (entitySize.x > 1 || entitySize.y > 1)
                    {
                        var candidates = GetAdjacentPlacementsForTarget(playerCurrentPoint, entitySize);
                        var validCandidates = candidates.Where(c => _prototype_GridManager.Instance != null && _prototype_GridManager.Instance.CanPlaceEntityFootprint(entityView.EntityData, c)).ToList();
                        if (validCandidates.Count > 0)
                        {
                            validCandidates.Sort((a, b) => 
                                (Math.Abs(a.x - myPoint.x) + Math.Abs(a.y - myPoint.y))
                                .CompareTo(Math.Abs(b.x - myPoint.x) + Math.Abs(b.y - myPoint.y)));
                            bestTargetPoint = validCandidates[0];
                        }
                    }

                    // 카드를 사용할 수 없으나 이동은 가능한 상태라면 플레이어 방향으로 이동 시도
                    List<_prototype_PointView> path = _prototype_GridManager.Instance.FindPath(myPoint, bestTargetPoint, entityView.EntityData, false, true, hazardousPoints);
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
                        ExecuteEnemyRest(entityView);

                        // 손패 순환을 위해 가장 쓸모없는(쿨타임이 가장 긴) 카드 1장 버리기
                        if (lifeData.cardDeck != null)
                        {
                            lifeData.cardDeck.DiscardHighestCooldownCard();
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

        private bool IsInRangeOfTarget(_prototype_EntityView entityView, _prototype_Point targetPoint, _prototype_CardData card)
        {
            if (entityView == null || _prototype_GridManager.Instance == null) return false;
            var entitySize = entityView.EntityData != null ? entityView.EntityData.size : _prototype_Point.one;
            var fp = _prototype_GridManager.Instance.GetFootprint(entityView.Point, entitySize);
            if (fp == null) return false;

            var battleCard = card as _prototype_BattleCardData;
            foreach (var pv in fp)
            {
                if (battleCard != null && battleCard.castRange != null)
                {
                    var validCastPoints = battleCard.castRange.GetValidCastPoints(pv.Point);
                    if (validCastPoints != null && validCastPoints.Contains(targetPoint))
                        return true;
                }
                else
                {
                    if (Math.Abs(pv.Point.x - targetPoint.x) + Math.Abs(pv.Point.y - targetPoint.y) == 1)
                        return true;
                }
            }
            return false;
        }

        private List<_prototype_Point> GetAdjacentPlacementsForTarget(_prototype_Point target, _prototype_Point size)
        {
            List<_prototype_Point> list = new();
            _prototype_Point[] cardinalDirs = new _prototype_Point[]
            {
                new(0, 1), new(1, 0), new(0, -1), new(-1, 0)
            };

            for (int dx = 0; dx < size.x; dx++)
            {
                for (int dy = 0; dy < size.y; dy++)
                {
                    foreach (var dir in cardinalDirs)
                    {
                        _prototype_Point adjTile = target + dir;
                        _prototype_Point candidateOrigin = new _prototype_Point(adjTile.x - dx, adjTile.y - dy);

                        // Check that footprint at candidateOrigin does NOT contain target
                        bool overlaps = target.x >= candidateOrigin.x && target.x < candidateOrigin.x + size.x &&
                                        target.y >= candidateOrigin.y && target.y < candidateOrigin.y + size.y;

                        if (!overlaps && !list.Contains(candidateOrigin))
                        {
                            list.Add(candidateOrigin);
                        }
                    }
                }
            }

            return list;
        }
    }
}
