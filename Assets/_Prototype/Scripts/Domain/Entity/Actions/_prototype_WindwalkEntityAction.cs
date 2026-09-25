using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace TDG0407._prototype
{
    /// <summary>
    /// 바람걸음 (Windwalk) 액션
    /// - 지정된 좌표로 순간이동 (3칸 이내)
    /// - 기류 축적(Flow Gauge) 1스택 추가
    /// </summary>
    [Serializable]
    public class _prototype_WindwalkEntityAction : _prototype_MoveToPointEntityAction
    {
        public override async UniTask ExecuteAction(
            _prototype_EntityData source, 
            IEnumerable<_prototype_EntityData> targets,
            _prototype_IActionParams @params)
        {
            isTeleport = true;
            await base.ExecuteAction(source, targets, @params);

            var sourceLife = source as _prototype_LifeData;
            if (sourceLife != null && !sourceLife.IsDead)
            {
                // 기류 축적 1스택 추가
                if (sourceLife.uniquePassive is _prototype_FlowGaugePassiveData flowPassive)
                {
                    flowPassive.AddFlowStack(sourceLife);
                }

                var sourceView = _prototype_GridManager.Instance?.GetPointView(source.point)?.PlacedEntityViews.Find(v => v.EntityData == source);
                if (sourceView != null)
                {
                    _prototype_FloatingText.SpawnOnEntity(sourceView, "바람걸음! (기류 +1)", new Color(0.4f, 0.9f, 1f), 1.15f);
                }
            }
        }
    }
}
