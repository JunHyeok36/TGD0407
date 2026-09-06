using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace TDG0407._prototype
{
    [System.Serializable]
    public class _prototype_ShootLaserCardAction : _prototype_CardAction
    {
        public _prototype_LaserProjectileDataModel laserProjectileDataModel;
        public GameObject laserProjectilePrefab;

        public override async UniTask ExecuteCardAction(
            _prototype_EntityData source, 
            IEnumerable<_prototype_EntityData> targets,
            _prototype_ICardActionParams @params)
        {
            var targetList = targets.ToList();
            if (source == null || targetList == null || targetList.Count == 0) return;

            var target = targetList[0];
            var startPoint = source.point;
            var targetPoint = target.point;

            var sourceView = _prototype_GridManager.Instance.GetPointView(startPoint)?.PlacedEntityViews.Find(v => v.EntityData == source);
            if (sourceView != null && targetPoint != startPoint)
            {
                sourceView.FaceTowards(targetPoint, 0.2f);
            }

            _prototype_Point dir = _prototype_Point.zero;
            if (targetPoint.x > startPoint.x) dir.x = 1;
            else if (targetPoint.x < startPoint.x) dir.x = -1;
            if (targetPoint.y > startPoint.y) dir.y = 1;
            else if (targetPoint.y < startPoint.y) dir.y = -1;

            if (dir == _prototype_Point.zero) return;

            _prototype_Point current = startPoint + dir;
            int limit = 100;
            List<_prototype_Point> laserPath = new();

            while (current != targetPoint + dir * 100 && limit > 0)
            {
                limit--;
                var ptView = _prototype_GridManager.Instance.GetPointView(current);
                if (ptView == null) break;

                // Stop if we hit an outer wall (not traversable for Flying)
                if (!ptView.IsTraversable(_prototype_MovementType.Flying)) break;

                laserPath.Add(current);

                current += dir;
            }

            int index = 0;
            HashSet<_prototype_EntityData> sharedDamagedEntities = new();
            foreach (var pt in laserPath)
            {
                var ptView = _prototype_GridManager.Instance.GetPointView(pt);
                if (ptView != null)
                {
                    var data = laserProjectileDataModel.CreateLaserProjectileData();
                    data.point = pt;
                    
                    if (source is _prototype_LifeData lifeData)
                    {
                        data.side = lifeData.side;
                    }

                    if (laserProjectilePrefab != null)
                    {
                        var go = UnityEngine.Object.Instantiate(laserProjectilePrefab);
                        var laserView = go.GetComponent<_prototype_LaserProjectileView>();
                        if (laserView != null)
                        {
                            laserView.InitializeLaser(data, ptView, index, sharedDamagedEntities);
                        }
                    }
                }
                index++;
            }
            
            await UniTask.CompletedTask;
        }
    }
}
