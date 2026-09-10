using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace TDG0407._prototype
{
    public enum _prototype_ProjectileTargetingType
    {
        FixedTrajectory, // 초기 위치를 향해 발사 (유도 안됨)
        Tracking         // 타겟을 계속 추적 (유도됨)
    }

    public enum _prototype_ProjectileEndCondition
    {
        HitOrWall,         // 벽이나 엔티티에 부딪힐 때까지 날아감 (기본)
        StopAtTargetPoint, // 지정한 타겟 위치에 도달하면 소멸 (FixedTrajectory 전용)
        MaxDistance        // 일정 거리만큼만 날아가고 소멸
    }

    [Serializable]
    public class _prototype_SpawnTargetedProjectileEntityAction : _prototype_EntityAction
    {
        public _prototype_ProjectileDataModel projectileModel;
        public GameObject projectilePrefab;
        [SerializeReference, SubclassSelector] public List<_prototype_EntityAction> onHitActions;
        
        public _prototype_ProjectileTargetingType targetingType = _prototype_ProjectileTargetingType.FixedTrajectory;
        public _prototype_ProjectileEndCondition endCondition = _prototype_ProjectileEndCondition.HitOrWall;

        public int speed;
        [Tooltip("매 칸 이동 시 꺾을 수 있는 최대 각도 (0이면 꺾지 않음) - Tracking 및 FixedTrajectory 모두 곡선 비행(유도)에 적용됨")]
        public float homingAnglePerStep = 45f;
        [Tooltip("최대 이동 거리 (MaxDistance 모드에서 사용, 그 외에는 무시됨)")]
        public int maxDistance = 5;

        public override async UniTask ExecuteAction(
            _prototype_EntityData source,
            IEnumerable<_prototype_EntityData> targets,
            _prototype_IActionParams @params)
        {
            var sourceLife = source as _prototype_LifeData;
            _prototype_Side mySide = sourceLife != null ? sourceLife.side : _prototype_Side.None;

            if (projectileModel == null || projectilePrefab == null)
            {
                Debug.LogError("ProjectileModel or ProjectilePrefab is missing!");
                return;
            }

            var sourceView = _prototype_GridManager.Instance.GetPointView(source.point)?.PlacedEntityViews.Find(v => v.EntityData == source);
            bool hasFaced = false;

            foreach (var target in targets)
            {
                if (!hasFaced && sourceView != null && target != source)
                {
                    sourceView.FaceTowards(target.point, 0.2f);
                    hasFaced = true;
                }
                // Calculate initial direction towards target
                int dx = target.point.x - source.point.x;
                int dy = target.point.y - source.point.y;

                // Normalize direction
                int dirX = dx == 0 ? 0 : (dx > 0 ? 1 : -1);
                int dirY = dy == 0 ? 0 : (dy > 0 ? 1 : -1);

                _prototype_Point direction = new(dirX, dirY);
                if (direction == _prototype_Point.zero) continue;

                _prototype_EntityData homingTarget = null;
                bool isTracking = (targetingType == _prototype_ProjectileTargetingType.Tracking);
                if (isTracking && !(target is _prototype_EmptyPointData))
                {
                    homingTarget = target;
                }

                bool stopAtTargetPoint = (endCondition == _prototype_ProjectileEndCondition.StopAtTargetPoint);
                int travelDist = (endCondition == _prototype_ProjectileEndCondition.MaxDistance) ? maxDistance : -1;

                var projectileData = projectileModel.CreateProjectileData(
                    mySide, direction, speed, source, onHitActions, 
                    homingAnglePerStep, homingTarget,
                    isTracking, stopAtTargetPoint, target.point, travelDist
                );
                projectileData.point = source.point;

                var spawnPointView = _prototype_GridManager.Instance.GetPointView(source.point);
                if (spawnPointView == null) continue;

                // Calculate spawn rotation and position BEFORE instantiate
                Quaternion spawnRotation = Quaternion.identity;
                var nextPointView = _prototype_GridManager.Instance.GetPointView(source.point + direction);
                if (nextPointView != null)
                {
                    Vector3 dir = nextPointView.transform.position - spawnPointView.transform.position;
                    dir.y = 0;
                    if (dir != Vector3.zero)
                    {
                        spawnRotation = Quaternion.LookRotation(dir.normalized);
                    }
                }

                Vector3 spawnPos = spawnPointView.transform.position + Vector3.up * 0.5f;

                var go = GameObject.Instantiate(projectilePrefab, spawnPos, spawnRotation, spawnPointView.transform);
                var view = go.GetComponent<_prototype_ProjectileView>() ?? go.AddComponent<_prototype_ProjectileView>();

                // Add to GridManager pointView
                view.InitializeProjectile(projectileData, spawnPointView);
                await spawnPointView.PlaceEntity(view, false);
                await view.ExecuteFirstTickMovement();
            }

            await UniTask.Yield();
        }
    }
}
