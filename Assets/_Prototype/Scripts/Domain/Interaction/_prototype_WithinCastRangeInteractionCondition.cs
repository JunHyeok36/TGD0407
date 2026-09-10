using System;
using System.Collections.Generic;
using UnityEngine;

namespace TDG0407._prototype
{
    [Serializable]
    public sealed class _prototype_WithinCastRangeInteractionCondition : _prototype_InteractionCondition
    {
        [UnityEngine.SerializeReference, SubclassSelector]
        public _prototype_ICastRangeSelector castRange = new _prototype_AroundRectCastSelector();

        public override _prototype_InteractionConditionResult Evaluate(
            _prototype_InteractionContext context)
        {
            if (castRange == null)
                return _prototype_InteractionConditionResult.Fail("No interaction range is configured.");

            List<_prototype_Point> points = castRange.GetValidCastPoints(context.Actor.point);
            return points.Contains(context.Target.point)
                ? _prototype_InteractionConditionResult.Success()
                : _prototype_InteractionConditionResult.Fail("The interactable is out of range.");
        }
    }
}