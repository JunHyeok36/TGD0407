using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace TDG0407._prototype
{
    public class _prototype_EntityAnimationPlayer : MonoBehaviour
    {
        public virtual async UniTask PlayHitAnimation()
        {
            if (this == null || gameObject == null)
                return;

            await transform.DOShakePosition(0.2f, 0.2f, 10, 90, false, true)
                .AsyncWaitForCompletion();
        }

        public virtual async UniTask PlayMoveAnimation(
            _prototype_PointView targetPointView,
            Vector3 localPositionOffset)
        {
            if (this == null || gameObject == null || targetPointView == null)
                return;

            transform.SetParent(targetPointView.transform, true);
            await transform.DOLocalMove(localPositionOffset, 0.2f)
                .SetEase(Ease.InOutSine)
                .AsyncWaitForCompletion();
        }

        public virtual UniTask PlayUniqueAnimation(
            _prototype_EntityAnimationType animationType,
            Vector3 direction)
        {
            return UniTask.CompletedTask;
        }
    }
}
