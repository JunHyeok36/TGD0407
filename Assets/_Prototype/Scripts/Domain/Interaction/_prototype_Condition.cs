using System;

namespace TDG0407._prototype
{
    [Serializable]
    public abstract class _prototype_Condition
    {
        public abstract _prototype_ConditionResult Evaluate(
            _prototype_ConditionContext context);
    }
}
