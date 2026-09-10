using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TDG0407._prototype
{
    [Serializable]
    public abstract class _prototype_EnemyAILogic
    {
        [Range(0f, 1f)]
        [Tooltip("AI의 실수 빈도 (0.0: 완벽한 판단, 1.0: 항상 실수). 실수 발생 시 SP가 0이 되어 녹다운(Knockdown)에 빠지는 위험을 무시하고 무리하게 카드를 발동하거나 휴식을 취하지 않습니다.")]
        public float mistakeRate = 0.05f;

        [NonSerialized] public _prototype_CardData plannedCard;
        [NonSerialized] public _prototype_Point plannedTarget;
        [NonSerialized] public bool hasPlannedIntent;

        public virtual void EvaluateIntent(_prototype_EntityView entityView) { }
        public virtual UniTask ExecuteAction(_prototype_EntityView entityView) { return UniTask.CompletedTask; }
        public abstract UniTask<_prototype_TickIntent> PlanAction(_prototype_EntityView entityView);
        public abstract _prototype_EnemyAILogic Clone();

        public virtual bool RollMistake()
        {
            if (mistakeRate <= 0f) return false;
            if (mistakeRate >= 1f) return true;
            return UnityEngine.Random.value < mistakeRate;
        }

        public virtual int GetCardStaminaCost(_prototype_LifeData lifeData, _prototype_CardData card)
        {
            if (card?.costValue == null || lifeData == null) return 0;
            switch (card.costValue.costType)
            {
                case _prototype_CostType.FixedStamina:
                    return (int)card.costValue.value;
                case _prototype_CostType.MaxStaminaRatio:
                    return UnityEngine.Mathf.RoundToInt(lifeData.stamina.Max * (card.costValue.value / 100f));
                case _prototype_CostType.CurrentStaminaRatio:
                    return UnityEngine.Mathf.RoundToInt(lifeData.stamina.Current * (card.costValue.value / 100f));
                default:
                    return 0;
            }
        }

        public virtual bool CanAffordCard(_prototype_LifeData lifeData, _prototype_CardData card, bool allowMistake = true)
        {
            if (lifeData == null || card == null) return false;
            if (card.costValue == null) return true;

            if (card.costValue.costType == _prototype_CostType.FixedHealth)
            {
                if (lifeData.health.Current <= card.costValue.value) return false;
            }

            int spCost = GetCardStaminaCost(lifeData, card);
            if (spCost > 0)
            {
                if (lifeData.stamina.Current < spCost) return false;

                // SP가 0 이하가 되어 녹다운(Knockdown)에 빠지는 것을 방지
                if (lifeData.stamina.Current <= spCost)
                {
                    bool isMistake = allowMistake && RollMistake();
                    if (!isMistake)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        protected async UniTask<bool> HandleStatusEffectsOverride(_prototype_EntityView entityView)
        {
            var lifeData = entityView.EntityData as _prototype_LifeData;
            if (lifeData == null) return false;

            var fearEffect = lifeData.GetStatusEffect(_prototype_StatusType.Fear);
            if (fearEffect != null && fearEffect.sourceEntity != null)
            {
                if (!lifeData.HasStatusEffect(_prototype_StatusType.Freeze))
                {
                    // Move away
                    _prototype_Point p = entityView.Point;
                    var pts = new _prototype_Point[] {
                        new _prototype_Point(p.x + 1, p.y),
                        new _prototype_Point(p.x - 1, p.y),
                        new _prototype_Point(p.x, p.y + 1),
                        new _prototype_Point(p.x, p.y - 1)
                    };
                    _prototype_Point bestPt = entityView.Point;
                    int maxDist = UnityEngine.Mathf.Abs(bestPt.x - fearEffect.sourceEntity.point.x) + UnityEngine.Mathf.Abs(bestPt.y - fearEffect.sourceEntity.point.y);
                    foreach (var pt in pts)
                    {
                        var ptView = _prototype_GridManager.Instance.GetPointView(pt);
                        if (ptView != null && ptView.CanPlaceEntity(lifeData))
                        {
                            int dist = UnityEngine.Mathf.Abs(pt.x - fearEffect.sourceEntity.point.x) + UnityEngine.Mathf.Abs(pt.y - fearEffect.sourceEntity.point.y);
                            if (dist > maxDist)
                            {
                                maxDist = dist;
                                bestPt = pt;
                            }
                        }
                    }
                    if (bestPt != entityView.Point)
                    {
                        var nextStep = _prototype_GridManager.Instance.GetPointView(bestPt);
                        await _prototype_InteractionManager.MoveEntity(entityView, _prototype_GridManager.Instance.GetPointView(entityView.Point), nextStep);
                    }
                }
                return true; // Overridden by fear
            }
            return false;
        }
    }
}
