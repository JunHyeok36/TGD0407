using System;
using UnityEngine;

namespace TDG0407._prototype
{
    public enum _prototype_ConditionTargetSource
    {
        Target,
        Actor
    }

    [Serializable]
    public class _prototype_EntityStateCondition : _prototype_Condition
    {
        public _prototype_ConditionTargetSource checkTarget = _prototype_ConditionTargetSource.Target;
        public string stateKey;
        public string expectedValue;

        public override _prototype_ConditionResult Evaluate(_prototype_ConditionContext context)
        {
            if (context == null) return _prototype_ConditionResult.Fail("Context is null.");

            _prototype_EntityData targetEntity = checkTarget == _prototype_ConditionTargetSource.Target
                ? context.Target
                : context.Actor;

            if (targetEntity == null)
            {
                return _prototype_ConditionResult.Fail($"Target entity ({checkTarget}) is null.");
            }

            if (targetEntity.components == null)
            {
                return _prototype_ConditionResult.Fail("Entity has no components.");
            }

            _prototype_StateComponentData stateComp = null;
            foreach (var comp in targetEntity.components)
            {
                if (comp is _prototype_StateComponentData sc)
                {
                    stateComp = sc;
                    break;
                }
            }

            if (stateComp == null)
            {
                return _prototype_ConditionResult.Fail("Entity does not have StateComponentData.");
            }

            string actualValue = stateComp.GetState(stateKey);
            bool isMatch = string.Equals(actualValue, expectedValue, StringComparison.OrdinalIgnoreCase);

            if (isMatch)
            {
                return _prototype_ConditionResult.Success();
            }

            return _prototype_ConditionResult.Fail($"State '{stateKey}' is '{actualValue}', expected '{expectedValue}'.");
        }
    }
}
