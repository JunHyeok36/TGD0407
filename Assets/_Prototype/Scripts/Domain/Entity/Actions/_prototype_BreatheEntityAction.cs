using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TDG0407._prototype
{
    /// <summary>
    /// 기류 호흡 (Breathe) 액션
    /// - 카드 1장 드로우
    /// - 기류 축적(Flow Gauge) 1스택 추가
    /// </summary>
    [Serializable]
    public class _prototype_BreatheEntityAction : _prototype_EntityAction
    {
        public int drawCount = 1;

        public override UniTask ExecuteAction(
            _prototype_EntityData source,
            IEnumerable<_prototype_EntityData> targets,
            _prototype_IActionParams @params)
        {
            if (source == null) return UniTask.CompletedTask;
            var sourceLife = source as _prototype_LifeData;
            if (sourceLife == null || sourceLife.IsDead) return UniTask.CompletedTask;

            // 1. 카드 1장 드로우
            int maxHand = sourceLife.lifeStat != null ? sourceLife.lifeStat.handCardSlotCount : 5;
            sourceLife.cardDeck?.DrawCards(drawCount, maxHand);

            // 2. 기류 축적 1스택 추가
            if (sourceLife.uniquePassive is _prototype_FlowGaugePassiveData flowPassive)
            {
                flowPassive.AddFlowStack(sourceLife);
            }

            var sourceView = _prototype_GridManager.Instance?.GetPointView(source.point)?.PlacedEntityViews.Find(v => v.EntityData == source);
            if (sourceView != null)
            {
                _prototype_FloatingText.SpawnOnEntity(sourceView, "기류 호흡! (+1 드로우)", new Color(0.5f, 0.9f, 1f), 1.15f);
            }

            return UniTask.CompletedTask;
        }
    }
}
