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

        
        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void HighlightPoint(_prototype_Point? point)
        {
            if (_lastHighlightedPointView != null)
            {
                _lastHighlightedPointView.Hovering(false);
                _lastHighlightedPointView = null;
            }

            _prototype_PointView pointView = point.HasValue ? _prototype_GridManager.Instance.GetPointView(point.Value) : null;
            if (pointView != null)
            {
                if (pointView.IsEntityPlaceable)
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
                }
                else
                {
                    _pointHoverIndicator.gameObject.SetActive(false);
                }
                
                pointView.Hovering(true);
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
                renderer.material.color = color;
            }
        }

    }

}