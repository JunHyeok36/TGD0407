using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

namespace TDG0407._prototype
{
    [Serializable]
    public class _prototype_ArcherAI : _prototype_EnemyAILogic
    {
        public int optimalRangeMin = 2;
        public int optimalRangeMax = 4;

        public override _prototype_EnemyAILogic Clone()
        {
            return new _prototype_ArcherAI 
            { 
                optimalRangeMin = this.optimalRangeMin,
                optimalRangeMax = this.optimalRangeMax,
                mistakeRate = this.mistakeRate
            };
        }

        public override async UniTask<_prototype_TickIntent> PlanAction(_prototype_EntityView entityView)
        {
            var intent = new _prototype_TickIntent
            {
                TargetsPlayer = false,
                Execute = async () => { await ExecuteAction(entityView); }
            };
            return intent; // Archer never targets the player directly (it spawns projectiles)
        }

        public override void EvaluateIntent(_prototype_EntityView entityView)
        {
            hasPlannedIntent = false;
            plannedCard = null;
            plannedTarget = entityView.Point;
            
            var lifeData = entityView.EntityData as _prototype_LifeData;
            if (lifeData == null) return;

            _prototype_Point myPoint = entityView.EntityData.point;
            _prototype_Point playerPoint = _prototype_PlayerController.Instance.ControlledEntityLastPoint;
            int distToPlayer = Math.Max(Math.Abs(myPoint.x - playerPoint.x), Math.Abs(myPoint.y - playerPoint.y));

            if (lifeData.cardDeck != null)
            {
                // 1. 중복된 대시/유틸리티 카드 정리 (궁수는 회피기가 1장만 있으면 충분함)
                int dashCount = 0;
                for (int i = lifeData.cardDeck.handedCardDatas.Count - 1; i >= 0; i--)
                {
                    var card = lifeData.cardDeck.handedCardDatas[i];
                    bool isDash = card.actionList != null && card.actionList.Find(a => a is _prototype_MoveToPointEntityAction) != null;
                    if (isDash)
                    {
                        dashCount++;
                        if (dashCount > 1)
                        {
                            lifeData.cardDeck.handedCardDatas.RemoveAt(i);
                            lifeData.cardDeck.discardedCardDatas.Add(card);
                        }
                    }
                }

                // 2. 플레이어와 거리가 있어 대시를 쓸 수 없는데 손패에 공격 카드가 없다면, 대시 카드를 버려 공격 카드를 뽑을 슬롯 확보
                if (distToPlayer > optimalRangeMin - 1)
                {
                    bool hasAttackCard = lifeData.cardDeck.handedCardDatas.Exists(c => c.actionList == null || c.actionList.Find(a => a is _prototype_MoveToPointEntityAction) == null);
                    if (!hasAttackCard && lifeData.cardDeck.handedCardDatas.Count > 0)
                    {
                        var discardCard = lifeData.cardDeck.handedCardDatas[0];
                        lifeData.cardDeck.handedCardDatas.RemoveAt(0);
                        lifeData.cardDeck.discardedCardDatas.Add(discardCard);
                    }
                }

                // 3. 카드 드로우
                int maxHandSize = lifeData.lifeStat.handCardSlotCount;
                if (maxHandSize <= 0) maxHandSize = 3;
                if (lifeData.cardDeck.handedCardDatas.Count < maxHandSize)
                {
                    lifeData.cardDeck.DrawCards(maxHandSize - lifeData.cardDeck.handedCardDatas.Count, maxHandSize);
                }
            }

            _prototype_CardData dashCard = null;
            _prototype_CardData attackCard = null;

            if (lifeData.cardDeck != null)
            {
                foreach (var card in lifeData.cardDeck.handedCardDatas)
                {
                    if (card.currentCoolTicks <= 0)
                    {
                        if (CanAffordCard(lifeData, card, allowMistake: true))
                        {
                            bool isDash = false;
                            if (card.actionList != null && card.actionList.Find(a => a is _prototype_MoveToPointEntityAction) != null) isDash = true;

                            if (isDash && distToPlayer <= optimalRangeMin - 1)
                            {
                                dashCard = card;
                            }
                            else if (!isDash)
                            {
                                if (card.castRange != null && card.castRange.GetValidCastPoints(myPoint).Contains(playerPoint))
                                {
                                    if (HasClearLineOfSight(myPoint, playerPoint))
                                    {
                                        attackCard = card;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            HashSet<_prototype_Point> hostileHazardousPoints = GetHostileHazardousPoints(lifeData);
            _prototype_Point bestDashPoint = myPoint;
            bool hasValidDashPoint = false;

            if (dashCard != null)
            {
                var validPoints = dashCard.castRange != null ? dashCard.castRange.GetValidCastPoints(myPoint) : new List<_prototype_Point>();
                int bestScore = -1;

                foreach (var pt in validPoints)
                {
                    var pointView = _prototype_GridManager.Instance.GetPointView(pt);
                    if (pointView == null || !pointView.CanPlaceEntity(entityView.EntityData)) continue;
                    if (hostileHazardousPoints.Contains(pt)) continue;

                    int d = Math.Max(Math.Abs(pt.x - playerPoint.x), Math.Abs(pt.y - playerPoint.y));
                    if (d <= distToPlayer) continue; // 플레이어로부터 실제로 멀어지는 타일만 고려

                    // 체비쇼프 거리 기반 점수 산정
                    int score = d * 10;
                    if (d >= optimalRangeMin && d <= optimalRangeMax)
                    {
                        score += 100; // 최적 사거리(2~4칸) 우선 보너스
                    }
                    else if (d > optimalRangeMax)
                    {
                        score += 50; // 사거리 밖으로 멀어지더라도 근접 상태 탈출 우선
                    }

                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestDashPoint = pt;
                        hasValidDashPoint = true;
                    }
                }

                // 유효하고 안전한 탈출 타일이 없다면 제자리 낭비 대시를 방지하기 위해 dashCard 취소
                if (!hasValidDashPoint)
                {
                    dashCard = null;
                }
            }

            _prototype_CardData cardToPlay = dashCard != null ? dashCard : attackCard;
            if (cardToPlay != null)
            {
                plannedCard = cardToPlay;
                _prototype_Point abilityTargetPoint = playerPoint;

                if (cardToPlay == dashCard)
                {
                    abilityTargetPoint = bestDashPoint;
                }
                
                plannedTarget = abilityTargetPoint;
                hasPlannedIntent = true;
                
                // Show hazard markers visually on the target range
                _prototype_GridVisualManager.Instance.ClearAllHazards(entityView);
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
            else
            {
                // Clear hazards if no intent
                _prototype_GridVisualManager.Instance.ClearAllHazards(entityView);
            }
        }

        public override async UniTask ExecuteAction(_prototype_EntityView entityView)
        {
            var lifeData = entityView.EntityData as _prototype_LifeData;
            if (lifeData == null) return;

            _prototype_GridVisualManager.Instance.ClearAllHazards(entityView);

            if (await HandleStatusEffectsOverride(entityView))
            {
                hasPlannedIntent = false;
                EvaluateIntent(entityView);
                return;
            }

            _prototype_Point myPoint = entityView.EntityData.point;
            _prototype_Point playerPoint = _prototype_PlayerController.Instance.ControlledEntityLastPoint;
            int distToPlayer = Math.Max(Math.Abs(myPoint.x - playerPoint.x), Math.Abs(myPoint.y - playerPoint.y));

            bool hasActed = false;
            bool cardPlayed = false;

            if (hasPlannedIntent && plannedCard != null)
            {
                if (lifeData.HasStatusEffect(_prototype_StatusType.Silence))
                {
                    // Can't cast card
                }
                else if (!CanAffordCard(lifeData, plannedCard, allowMistake: true))
                {
                    // 녹다운 방지 또는 자원 부족으로 시전 취소 -> 휴식/이동으로 전환
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

                    lifeData.cardDeck.handedCardDatas.Remove(plannedCard);
                    if (destroyed) lifeData.cardDeck.destroyedCardDatas.Add(plannedCard);
                    else lifeData.cardDeck.discardedCardDatas.Add(plannedCard);

                    var cost = plannedCard.costValue;
                    if (cost != null)
                    {
                        if (cost.costType == _prototype_CostType.FixedStamina) lifeData.stamina.Current -= (int)cost.value;
                        else if (cost.costType == _prototype_CostType.FixedHealth) lifeData.health.Current -= (int)cost.value;
                    }

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
                    hasActed = true;
                    cardPlayed = true;
                }
            }
            else
            {
                bool shouldRest = false;

                if (lifeData.stamina.Current < lifeData.stamina.Max && distToPlayer > optimalRangeMax)
                {
                    shouldRest = true;
                }
                else
                {
                    if (lifeData.cardDeck != null)
                    {
                        int minSpNeeded = 999;
                        foreach (var card in lifeData.cardDeck.handedCardDatas)
                        {
                            if (card.currentCoolTicks <= 0)
                            {
                                int sp = GetCardStaminaCost(lifeData, card);
                                if (sp > 0) minSpNeeded = Math.Min(minSpNeeded, sp);
                                else minSpNeeded = 0;
                            }
                        }

                        bool isRestMistake = RollMistake();
                        int safeSpNeeded = isRestMistake ? minSpNeeded : minSpNeeded + 1;

                        if (minSpNeeded > 0 && minSpNeeded != 999 && lifeData.stamina.Current < safeSpNeeded)
                            shouldRest = true;
                    }
                }

                HashSet<_prototype_Point> hazardousPoints = GetHostileHazardousPoints(lifeData);

                if (shouldRest && hazardousPoints.Contains(myPoint))
                {
                    shouldRest = false;
                }

                if (!shouldRest)
                {
                    bool isInHazard = hazardousPoints.Contains(myPoint);

                    if (distToPlayer > optimalRangeMax && !isInHazard)
                    {
                        // 다가가기
                        List<_prototype_PointView> path = _prototype_GridManager.Instance.FindPath(myPoint, playerPoint, entityView.EntityData, false, true, hazardousPoints);
                        if (path != null && path.Count > 0)
                        {
                            _prototype_PointView nextStep = path[0];
                            if (nextStep.CanPlaceEntity(entityView.EntityData))
                            {
                                var currentPv = _prototype_GridManager.Instance.GetPointView(myPoint);
                                await _prototype_InteractionManager.MoveEntity(entityView, currentPv, nextStep);
                                hasActed = true;
                            }
                        }
                    }
                    else if (distToPlayer < optimalRangeMin || isInHazard)
                    {
                        // 도망치기 (카이팅)
                        _prototype_PointView bestEscapePoint = null;
                        int maxDist = distToPlayer;

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
                                int testDist = Math.Max(Math.Abs(p.x - playerPoint.x), Math.Abs(p.y - playerPoint.y));
                                if (testDist > maxDist || bestEscapePoint == null)
                                {
                                    maxDist = testDist;
                                    bestEscapePoint = pv;
                                }
                            }
                        }

                        if (bestEscapePoint != null)
                        {
                            var currentPv = _prototype_GridManager.Instance.GetPointView(myPoint);
                            await _prototype_InteractionManager.MoveEntity(entityView, currentPv, bestEscapePoint);
                            hasActed = true;
                        }
                    }
                }

                if (!hasActed)
                {
                    lifeData.stamina.Current = Mathf.Min(lifeData.stamina.Max, lifeData.stamina.Current + lifeData.lifeStat.staminaRecoverAmount);
                    entityView.transform.DOPunchScale(new Vector3(0.1f, -0.1f, 0), 0.2f, 1, 0f);
                }

                // 카드를 사용하지 못했다면 (이동만 했거나 휴식했을 때), 손패 순환을 위해 가장 쓸모없는 카드 1장 버리기
                if (!cardPlayed && lifeData.cardDeck != null && lifeData.cardDeck.handedCardDatas.Count > 0)
                {
                    _prototype_CardData cardToDiscard = null;
                    if (distToPlayer > optimalRangeMin - 1)
                    {
                        cardToDiscard = lifeData.cardDeck.handedCardDatas.Find(c => c.actionList != null && c.actionList.Find(a => a is _prototype_MoveToPointEntityAction) != null);
                    }

                    if (cardToDiscard == null)
                    {
                        cardToDiscard = lifeData.cardDeck.handedCardDatas[0];
                        foreach (var c in lifeData.cardDeck.handedCardDatas)
                        {
                            if (c.currentCoolTicks > cardToDiscard.currentCoolTicks)
                                cardToDiscard = c;
                        }
                    }

                    if (cardToDiscard != null)
                    {
                        lifeData.cardDeck.handedCardDatas.Remove(cardToDiscard);
                        lifeData.cardDeck.discardedCardDatas.Add(cardToDiscard);
                    }
                }
            }

            // Next turn intent evaluation
            EvaluateIntent(entityView);
        }

        private bool HasClearLineOfSight(_prototype_Point start, _prototype_Point end)
        {
            int x0 = start.x;
            int y0 = start.y;
            int x1 = end.x;
            int y1 = end.y;

            int dx = Math.Abs(x1 - x0);
            int dy = Math.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;
            int err = dx - dy;

            while (true)
            {
                if (x0 == x1 && y0 == y1) break;

                if (x0 != start.x || y0 != start.y)
                {
                    var current = new _prototype_Point(x0, y0);
                    var ptView = _prototype_GridManager.Instance.GetPointView(current);
                    if (ptView == null) return false;
                    
                    if (!ptView.IsTraversable(_prototype_MovementType.Flying)) return false;

                    foreach (var ev in ptView.PlacedEntityViews)
                    {
                        if (ev.EntityData is _prototype_ObstacleData) return false;
                    }
                }

                int e2 = 2 * err;
                if (e2 > -dy)
                {
                    err -= dy;
                    x0 += sx;
                }
                if (e2 < dx)
                {
                    err += dx;
                    y0 += sy;
                }
            }

            return true;
        }

        private HashSet<_prototype_Point> GetHostileHazardousPoints(_prototype_LifeData lifeData)
        {
            HashSet<_prototype_Point> hazardousPoints = new HashSet<_prototype_Point>();
            var allProjectiles = UnityEngine.Object.FindObjectsByType<_prototype_ProjectileView>();
            foreach (var proj in allProjectiles)
            {
                if (proj.Data != null && lifeData != null && proj.Data.side != lifeData.side)
                {
                    foreach (var p in proj.GetPlannedPathPoints())
                        hazardousPoints.Add(p);
                }
            }
            return hazardousPoints;
        }
    }
}
