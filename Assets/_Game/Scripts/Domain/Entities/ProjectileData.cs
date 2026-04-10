using System;

namespace TDG0407.Domain.Entities
{

    using Core.Grid;
    using Domain.Exceptions;

    /// <summary>
    /// 투사체 Entity 데이터입니다.
    /// </summary>
    [Serializable]
    public class ProjectileData : ObstacleData
    {
        #region Fields

        public int damage = 0;

        public Point startPosition = Point.zero;
        public Point targetPosition = Point.zero;
        public Point velocity = Point.zero;


        #endregion
        #region Constructors

        public ProjectileData() : base() { }

        #endregion
        #region Methods

        public override void ValidateData()
        {
            base.ValidateData();
            if(damage < 0) throw new DataValidityViolationException("Invalid value assigned to Projectile Damage.");
        }

        #endregion
    }

}