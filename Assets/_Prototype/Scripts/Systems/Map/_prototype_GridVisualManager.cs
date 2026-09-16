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

        //[Header("Settings")]

        private _prototype_PointView _lastHighlightedPointView;
        private _prototype_MoveIndicator _moveIndicator;

        private void Awake()
        {
            if (_instance == null) _instance = this;
            else if (_instance != this) Destroy(gameObject);

            transform.position = Vector3.zero;
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
            
            if (_moveIndicator == null)
            {
                GameObject moveIndObj = new GameObject("MoveIndicator");
                moveIndObj.transform.SetParent(transform);
                _moveIndicator = moveIndObj.AddComponent<_prototype_MoveIndicator>();
            }
            _moveIndicator.Initialize(_dashPatternTexture, _targetCircleTexture);
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

        private List<Transform> _castRangeIndicators = new();
        private List<Transform> _targetRangeIndicators = new();
        private Transform _targetCenterReticle;
        private Material[] _cachedConnectedMaterials = new Material[16];
        private Material[] _cachedTargetMaterials = new Material[16];
        private Material[] _cachedHazardMaterials = new Material[16];
        private Material _targetReticleMaterial;
        private bool _isCastRangeActive = false;
        private List<_prototype_Point> _lastTargetPoints = new();
        private _prototype_Point? _lastCenterPoint = null;
        private Dictionary<_prototype_EntityView, HashSet<_prototype_Point>> _hazardPointsPerOwner = new();

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
            go.name = name;
            Destroy(go.GetComponent<Collider>());
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
                }
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

        public void ShowMovementPath(List<_prototype_Point> path)
        {
            if (_moveIndicator != null)
            {
                _moveIndicator.ShowPath(path);
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

        public void ShowHazard(_prototype_Point pt, _prototype_EntityView owner)
        {
            if (owner == null || !_prototype_GridManager.Instance.IsWithinBounds(pt)) return;

            if (!_hazardPointsPerOwner.ContainsKey(owner))
                _hazardPointsPerOwner[owner] = new HashSet<_prototype_Point>();

            _hazardPointsPerOwner[owner].Add(pt);
            RefreshHazardVisuals(owner);
        }

        public void ShowHazards(IEnumerable<_prototype_Point> points, _prototype_EntityView owner)
        {
            if (owner == null || points == null) return;

            if (!_hazardPointsPerOwner.ContainsKey(owner))
                _hazardPointsPerOwner[owner] = new HashSet<_prototype_Point>();

            foreach (var pt in points)
            {
                if (_prototype_GridManager.Instance.IsWithinBounds(pt))
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
                    r.sharedMaterial = _cachedHazardMaterials[mask];
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
            if (owner != null && _hazardPointsPerOwner.ContainsKey(owner))
            {
                _hazardPointsPerOwner[owner].Clear();
            }

            if (owner != null && _hazardIndicators.TryGetValue(owner, out var list))
            {
                foreach (var ind in list)
                {
                    if (ind != null)
                    {
                        ind.DOKill();
                        ind.gameObject.SetActive(false);
                    }
                }
            }
        }
    }
}