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
            _prototype_Point targetPoint = playerCurrentPoint;
            _prototype_Point myPoint = entityView.Point;

            var lifeData = entityView.EntityData as _prototype_LifeData;
            if (lifeData != null && lifeData.cardDeck != null)
            {
                // 1. 핸드 카드 보충 (최대 슬롯 개수까지 드로우)
                int maxHandSize = lifeData.lifeStat.handCardSlotCount;
                if (maxHandSize <= 0) maxHandSize = 3;
                if (lifeData.cardDeck.handedCardDatas.Count < maxHandSize)
                {
                    lifeData.cardDeck.DrawCards(maxHandSize - lifeData.cardDeck.handedCardDatas.Count, maxHandSize);
                }
            }

            _prototype_CardData cardToPlay = null;

            if (lifeData != null && lifeData.cardDeck != null)
            {
                // 2. 현재 위치에서 플레이어를 공격할 수 있는 카드 찾기
                foreach (var card in lifeData.cardDeck.handedCardDatas)
                {
                    if (card.currentCoolTicks <= 0)
                    {
                        bool canAfford = true;
                        if (card.costValue != null)
                        {
                            int amount = (int)card.costValue.value;
                            if (card.costValue.costType == _prototype_CostType.FixedStamina && lifeData.stamina.Current < amount) canAfford = false;
                            if (card.costValue.costType == _prototype_CostType.FixedHealth && lifeData.health.Current <= amount) canAfford = false; // 자살 방지
                        }

                        if (canAfford)
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
                                // 범위 지정이 없으면 기본적으로 인접(Melee)해야 한다고 간주
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
            }

            bool hasActed = false;

            if (cardToPlay != null)
            {
                // Cost Deduction
                var cost = cardToPlay.costValue;
                if (cost != null)
                {
                    int amount = (int)cost.value;
                    if (cost.costType == _prototype_CostType.FixedStamina) lifeData.stamina.Current -= amount;
                    else if (cost.costType == _prototype_CostType.FixedHealth) lifeData.health.Current -= amount;
                }

                // Discard Card
                lifeData.cardDeck.handedCardDatas.Remove(cardToPlay);
                lifeData.cardDeck.discardedCardDatas.Add(cardToPlay);

                // Execute Card
                if (cardToPlay.actionList != null)
                {
                    List<_prototype_Point> targetRange = cardToPlay.targetRange != null 
                        ? cardToPlay.targetRange.GetValidTargetPoints(myPoint, playerCurrentPoint) 
                        : new List<_prototype_Point> { playerCurrentPoint };

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
                            {
                                targets.Add(new _prototype_EmptyPointData(pt));
                            }
                        }
                    }

                    foreach (var action in cardToPlay.actionList)
                    {
                        var filteredTargets = targets;
                        if (!action.includeSelf)
                        {
                            filteredTargets = targets.FindAll(t => t != entityView.EntityData);
                        }
                        await action.ExecuteCardAction(entityView.EntityData, filteredTargets, null);
                    }
                }
                
                hasActed = true;
            }
            else
            {
                // SP가 부족하여 카드를 쓸 수 없는 상황인지 확인 (미리 SP 회복)
                bool needsSpInAdvance = false;
                if (lifeData != null && lifeData.cardDeck != null)
                {
                    int minSpNeeded = 999;
                    foreach (var card in lifeData.cardDeck.handedCardDatas)
                    {
                        if (card.costValue != null && card.costValue.costType == _prototype_CostType.FixedStamina)
                            minSpNeeded = Math.Min(minSpNeeded, (int)card.costValue.value);
                        else
                            minSpNeeded = 0;
                    }
                    if (minSpNeeded > 0 && minSpNeeded != 999 && lifeData.stamina.Current < minSpNeeded)
                    {
                        needsSpInAdvance = true;
                    }
                }

                if (!needsSpInAdvance)
                {
                    // 카드를 사용할 수 없으나 이동은 가능한 상태라면 플레이어 방향으로 이동 시도
                    List<_prototype_PointView> path = _prototype_GridManager.Instance.FindPath(myPoint, targetPoint, entityView.EntityData.movementType, false, true);
                    if (path != null && path.Count > 0)
                    {
                        _prototype_PointView nextStep = path[0];
                        if (nextStep.CanPlaceEntity(entityView.EntityData.movementType))
                        {
                            _prototype_PointView currentPointView = _prototype_GridManager.Instance.GetPointView(myPoint);
                            if (currentPointView != null)
                            {
                                await _prototype_InteractionManager.MoveEntity(entityView, currentPointView, nextStep);
                                hasActed = true;
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
        }
    }
}
