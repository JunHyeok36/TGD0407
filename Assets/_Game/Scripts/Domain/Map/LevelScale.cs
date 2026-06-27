using System;

namespace TDG0407.Domain.Map
{

    using Core.Grid;

    /// <summary>
    /// 레벨의 크기를 나타냅니다.
    /// </summary>
    public enum LevelScale : sbyte
    {
        Random = -2,
        NULL = -1,
        /// <summary>
        /// 5x5
        /// </summary>
        UltraSmall,
        /// <summary>
        /// 7x7
        /// </summary>
        VerySmall,
        /// <summary>
        /// 9x9
        /// </summary>
        Small,
        /// <summary>
        /// 11x11
        /// </summary>
        Medium,
        /// <summary>
        /// 13x13
        /// </summary>
        Large,
        /// <summary>
        /// 15x15
        /// </summary>
        VeryLarge,
        /// <summary>
        /// 25x25
        /// </summary>
        UltraLarge,
    }

    public static class LevelScaleExtensions
    {

        public static Point GetSize(this LevelScale levelScale)
        {
            return levelScale switch
            {
                LevelScale.UltraSmall => new Point(5, 5),
                LevelScale.VerySmall => new Point(7, 7),
                LevelScale.Small => new Point(9, 9),
                LevelScale.Medium => new Point(11, 11),
                LevelScale.Large => new Point(13, 13),
                LevelScale.VeryLarge => new Point(15, 15),
                LevelScale.UltraLarge => new Point(25, 25),
                _ => throw new ArgumentOutOfRangeException(nameof(levelScale), $"Unhandled level scale: {levelScale}"),
            };
        }

    }

}