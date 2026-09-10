using System.Collections.Generic;
using UnityEngine;

namespace TDG0407._prototype
{
    
    public class _prototype_GridVisualManager : MonoBehaviour
    {
        
        public static _prototype_GridVisualManager Instance { get; private set; }

        [Header("References")]
        [SerializeField] private Transform _pointHoverIndicator;

        //[Header("Settings")]

        private _prototype_PointView _lastHighlightedPointView;
        private _prototype_MoveIndicator _moveIndicator;

        
        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            if (_pointHoverIndicator != null)
            {
                var colliders = _pointHoverIndicator.GetComponentsInChildren<Collider>(true);
                foreach (var col in colliders)
                {
                    Destroy(col);
                }
            }
            
            GameObject moveIndObj = new GameObject("MoveIndicator");
            moveIndObj.transform.SetParent(transform);
            _moveIndicator = moveIndObj.AddComponent<_prototype_MoveIndicator>();
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
                    _pointHoverIndicator.localPosition = new(point.Value.x, 0f, point.Value.y);
                    
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

        private List<Transform> _castRangeIndicators = new();
        private List<Transform> _targetRangeIndicators = new();

        public void ShowCastRange(List<_prototype_Point> points)
        {
            UpdateIndicators(_castRangeIndicators, points, Color.cyan, 0.04f);
        }

        public void ShowTargetRange(List<_prototype_Point> points)
        {
            UpdateIndicators(_targetRangeIndicators, points, Color.red, 0.06f);
        }

        public void ClearRanges()
        {
            foreach (var indicator in _castRangeIndicators) indicator.gameObject.SetActive(false);
            foreach (var indicator in _targetRangeIndicators) indicator.gameObject.SetActive(false);
        }

        private void UpdateIndicators(List<Transform> indicators, List<_prototype_Point> points, Color color, float yOffset)
        {
            // Set all inactive first
            foreach (var indicator in indicators) indicator.gameObject.SetActive(false);

            if (points == null) return;

            int activeIndex = 0;
            for (int i = 0; i < points.Count; i++)
            {
                if (!_prototype_GridManager.Instance.IsWithinBounds(points[i])) continue;

                if (activeIndex >= indicators.Count)
                {
                    Transform newIndicator = Instantiate(_pointHoverIndicator, transform);
                    indicators.Add(newIndicator);
                }

                Transform activeIndicator = indicators[activeIndex];
                activeIndicator.gameObject.SetActive(true);
                activeIndicator.localPosition = new Vector3(points[i].x, yOffset, points[i].y);
                
                if (activeIndicator.TryGetComponent<Renderer>(out var renderer))
                {
                    // Transparent color for ranges
                    renderer.material.color = new Color(color.r, color.g, color.b, 0.5f);
                }

                activeIndex++;
            }
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
            if (!_prototype_GridManager.Instance.IsWithinBounds(pt)) return;

            if (!_hazardIndicators.ContainsKey(owner))
                _hazardIndicators[owner] = new List<Transform>();

            Transform newIndicator = Instantiate(_pointHoverIndicator, transform);
            newIndicator.gameObject.SetActive(true);
            newIndicator.localPosition = new Vector3(pt.x, 0.05f, pt.y);
            
            if (newIndicator.TryGetComponent<Renderer>(out var renderer))
            {
                // Dark red/orange warning color with high opacity
                renderer.material.color = new Color(1f, 0.2f, 0f, 0.7f);
            }
            
            _hazardIndicators[owner].Add(newIndicator);
        }

        public void ClearAllHazards(_prototype_EntityView owner)
        {
            if (_hazardIndicators.TryGetValue(owner, out var list))
            {
                foreach (var ind in list)
                {
                    if (ind != null) Destroy(ind.gameObject);
                }
                list.Clear();
            }
        }
    }
}