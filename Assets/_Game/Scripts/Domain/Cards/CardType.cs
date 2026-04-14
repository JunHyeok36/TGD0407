namespace TDG0407.Domain.Cards
{

    /// <summary>
    /// 카드의 유형을 나타냅니다.
    /// </summary>
    public enum CardType : sbyte
    {
        NULL = -1,
        Normal = 0,
        Attack = 1,
        Skill = 2,

        Curse = 9,
        Interaction = 10,
    }

}