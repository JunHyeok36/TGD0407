namespace TDG0407.Domain.Map
{

    /// <summary>
    /// Room의 유형을 나타내는 열거형입니다.
    /// </summary>
    public enum RoomType : short
    {
        Random = -2,
        NULL = -1,

        // 0x000 ~ 0x0FF : game system
        Final = 0x0FF,

        _gamesys_max = 0x0FF,
        // 0xB00 ~ 0xBFF : noraml
        Empty = 0xB00,
        Battle = 0xB01,
        SoftBattle = 0xB02,
        HardBattle = 0xB03,
        Puzzle = 0xB04,

        _normal_max = 0xBFF,
        // 0xE00 ~ 0xEFF : event
        Shop = 0xE00,

        _event_max = 0xEFF,
        // 0xC00 ~ 0xCFF : secret
        JustChest = 0xC00,

        

        _secret_max = 0xCFF,
        // 0x200 ~ 0x2FF : twin room

        // 0x300 ~ 0x3FF : third room
    }
    
}