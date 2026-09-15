using System;

namespace TDG0407._prototype
{
    [Serializable]
    public sealed class _prototype_PlayModeCondition : _prototype_Condition
    {
        public _prototype_PlayMode requiredMode = _prototype_PlayMode.Exploration;

        public override _prototype_ConditionResult Evaluate(
            _prototype_ConditionContext context)
        {
            return context.PlayMode == requiredMode
                ? _prototype_ConditionResult.Success()
                : _prototype_ConditionResult.Fail($"Requires play mode: {requiredMode}.");
        }
    }
}
