using System;

namespace TDG0407.Domain.Entities
{

    /// <summary>
    /// 상호작용 없는 일반 비생명체의 상태입니다.
    /// </summary>
    [Serializable]
    public class ObstacleState : EntityState
    {
        #region Constructors

        public ObstacleState() : base() { }

        #endregion
        #region Methods

        public override void Initialize()
        {
            base.Initialize();
        }

        #endregion
    }
    
}