using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using UnityEngine;

namespace TDG0407._prototype
{ 

    public abstract class _prototype_EntityView : MonoBehaviour
    {

        [Header("Dev References")]
        [SerializeField] private _prototype_EntityDataModel DEV_entityDataModel;

        protected _prototype_EntityAnimationPlayer AnimationPlayer { get; set; }
        
        
        protected _prototype_EntityData _entityData;
        public _prototype_EntityData EntityData { get { return _entityData; } }

        public _prototype_Point Point { get { return _entityData.point; } }

        protected virtual Vector3 LocalPositionOffset => Vector3.zero;

        public virtual void Initialize(
            _prototype_EntityData entityData,
            _prototype_PointView pointView)
        {
            if (pointView == null) throw new ArgumentNullException(nameof(pointView));

            _entityData = DEV_entityDataModel != null
                ? DEV_entityDataModel.CreateData(entityData)
                : entityData ?? throw new Exception("An EntityDataModel or EntityData is required.");
            _entityData.point = pointView.Point;
            if (TryGetComponent(out _prototype_EntityAnimationPlayer animationPlayer))
            {
                AnimationPlayer = animationPlayer;
            }
            else
            {
                throw new Exception($"{name} requires an _prototype_EntityAnimationPlayer component.");
            }

            if (TryGetComponent(out _prototype_EnemyAIController enemyAIController))
            {
                enemyAIController.Initialize(this);
            }

            if (_entityData.components != null)
            {
                foreach (var comp in _entityData.components)
                {
                    if (comp != null) comp.Initialize(this);
                }
            }

            _prototype_TickManager.RegisterPostTick(OnPostTick);
        }

        protected virtual void OnDestroy()
        {
            _prototype_TickManager.UnregisterPostTick(OnPostTick);
        }

        private async UniTask OnPostTick()
        {
            if (this == null || gameObject == null || _entityData == null || _entityData.components == null) return;
            foreach (var comp in _entityData.components)
            {
                if (comp != null) await comp.OnTick(this);
            }
        }

        public virtual async UniTask MoveTo(_prototype_PointView targetPointView)
        {
            if (this == null || gameObject == null || targetPointView == null) return;

            transform.SetParent(targetPointView.transform, true);

            FaceTowards(targetPointView.Point, 0.2f);
            if (AnimationPlayer != null)
            {
                await AnimationPlayer.PlayMoveAnimation(targetPointView, LocalPositionOffset);
            }
            else
            {
                transform.localPosition = LocalPositionOffset;
            }
            
            if (_entityData != null)
            {
                _entityData.point = targetPointView.Point;
            }
        }

        public virtual void SetPointImmediate(_prototype_PointView targetPointView)
        {
            if (this == null || gameObject == null || targetPointView == null) return;

            transform.SetParent(targetPointView.transform, false);
            transform.localPosition = LocalPositionOffset;
            if (_entityData != null)
            {
                _entityData.point = targetPointView.Point;
            }
        }

        public UniTask PlayHitAnimation()
        {
            if (AnimationPlayer != null) return AnimationPlayer.PlayHitAnimation();
            return UniTask.CompletedTask;
        }

        public UniTask PlayUniqueAnimation(
            _prototype_EntityAnimationType animationType,
            Vector3 direction)
        {
            if (AnimationPlayer != null) return AnimationPlayer.PlayUniqueAnimation(animationType, direction);
            return UniTask.CompletedTask;
        }

        public virtual void FaceTowards(_prototype_Point targetPoint, float duration = 0.2f)
        {
            if (!(_entityData is _prototype_LifeData || _entityData is _prototype_ProjectileData || _entityData is _prototype_LaserProjectileData)) return;

            var targetPointView = _prototype_GridManager.Instance.GetPointView(targetPoint);
            if (targetPointView == null) return;

            Vector3 dir = targetPointView.transform.position - transform.position;
            dir.y = 0;
            if (dir != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(dir.normalized);
                if (duration > 0f)
                {
                    transform.DORotateQuaternion(targetRot, duration).SetEase(Ease.InOutSine);
                }
                else
                {
                    transform.rotation = targetRot;
                }
            }
        }

    }

}