namespace TDG0407.Domain
{

    
    /// <summary>
    /// 효과나 카드의 지속 시간을 나타냅니다.
    /// </summary>
    public enum TickDurationType : sbyte
    {
        NULL = -1,
        /// <summary>
        /// 틱 기반의 지속 시간입니다.
        /// </summary>
        TickBased,
        /// <summary>
        /// 지속 시간이 없습니다.
        /// </summary>
        Forever,
        /// <summary>
        /// 지속 시간이 조건에 따라 결정됩니다.
        /// </summary>
        Conditional,
    }

} 