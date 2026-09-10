using System;

namespace TDG0407._prototype
{
    [Serializable]
    public sealed class _prototype_LifeOnlyInteractionCondition : _prototype_InteractionCondition
    {
        public override _prototype_InteractionConditionResult Evaluate(
            _prototype_InteractionContext context)
        {
            return context.Actor is _prototype_LifeData
                ? _prototype_InteractionConditionResult.Success()
                : _prototype_InteractionConditionResult.Fail("Only Life entities can interact with this object.");
        }
    }
}