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
                optimalRangeMax = this.optimalRangeMax 
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

        public override async UniTask ExecuteAction(_prototype_EntityView entityView)
        {
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

            _prototype_Point myPoint = entityView.EntityData.point;
            _prototype_Point playerPoint = _prototype_PlayerController.Instance.ControlledEntityLastPoint;
            
            int distToPlayer = Math.Abs(myPoint.x - playerPoint.x) + Math.Abs(myPoint.y - playerPoint.y);

            _prototype_CardData dashCard = null;
            _prototype_CardData attackCard = null;

            if (lifeData.cardDeck != null)
            {
                foreach (var card in lifeData.cardDeck.handedCardDatas)
                {
                    if (card.currentCoolTicks <= 0)
                    {
                        bool canAfford = true;
                        if (card.costValue != null)
                        {
                            if (card.costValue.costType == _prototype_CostType.FixedStamina && lifeData.stamina.Current < card.costValue.value) canAfford = false;
                            if (card.costValue.costType == _prototype_CostType.FixedHealth && lifeData.health.Current <= card.costValue.value) canAfford = false;
                        }

                        if (canAfford)
                        {
                            bool isDash = false;
                            if (card.actionList != null && card.actionList.Find(a => a is _prototype_MoveToPointCardAction) != null) isDash = true;

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

            bool hasActed = false;
            _prototype_CardData cardToPlay = dashCard != null ? dashCard : attackCard;

            if (cardToPlay != null)
            {
                var cost = cardToPlay.costValue;
                if (cost != null)
                {
                    if (cost.costType == _prototype_CostType.FixedStamina) lifeData.stamina.Current -= (int)cost.value;
                    else if (cost.costType == _prototype_CostType.FixedHealth) lifeData.health.Current -= (int)cost.value;
                }

                lifeData.cardDeck.handedCardDatas.Remove(cardToPlay);
                lifeData.cardDeck.discardedCardDatas.Add(cardToPlay);

                _prototype_Point abilityTargetPoint = playerPoint;

                if (cardToPlay == dashCard)
                {
                    var validPoints = dashCard.castRange != null ? dashCard.castRange.GetValidCastPoints(myPoint) : new List<_prototype_Point>();
                    int bestDist = -1;
                    _prototype_Point bestPoint = myPoint;
                    
                    foreach (var pt in validPoints)
                    {
                        var pointView = _prototype_GridManager.Instance.GetPointView(pt);
                        if (pointView != null && pointView.CanPlaceEntity(entityView.EntityData))
                        {
                            int d = Math.Abs(pt.x - playerPoint.x) + Math.Abs(pt.y - playerPoint.y);
                            if (d > bestDist && d <= optimalRangeMax + 1)
                            {
                                bestDist = d;
                                bestPoint = pt;
                            }
                        }
                    }
                    abilityTargetPoint = bestPoint;
                }

                if (cardToPlay.actionList != null)
                {
                    List<_prototype_Point> targetRange = cardToPlay.targetRange != null 
                        ? cardToPlay.targetRange.GetValidTargetPoints(myPoint, abilityTargetPoint) 
                        : new List<_prototype_Point> { abilityTargetPoint };

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
                            if (!found && cardToPlay.targetRange != null && cardToPlay.targetRange.IncludeEmptyPoints)
                                targets.Add(new _prototype_EmptyPointData(pt));
                        }
                    }

                    foreach (var action in cardToPlay.actionList)
                    {
                        var filteredTargets = targets;
                        if (!action.includeSelf) filteredTargets = targets.FindAll(t => t != entityView.EntityData);
                        await action.ExecuteCardAction(entityView.EntityData, filteredTargets, null);
                    }
                }
                hasActed = true;
            }
            else
            {
                bool needsSpInAdvance = false;
                if (lifeData.cardDeck != null)
                {
                    int minSpNeeded = 999;
                    foreach (var card in lifeData.cardDeck.handedCardDatas)
                    {
                        if (card.costValue != null && card.costValue.costType == _prototype_CostType.FixedStamina)
                            minSpNeeded = Math.Min(minSpNeeded, (int)card.costValue.value);
                        else minSpNeeded = 0;
                    }
                    if (minSpNeeded > 0 && minSpNeeded != 999 && lifeData.stamina.Current < minSpNeeded)
                        needsSpInAdvance = true;
                }

                if (!needsSpInAdvance)
                {
                    if (distToPlayer > optimalRangeMax || distToPlayer < optimalRangeMin)
                    {
                        List<_prototype_PointView> path = _prototype_GridManager.Instance.FindPath(myPoint, playerPoint, entityView.EntityData, false, true);
                        if (path != null && path.Count > 0)
                        {
                            if (distToPlayer > optimalRangeMax)
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
                    }
                }

                if (!hasActed)
                {
                    lifeData.stamina.Current = Mathf.Min(lifeData.stamina.Max, lifeData.stamina.Current + lifeData.lifeStat.staminaRecoverAmount);
                    entityView.transform.DOPunchScale(new Vector3(0.1f, -0.1f, 0), 0.2f, 1, 0f);
                }
            }
        }

        private bool HasClearLineOfSight(_prototype_Point start, _prototype_Point end)
        {
            _prototype_Point dir = _prototype_Point.zero;
            
            if (end.x > start.x) dir.x = 1;
            else if (end.x < start.x) dir.x = -1;

            if (end.y > start.y) dir.y = 1;
            else if (end.y < start.y) dir.y = -1;

            _prototype_Point current = start + dir;

            // 100 limit to prevent infinite loops just in case
            int limit = 100;
            while (current != end && limit > 0)
            {
                limit--;
                var ptView = _prototype_GridManager.Instance.GetPointView(current);
                if (ptView == null) return false;
                
                if (!ptView.IsTraversable(_prototype_MovementType.Flying)) return false;

                foreach (var ev in ptView.PlacedEntityViews)
                {
                    if (ev.EntityData is _prototype_ObstacleData) return false;
                }

                current += dir;
            }

            return limit > 0;
        }
    }
}
