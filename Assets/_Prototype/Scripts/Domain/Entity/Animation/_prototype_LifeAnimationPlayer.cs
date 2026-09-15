using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace TDG0407._prototype
{
    public sealed class _prototype_LifeAnimationPlayer : _prototype_EntityAnimationPlayer
    {
        public override async UniTask PlayUniqueAnimation(
            _prototype_EntityAnimationType animationType,
            Vector3 direction)
        {
            if (animationType != _prototype_EntityAnimationType.Attack)
                return;

            await transform.DOPunchPosition(direction * 0.3f, 0.2f, 1, 0)
                .AsyncWaitForCompletion();
        }
    }
}
