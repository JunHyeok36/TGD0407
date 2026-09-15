using System;

namespace TDG0407._prototype
{
    [Serializable]
    public sealed class _prototype_LifeOnlyCondition : _prototype_Condition
    {
        public override _prototype_ConditionResult Evaluate(
            _prototype_ConditionContext context)
        {
            return context.Actor is _prototype_LifeData
                ? _prototype_ConditionResult.Success()
                : _prototype_ConditionResult.Fail("Only Life entities can interact with this object.");
        }
    }
}
