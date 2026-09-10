namespace TDG0407._prototype
{
    /// <summary>
    /// 현재 게임 플레이 모드를 나타냅니다.
    /// </summary>
    public enum _prototype_PlayMode
    {
        /// <summary>전투 모드 — 적이 생존 중이며 카드 사용과 전투 행동이 활성화됩니다.</summary>
        Battle,
        /// <summary>탐색 모드 — 적이 없으며 이동 중심 탐색이 허용됩니다. Utility/Communication/None 카드만 사용 가능.</summary>
        Exploration
    }
}
