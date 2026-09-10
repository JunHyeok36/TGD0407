using System;

namespace TDG0407._prototype
{
    [Serializable]
    public abstract class _prototype_InteractionCondition
    {
        public abstract _prototype_InteractionConditionResult Evaluate(
            _prototype_InteractionContext context);
    }
}