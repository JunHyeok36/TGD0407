using System;

namespace TDG0407._prototype
{
    public enum _prototype_TargetAnchorType : sbyte
    {
        /// <summary>
        /// 시전자가 밀리거나 이동하면 이동 변위만큼 조준점과 공격 범위가 함께 이동하여 새 위치에서 재계산됩니다. (근접/방향성 공격 기본값)
        /// </summary>
        FollowCaster = 0,

        /// <summary>
        /// 시전자가 이동하더라도 조준된 바닥 좌표(월드 좌표)가 고정된 채 새 위치에서 공격 범위가 재계산됩니다. (바닥 설치기, 지점 폭격 등)
        /// </summary>
        FixedGround = 1
    }
}
