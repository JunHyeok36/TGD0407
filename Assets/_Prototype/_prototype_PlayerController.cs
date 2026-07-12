using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TDG0407._prototype
{
    
    public class _prototype_PlayerController : MonoBehaviour
    {
        public static _prototype_PlayerController Instance { get; private set; }
        
        [Header("References")]
        [SerializeField] private _prototype_EntityView _controlledEntityView;
        [SerializeField] private _prototype_CameraController _cameraController; 

        //[Header("Settings")]

        public _prototype_EntityView ControlledEntityView => _controlledEntityView;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Update()
        {
            _prototype_Point? mousePoint = GetIsometricMousePoint();

            if (Mouse.current.rightButton.isPressed)
            {    
                if (mousePoint.HasValue)
                {
                    _prototype_GridVisualManager.Instance.UpdatePointHoverIndicatorColor(Color.red);
                }
                HandleMovementInput(mousePoint.Value);
            }
            else
            {
                _prototype_GridVisualManager.Instance.UpdatePointHoverIndicatorColor(Color.white);
            }
            
            _prototype_GridVisualManager.Instance.HighlightPoint(mousePoint);
        }

        public _prototype_Point? GetIsometricMousePoint()
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();

            Ray ray = _cameraController.MainCamera.ScreenPointToRay(mousePos);
            Plane xzGridPlane = new(Vector3.up, Vector3.zero);
            if (xzGridPlane.Raycast(ray, out float enterDistance))
            {
                Vector3 hitPoint = ray.GetPoint(enterDistance);
                return new(Mathf.RoundToInt(hitPoint.x), Mathf.RoundToInt(hitPoint.z));
            }
            return null;
        }
        
        private void HandleMovementInput(_prototype_Point? targetPoint)
        {
            if (targetPoint == null) 
                return;

            // 1. 마우스 우클릭을 시작한 '첫 프레임' (짧은 클릭 처리)
            if (Mouse.current.rightButton.wasPressedThisFrame)
                ExecuteSingleMoveStep(targetPoint.Value);

            void ExecuteSingleMoveStep(_prototype_Point targetPoint)
            {
                if (_controlledEntityView.Point == targetPoint) return;

                List<_prototype_PointView> path = _prototype_GridManager.Instance.FindPath(_controlledEntityView.Point, targetPoint);
                if (path != null && path.Count > 0)
                {
                    _prototype_PointView nextStep = path[0];

                    _prototype_TickManager.AdvanceTick(() => {
                        return _prototype_InteractionManager.MoveEntity(_controlledEntityView, _prototype_GridManager.Instance.GetPointView(_controlledEntityView.Point), nextStep);
                    }).Forget();
                    //_prototype_GridVisualManager.Instance.PlayClickEffect(nextStep);
                }
            }
        }
    }

}