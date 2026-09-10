namespace TDG0407._prototype
{
    public readonly struct _prototype_InteractionConditionResult
    {
        public bool IsSatisfied { get; }
        public string FailureReason { get; }

        private _prototype_InteractionConditionResult(bool isSatisfied, string failureReason)
        {
            IsSatisfied = isSatisfied;
            FailureReason = failureReason;
        }

        public static _prototype_InteractionConditionResult Success() => new(true, null);
        public static _prototype_InteractionConditionResult Fail(string reason) => new(false, reason);
    }
}