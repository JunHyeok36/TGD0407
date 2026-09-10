using System;

namespace TDG0407._prototype
{
    [Serializable]
    public sealed class _prototype_PlayModeInteractionCondition : _prototype_InteractionCondition
    {
        public _prototype_PlayMode requiredMode = _prototype_PlayMode.Exploration;

        public override _prototype_InteractionConditionResult Evaluate(
            _prototype_InteractionContext context)
        {
            return context.PlayMode == requiredMode
                ? _prototype_InteractionConditionResult.Success()
                : _prototype_InteractionConditionResult.Fail($"Requires play mode: {requiredMode}.");
        }
    }
}