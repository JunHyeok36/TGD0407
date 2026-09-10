using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using System.Collections.Generic;

namespace TDG0407._prototype
{
    public class _prototype_ProjectileView : _prototype_EntityView
    {
        public new _prototype_ProjectileData Data => _entityData as _prototype_ProjectileData;

        private LineRenderer _lineRenderer;

        private int _spawnTick = -1;

        public void InitializeProjectile(_prototype_ProjectileData data, _prototype_PointView pointView)
        {
            _entityData = data;
            _spawnTick = _prototype_TickManager.CurrentTick;

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

            _prototype_TickManager.RegisterPostTick(PreCalculatePath);
            _prototype_TickManager.RegisterTick(ProcessTick);

            SetupLineRenderer();
            PreCalculatePath().Forget();
            UpdateTrajectoryLine();
        }

        private Queue<_prototype_Point> _plannedDirections = new();
        private float _plannedEndFloatAngle = float.NaN;

        public void SetTrajectoryVisible(bool visible)
        {
            if (_lineObj != null)
            {
                _lineObj.SetActive(visible);
            }
            if (_endCapObject != null)
            {
                if (visible)
                {
                    bool hasPoints = _lineRenderer != null && _lineRenderer.positionCount >= 2;
                    _endCapObject.SetActive(hasPoints);
                }
                else
                {
                    _endCapObject.SetActive(false);
                }
            }
        }

        private async UniTask PreCalculatePath()
        {
            if (Data == null || Data.direction == _prototype_Point.zero)
            {
                SetTrajectoryVisible(false);
                return;
            }

            _plannedDirections.Clear();
            var currentPoint = Data.point;
            var checkDir = Data.direction;
            float checkFloatAngle = float.IsNaN(Data.currentFloatAngle) ? Mathf.Atan2(checkDir.y, checkDir.x) * Mathf.Rad2Deg : Data.currentFloatAngle;

            int tempTraveled = Data.traveledDistance;

            for (int i = 0; i < Data.speed; i++)
            {
                // Max Travel Distance 검사
                if (Data.maxTravelDistance > 0 && tempTraveled >= Data.maxTravelDistance)
                {
                    break;
                }

                // Homing / Curve logic
                if (Data.homingAnglePerStep > 0f)
                {
                    bool shouldHoming = false;
                    _prototype_Point currentTargetPoint = currentPoint;

                    if (Data.isTracking && Data.homingTarget != null)
                    {
                        bool isDead = Data.homingTarget is _prototype_LifeData ld && ld.health.Current <= 0;
                        if (!isDead && Data.homingTarget.point != currentPoint)
                        {
                            shouldHoming = true;
                            currentTargetPoint = Data.homingTarget.point;
                        }
                    }
                    else if (!Data.isTracking && Data.fixedTargetPoint != currentPoint)
                    {
                        shouldHoming = true;
                        currentTargetPoint = Data.fixedTargetPoint;
                    }

                    if (shouldHoming)
                    {
                        float targetAngle = Mathf.Atan2(currentTargetPoint.y - currentPoint.y, currentTargetPoint.x - currentPoint.x) * Mathf.Rad2Deg;
                        float angleDiff = Mathf.DeltaAngle(checkFloatAngle, targetAngle);
                        checkFloatAngle += Mathf.Clamp(angleDiff, -Data.homingAnglePerStep, Data.homingAnglePerStep);

                        float nx = Mathf.Cos(checkFloatAngle * Mathf.Deg2Rad);
                        float ny = Mathf.Sin(checkFloatAngle * Mathf.Deg2Rad);

                        int dirX = Mathf.RoundToInt(nx);
                        int dirY = Mathf.RoundToInt(ny);
                        checkDir = new _prototype_Point(dirX, dirY);
                    }
                }

                _plannedDirections.Enqueue(checkDir);
                currentPoint += checkDir;
                tempTraveled++;

                // 맵 바깥이거나 통과 불가 타일이면 더 이상 경로 계산하지 않음
                if (!_prototype_GridManager.Instance.IsWithinBounds(currentPoint))
                {
                    break;
                }
                var nextPv = _prototype_GridManager.Instance.GetPointView(currentPoint);
                if (nextPv != null && !nextPv.IsTraversable(Data.movementType))
                {
                    break;
                }

                // Fixed Point 목표 지점에 도달했는지 검사
                if (Data.stopAtTargetPoint && currentPoint == Data.fixedTargetPoint)
                {
                    break;
                }
            }

            _plannedEndFloatAngle = checkFloatAngle;
            UpdateTrajectoryLine();
            SetTrajectoryVisible(true);
        }

