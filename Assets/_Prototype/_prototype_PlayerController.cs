using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TDG0407._prototype
{
    
    public class _prototype_PlayerController : MonoBehaviour
    {
        
        [Header("References")]
        [SerializeField] private _prototype_EntityView _controlledEntityView;
        [SerializeField] private _prototype_CameraController _cameraController;

        [Header("Settings")]
        [SerializeField] private float longPressThreshold = 0.25f; // 이 시간(초) 이상 누르면 '길게 누르기'로 판단
        [SerializeField] private float moveHoldInterval = 0.15f;   // 길게 누르고 있을 때 다음 칸으로 이동하는 시간 차(딜레이)

        private float buttonPressedTime = 0f;  // 버튼을 누르고 있는 누적 시간
        private float holdMoveTimer = 0f;      // 연속 이동 간격을 계산하기 위한 타이머
        private bool isExecutingHoldMove = false;

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
            
            _prototype_GridVisualManager.Instance.UpdatePointHoverIndicatorPosition(mousePoint);
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

                List<_prototype_PointView> path = _prototype_GridManager.Instance.FindPathViews(_controlledEntityView.Point, targetPoint);
                if (path != null && path.Count > 0)
                {
                    _prototype_PointView nextStep = path[0];
                    _controlledEntityView.MoveTo(nextStep);

                    //_prototype_TickManager.Instance.AdvanceTick();
                    //_prototype_GridVisualManager.Instance.PlayClickEffect(nextStep);
                }
            }
        }
    }

}