using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TDG0407._prototype
{
    /// <summary>
    /// 바람 장막 (Windwall) 액션
    /// - 3틱간 [10 + 최대 체력 4%]의 보호막 획득
    /// - 1틱간 저지 불가 (Unstoppable) 효과 획득
    /// </summary>
    [Serializable]
    public class _prototype_WindwallEntityAction : _prototype_EntityAction
    {
        public int baseShield = 10;
        public float maxHealthRatio = 0.04f;
        public int shieldDuration = 3;
        public int unstoppableDuration = 1;

        public override UniTask ExecuteAction(
            _prototype_EntityData source,
            IEnumerable<_prototype_EntityData> targets,
            _prototype_IActionParams @params)
        {
            if (source == null) return UniTask.CompletedTask;
            var sourceLife = source as _prototype_LifeData;
            if (sourceLife == null || sourceLife.IsDead) return UniTask.CompletedTask;

            int maxHp = sourceLife.health != null ? sourceLife.health.Max : 100;
            int shieldAmount = baseShield + Mathf.RoundToInt(maxHp * maxHealthRatio);

            // 저지 불가 1틱 부여
            sourceLife.ApplyStatusEffect(new _prototype_StatusEffect(_prototype_StatusType.Unstoppable, unstoppableDuration));

            // 기획 명세: 3틱간 [10 + 최대 체력 4%]의 보호막 획득
            sourceLife.AddShield(shieldAmount, shieldDuration);

            var sourceView = _prototype_GridManager.Instance?.GetPointView(source.point)?.PlacedEntityViews.Find(v => v.EntityData == source);
            if (sourceView != null)
            {
                _prototype_FloatingText.SpawnOnEntity(sourceView, $"바람 장막! +{shieldAmount} 보호막", new Color(0.27f, 0.8f, 1f), 1.2f);
            }

            return UniTask.CompletedTask;
        }
    }
}