        public IEnumerable<_prototype_Point> GetPlannedPathPoints()
        {
            var points = new List<_prototype_Point>();
            if (Data == null) return points;

            var checkPoint = Data.point;
            foreach (var checkDir in _plannedDirections)
            {
                checkPoint += checkDir;
                points.Add(checkPoint);
            }
            return points;
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

                Gradient gradient = new Gradient();
                gradient.SetKeys(
                    new GradientColorKey[] { new GradientColorKey(lineColor, 0.0f), new GradientColorKey(lineColor, 1.0f) },
                    new GradientAlphaKey[] {
                        new GradientAlphaKey(0.0f, 0.0f),
                        new GradientAlphaKey(lineColor.a, 0.2f),
                        new GradientAlphaKey(lineColor.a, 1.0f)
                    }
                );
                _lineRenderer.colorGradient = gradient;
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

                Color endCapColor = Data.side == _prototype_Side.None ? new Color(0.2f, 0.6f, 1f, 0.5f) : new Color(1f, 0.2f, 0.2f, 0.5f);
                renderer.material.color = endCapColor;

                // Create Destruction Marker (X mark)
                GameObject xMark = new GameObject("DestructionMarker");
                xMark.transform.SetParent(_endCapObject.transform);
                xMark.transform.localPosition = new Vector3(0, 1f, 0); // slightly above the cap relative to scale
                xMark.transform.localRotation = Quaternion.identity;
                xMark.transform.localScale = Vector3.one;

                var line1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                line1.transform.SetParent(xMark.transform);
                line1.transform.localPosition = Vector3.zero;
                line1.transform.localEulerAngles = new Vector3(0, 45, 0);
                line1.transform.localScale = new Vector3(0.8f, 1f, 0.2f);
                Destroy(line1.GetComponent<Collider>());
                var r1 = line1.GetComponent<MeshRenderer>();
                r1.material = new Material(Shader.Find("Sprites/Default"));
                r1.material.color = Color.white;

                var line2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                line2.transform.SetParent(xMark.transform);
                line2.transform.localPosition = Vector3.zero;
                line2.transform.localEulerAngles = new Vector3(0, -45, 0);
                line2.transform.localScale = new Vector3(0.8f, 1f, 0.2f);
                Destroy(line2.GetComponent<Collider>());
                var r2 = line2.GetComponent<MeshRenderer>();
                r2.material = new Material(Shader.Find("Sprites/Default"));
                r2.material.color = Color.white;

                xMark.SetActive(false);
            }
        }

        private void Update()
        {
            if (_lineRenderer != null && _lineRenderer.material != null)
            {
                float scrollSpeed = -1.0f;
                float offset = Time.time * scrollSpeed;
                _lineRenderer.material.mainTextureOffset = new Vector2(offset, 0);
            }
        }

