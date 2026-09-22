using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace TDG0407._prototype
{
    
    public class _prototype_GridVisualManager : MonoBehaviour
    {
        
        private static _prototype_GridVisualManager _instance;
        public static _prototype_GridVisualManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindAnyObjectByType<_prototype_GridVisualManager>(FindObjectsInactive.Include);
                }
                return _instance;
            }
            private set => _instance = value;
        }

        [Header("References")]
        [SerializeField] private Transform _pointHoverIndicator;

        [Header("Move Indicator Textures")]
        [SerializeField] private Texture2D _dashPatternTexture;
        [SerializeField] private Texture2D _targetCircleTexture;
        [SerializeField] private Texture2D _targetSquareTexture;

        //[Header("Settings")]

        private _prototype_PointView _lastHighlightedPointView;
        private _prototype_MoveIndicator _moveIndicator;

        private void Awake()
        {
            if (_instance == null) _instance = this;
            else if (_instance != this) Destroy(gameObject);

            transform.position = Vector3.zero;
            Application.runInBackground = true;
        }

        public void Initialize()
        {
            if (_pointHoverIndicator != null)
            {
                var colliders = _pointHoverIndicator.GetComponentsInChildren<Collider>(true);
                foreach (var col in colliders)
                {
                    Destroy(col);
                }
            }
            
            // Clean up any stale child indicators from previous sessions or tests
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                var child = transform.GetChild(i);
                if (child.name.StartsWith("HazardIndicator_") || child.name.StartsWith("ProjectileArrow_"))
                {
                    Destroy(child.gameObject);
                }
            }

            if (_moveIndicator == null)
            {
                GameObject moveIndObj = new GameObject("MoveIndicator");
                moveIndObj.transform.SetParent(transform);
                _moveIndicator = moveIndObj.AddComponent<_prototype_MoveIndicator>();
            }

#if UNITY_EDITOR
            if (_targetSquareTexture == null)
            {
                _targetSquareTexture = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Prototype/Textures/TargetSquare.png");
            }
