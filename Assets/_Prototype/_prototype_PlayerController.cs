using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using System.Linq;

namespace TDG0407._prototype
{
    
    public class _prototype_PlayerController : MonoBehaviour
    {
        public static _prototype_PlayerController Instance { get; private set; }
        
        [Header("References")]
        [SerializeField] private _prototype_EntityView _controlledEntityView;
        [SerializeField] private _prototype_CameraController _cameraController;

        //[Header("Settings")] 

        public _prototype_Point ControlledEntityLastPoint => _controlledEntityLastPoint ?? _controlledEntityView.Point;
        public _prototype_EntityView ControlledEntityView => _controlledEntityView;
        
        private _prototype_Point? _controlledEntityLastPoint = null; 

        private bool _isMovable = true;

        public bool IsMovable
        {
            get => _isMovable;
            set => _isMovable = value;
        }

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }
        private void Update()
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            
            // 핸드 카드 보기 모드 스위칭: 마우스가 화면 하단의 10% 이내에 있거나, 카드에 호버 중이면 활성화
            bool isHandViewActive = false;
            if (_prototype_PlayerUIView.Instance != null)
            {
                bool isMouseNearBottom = mousePos.y < Screen.height * 0.1f;
                bool isHoveringCard = _prototype_PlayerUIView.Instance.IsCardHovered;
                isHandViewActive = isMouseNearBottom || isHoveringCard;
                
                // 타겟팅 모드(카드 활성화 상태)에서는 무조건 핸드 컨테이너를 최소화(비활성화)
                if (_targetingCard != null) isHandViewActive = false;
                
                _prototype_PlayerUIView.Instance.SetHandViewModeActive(isHandViewActive);
            }

            _prototype_Point? mousePoint = GetIsometricMousePoint();

