using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace TDG0407._prototype
{
    [Serializable]
    public class _prototype_ApplyStatusEffectEntityAction : _prototype_EntityAction
    {
        public _prototype_StatusType statusType;
        public int durationTicks;
        public float value;

        public override UniTask ExecuteAction(
            _prototype_EntityData source,
            IEnumerable<_prototype_EntityData> targets,
            _prototype_IActionParams @params)
        {
            if (targets == null) return UniTask.CompletedTask;
            
            var sourceView = _prototype_GridManager.Instance.GetPointView(source.point)?.PlacedEntityViews.Find(v => v.EntityData == source);
            bool hasFaced = false;

            foreach (var target in targets)
            {
                if (!hasFaced && sourceView != null && target != source)
                {
                    sourceView.FaceTowards(target.point, 0.2f);
                    hasFaced = true;
                }

                var targetLife = target as _prototype_LifeData;
                if (targetLife != null)
                {
                    var effect = new _prototype_StatusEffect(statusType, durationTicks, source, target, value);
                    targetLife.ApplyStatusEffect(effect);
                }
            }

            return UniTask.CompletedTask;
        }
    }
}
