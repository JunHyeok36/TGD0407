using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace TDG0407._prototype
{
    [Serializable]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceClassName: "_prototype_SpawnProjectileCardAction")]
    public class _prototype_SpawnDirectionalProjectileCardAction : _prototype_CardAction
    {
        public _prototype_ProjectileDataModel projectileModel;
        public GameObject projectilePrefab;
        [SerializeReference, SubclassSelector] public List<_prototype_CardAction> onHitActions;
        public int speed;

        public override async UniTask ExecuteCardAction(
            _prototype_EntityData source,
            IEnumerable<_prototype_EntityData> targets,
            _prototype_ICardActionParams @params)
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
                // Calculate direction towards target
                int dx = target.point.x - source.point.x;
                int dy = target.point.y - source.point.y;

                // Normalize direction
                int dirX = dx == 0 ? 0 : (dx > 0 ? 1 : -1);
                int dirY = dy == 0 ? 0 : (dy > 0 ? 1 : -1);

                _prototype_Point direction = new(dirX, dirY);
                if (direction == _prototype_Point.zero) continue;

                // 직선 투사체이므로 homingTarget은 null, homingAnglePerStep은 0
                var projectileData = projectileModel.CreateProjectileData(mySide, direction, speed, source, onHitActions, 0f, null);
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
