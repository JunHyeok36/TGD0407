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
                    var lifeData = entityView.EntityData as _prototype_LifeData;
                    _prototype_CardData cardToPlay = null;

                    if (lifeData != null && lifeData.cardDeck != null)
                    {
                        // 1. 핸드 카드 보충 (최대 슬롯 개수까지 드로우)
                        int maxHandSize = lifeData.lifeStat.handCardSlotCount;
                        if (maxHandSize <= 0) maxHandSize = 3;
                        if (lifeData.cardDeck.handedCardDatas.Count < maxHandSize)
                        {
                            lifeData.cardDeck.DrawCards(maxHandSize - lifeData.cardDeck.handedCardDatas.Count, maxHandSize);
                        }

                        // 2. 사용 가능한 카드 찾기 (쿨타임 0 이하, 코스트 지불 가능)
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
                                    cardToPlay = card;
                                    break;
                                }
                            }
                        }
                    }

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
                            var targets = new List<_prototype_EntityData> { playerView.EntityData };
                            foreach (var action in cardToPlay.actionList)
                            {
                                await action.ExecuteCardAction(entityView.EntityData, targets, null);
                            }
                        }
                        
                        hasActed = true;
                    }
                    else
                    {
                        // 공격 범위에 있지만 사용할 수 있는 카드가 없는 경우 (코스트 부족 또는 쿨타임)
                        // 제자리에서 대기(휴식)하여 스테미나를 회복
                        if (lifeData != null)
                        {
                            lifeData.stamina.Current += lifeData.lifeStat.staminaRecoverAmount;
                            
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
