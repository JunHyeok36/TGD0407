using System;
using UnityEngine;
using DG.Tweening;

namespace TDG0407._prototype
{ 

    public abstract class _prototype_EntityView : MonoBehaviour
    {

        [Header("Dev References")]
        [SerializeField] private _prototype_EntityDataModel entityDataModel;
        
        
        protected _prototype_EntityData _entityData;
        public _prototype_EntityData EntityData { get { return _entityData; } }

        public _prototype_Point Point { get { return _entityData.point; } }

        protected virtual Vector3 LocalPositionOffset => Vector3.zero;

        public virtual void Initialize(_prototype_PointView pointView)
        {
            if (entityDataModel == null) throw new Exception("Entity Data Model is not assigned.");

            if (entityDataModel is _prototype_LifeDataModel lifeDataModel)
            {
                _entityData = lifeDataModel.CreateLifeData();
            }
            else if (entityDataModel is _prototype_ObstacleDataModel obstacleDataModel)
            {
                _entityData = obstacleDataModel.CreateObstacleData();
            }
            else
            {
                throw new Exception("Unsupported Entity Data Model type.");
            }
            _entityData.point = pointView.Point;

            if (TryGetComponent(out _prototype_EnemyAIController enemyAIController))
            {
                enemyAIController.Initialize(this);
            }
        }

        public virtual async Cysharp.Threading.Tasks.UniTask MoveTo(_prototype_PointView targetPointView)
        {
            if (this == null || gameObject == null || targetPointView == null) return;
            
            transform.SetParent(targetPointView.transform, true);

            var moveTask = transform.DOLocalMove(LocalPositionOffset, 0.2f).SetEase(Ease.InOutSine).AsyncWaitForCompletion();
            
            FaceTowards(targetPointView.Point, 0.2f);
            
            await moveTask;
            
            _entityData.point = targetPointView.Point;
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