using System;

namespace TDG0407.Domain.Entities
{

    using Core.Grid;
    using Domain.Map;

    /// <summary>
    /// 워프 포인트의 상태입니다.
    /// </summary>
    [Serializable]
    public class WarppointState : ObstacleState
    {
        #region Fields

        public readonly RoomPoint roomPoint = new();

        #endregion
        #region Constructors

        public WarppointState() : base() { }

        #endregion
        #region Methods

        public override void Initialize()
        {
            base.Initialize();
        }

        #endregion
    }
    
}