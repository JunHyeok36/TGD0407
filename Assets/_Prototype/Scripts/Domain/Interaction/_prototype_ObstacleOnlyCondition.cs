using System;

namespace TDG0407._prototype
{
    [Serializable]
    public sealed class _prototype_ObstacleOnlyCondition : _prototype_Condition
    {
        public override _prototype_ConditionResult Evaluate(
            _prototype_ConditionContext context)
        {
            return context.Target is _prototype_ObstacleData
                ? _prototype_ConditionResult.Success()
                : _prototype_ConditionResult.Fail("Target must be an Obstacle.");
        }
    }
}
