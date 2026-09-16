using System;
using UnityEngine;

namespace TDG0407._prototype
{
    /// <summary>
    /// 시전자(Actor)가 특정 아이템을 지정 수량 이상 보유하고 있는지 검사하는 조건
    /// (예: 던전 문 열기 위한 열쇠 보유, 발전기 작동을 위한 배터리 보유 등)
    /// </summary>
    [Serializable]
    public sealed class _prototype_HasItemCondition : _prototype_Condition
    {
        [Tooltip("필요한 아이템 ID")]
        public string requiredItemId;

        [Tooltip("필요한 최소 수량 (기본 1)")]
        [Min(1)]
        public int requiredCount = 1;

        [Tooltip("상호작용 성공 시 해당 아이템을 인벤토리에서 자동 소모할지 여부")]
        public bool consumeOnSuccess = false;

        public _prototype_HasItemCondition() { }

        public _prototype_HasItemCondition(string requiredItemId, int requiredCount = 1, bool consumeOnSuccess = false)
        {
            this.requiredItemId = requiredItemId;
            this.requiredCount = Math.Max(1, requiredCount);
            this.consumeOnSuccess = consumeOnSuccess;
        }

        public override _prototype_ConditionResult Evaluate(_prototype_ConditionContext context)
        {
            if (string.IsNullOrEmpty(requiredItemId))
            {
                return _prototype_ConditionResult.Success();
            }

            if (context.Actor is _prototype_LifeData life)
            {
                if (life.inventory != null && life.inventory.HasItem(requiredItemId, requiredCount))
                {
                    if (consumeOnSuccess)
                    {
                        life.inventory.ConsumeItem(requiredItemId, requiredCount);
                    }
                    return _prototype_ConditionResult.Success();
                }

                return _prototype_ConditionResult.Fail($"Requires {requiredCount} of {requiredItemId}.");
            }

            return _prototype_ConditionResult.Fail("Actor does not have an inventory.");
        }
    }
}
