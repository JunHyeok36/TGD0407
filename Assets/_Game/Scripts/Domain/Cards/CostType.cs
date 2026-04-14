namespace TDG0407.Domain.Cards
{

    /// <summary>
    /// 주요 소모 자원을 나타냅니다.
    /// </summary>
    public enum CostType : sbyte
    {
        NULL = -1,
        None = 0,
        FixedHealth,
        CurrentHealthRatio,
        MaxHealthRatio,
        FixedStamina,
        CurrentStaminaRatio,
        MaxStaminaRatio,
        Other
    }

}