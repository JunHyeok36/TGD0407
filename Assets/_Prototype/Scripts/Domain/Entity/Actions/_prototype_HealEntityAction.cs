using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace TDG0407._prototype
{
    /// <summary>
    /// 대상 엔티티의 체력 및 스태미나를 회복시키는 엔티티 액션 (포션 및 회복 스킬용)
    /// </summary>
    [Serializable]
    public class _prototype_HealEntityAction : _prototype_EntityAction
    {
        [Tooltip("회복할 체력 수치")]
        public int healAmount = 10;

        [Tooltip("회복할 스태미나 수치")]
        public int staminaAmount = 0;

        public _prototype_HealEntityAction() { }

        public _prototype_HealEntityAction(int healAmount, int staminaAmount = 0)
        {
            this.healAmount = healAmount;
            this.staminaAmount = staminaAmount;
        }

        public override UniTask ExecuteAction(
            _prototype_EntityData source,
            IEnumerable<_prototype_EntityData> targets,
            _prototype_IActionParams @params)
        {
            if (targets == null) return UniTask.CompletedTask;

            foreach (var target in targets)
            {
                if (target is _prototype_LifeData life)
                {
                    if (healAmount > 0)
                    {
                        life.health.Current = Mathf.Min(life.health.Max, life.health.Current + healAmount);
                    }
                    if (staminaAmount > 0)
                    {
                        life.stamina.Current = Mathf.Min(life.stamina.Max, life.stamina.Current + staminaAmount);
                    }
                }
            }

            return UniTask.CompletedTask;
        }
    }
}
