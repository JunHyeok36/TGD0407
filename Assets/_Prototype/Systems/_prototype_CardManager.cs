using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TDG0407._prototype
{
    public static class _prototype_CardManager
    {
        public static bool CanCast(
            _prototype_LifeData caster,
            _prototype_CardData card,
            _prototype_Point castPoint)
        {
            if (caster == null) return false;
            if (card == null) return false;
            if (_prototype_GridManager.Instance == null) return false;
            if (card.IsCoolingDown) return false;
            if (!CanPayCost(caster, card.costValue)) return false;
            return card.IsCastableAt(caster.point, castPoint);
        }

        public static async UniTask<bool> TryCastCard(
            _prototype_LifeView casterView,
            _prototype_CardData card,
            _prototype_Point castPoint)
        {
            if (casterView == null) throw new ArgumentNullException(nameof(casterView));
            _prototype_LifeData caster = casterView.Data;
            if (!CanCast(caster, card, castPoint)) return false;
            if (!caster.cardDeck.handedCardDatas.Contains(card)) return false;

            List<_prototype_EntityData> targets = ResolveTargets(caster.point, castPoint, card.targetRange);

            SpendCost(caster, card.costValue);
            _prototype_CardActionParams actionParams = new(card, caster.point, castPoint);
            List<_prototype_ICardAction> actions = card.actionList ?? new List<_prototype_ICardAction>();
            for (int i = 0; i < actions.Count; i++)
            {
                if (actions[i] == null) continue;
                await actions[i].ExecuteCardAction(caster, targets, actionParams);
            }

            card.SetOnCooldown();
            caster.cardDeck.TryDiscardFromHand(card);
            caster.cardDeck.Draw(1);

            return true;
        }

        public static void TickDeck(_prototype_CardDeck deck)
        {
            if (deck == null) return;
            deck.TickCooldowns();
        }

        public static bool CanPayCost(_prototype_LifeData caster, _prototype_CostValue cost)
        {
            if (cost == null || cost.costType == _prototype_CostType.None) return true;
            float required = Mathf.Max(0f, cost.value);

            return cost.costType switch
            {
                _prototype_CostType.FixedHealth => caster.health.Current > required,
                _prototype_CostType.FixedStamina => caster.stamina.Current >= required,
                _prototype_CostType.CurrentHealthRatio => caster.health.Current >= caster.health.Current * required,
                _prototype_CostType.CurrentStaminaRatio => caster.stamina.Current >= caster.stamina.Current * required,
                _prototype_CostType.LostHealthRatio => caster.health.Current >= (caster.health.Max - caster.health.Current) * required,
                _prototype_CostType.LostStaminaRatio => caster.stamina.Current >= (caster.stamina.Max - caster.stamina.Current) * required,
                _prototype_CostType.MaxHealthRatio => caster.health.Current >= caster.health.Max * required,
                _prototype_CostType.MaxStaminaRatio => caster.stamina.Current >= caster.stamina.Max * required,
                _ => false
            };
        }

        public static void SpendCost(_prototype_LifeData caster, _prototype_CostValue cost)
        {
            if (cost == null || cost.costType == _prototype_CostType.None) return;

            int spendValue = EvaluateCostValue(caster, cost);
            switch (cost.costType)
            {
                case _prototype_CostType.FixedHealth:
                case _prototype_CostType.CurrentHealthRatio:
                case _prototype_CostType.LostHealthRatio:
                case _prototype_CostType.MaxHealthRatio:
                    caster.health.Current -= spendValue;
                    break;
                case _prototype_CostType.FixedStamina:
                case _prototype_CostType.CurrentStaminaRatio:
                case _prototype_CostType.LostStaminaRatio:
                case _prototype_CostType.MaxStaminaRatio:
                    caster.stamina.Current -= spendValue;
                    break;
            }
        }

        private static int EvaluateCostValue(_prototype_LifeData caster, _prototype_CostValue cost)
        {
            float value = Mathf.Max(0f, cost.value);
            return cost.costType switch
            {
                _prototype_CostType.FixedHealth => Mathf.CeilToInt(value),
                _prototype_CostType.FixedStamina => Mathf.CeilToInt(value),
                _prototype_CostType.CurrentHealthRatio => Mathf.CeilToInt(caster.health.Current * value),
                _prototype_CostType.CurrentStaminaRatio => Mathf.CeilToInt(caster.stamina.Current * value),
                _prototype_CostType.LostHealthRatio => Mathf.CeilToInt((caster.health.Max - caster.health.Current) * value),
                _prototype_CostType.LostStaminaRatio => Mathf.CeilToInt((caster.stamina.Max - caster.stamina.Current) * value),
                _prototype_CostType.MaxHealthRatio => Mathf.CeilToInt(caster.health.Max * value),
                _prototype_CostType.MaxStaminaRatio => Mathf.CeilToInt(caster.stamina.Max * value),
                _ => 0
            };
        }

        private static List<_prototype_EntityData> ResolveTargets(
            _prototype_Point sourcePoint,
            _prototype_Point castPoint,
            _prototype_ITargetRangeSelector targetRangeSelector)
        {
            List<_prototype_Point> targetPoints = targetRangeSelector == null
                ? new List<_prototype_Point> { castPoint }
                : targetRangeSelector.GetValidTargetPoints(sourcePoint, castPoint);

            List<_prototype_EntityData> targets = new();
            for (int i = 0; i < targetPoints.Count; i++)
            {
                _prototype_Point point = targetPoints[i];
                _prototype_PointView pointView = _prototype_GridManager.Instance.GetPointView(point);
                if (pointView == null) continue;

                IReadOnlyList<_prototype_EntityData> entities = pointView.PlacedEntityDatas;
                for (int j = 0; j < entities.Count; j++)
                {
                    _prototype_EntityData target = entities[j];
                    if (target != null) targets.Add(target);
                }
            }

            return targets.Distinct().ToList();
        }
    }
}
