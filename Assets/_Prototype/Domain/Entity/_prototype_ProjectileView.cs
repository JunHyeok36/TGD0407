using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using System.Collections.Generic;

namespace TDG0407._prototype
{
    public class _prototype_ProjectileView : _prototype_EntityView
    {
        public _prototype_ProjectileData Data => _entityData as _prototype_ProjectileData;

        private LineRenderer _lineRenderer;

        public void InitializeProjectile(_prototype_ProjectileData data, _prototype_PointView pointView)
        {
            _entityData = data;
            
            // set position
            transform.position = pointView.transform.position + Vector3.up * 0.5f;

            _prototype_TickManager.RegisterTick(ProcessTick);
            
            SetupLineRenderer();
            UpdateTrajectoryLine();
        }

        private void SetupLineRenderer()
        {
            if (_lineRenderer == null)
            {
                _lineRenderer = GetComponent<LineRenderer>();
                if (_lineRenderer == null)
                {
                    _lineRenderer = gameObject.AddComponent<LineRenderer>();
                }
                
                _lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
                _lineRenderer.startWidth = 0.15f;
                _lineRenderer.endWidth = 0.15f;
                _lineRenderer.numCapVertices = 5;
                _lineRenderer.numCornerVertices = 5;
                
                Color lineColor = Data.side == _prototype_Side.None ? new Color(0.2f, 0.6f, 1f, 0.4f) : new Color(1f, 0.2f, 0.2f, 0.4f);
                _lineRenderer.startColor = lineColor;
                _lineRenderer.endColor = lineColor;
            }
        }

        private void Update()
        {
            if (!_prototype_TickManager.IsTickProcessing)
            {
                UpdateTrajectoryLine();
            }
        }

        public void UpdateTrajectoryLine()
        {
            if (Data == null || Data.direction == _prototype_Point.zero || _lineRenderer == null) return;

            List<Vector3> points = new List<Vector3>();
            
            var currentPoint = Data.point;
            var currentPointView = _prototype_GridManager.Instance.GetPointView(currentPoint);
            if (currentPointView != null)
            {
                points.Add(currentPointView.transform.position + Vector3.up * 0.5f);
            }

            for (int i = 0; i < 20; i++) // 최대 20칸까지만 예측
            {
                var nextPoint = currentPoint + Data.direction;
                var nextPointView = _prototype_GridManager.Instance.GetPointView(nextPoint);

                if (nextPointView == null || !nextPointView.IsTraversable(Data.movementType))
                {
                    if (nextPointView != null)
                        points.Add(nextPointView.transform.position + Vector3.up * 0.5f);
                    else
                        points.Add((currentPointView != null ? currentPointView.transform.position : transform.position) + new Vector3(Data.direction.x, 0, Data.direction.y) + Vector3.up * 0.5f);
                    break;
                }

                points.Add(nextPointView.transform.position + Vector3.up * 0.5f);
                currentPoint = nextPoint;
                currentPointView = nextPointView;

                bool hit = false;
                foreach (var entityView in nextPointView.PlacedEntityViews)
                {
                    var entityData = entityView.EntityData;
                    if (entityData is _prototype_LifeData lifeData)
                    {
                        if (lifeData.side != _prototype_Side.None && lifeData.side == Data.side) continue;
                        hit = true;
                    }
                    else if (entityData is _prototype_ObstacleData obsData)
                    {
                        hit = true;
                    }
                }

                if (hit) break;
            }

            _lineRenderer.positionCount = points.Count;
            _lineRenderer.SetPositions(points.ToArray());
        }

        private void OnDestroy()
        {
            _prototype_TickManager.UnregisterTick(ProcessTick);
        }

        private async UniTask ProcessTick()
        {
            if (Data == null || Data.direction == _prototype_Point.zero) return;

            for (int i = 0; i < Data.speed; i++)
            {
                var currentPoint = Data.point;
                var nextPoint = currentPoint + Data.direction;
                var nextPointView = _prototype_GridManager.Instance.GetPointView(nextPoint);
                var currentPointView = _prototype_GridManager.Instance.GetPointView(currentPoint);

                if (nextPointView == null || !nextPointView.IsTraversable(Data.movementType))
                {
                    // Blocked by terrain according to movement type
                    DestroyProjectile();
                    return;
                }

                // Check collisions
                bool hit = false;
                List<_prototype_EntityData> targetsToDamage = new();

                foreach (var entityView in nextPointView.PlacedEntityViews)
                {
                    var entityData = entityView.EntityData;
                    if (entityData is _prototype_LifeData lifeData)
                    {
                        if (lifeData.side != _prototype_Side.None && lifeData.side == Data.side)
                        {
                            continue; // Friendly fire disabled
                        }

                        targetsToDamage.Add(entityData);
                        hit = true;
                    }
                    else if (entityData is _prototype_ObstacleData obsData)
                    {
                        targetsToDamage.Add(entityData);
                        hit = true;
                    }
                }

                foreach (var target in targetsToDamage)
                {
                    var dmgContext = new _prototype_DamageContext(
                        Data, target, _prototype_DamageType.Physical, Data.damage, Data.damage
                    );
                    await _prototype_InteractionManager.ApplyDamage(dmgContext);
                }

                if (hit)
                {
                    DestroyProjectile();
                    return;
                }

                // Move visual
                await _prototype_InteractionManager.MoveEntity(this, currentPointView, nextPointView);
            }
        }

        private void DestroyProjectile()
        {
            var pointView = _prototype_GridManager.Instance.GetPointView(Data.point);
            if (pointView != null) pointView.RemoveEntity(this);
            if (gameObject != null) Destroy(gameObject);
        }
    }
}
