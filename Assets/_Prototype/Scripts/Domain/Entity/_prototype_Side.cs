using System;

namespace TDG0407._prototype
{
    public enum _prototype_Side : sbyte
    {
        None = 0, 
        A = 1, // Player Default Side
        B = 2 // Enemy Default Side
    }

    public static class _prototype_EntitySideExtensions
    {
        /// <summary>
        /// 엔티티의 소속 진영(Side)을 반환합니다.
        /// 투사체(Projectile)의 경우 자체 side 또는 생성자(shooter)의 side를 반환합니다.
        /// </summary>
        public static _prototype_Side GetSide(this _prototype_EntityData entity)
        {
            if (entity == null) return _prototype_Side.None;
            if (entity is _prototype_LifeData life) return life.side;
            if (entity is _prototype_ProjectileData proj)
            {
                if (proj.side != _prototype_Side.None) return proj.side;
                if (proj.shooter is _prototype_LifeData shooterLife) return shooterLife.side;
            }
            if (entity is _prototype_AreaEffectData area) return area.side;
            return _prototype_Side.None;
        }

        /// <summary>
        /// 두 엔티티가 동일한 진영(Side)에 속해 있는지, 혹은 생성자(shooter) 관계인지를 확인합니다.
        /// 투사체와 생성자, 혹은 동일한 진영이 생성한 투사체 간에는 true를 반환합니다.
        /// </summary>
        public static bool IsSameSide(this _prototype_EntityData a, _prototype_EntityData b)
        {
            if (a == null || b == null) return false;
            if (ReferenceEquals(a, b)) return true;

            // 직접 생성자 관계 검사 (투사체와 생성자)
            if (a is _prototype_ProjectileData projA && ReferenceEquals(projA.shooter, b))
                return true;
            if (b is _prototype_ProjectileData projB && ReferenceEquals(projB.shooter, a))
                return true;

            // 동일한 생성자로부터 생성된 투사체 간 검사
            if (a is _prototype_ProjectileData pA && b is _prototype_ProjectileData pB && pA.shooter != null && ReferenceEquals(pA.shooter, pB.shooter))
                return true;

            var sideA = a.GetSide();
            var sideB = b.GetSide();

            if (sideA == _prototype_Side.None || sideB == _prototype_Side.None)
                return false;

            return sideA == sideB;
        }
    }
}
