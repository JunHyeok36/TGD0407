namespace TDG0407._prototype
{
    public readonly struct _prototype_ConditionResult
    {
        public bool IsSatisfied { get; }
        public string FailureReason { get; }

        private _prototype_ConditionResult(bool isSatisfied, string failureReason)
        {
            IsSatisfied = isSatisfied;
            FailureReason = failureReason;
        }

        public static _prototype_ConditionResult Success() => new(true, null);
        public static _prototype_ConditionResult Fail(string reason) => new(false, reason);
    }
}