        public void UpdateTrajectoryLine()
        {
            if (Data == null || Data.direction == _prototype_Point.zero || _lineRenderer == null)
            {
                SetTrajectoryVisible(false);
                return;
            }
            if (_plannedDirections.Count == 0)
            {
                SetTrajectoryVisible(false);
                return;
            }

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

            bool willBeDestroyed = false;
            int simulatedTravel = Data.traveledDistance;

            foreach (var checkDir in _plannedDirections)
            {
                var nextPoint = currentPoint + checkDir;
                var nextPointView = _prototype_GridManager.Instance.GetPointView(nextPoint);

                // 맵 밖이거나 비정상 타일(Abyss, Wall 등)이면 경로 표시 중단
                if (nextPointView == null || !_prototype_GridManager.Instance.IsWithinBounds(nextPoint) || nextPointView.PointData.type != _prototype_PointType.Normal)
                {
                    willBeDestroyed = true;
                    break;
                }

                points.Add(nextPointView.transform.position + Vector3.up * 0.05f);
                currentPoint = nextPoint;
                simulatedTravel++;

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

                if (hit)
                {
                    // 엔티티(캐릭터, 장애물 등)에 적중 시 X마커 표시 안 함
                    break;
                }

                if (Data.maxTravelDistance > 0 && simulatedTravel >= Data.maxTravelDistance)
                {
                    willBeDestroyed = true;
                    break;
                }

                if (Data.stopAtTargetPoint && currentPoint == Data.fixedTargetPoint)
                {
                    willBeDestroyed = true;
                    break;
                }
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
                    _endCapObject.transform.rotation = Quaternion.identity;

                    var xMark = _endCapObject.transform.Find("DestructionMarker");
                    if (xMark != null)
                    {
                        xMark.gameObject.SetActive(willBeDestroyed);
                    }
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
            _prototype_TickManager.UnregisterPostTick(PreCalculatePath);
            _prototype_TickManager.UnregisterTick(ProcessTick);
        }

        public async UniTask ExecuteMovement(Queue<_prototype_Point> executeQueue)
        {
            if (_isDestroyed || this == null || gameObject == null || Data == null || Data.direction == _prototype_Point.zero) return;

            SetTrajectoryVisible(false);

            Data.currentFloatAngle = _plannedEndFloatAngle;

            while (executeQueue.Count > 0)
            {
                var checkDir = executeQueue.Dequeue();
                Data.direction = checkDir;

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

                if (Data.onHitActions != null)
                {
                    foreach (var action in Data.onHitActions)
                    {
                        await action.ExecuteAction(Data.shooter, targetsToDamage, null);
                    }
                }

                if (hit)
                {
                    // 타격 위치로 이동 후 소멸
                    if (!nextPointView.CanPlaceEntity(Data))
                    {
                        // 이미 자리가 꽉 차서 이동 불가한 경우 애니메이션만 재생
                        Vector3 targetPos = nextPointView.transform.position + LocalPositionOffset;
                        await transform.DOMove(targetPos, 0.15f).SetEase(Ease.Linear).AsyncWaitForCompletion();
                    }
                    else
                    {
                        await _prototype_InteractionManager.MoveEntity(this, currentPointView, nextPointView);
                    }
                    DestroyProjectile();
                    return;
                }

                await _prototype_InteractionManager.MoveEntity(this, currentPointView, nextPointView);
                Data.traveledDistance++;

                if (Data.maxTravelDistance > 0 && Data.traveledDistance >= Data.maxTravelDistance)
                {
                    DestroyProjectile();
                    return;
                }

                if (Data.stopAtTargetPoint && Data.point == Data.fixedTargetPoint)
                {
                    DestroyProjectile();
                    return;
                }
            }
        }

        public async UniTask ExecuteFirstTickMovement()
        {
            if (_isDestroyed || this == null || gameObject == null || Data == null || Data.direction == _prototype_Point.zero) return;

            if (_plannedDirections.Count == 0)
            {
                await PreCalculatePath();
            }

            if (_plannedDirections.Count == 0) return;

            Queue<_prototype_Point> executeQueue = new Queue<_prototype_Point>(_plannedDirections);
            await ExecuteMovement(executeQueue);

            if (!_isDestroyed && this != null && gameObject != null)
            {
                await PreCalculatePath();
            }
        }

        private async UniTask<_prototype_TickIntent> ProcessTick()
        {
            var intent = new _prototype_TickIntent
            {
                TargetsPlayer = false,
                Execute = async () => { await UniTask.Yield(); }
            };

            if (_spawnTick == _prototype_TickManager.CurrentTick)
            {
                return intent;
            }

            if (Data == null || Data.direction == _prototype_Point.zero || _plannedDirections.Count == 0) return intent;

            bool willHitPlayer = false;
            var checkPoint = Data.point;

            Queue<_prototype_Point> executeQueue = new Queue<_prototype_Point>(_plannedDirections);

            foreach (var checkDir in _plannedDirections)
            {
                checkPoint += checkDir;
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
                await ExecuteMovement(executeQueue);
            };

            return intent;
        }

        public async UniTask HitTarget(_prototype_EntityData target)
        {
            if (_isDestroyed) return;
            if (Data.onHitActions != null)
            {
                var targetsToDamage = new List<_prototype_EntityData> { target };
                foreach (var action in Data.onHitActions)
                {
                        await action.ExecuteAction(Data.shooter, targetsToDamage, null);
                }
            }
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