#endif

            Texture2D markerTex = _targetSquareTexture != null ? _targetSquareTexture : _targetCircleTexture;
            _moveIndicator.Initialize(_dashPatternTexture, markerTex);
        }

        public void HighlightPoint(_prototype_Point? point)
        {
            if (_lastHighlightedPointView != null)
            {
                //_lastHighlightedPointView.Hovering(false);
                _lastHighlightedPointView = null;
            }

            _prototype_PointView pointView = point.HasValue ? _prototype_GridManager.Instance.GetPointView(point.Value) : null;
            if (pointView != null)
            {
                var playerEntity = _prototype_PlayerController.Instance != null ? _prototype_PlayerController.Instance.ControlledEntityView?.EntityData : null;
                if (pointView.CanPlaceEntity(playerEntity))
                {
                    _prototype_Point minPoint = _prototype_GridManager.Instance.MinPoint;
                    _prototype_Point maxPoint = _prototype_GridManager.Instance.MaxPoint;

                    if (!_prototype_GridManager.Instance.IsWithinBounds(point.Value))
                    {
                        _pointHoverIndicator.gameObject.SetActive(false);
                        return;
                    }

                    _pointHoverIndicator.gameObject.SetActive(true);
                    _pointHoverIndicator.localPosition = new(point.Value.x, 0.001f, point.Value.y);
                    
                    if (_pointHoverIndicator.TryGetComponent<Renderer>(out var r))
                    {
                        var col = r.material.color;
                        r.material.color = new Color(col.r, col.g, col.b, 0.5f);
                    }
                }
                else
                {
                    _pointHoverIndicator.gameObject.SetActive(false);
                }
                
                //pointView.Hovering(true);
                _lastHighlightedPointView = pointView;
            }
            else
            {
                _pointHoverIndicator.gameObject.SetActive(false);
            }
        }

        public void UpdatePointHoverIndicatorColor(Color color)
        {
            if (_pointHoverIndicator.TryGetComponent<Renderer>(out var renderer))
            {
                renderer.material.color = new Color(color.r, color.g, color.b, 0.5f);
            }
        }

        [Header("Range Indicators")]
        [SerializeField] private Color _castRangeColor = new Color(0.15f, 0.9f, 1f, 0.88f);
        [SerializeField] private Color _targetRangeColor = new Color(1f, 0.25f, 0.35f, 0.95f);
        [SerializeField] private Color _targetReticleColor = new Color(1f, 0.92f, 0.25f, 1.0f);
        [SerializeField] private Texture2D[] _connectedTileTextures = new Texture2D[16];
        [SerializeField] private Texture2D _targetReticleTexture;

        [Header("Hazard Indicators (Caution Stripes)")]
        [SerializeField] private Color _hazardRangeColor = new Color(1.0f, 0.42f, 0.05f, 0.95f);
        [SerializeField] private Texture2D[] _hazardTileTextures = new Texture2D[16];
        [SerializeField] private Color _friendlyProjectileColor = new Color(0.2f, 0.6f, 1f, 0.95f); // 아군 투사체 파란색

        private List<Transform> _castRangeIndicators = new();
        private List<Transform> _targetRangeIndicators = new();
        private Transform _targetCenterReticle;
        private Material[] _cachedConnectedMaterials = new Material[16];
        private Material[] _cachedTargetMaterials = new Material[16];
        private Material[] _cachedHazardMaterials = new Material[16];
        private Material[] _cachedFriendlyHazardMaterials = new Material[16];      // 아군 투사체용
        private Material[] _cachedFriendlyHazardBorderMaterials = new Material[16]; // 아군 투사체용
        private Material _targetReticleMaterial;
        private bool _isCastRangeActive = false;
        private List<_prototype_Point> _lastTargetPoints = new();
        private _prototype_Point? _lastCenterPoint = null;
        private Dictionary<_prototype_EntityView, HashSet<_prototype_Point>> _hazardPointsPerOwner = new();

        // 투사체 경로 화살표 오버레이 및 끝 지점 추적
        private Material[] _cachedHazardBorderMaterials = new Material[16];
        private Dictionary<_prototype_ProjectileView, _prototype_Point> _projectileEndPoints = new();
        private Dictionary<_prototype_ProjectileView, List<GameObject>> _projectileArrowOverlays = new();
        private Dictionary<_prototype_ProjectileView, List<Mesh>> _projectileArrowMeshes = new();
        private Dictionary<_prototype_ProjectileView, List<Vector2[]>> _projectileArrowBaseUvs = new();
        private Material _projectileArrowMaterial;          // 적군(오렌지) 화살표 머티리얼
        private Material _projectileFriendlyArrowMaterial;  // 아군(파란) 화살표 머티리얼
        private HashSet<_prototype_ProjectileView> _friendlyProjectileOwners = new(); // 아군 투사체 추적
        [SerializeField] private float _projectileArrowScrollSpeed = 1.25f;
        private float _projectileArrowScrollOffset = 0f;


        public void ShowCastRange(List<_prototype_Point> points, _prototype_Point? casterPoint = null)
        {
            InitRangeMaterials();

            // Set all inactive first
            foreach (var indicator in _castRangeIndicators) indicator.gameObject.SetActive(false);

            if (points == null || points.Count == 0)
            {
                _isCastRangeActive = false;
                return;
            }

            _isCastRangeActive = true;

            List<_prototype_Point> displayPoints = new(points);
            if (casterPoint.HasValue && !displayPoints.Contains(casterPoint.Value))
            {
                var cp = casterPoint.Value;
                bool touchesCaster = false;
                for (int i = 0; i < points.Count; i++)
                {
                    if (Mathf.Abs(points[i].x - cp.x) <= 1 && Mathf.Abs(points[i].y - cp.y) <= 1)
                    {
                        touchesCaster = true;
                        break;
                    }
                }
                if (touchesCaster)
                {
                    displayPoints.Add(cp);
                }
            }

            HashSet<_prototype_Point> pointSet = new(displayPoints);

            int activeIndex = 0;
            for (int i = 0; i < displayPoints.Count; i++)
            {
                var pt = displayPoints[i];
                if (!_prototype_GridManager.Instance.IsWithinBounds(pt)) continue;

                if (activeIndex >= _castRangeIndicators.Count)
                {
                    Transform newIndicator = CreateIndicatorQuad("CastIndicator_" + activeIndex);
                    _castRangeIndicators.Add(newIndicator);
                }

                Transform indicator = _castRangeIndicators[activeIndex];
                indicator.gameObject.SetActive(true);
                indicator.localPosition = new Vector3(pt.x, 0.008f, pt.y);
                indicator.localRotation = Quaternion.Euler(90f, 0f, 0f);
                indicator.localScale = Vector3.one;

                bool hasN = pointSet.Contains(new _prototype_Point(pt.x, pt.y + 1));
                bool hasS = pointSet.Contains(new _prototype_Point(pt.x, pt.y - 1));
                bool hasE = pointSet.Contains(new _prototype_Point(pt.x + 1, pt.y));
                bool hasW = pointSet.Contains(new _prototype_Point(pt.x - 1, pt.y));
                int mask = (hasN ? 1 : 0) | (hasS ? 2 : 0) | (hasE ? 4 : 0) | (hasW ? 8 : 0);

                if (indicator.TryGetComponent<Renderer>(out var r))
                {
                    r.sharedMaterial = _cachedConnectedMaterials[mask];
                }

                activeIndex++;
            }
        }

        public void ShowTargetRange(List<_prototype_Point> points, _prototype_Point? centerPoint = null)
        {
            InitRangeMaterials();

            foreach (var indicator in _targetRangeIndicators) indicator.gameObject.SetActive(false);
            if (_targetCenterReticle != null) _targetCenterReticle.gameObject.SetActive(false);

            if (points == null || points.Count == 0)
            {
                _lastTargetPoints.Clear();
                _lastCenterPoint = null;
                return;
            }

            _prototype_Point cPt = centerPoint ?? points[0];
            bool pointsChanged = !ArePointsEqual(_lastTargetPoints, points) || _lastCenterPoint != cPt;
            _lastTargetPoints = new List<_prototype_Point>(points);
            _lastCenterPoint = cPt;

            HashSet<_prototype_Point> pointSet = new(points);

            int activeIndex = 0;
            for (int i = 0; i < points.Count; i++)
            {
                var pt = points[i];
                if (!_prototype_GridManager.Instance.IsWithinBounds(pt)) continue;

                if (activeIndex >= _targetRangeIndicators.Count)
                {
                    Transform newIndicator = CreateIndicatorQuad("TargetIndicator_" + activeIndex);
                    _targetRangeIndicators.Add(newIndicator);
                }

                Transform indicator = _targetRangeIndicators[activeIndex];
                indicator.gameObject.SetActive(true);
                indicator.localPosition = new Vector3(pt.x, 0.012f, pt.y);
                indicator.localRotation = Quaternion.Euler(90f, 0f, 0f);
                indicator.localScale = Vector3.one;

                bool hasN = pointSet.Contains(new _prototype_Point(pt.x, pt.y + 1));
                bool hasS = pointSet.Contains(new _prototype_Point(pt.x, pt.y - 1));
                bool hasE = pointSet.Contains(new _prototype_Point(pt.x + 1, pt.y));
                bool hasW = pointSet.Contains(new _prototype_Point(pt.x - 1, pt.y));
                int mask = (hasN ? 1 : 0) | (hasS ? 2 : 0) | (hasE ? 4 : 0) | (hasW ? 8 : 0);

                if (indicator.TryGetComponent<Renderer>(out var r))
                {
                    r.sharedMaterial = _cachedTargetMaterials[mask];
                }

                activeIndex++;
            }

            // Position and animate center reticle on the targeted focus tile
            if (_prototype_GridManager.Instance.IsWithinBounds(cPt))
            {
                if (_targetCenterReticle == null)
                {
                    _targetCenterReticle = CreateIndicatorQuad("TargetCenterReticle");
                }

                _targetCenterReticle.gameObject.SetActive(true);
                _targetCenterReticle.localPosition = new Vector3(cPt.x, 0.014f, cPt.y);
                _targetCenterReticle.localRotation = Quaternion.Euler(90f, 0f, 0f);
                _targetCenterReticle.localScale = new Vector3(0.92f, 0.92f, 1f);

                if (_targetCenterReticle.TryGetComponent<Renderer>(out var r))
                {
                    r.sharedMaterial = _targetReticleMaterial;
                }

                if (pointsChanged)
                {
                    _targetCenterReticle.DOKill();
                    _targetCenterReticle.localScale = new Vector3(0.92f, 0.92f, 1f);
                    _targetCenterReticle.DOPunchScale(new Vector3(0.2f, 0.2f, 0f), 0.2f, 6, 0.5f);
                }
            }
        }

        public void ClearRanges()
        {
            _isCastRangeActive = false;
            _lastTargetPoints.Clear();
            _lastCenterPoint = null;
            foreach (var indicator in _castRangeIndicators)
            {
                indicator.DOKill();
                indicator.gameObject.SetActive(false);
            }
            foreach (var indicator in _targetRangeIndicators)
            {
                indicator.DOKill();
                indicator.gameObject.SetActive(false);
            }
            if (_targetCenterReticle != null)
            {
                _targetCenterReticle.DOKill();
                _targetCenterReticle.gameObject.SetActive(false);
            }
        }

        private Transform CreateIndicatorQuad(string name)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            var col = go.GetComponent<Collider>();
            if (col != null)
            {
                if (Application.isPlaying) Destroy(col);
                else DestroyImmediate(col);
            }
            go.transform.SetParent(transform);
            return go.transform;
        }

        private void InitRangeMaterials()
        {
            if (_cachedConnectedMaterials[0] != null) return;

            var shader = Shader.Find("Sprites/Default");

            for (int i = 0; i < 16; i++)
            {
                _cachedConnectedMaterials[i] = new Material(shader);
                _cachedTargetMaterials[i] = new Material(shader);

                if (_connectedTileTextures != null && i < _connectedTileTextures.Length && _connectedTileTextures[i] != null)
                {
                    _cachedConnectedMaterials[i].mainTexture = _connectedTileTextures[i];
                    _cachedTargetMaterials[i].mainTexture = _connectedTileTextures[i];
                }
                _cachedConnectedMaterials[i].color = _castRangeColor;
                _cachedTargetMaterials[i].color = _targetRangeColor;
            }

            _targetReticleMaterial = new Material(shader);
            if (_targetReticleTexture != null)
            {
                _targetReticleMaterial.mainTexture = _targetReticleTexture;
            }
            _targetReticleMaterial.color = _targetReticleColor;

            InitHazardMaterials(shader);
        }

        private void InitHazardMaterials(Shader shader = null)
        {
            if (_cachedHazardMaterials[0] != null) return;

            if (shader == null) shader = Shader.Find("Sprites/Default");

#if UNITY_EDITOR
            if (_hazardTileTextures == null || _hazardTileTextures.Length < 16 || _hazardTileTextures[0] == null)
            {
                _hazardTileTextures = new Texture2D[16];
                for (int i = 0; i < 16; i++)
                {
                    _hazardTileTextures[i] = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>($"Assets/_Prototype/Textures/GridRange/HazardTile_{i}.png");
                }
            }
#endif

            for (int i = 0; i < 16; i++)
            {
                _cachedHazardMaterials[i] = new Material(shader);
                if (_hazardTileTextures != null && i < _hazardTileTextures.Length && _hazardTileTextures[i] != null)
                {
                    _cachedHazardMaterials[i].mainTexture = _hazardTileTextures[i];
                }
                _cachedHazardMaterials[i].color = _hazardRangeColor;

                // 투사체 경로 전용: 테두리만 있는 텍스처 머티리얼 (내부 빗금 없음)
                _cachedHazardBorderMaterials[i] = new Material(shader);
#if UNITY_EDITOR
                var borderTex = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>($"Assets/_Prototype/Textures/GridRange/HazardBorderTile_{i}.png");
                if (borderTex != null)
                {
                    _cachedHazardBorderMaterials[i].mainTexture = borderTex;
                }
#endif
                _cachedHazardBorderMaterials[i].color = _hazardRangeColor;

                // 아군 투사체 경로용: 파란색 머티리얼 세트
                _cachedFriendlyHazardMaterials[i] = new Material(shader);
                if (_hazardTileTextures != null && i < _hazardTileTextures.Length && _hazardTileTextures[i] != null)
                {
                    _cachedFriendlyHazardMaterials[i].mainTexture = _hazardTileTextures[i];
                }
                _cachedFriendlyHazardMaterials[i].color = _friendlyProjectileColor;

                _cachedFriendlyHazardBorderMaterials[i] = new Material(shader);
#if UNITY_EDITOR
                var friendlyBorderTex = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>($"Assets/_Prototype/Textures/GridRange/HazardBorderTile_{i}.png");
                if (friendlyBorderTex != null)
                {
                    _cachedFriendlyHazardBorderMaterials[i].mainTexture = friendlyBorderTex;
                }
#endif
                _cachedFriendlyHazardBorderMaterials[i].color = _friendlyProjectileColor;
            }
        }

        private void Update()
        {
            if (_isCastRangeActive && _cachedConnectedMaterials[0] != null)
            {
                float pulseAlpha = _castRangeColor.a * (0.82f + 0.18f * Mathf.Sin(Time.time * 3.5f));
                Color pulseColor = new Color(_castRangeColor.r, _castRangeColor.g, _castRangeColor.b, pulseAlpha);
                for (int i = 0; i < 16; i++)
                {
                    if (_cachedConnectedMaterials[i] != null)
                    {
                        _cachedConnectedMaterials[i].color = pulseColor;
                    }
                }
            }

            if (_hazardPointsPerOwner.Count > 0 && _cachedHazardMaterials[0] != null)
            {
                // Warning caution pulse (frequency ~1.1Hz: quick alert pulsation)
                float hazardPulse = 0.82f + 0.18f * Mathf.Sin(Time.time * 7f);
                Color hazardColor = new Color(
                    _hazardRangeColor.r * hazardPulse,
                    _hazardRangeColor.g * hazardPulse,
                    _hazardRangeColor.b * hazardPulse,
                    _hazardRangeColor.a * (0.85f + 0.15f * hazardPulse));

                for (int i = 0; i < 16; i++)
                {
                    if (_cachedHazardMaterials[i] != null)
                    {
                        _cachedHazardMaterials[i].color = hazardColor;
                    }
                    if (_cachedHazardBorderMaterials[i] != null)
                    {
                        _cachedHazardBorderMaterials[i].color = hazardColor;
                    }
                }

                // 화살표 머티리얼 색상 및 펄스 동기화
                if (_projectileArrowMaterial != null)
                {
                    _projectileArrowMaterial.color = hazardColor;
                    if (_projectileArrowMaterial.HasProperty("_BaseColor"))
                    {
                        _projectileArrowMaterial.SetColor("_BaseColor", hazardColor);
                    }
                    if (_projectileArrowMaterial.HasProperty("_TintColor"))
                    {
                        _projectileArrowMaterial.SetColor("_TintColor", hazardColor);
                    }
                }
            }

            // 아군 투사체 색상 펄스 (파란색 계열, 부드러운 펄스)
            if (_friendlyProjectileOwners.Count > 0 && _cachedFriendlyHazardMaterials[0] != null)
            {
                float friendlyPulse = 0.85f + 0.15f * Mathf.Sin(Time.time * 5f);
                Color friendlyColor = new Color(
                    _friendlyProjectileColor.r * friendlyPulse,
                    _friendlyProjectileColor.g * friendlyPulse,
                    _friendlyProjectileColor.b * friendlyPulse,
                    _friendlyProjectileColor.a * (0.85f + 0.15f * friendlyPulse));

                for (int i = 0; i < 16; i++)
                {
                    if (_cachedFriendlyHazardMaterials[i] != null)
                        _cachedFriendlyHazardMaterials[i].color = friendlyColor;
                    if (_cachedFriendlyHazardBorderMaterials[i] != null)
                        _cachedFriendlyHazardBorderMaterials[i].color = friendlyColor;
                }

                if (_projectileFriendlyArrowMaterial != null)
                {
                    _projectileFriendlyArrowMaterial.color = friendlyColor;
                    if (_projectileFriendlyArrowMaterial.HasProperty("_BaseColor"))
                        _projectileFriendlyArrowMaterial.SetColor("_BaseColor", friendlyColor);
                    if (_projectileFriendlyArrowMaterial.HasProperty("_TintColor"))
                        _projectileFriendlyArrowMaterial.SetColor("_TintColor", friendlyColor);
                }
            }

            // 투사체 경로 화살표 UV 스크롤 (진행 방향으로 정점 UV 이동, 타일별 개별 메시 지원)
            if (_projectileArrowMeshes.Count > 0)
            {
                _projectileArrowScrollOffset -= Time.deltaTime * _projectileArrowScrollSpeed;
                if (_projectileArrowScrollOffset <= -1f) _projectileArrowScrollOffset += 1f;

                Vector2 offsetVec = new Vector2(_projectileArrowScrollOffset, 0f);

                foreach (var kvp in _projectileArrowMeshes)
                {
                    var meshList = kvp.Value;
                    if (meshList != null && _projectileArrowBaseUvs.TryGetValue(kvp.Key, out var baseUvList) && baseUvList != null)
                    {
                        for (int m = 0; m < meshList.Count && m < baseUvList.Count; m++)
                        {
                            var mesh = meshList[m];
                            var baseUvs = baseUvList[m];
                            if (mesh != null && baseUvs != null)
                            {
                                Vector2[] shiftedUvs = new Vector2[4];
                                for (int i = 0; i < 4; i++)
                                {
                                    shiftedUvs[i] = baseUvs[i] + offsetVec;
                                }
                                mesh.uv = shiftedUvs;
                            }
                        }
                    }
                }
            }

            // 혹시라도 DestroyProjectile 등에서 정리되지 않고 파괴된 객체(Missing/null) 자동 정리
            CleanUpStaleOwners();
        }

        private List<_prototype_ProjectileView> _staleProjectilesToClean = new();
        private List<_prototype_EntityView> _staleEntitiesToClean = new();

        private void CleanUpStaleOwners()
        {
            _staleProjectilesToClean.Clear();
            foreach (var kvp in _projectileArrowOverlays)
            {
                if (kvp.Key == null) _staleProjectilesToClean.Add(kvp.Key);
            }
            foreach (var kvp in _projectileEndPoints)
            {
                if (kvp.Key == null && !_staleProjectilesToClean.Contains(kvp.Key)) _staleProjectilesToClean.Add(kvp.Key);
            }
            for (int i = 0; i < _staleProjectilesToClean.Count; i++)
            {
                ClearProjectileHazards(_staleProjectilesToClean[i]);
            }

            _staleEntitiesToClean.Clear();
            foreach (var kvp in _hazardPointsPerOwner)
            {
                if (kvp.Key == null) _staleEntitiesToClean.Add(kvp.Key);
            }
            for (int i = 0; i < _staleEntitiesToClean.Count; i++)
            {
                ClearAllHazards(_staleEntitiesToClean[i]);
            }
        }

        private bool ArePointsEqual(List<_prototype_Point> a, List<_prototype_Point> b)
        {
            if (a == null && b == null) return true;
            if (a == null || b == null || a.Count != b.Count) return false;
            for (int i = 0; i < a.Count; i++)
            {
                if (a[i] != b[i]) return false;
            }
            return true;
        }

        public void ShowMovementPath(List<_prototype_Point> path, int moveDistancePerTick = 1)
        {
            if (_moveIndicator != null)
            {
                _moveIndicator.ShowPath(path, moveDistancePerTick);
            }
        }

        public void HideMovementPath()
        {
            if (_moveIndicator != null)
            {
                _moveIndicator.Hide();
            }
        }

        private Dictionary<_prototype_EntityView, List<Transform>> _hazardIndicators = new();
        private Dictionary<_prototype_EntityView, Transform> _knockbackIndicators = new();
        private Material _knockbackLandingMaterial;

        public void ShowKnockbackHazard(_prototype_EntityView owner, _prototype_Point landingPoint)
        {
            if (owner == null) return;
            if (_prototype_GridManager.Instance != null && !_prototype_GridManager.Instance.IsWithinBounds(landingPoint)) return;

            if (!_knockbackIndicators.TryGetValue(owner, out var indicator) || indicator == null)
            {
                indicator = CreateIndicatorQuad($"KnockbackLanding_{owner.name}");
                _knockbackIndicators[owner] = indicator;
            }

            if (_knockbackLandingMaterial == null)
            {
                var shader = Shader.Find("Sprites/Default");
                _knockbackLandingMaterial = new Material(shader)
                {
                    color = new Color(1f, 0.2f, 0.8f, 0.65f)
                };
            }

            indicator.gameObject.SetActive(true);
            indicator.localPosition = new Vector3(landingPoint.x, 0.016f, landingPoint.y);
            indicator.localRotation = Quaternion.Euler(90f, 0f, 0f);
            indicator.localScale = new Vector3(0.85f, 0.85f, 1f);

            if (indicator.TryGetComponent<Renderer>(out var r))
            {
                r.sharedMaterial = _knockbackLandingMaterial;
            }
        }

        public void ClearKnockbackHazard(_prototype_EntityView owner)
        {
            if (owner != null && _knockbackIndicators.TryGetValue(owner, out var ind) && ind != null)
            {
                ind.gameObject.SetActive(false);
            }
        }

        public void ShowHazard(_prototype_Point pt, _prototype_EntityView owner)
        {
            if (owner == null) return;

            if (owner.EntityData is _prototype_LifeData life && (life.HasStatusEffect(_prototype_StatusType.Stun) || life.HasStatusEffect(_prototype_StatusType.Silence)))
            {
                return;
            }

            if (_prototype_GridManager.Instance != null && !_prototype_GridManager.Instance.IsWithinBounds(pt)) return;

            if (!_hazardPointsPerOwner.ContainsKey(owner))
                _hazardPointsPerOwner[owner] = new HashSet<_prototype_Point>();

            _hazardPointsPerOwner[owner].Add(pt);
            RefreshHazardVisuals(owner);
        }

        public void ShowHazards(IEnumerable<_prototype_Point> points, _prototype_EntityView owner)
        {
            if (owner == null || points == null) return;

            if (owner.EntityData is _prototype_LifeData life && (life.HasStatusEffect(_prototype_StatusType.Stun) || life.HasStatusEffect(_prototype_StatusType.Silence)))
            {
                return;
            }

            if (!_hazardPointsPerOwner.ContainsKey(owner))
                _hazardPointsPerOwner[owner] = new HashSet<_prototype_Point>();

            foreach (var pt in points)
            {
                if (_prototype_GridManager.Instance == null || _prototype_GridManager.Instance.IsWithinBounds(pt))
                    _hazardPointsPerOwner[owner].Add(pt);
            }
            RefreshHazardVisuals(owner);
        }

        private void RefreshHazardVisuals(_prototype_EntityView owner)
        {
            InitRangeMaterials();
            InitHazardMaterials();

            if (!_hazardPointsPerOwner.TryGetValue(owner, out var pointSet) || pointSet.Count == 0)
            {
                ClearAllHazards(owner);
                return;
            }

            if (!_hazardIndicators.ContainsKey(owner))
                _hazardIndicators[owner] = new List<Transform>();

            var indicatorList = _hazardIndicators[owner];
            int activeIndex = 0;

            foreach (var pt in pointSet)
            {
                if (activeIndex >= indicatorList.Count)
                {
                    Transform newIndicator = CreateIndicatorQuad($"HazardIndicator_{owner.name}_{activeIndex}");
                    indicatorList.Add(newIndicator);
                }

                Transform indicator = indicatorList[activeIndex];
                indicator.gameObject.SetActive(true);
                indicator.localPosition = new Vector3(pt.x, 0.015f, pt.y);
                indicator.localRotation = Quaternion.Euler(90f, 0f, 0f);
                indicator.localScale = Vector3.one;

                bool hasN = pointSet.Contains(new _prototype_Point(pt.x, pt.y + 1));
                bool hasS = pointSet.Contains(new _prototype_Point(pt.x, pt.y - 1));
                bool hasE = pointSet.Contains(new _prototype_Point(pt.x + 1, pt.y));
                bool hasW = pointSet.Contains(new _prototype_Point(pt.x - 1, pt.y));
                int mask = (hasN ? 1 : 0) | (hasS ? 2 : 0) | (hasE ? 4 : 0) | (hasW ? 8 : 0);

                if (indicator.TryGetComponent<Renderer>(out var r))
                {
                    if (owner is _prototype_ProjectileView proj && _projectileEndPoints.TryGetValue(proj, out var endPt))
                    {
                        bool isFriendly = _friendlyProjectileOwners.Contains(proj);
                        // 투사체: 끝 지점만 빗금 Hazard 타일, 경로는 빗금 없는 테두리만 (색상은 아군/적군 구분)
                        if (pt == endPt)
                            r.sharedMaterial = isFriendly ? _cachedFriendlyHazardMaterials[mask] : _cachedHazardMaterials[mask];
                        else
                            r.sharedMaterial = isFriendly ? _cachedFriendlyHazardBorderMaterials[mask] : _cachedHazardBorderMaterials[mask];
                    }
                    else
                    {
                        r.sharedMaterial = _cachedHazardMaterials[mask];
                    }
                }

                activeIndex++;
            }

            for (int i = activeIndex; i < indicatorList.Count; i++)
            {
                indicatorList[i].gameObject.SetActive(false);
            }
        }

        public void ClearAllHazards(_prototype_EntityView owner)
        {
            if (!System.Object.ReferenceEquals(owner, null))
            {
                if (_hazardPointsPerOwner.TryGetValue(owner, out var pts))
                {
                    pts.Clear();
                    _hazardPointsPerOwner.Remove(owner);
                }

                if (_hazardIndicators.TryGetValue(owner, out var list))
                {
                    if (list != null)
                    {
                        foreach (var ind in list)
                        {
                            if (ind != null)
                            {
                                ind.DOKill();
                                ind.gameObject.SetActive(false);
                            }
                        }
                        list.Clear();
                    }
                    _hazardIndicators.Remove(owner);
                }

                ClearKnockbackHazard(owner);
            }
        }

        /// <summary>
        /// 특정 타일을 조준하고 있는 모든 적의 공격 예정 정보 목록을 조회합니다.
        /// 투사체(ProjectileView) 소유자도 포함합니다.
        /// </summary>
        public List<_prototype_HazardAttackInfo> GetHazardAttackInfosAtPoint(_prototype_Point pt)
        {
            List<_prototype_HazardAttackInfo> list = new();
            foreach (var kvp in _hazardPointsPerOwner)
            {
                var owner = kvp.Key;
                var pointSet = kvp.Value;
                if (owner == null || pointSet == null || !pointSet.Contains(pt)) continue;

                // 투사체 케이스 (아군/적군 모두 포함)
                if (owner is _prototype_ProjectileView projectileView)
                {
                    if (projectileView.Data != null)
                    {
                        var info = _prototype_HazardAttackInfo.CreateFromProjectile(projectileView);
                        if (info != null) list.Add(info);
                    }
                    continue;
                }

                if (owner.EntityData is _prototype_LifeData life && !life.IsDead)
                {
                    if (life.HasStatusEffect(_prototype_StatusType.Stun) || life.HasStatusEffect(_prototype_StatusType.Silence))
                        continue;

                    if (life.aiLogic != null && life.aiLogic.hasPlannedIntent && life.aiLogic.plannedCard != null)
                    {
                        var info = _prototype_HazardAttackInfo.Create(owner, life.aiLogic.plannedCard);
                        if (info != null) list.Add(info);
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// 투사체의 경로 타일에 Hazard 타일 + 흐르는 화살표 오버레이를 표시합니다.
        /// pathPoints: 중간 경로 타일 (테두리 + 화살표 표시), endPoint: 최종 도달점 (일반 빗금 Hazard 타일)
        /// isFriendly: true이면 파란색(아군), false이면 오렌지(적군)
        /// </summary>
        public void ShowProjectileHazards(
            List<_prototype_Point> pathPoints,
            _prototype_Point endPoint,
            _prototype_ProjectileView owner,
            _prototype_Point direction,
            bool isFriendly = false)
        {
            if (owner == null) return;

            // 아군/적군 추적 등록
            if (isFriendly)
                _friendlyProjectileOwners.Add(owner);
            else
                _friendlyProjectileOwners.Remove(owner);

            // 끝 지점 추적 등록 (RefreshHazardVisuals에서 빗금 vs 테두리 구분용)
            _projectileEndPoints[owner] = endPoint;

            // 기존 화살표 오버레이 제거
            ClearProjectileArrowOverlays(owner);

            // 기존 해저드 포인트 초기화 (이미 지나간 타일의 잔존 테두리 완전 방지)
            if (_hazardPointsPerOwner.TryGetValue(owner, out var pts))
            {
                pts.Clear();
            }
            else
            {
                _hazardPointsPerOwner[owner] = new HashSet<_prototype_Point>();
            }

            // 모든 타일을 Hazard로 등록 (경로 + 끝점)
            var allPoints = new List<_prototype_Point>();
            if (pathPoints != null) allPoints.AddRange(pathPoints);
            allPoints.Add(endPoint);
            ShowHazards(allPoints, owner);

            // 경로 타일에만 화살표 오버레이 추가
            if (pathPoints == null || pathPoints.Count == 0) return;

            // 아군/적군 화살표 머티리얼 초기화
            InitProjectileArrowMaterial(isFriendly);
            Material arrowMat = isFriendly ? _projectileFriendlyArrowMaterial : _projectileArrowMaterial;

            if (!_projectileArrowOverlays.ContainsKey(owner))
                _projectileArrowOverlays[owner] = new List<GameObject>();
            if (!_projectileArrowMeshes.ContainsKey(owner))
                _projectileArrowMeshes[owner] = new List<Mesh>();
            if (!_projectileArrowBaseUvs.ContainsKey(owner))
                _projectileArrowBaseUvs[owner] = new List<Vector2[]>();

            var overlayList = _projectileArrowOverlays[owner];
            var meshList = _projectileArrowMeshes[owner];
            var baseUvList = _projectileArrowBaseUvs[owner];

            Vector2[] verts2D = new Vector2[]
            {
                new Vector2(-0.5f, -0.5f), // v0: (-X, -Z)
                new Vector2( 0.5f, -0.5f), // v1: (+X, -Z)
                new Vector2(-0.5f,  0.5f), // v2: (-X, +Z)
                new Vector2( 0.5f,  0.5f)  // v3: (+X, +Z)
            };

            for (int k = 0; k < pathPoints.Count; k++)
            {
                var pt = pathPoints[k];
                if (_prototype_GridManager.Instance == null || !_prototype_GridManager.Instance.IsWithinBounds(pt)) continue;

                // 다음 경로 타일 결정: 마지막 타일이면 endPoint, 아니면 바로 다음 pathPoints[k+1]
                _prototype_Point nextPt = (k < pathPoints.Count - 1) ? pathPoints[k + 1] : endPoint;

                // 타일별 개별 진행 방향 벡터 계산 (월드 X, Z 평면)
                Vector2 delta = new Vector2(nextPt.x - pt.x, nextPt.y - pt.y);
                Vector2 dir = delta.sqrMagnitude > 0.001f ? delta.normalized : new Vector2(direction.x, direction.y).normalized;
                if (dir.sqrMagnitude < 0.001f) dir = Vector2.right;

                Vector2 uDir = dir;
                Vector2 vDir = new Vector2(-dir.y, dir.x);

                float maxV = 0f;
                for (int i = 0; i < 4; i++)
                {
                    float vDist = Mathf.Abs(Vector2.Dot(verts2D[i], vDir));
                    if (vDist > maxV) maxV = vDist;
                }
                if (maxV < 0.001f) maxV = 0.5f;

                Vector2[] uvs = new Vector2[4];
                for (int i = 0; i < 4; i++)
                {
                    float u = Vector2.Dot(verts2D[i], uDir) + 0.5f;
                    float v = (Vector2.Dot(verts2D[i], vDir) / (maxV * 2f)) + 0.5f;
                    uvs[i] = new Vector2(u, v);
                }

                // 타일별 개별 Mesh 생성 (타일마다 다음 타일을 향한 독립적인 UV 적용)
                Mesh tileMesh = new Mesh();
                tileMesh.name = $"ProjectileArrowMesh_{owner.name}_{k}";
                tileMesh.vertices = new Vector3[]
                {
                    new Vector3(-0.5f, -0.5f, 0f),
                    new Vector3( 0.5f, -0.5f, 0f),
                    new Vector3(-0.5f,  0.5f, 0f),
                    new Vector3( 0.5f,  0.5f, 0f)
                };
                tileMesh.triangles = new int[] { 0, 2, 1, 2, 3, 1 };
                tileMesh.normals = new Vector3[] { -Vector3.forward, -Vector3.forward, -Vector3.forward, -Vector3.forward };
                tileMesh.uv = uvs;

                meshList.Add(tileMesh);
                baseUvList.Add((Vector2[])uvs.Clone());

                GameObject quad = new GameObject($"ProjectileArrow_{owner.name}_{k}");
                quad.transform.SetParent(transform);

                // GridVisualManager의 타일 로컬 좌표계로 정확히 스냅
                // Y=0.0145f로 테두리 인디케이터(Y=0.015f) 바로 밑에 배치하여 3px 테두리가 화살표 경계를 깔끔하게 마감
                quad.transform.localPosition = new Vector3(pt.x, 0.0145f, pt.y);
                quad.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                quad.transform.localScale = Vector3.one;

                var mf = quad.AddComponent<MeshFilter>();
                mf.sharedMesh = tileMesh;

                var renderer = quad.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = arrowMat;

                overlayList.Add(quad);
            }
        }

        private Texture2D _chevronTexture; // 아군/적군 공유 체브론 텍스처

        private Texture2D GetOrCreateChevronTexture()
        {
            if (_chevronTexture != null) return _chevronTexture;

            // 90도 살각 체브론 패턴을 코드로 생성
            // U축(+오른쪽)이 화살표 진행 방향. 체브론 > 형태 (오른쪽으로 뾰족한 첨단).
            // 살각 90도 = 두 팔이 각각 U축 기준 ±45도
            int texW = 256;
            int texH = 256;
            _chevronTexture = new Texture2D(texW, texH, TextureFormat.RGBA32, false);
            _chevronTexture.wrapMode = TextureWrapMode.Repeat;
            _chevronTexture.filterMode = FilterMode.Bilinear;

            Color[] pixels = new Color[texW * texH];
            float halfH = texH / 2f;
            for (int py = 0; py < texH; py++)
            {
                float dy = Mathf.Abs(py - halfH); // 가운데(dy=0)에서 첨단, 위/아래 가장자리(dy=128)에서 날개
                for (int offset = 0; offset <= texW; offset += 128)
                {
                    float targetX = offset - dy;
                    for (int px = 0; px < texW; px++)
                    {
                        float dist = Mathf.Abs(px - targetX);
                        dist = Mathf.Min(dist, Mathf.Abs(px - (targetX + texW)));
                        dist = Mathf.Min(dist, Mathf.Abs(px - (targetX - texW)));

                        if (dist < 9f)
                        {
                            float a = Mathf.SmoothStep(1f, 0f, dist / 9f) * 0.9f;
                            Color c = new Color(1f, 1f, 1f, a);
                            Color prev = pixels[py * texW + px];
                            if (c.a > prev.a) pixels[py * texW + px] = c;
                        }
                    }
                }
            }

            _chevronTexture.SetPixels(pixels);
            _chevronTexture.Apply();
            return _chevronTexture;
        }

        private void InitProjectileArrowMaterial(bool isFriendly)
        {
            var shader = Shader.Find("Custom/HazardArrowScrolling");
            if (shader == null) shader = Shader.Find("Legacy Shaders/Particles/Alpha Blended");
            if (shader == null) shader = Shader.Find("Sprites/Default");

            var chevronTex = GetOrCreateChevronTexture();

            if (isFriendly)
            {
                // 아군(파란) 화살표 머티리얼 초기화
                if (_projectileFriendlyArrowMaterial == null || _projectileFriendlyArrowMaterial.shader != shader)
                {
                    _projectileFriendlyArrowMaterial = new Material(shader);
                    _projectileFriendlyArrowMaterial.renderQueue = 2999;
                    _projectileFriendlyArrowMaterial.color = _friendlyProjectileColor;
                }
                if (_projectileFriendlyArrowMaterial.mainTexture == null)
                {
                    _projectileFriendlyArrowMaterial.mainTexture = chevronTex;
                    _projectileFriendlyArrowMaterial.mainTextureScale = new Vector2(1f, 1f);
                }
            }
            else
            {
                // 적군(오렌지) 화살표 머티리얼 초기화
                if (_projectileArrowMaterial == null || _projectileArrowMaterial.shader != shader)
                {
                    _projectileArrowMaterial = new Material(shader);
                    _projectileArrowMaterial.renderQueue = 2999;
                    _projectileArrowMaterial.color = _hazardRangeColor;
                }
                if (_projectileArrowMaterial.mainTexture == null)
                {
                    _projectileArrowMaterial.mainTexture = chevronTex;
                    _projectileArrowMaterial.mainTextureScale = new Vector2(1f, 1f);
                }
            }
        }

        private void ClearProjectileArrowOverlays(_prototype_ProjectileView owner)
        {
            if (System.Object.ReferenceEquals(owner, null)) return;

            if (_projectileArrowMeshes.TryGetValue(owner, out var meshList) && meshList != null)
            {
                foreach (var m in meshList)
                {
                    if (m != null)
                    {
                        if (Application.isPlaying) Object.Destroy(m);
                        else Object.DestroyImmediate(m);
                    }
                }
                meshList.Clear();
            }
            _projectileArrowMeshes.Remove(owner);

            if (_projectileArrowBaseUvs.TryGetValue(owner, out var uvList) && uvList != null)
            {
                uvList.Clear();
            }
            _projectileArrowBaseUvs.Remove(owner);

            if (_projectileArrowOverlays.TryGetValue(owner, out var list) && list != null)
            {
                foreach (var go in list)
                {
                    if (go != null)
                    {
                        if (Application.isPlaying) Object.Destroy(go);
                        else Object.DestroyImmediate(go);
                    }
                }
                list.Clear();
            }
            _projectileArrowOverlays.Remove(owner);
        }

        /// <summary>
        /// 투사체 소멸 시 화살표 오버레이 및 Hazard 타일을 정리합니다.
        /// </summary>
        public void ClearProjectileHazards(_prototype_ProjectileView owner)
        {
            if (System.Object.ReferenceEquals(owner, null)) return;

            ClearProjectileArrowOverlays(owner);
            _projectileArrowOverlays.Remove(owner);
            _projectileArrowMeshes.Remove(owner);
            _projectileArrowBaseUvs.Remove(owner);
            _projectileEndPoints.Remove(owner);
            _friendlyProjectileOwners.Remove(owner); // 아군 추적 해제
            ClearAllHazards(owner);
        }

        private _prototype_EntityView _highlightedHazardOwner;
        private Transform _hazardAttackerReticle;

        /// <summary>
        /// 정보 확인 중인 특정 적 엔티티의 머리 위/발밑 강조 및 공격 범위 동기화
        /// </summary>
        public void HighlightHazardAttacker(_prototype_EntityView owner, bool highlight)
        {
            if (highlight && owner != null)
            {
                _highlightedHazardOwner = owner;

                if (_hazardAttackerReticle == null)
                {
                    _hazardAttackerReticle = CreateIndicatorQuad("HazardAttackerReticle");
                }

                _hazardAttackerReticle.gameObject.SetActive(true);
                _hazardAttackerReticle.position = owner.transform.position + new Vector3(0, 0.03f, 0);
                _hazardAttackerReticle.localScale = new Vector3(1.15f, 1.15f, 1f);
                _hazardAttackerReticle.localRotation = Quaternion.Euler(90f, 0f, 0f);

                if (_hazardAttackerReticle.TryGetComponent<Renderer>(out var r))
                {
                    if (_targetReticleMaterial != null)
                    {
                        r.sharedMaterial = _targetReticleMaterial;
                        r.material.color = new Color(1f, 0.35f, 0.1f, 0.95f); // 발광 주황
                    }
                }

                _hazardAttackerReticle.DOKill();
                _hazardAttackerReticle.localScale = new Vector3(1.15f, 1.15f, 1f);
                _hazardAttackerReticle.DOPunchScale(new Vector3(0.2f, 0.2f, 0f), 0.3f, 5, 0.5f);
            }
            else
            {
                if (_hazardAttackerReticle != null)
                {
                    _hazardAttackerReticle.DOKill();
                    _hazardAttackerReticle.gameObject.SetActive(false);
                }
                _highlightedHazardOwner = null;
            }
        }
    }
}