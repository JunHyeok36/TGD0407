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
            transform.position = pointView.transform.position + Vector3.up * 0.05f;

            // set rotation
            var nextPointView = _prototype_GridManager.Instance.GetPointView(data.point + data.direction);
            if (nextPointView != null)
            {
                Vector3 dir = nextPointView.transform.position - pointView.transform.position;
                dir.y = 0;
                if (dir != Vector3.zero)
                {
                    transform.forward = dir.normalized;
                }
            }

            _prototype_TickManager.RegisterTick(ProcessTick);
            
            SetupLineRenderer();
            UpdateTrajectoryLine();
        }
        private GameObject _endCapObject;
        private GameObject _lineObj;

        private void SetupLineRenderer()
        {
            if (_lineRenderer == null)
            {
                var oldLine = GetComponent<LineRenderer>();
                if (oldLine != null) Destroy(oldLine);

                var childTransform = transform.Find("LineObj");
                if (childTransform != null)
                {
                    _lineObj = childTransform.gameObject;
                }
                else
                {
                    _lineObj = new GameObject("LineObj");
                    _lineObj.transform.SetParent(transform);
                    _lineObj.transform.localPosition = Vector3.zero;
                    _lineObj.transform.localRotation = Quaternion.Euler(-90, 0, 0);
                }

                _lineRenderer = _lineObj.GetComponent<LineRenderer>();
                if (_lineRenderer == null)
                {
                    _lineRenderer = _lineObj.AddComponent<LineRenderer>();
                }
                
                _lineRenderer.useWorldSpace = true;
                _lineRenderer.alignment = LineAlignment.TransformZ;
                
                Material arrowMat = Resources.Load<Material>("Materials/ArrowMaterial");
                if (arrowMat != null)
                {
                    _lineRenderer.material = arrowMat;
                }
                else
                {
                    _lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
                }
                
                _lineRenderer.textureMode = LineTextureMode.Tile;
                _lineRenderer.startWidth = 0.55f;
                _lineRenderer.endWidth = 0.55f;
                _lineRenderer.numCapVertices = 0;
                _lineRenderer.numCornerVertices = 5;
                
                Color lineColor = Data.side == _prototype_Side.None ? new Color(0.2f, 0.6f, 1f, 0.5f) : new Color(1f, 0.2f, 0.2f, 0.5f);
                _lineRenderer.startColor = lineColor;
                _lineRenderer.endColor = lineColor;
            }

            if (_endCapObject == null)
            {
                _endCapObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                _endCapObject.name = "EndCap";
                _endCapObject.transform.SetParent(transform);
                
                float size = _lineRenderer.endWidth * 1.1f;
                _endCapObject.transform.localScale = new Vector3(size, 0.001f, size);
                
                var col = _endCapObject.GetComponent<Collider>();
                if (col != null) Destroy(col);
                
                var renderer = _endCapObject.GetComponent<MeshRenderer>();
                renderer.material = new Material(Shader.Find("Sprites/Default"));
                renderer.material.color = _lineRenderer.endColor;
            }
        }

        private void Update()
        {
            UpdateTrajectoryLine();

            if (_lineRenderer != null && _lineRenderer.material != null)
            {
                float scrollSpeed = -2.0f; 
                float offset = Time.time * scrollSpeed;
                _lineRenderer.material.mainTextureOffset = new Vector2(offset, 0);
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
                points.Add(currentPointView.transform.position + Vector3.up * 0.05f);
            }
            else
            {
                points.Add(transform.position + Vector3.up * 0.05f);
            }

            for (int i = 0; i < Data.speed; i++) // 다음 틱에 이동할 칸 수만큼만 예측
            {
                var nextPoint = currentPoint + Data.direction;
                var nextPointView = _prototype_GridManager.Instance.GetPointView(nextPoint);

                if (nextPointView == null || !nextPointView.IsTraversable(Data.movementType))
                {
                    break;
                }

                points.Add(nextPointView.transform.position + Vector3.up * 0.05f);
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
            if (points.Count >= 2)
            {
                var originalLastPoint = points[points.Count - 1];
                var prevPoint = points[points.Count - 2];
                var dirToLast = (originalLastPoint - prevPoint).normalized;
                
                float capSize = _lineRenderer.endWidth * 1.1f;
                float shortenDist = capSize * 0.5f;
                
                float dist = Vector3.Distance(prevPoint, originalLastPoint);
                if (dist > shortenDist)
                {
                    points[points.Count - 1] = originalLastPoint - dirToLast * shortenDist;
                }
                else
                {
                    points[points.Count - 1] = prevPoint;
                }
                
                if (_endCapObject != null)
                {
                    _endCapObject.SetActive(true);
                    _endCapObject.transform.position = originalLastPoint;
                }
            }
            else
            {
                if (_endCapObject != null)
                {
                    _endCapObject.SetActive(false);
                }
            }

            _lineRenderer.positionCount = points.Count;
            _lineRenderer.SetPositions(points.ToArray());
        }

        private void OnDestroy()
        {
            _prototype_TickManager.UnregisterTick(ProcessTick);
        }

        private async UniTask<_prototype_TickIntent> ProcessTick()
        {
            var intent = new _prototype_TickIntent
            {
                TargetsPlayer = false,
                Execute = async () => { await UniTask.Yield(); }
            };

            if (Data == null || Data.direction == _prototype_Point.zero) return intent;

            // 예측: 플레이어를 맞추는지 확인
            bool willHitPlayer = false;
            var checkPoint = Data.point;
            for (int i = 0; i < Data.speed; i++)
            {
                checkPoint += Data.direction;
                var checkPointView = _prototype_GridManager.Instance.GetPointView(checkPoint);
                if (checkPointView == null || !checkPointView.IsTraversable(Data.movementType)) break;

                bool blocked = false;
                foreach (var entityView in checkPointView.PlacedEntityViews)
                {
                    if (entityView.EntityData is _prototype_LifeData lifeData)
                    {
                        if (lifeData.side != _prototype_Side.None && lifeData.side == Data.side) continue;
                        if (lifeData.side == _prototype_Side.A) willHitPlayer = true;
                        blocked = true;
                    }
                    else if (entityView.EntityData is _prototype_ObstacleData)
                    {
                        blocked = true;
                    }
                }
                if (blocked) break;
            }

            intent.TargetsPlayer = willHitPlayer;
            intent.Execute = async () =>
            {
                if (_isDestroyed || this == null || gameObject == null || Data == null || Data.direction == _prototype_Point.zero) return;

                for (int i = 0; i < Data.speed; i++)
                {
                    if (_isDestroyed || this == null || gameObject == null) return;
                    var currentPoint = Data.point;
                    var nextPoint = currentPoint + Data.direction;
                    var nextPointView = _prototype_GridManager.Instance.GetPointView(nextPoint);
                    var currentPointView = _prototype_GridManager.Instance.GetPointView(currentPoint);

                    if (nextPointView == null || !nextPointView.IsTraversable(Data.movementType))
                    {
                        DestroyProjectile();
                        return;
                    }

                    bool hit = false;
                    List<_prototype_EntityData> targetsToDamage = new();

                    foreach (var entityView in nextPointView.PlacedEntityViews)
                    {
                        var entityData = entityView.EntityData;
                        if (entityData is _prototype_LifeData lifeData)
                        {
                            if (lifeData.side != _prototype_Side.None && lifeData.side == Data.side) continue;

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

                    await _prototype_InteractionManager.MoveEntity(this, currentPointView, nextPointView);
                }
            };

            return intent;
        }

        public async UniTask HitTarget(_prototype_EntityData target)
        {
            if (_isDestroyed) return;
            var dmgContext = new _prototype_DamageContext(
                Data, target, _prototype_DamageType.Physical, Data.damage, Data.damage
            );
            await _prototype_InteractionManager.ApplyDamage(dmgContext);
            DestroyProjectile();
        }

        private bool _isDestroyed = false;

        public void DestroyProjectile()
        {
            if (_isDestroyed) return;
            _isDestroyed = true;
            var pointView = _prototype_GridManager.Instance.GetPointView(Data.point);
            if (pointView != null) pointView.RemoveEntity(this);
            if (gameObject != null) Destroy(gameObject);
        }
    }
}
