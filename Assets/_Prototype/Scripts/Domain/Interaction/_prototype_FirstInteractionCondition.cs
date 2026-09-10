using System;

namespace TDG0407._prototype
{
    [Serializable]
    public sealed class _prototype_FirstInteractionCondition : _prototype_InteractionCondition
    {
        public override _prototype_InteractionConditionResult Evaluate(
            _prototype_InteractionContext context)
        {
            return context.Target.interactionCount == 0
                ? _prototype_InteractionConditionResult.Success()
                : _prototype_InteractionConditionResult.Fail("This interactable has already been used.");
        }
    }
}