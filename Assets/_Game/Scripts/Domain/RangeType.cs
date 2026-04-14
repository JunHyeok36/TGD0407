namespace TDG0407.Domain
{
    
    /// <summary>
    /// 범위의 유형을 나타냅니다.
    /// </summary>
    public enum RangeType : sbyte
    {
        NULL = -1,
        /// <summary>
        /// 범위의 point를 하나씩 지정하는 형식
        /// </summary>
        Default,
        /// <summary>
        /// 범위의 꼭지점을 지정하는 형식
        /// </summary>
        Polygon,
        /// <summary>
        /// PointArea 첫번째 요소에 타원의 방정식의 (a, b) 값을 지정하는 형식
        /// </summary>
        Circle,
        /// <summary>
        /// <para>PointArea 첫번째 요소에 직선의 방정식이 지나는 두 점 중 (0, 0)과 (x,y)를 지정하는 형식</para>
        /// <para>(두번째 요소에 왼쪽으로 확장할 단계 양수 a와 오른쪽으로 확장할 단계 양수 b를 지정합니다. -1을 지정하면 무한하게 확장됩니다.)</para>
        /// </summary>
        Line,
    }

}