            if (_targetingCard != null)
            {
                _prototype_GridVisualManager.Instance.HighlightPoint(null); // Hide single white indicator while targeting
                _prototype_PlayerUIView.Instance.UpdateTargetingTooltipPosition(mousePos);
                HandleTargetingInput(mousePoint);
            }
            else
            {
                if (isHandViewActive)
                {
                    // 핸드 컨테이너가 활성화된 상태에서는 이동 불가 및 이동 경로 숨김
                    _prototype_GridVisualManager.Instance.ShowTargetRange(null);
                    _prototype_GridVisualManager.Instance.HighlightPoint(null);
                    return;
                }

                if (!_isMovable)
                {
                    // 카드 드래그 중 등 이동 불가 상태일 때 경로 표시 숨김
                    _prototype_GridVisualManager.Instance.ShowTargetRange(null);
                    _prototype_GridVisualManager.Instance.HighlightPoint(null);
                    return;
                }

                // 평소에는 이동 모드로 동작: 현재 마우스 위치까지의 경로(Path)를 TargetRange로 표시
                if (mousePoint.HasValue)
                {
                    List<_prototype_PointView> path = _prototype_GridManager.Instance.FindPath(_controlledEntityView.Point, mousePoint.Value);
                    if (path != null && path.Count > 0)
                    {
                        List<_prototype_Point> pathPoints = new();
                        foreach (var pv in path) pathPoints.Add(pv.Point);

                        // 경로 표시 (TargetRange 색상 활용)
                        _prototype_GridVisualManager.Instance.ShowTargetRange(pathPoints);

                        if (Mouse.current.rightButton.wasPressedThisFrame)
                        {
                            HandleMovementInput(mousePoint.Value);
                        }
                    }
                    else
                    {
                        _prototype_GridVisualManager.Instance.ShowTargetRange(null);
                    }
                }
                else
                {
                    _prototype_GridVisualManager.Instance.ShowTargetRange(null);
                }
                
                // 단일 포인트 호버(원래 기능)은 숨기거나 다른 방식으로 씀 (여기선 하위 호환을 위해 투명화 혹은 제거 가능하나 일단 남겨둠)
                _prototype_GridVisualManager.Instance.HighlightPoint(null);
            }
        }

        public _prototype_Point? GetIsometricMousePoint()
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();

            Ray ray = _cameraController.MainCamera.ScreenPointToRay(mousePos);
            
            // 타겟팅 중일 때는 CastRangeIndicator(높이 0.04f) 시각적 위치에 맞춰 레이캐스트 평면을 띄움
            float planeY = _targetingCard != null ? 0.04f : 0f;
            Plane xzGridPlane = new(Vector3.up, new Vector3(0, planeY, 0));
            
            if (xzGridPlane.Raycast(ray, out float enterDistance))
            {
                Vector3 hitPoint = ray.GetPoint(enterDistance);
                return new(Mathf.RoundToInt(hitPoint.x), Mathf.RoundToInt(hitPoint.z));
            }
            return null;
        }
        
        private void HandleMovementInput(_prototype_Point targetPoint)
        {
            if (!_isMovable || _prototype_TickManager.IsTickProcessing) return;
            if (_controlledEntityView.Point == targetPoint) return;

            List<_prototype_PointView> path = _prototype_GridManager.Instance.FindPath(_controlledEntityView.Point, targetPoint);
            if (path != null && path.Count > 0)
            {
                _prototype_PointView nextStep = path[0];

                _controlledEntityLastPoint = _controlledEntityView.Point;
                _prototype_TickManager.AdvanceTick(async () => {  
                    await _prototype_InteractionManager.MoveEntity(_controlledEntityView, _prototype_GridManager.Instance.GetPointView(_controlledEntityView.Point), nextStep);
                    _prototype_PlayerUIView.Instance.UpdatePlayerInfo();
                }).Forget();
            }
        }

        private _prototype_CardData _targetingCard;
        private List<_prototype_Point> _currentCastRange;

        public void StartTargeting(_prototype_CardData cardData)
        {
            _targetingCard = cardData;
            _isMovable = false;

            if (_targetingCard.castRange != null)
            {
                _currentCastRange = _targetingCard.castRange.GetValidCastPoints(_controlledEntityView.Point);
                _prototype_GridVisualManager.Instance.ShowCastRange(_currentCastRange);
            }
            _prototype_PlayerUIView.Instance.ShowTargetingUI(_targetingCard);
        }

        public void CancelTargeting()
        {
            _targetingCard = null;
            _isMovable = true;
            _currentCastRange = null;
            _prototype_GridVisualManager.Instance.ClearRanges();
            _prototype_PlayerUIView.Instance.HideTargetingUI();
            _prototype_PlayerUIView.Instance.UpdatePlayerCardDeck();
        }

        private void HandleTargetingInput(_prototype_Point? mousePoint)
        {
            if (_prototype_TickManager.IsTickProcessing) return;

            if (Mouse.current.rightButton.wasPressedThisFrame || Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                CancelTargeting();
                return;
            }

            if (mousePoint.HasValue && _currentCastRange != null && _currentCastRange.Contains(mousePoint.Value))
            {
                // Show Target Range
                List<_prototype_Point> targetRange = _targetingCard.targetRange?.GetValidTargetPoints(_controlledEntityView.Point, mousePoint.Value) ?? new List<_prototype_Point> { mousePoint.Value };
                _prototype_GridVisualManager.Instance.ShowTargetRange(targetRange);

                // Execute on Left Click
                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    ExecuteCardCast(targetRange);
                }
            }
            else
            {
                _prototype_GridVisualManager.Instance.ShowTargetRange(null); // clear target range
                
                // 맨땅 좌클릭 시 취소
                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    CancelTargeting();
                }
            }
        }

        private void ExecuteCardCast(List<_prototype_Point> targetRange)
        {
            var cardToCast = _targetingCard;
            var playerLife = _controlledEntityView as _prototype_LifeView;
            
            // 1. Move card to discard pile and Deduct Cost
            if (playerLife?.Data?.cardDeck != null)
            {
                playerLife.Data.cardDeck.handedCardDatas.Remove(cardToCast);
                playerLife.Data.cardDeck.discardedCardDatas.Add(cardToCast);

                var cost = cardToCast.costValue;
                int amount = (int)cost.value;
                if (cost.costType == _prototype_CostType.FixedStamina)
                {
                    playerLife.Data.stamina.Current -= amount;
                }
                else if (cost.costType == _prototype_CostType.FixedHealth)
                {
                    playerLife.Data.health.Current -= amount;
                }
            }

            CancelTargeting();

            _controlledEntityLastPoint = _controlledEntityView.Point;
            // 2. Advance Tick and Apply Action
            _prototype_TickManager.AdvanceTick(async () => {
                List<_prototype_EntityData> targets = new();
                foreach (var pt in targetRange)
                {
                    var pointView = _prototype_GridManager.Instance.GetPointView(pt);
                    if (pointView != null)
                    {
                        foreach (var entityView in pointView.PlacedEntityViews)
                            targets.Add(entityView.EntityData);
                    }
                }

                if (cardToCast.actionList != null)
                {
                    var param = new _prototype_CardActionParams();
                    foreach (var action in cardToCast.actionList)
                    {
                        var filteredTargets = targets;
                        if (!action.includeSelf)
                        {
                            filteredTargets = targets.Where(t => t != _controlledEntityView.EntityData).ToList();
                        }
                        await action.ExecuteCardAction(_controlledEntityView.EntityData, filteredTargets, param);
                    }
                }

                _prototype_PlayerUIView.Instance.UpdatePlayerInfo();
                _prototype_PlayerUIView.Instance.UpdatePlayerCardDeck();
            }).Forget();
        }
    }

}