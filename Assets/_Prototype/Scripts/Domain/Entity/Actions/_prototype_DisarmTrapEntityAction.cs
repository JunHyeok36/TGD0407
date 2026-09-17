using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TDG0407._prototype
{
    [Serializable]
    public class _prototype_DisarmTrapEntityAction : _prototype_EntityAction
    {
        public override async UniTask ExecuteAction(
            _prototype_EntityData source,
            IEnumerable<_prototype_EntityData> targets,
            _prototype_IActionParams @params)
        {
            List<_prototype_TrapComponentData> trapsToDisarm = new();

            if (targets != null)
            {
                foreach (var target in targets)
                {
                    if (target.TryGetComponent<_prototype_TrapComponentData>(out var trapComp))
                    {
                        if (!trapsToDisarm.Contains(trapComp)) trapsToDisarm.Add(trapComp);
                    }
                }
            }

            if (@params is _prototype_CardActionParams cardParams && cardParams.TargetedPoint != default)
            {
                var pv = _prototype_GridManager.Instance?.GetPointView(cardParams.TargetedPoint);
                if (pv != null)
                {
                    var views = pv.PlacedEntityViews.ToList();
                    foreach (var view in views)
                    {
                        if (view != null && view.EntityData != null && view.EntityData.TryGetComponent<_prototype_TrapComponentData>(out var t))
                        {
                            if (!trapsToDisarm.Contains(t)) trapsToDisarm.Add(t);
                        }
                    }
                }
            }

            foreach (var t in trapsToDisarm)
            {
                await t.Disarm();
            }
        }
    }
}
