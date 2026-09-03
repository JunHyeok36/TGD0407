using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using System.Linq;
using DG.Tweening;

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
        private bool _initialDrawDone = false;

        private void Start()
        {
            _prototype_TickManager.RegisterPostTick(OnPostTick);
        }

        private void OnDestroy()
        {
            _prototype_TickManager.UnregisterPostTick(OnPostTick);
        }

        private async UniTask OnPostTick()
        {
            var playerLife = _controlledEntityView as _prototype_LifeView;
            if (playerLife?.Data?.cardDeck != null)
            {
                var deck = playerLife.Data.cardDeck;
                
                // 최초 드로우 (턴 시작 시 뽑아야 할 만큼 뽑음)
                int maxHandSize = playerLife.Data.lifeStat.handCardSlotCount;
                if (maxHandSize <= 0) maxHandSize = 5;

                if (deck.handedCardDatas.Count < maxHandSize)
                {
                    deck.DrawCards(maxHandSize - deck.handedCardDatas.Count, maxHandSize);
                }
                
                // 항상 갱신하여 쿨타임 및 카드 변동이 UI에 반영되도록 함
                if (_prototype_PlayerUIView.Instance != null)
                    _prototype_PlayerUIView.Instance.UpdatePlayerCardDeck();
            }
        }

        private void Update()
        {
            // Data가 준비되면 최초 1회 자동 드로우 실행
            if (!_initialDrawDone && _controlledEntityView != null && _controlledEntityView.EntityData != null)
            {
                _initialDrawDone = true;
                OnPostTick().Forget();
            }

            if (Keyboard.current.rKey.wasPressedThisFrame)
            {
                if (!_isMovable || _prototype_TickManager.IsTickProcessing) return;
                HandleRestInput();
                return;
            }

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
                _prototype_GridVisualManager.Instance?.HighlightPoint(null); // Hide single white indicator while targeting
                _prototype_PlayerUIView.Instance.UpdateTargetingTooltipPosition(mousePos);
                HandleTargetingInput(mousePoint);
            }
            else
            {
                if (isHandViewActive)
                {
                    // 핸드 컨테이너가 활성화된 상태에서는 이동 불가 및 이동 경로 숨김
                    _prototype_GridVisualManager.Instance?.ShowTargetRange(null);
                    _prototype_GridVisualManager.Instance?.HighlightPoint(null);
                    return;
                }

                if (!_isMovable)
                {
                    // 카드 드래그 중 등 이동 불가 상태일 때 경로 표시 숨김
                    _prototype_GridVisualManager.Instance?.ShowTargetRange(null);
                    _prototype_GridVisualManager.Instance?.HighlightPoint(null);
                    return;
                }

                // 평소에는 이동 모드로 동작: 현재 마우스 위치까지의 경로(Path)를 TargetRange로 표시
                if (mousePoint.HasValue)
                {
                    List<_prototype_PointView> path = _prototype_GridManager.Instance.FindPath(_controlledEntityView.Point, mousePoint.Value, _controlledEntityView.EntityData);
                    if (path != null && path.Count > 0)
                    {
                        List<_prototype_Point> pathPoints = new();
                        foreach (var pv in path) pathPoints.Add(pv.Point);

                        // 경로 표시 (TargetRange 색상 활용)
                        _prototype_GridVisualManager.Instance?.ShowTargetRange(pathPoints);

                        if (Mouse.current.rightButton.wasPressedThisFrame)
                        {
                            HandleMovementInput(mousePoint.Value);
                        }
                    }
                    else if (mousePoint.Value == _controlledEntityView.Point)
                    {
                        // 자기 자신 위치를 호버 중일 때 클릭하면 대기(Rest)
                        List<_prototype_Point> selfPoint = new() { mousePoint.Value };
                        _prototype_GridVisualManager.Instance?.ShowTargetRange(selfPoint);

                        if (Mouse.current.rightButton.wasPressedThisFrame)
                        {
                            HandleMovementInput(mousePoint.Value);
                        }
                    }
                    else
                    {
                        _prototype_GridVisualManager.Instance?.ShowTargetRange(null);
                    }
                }
                else
                {
                    _prototype_GridVisualManager.Instance?.ShowTargetRange(null);
                }
                
                // 단일 포인트 호버(원래 기능)은 숨기거나 다른 방식으로 씀 (여기선 하위 호환을 위해 투명화 혹은 제거 가능하나 일단 남겨둠)
                _prototype_GridVisualManager.Instance?.HighlightPoint(null);
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
        
        private bool CheckAndHandleStun()
        {
            var lifeData = _controlledEntityView.EntityData as _prototype_LifeData;
            if (lifeData != null && lifeData.statusEffects.Find(s => s.type == _prototype_StatusType.Stun) != null)
            {
                _prototype_TickManager.AdvanceTick(async () => {
                    _controlledEntityView.transform.DOShakePosition(0.3f, 0.1f, 10, 90f, false, true);
                    await UniTask.Delay(300);
                }).Forget();
                return true;
            }
            return false;
        }

        private void HandleMovementInput(_prototype_Point targetPoint)
        {
            if (!_isMovable || _prototype_TickManager.IsTickProcessing) return;
            
            if (CheckAndHandleStun()) return;

            if (_controlledEntityView.Point == targetPoint)
            {
                HandleRestInput();
                return;
            }

            List<_prototype_PointView> path = _prototype_GridManager.Instance.FindPath(_controlledEntityView.Point, targetPoint, _controlledEntityView.EntityData);
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

        private void HandleRestInput()
        {
            if (CheckAndHandleStun()) return;

            if (_controlledEntityView is _prototype_LifeView lifeView && lifeView.Data != null)
            {
                _prototype_TickManager.AdvanceTick(async () => {
                    int recoverAmount = lifeView.Data.lifeStat.staminaRecoverAmount;
                    if (recoverAmount <= 0) recoverAmount = 2; // Fallback 기본 회복량

                    lifeView.Data.stamina.Current = Mathf.Min(lifeView.Data.stamina.Max, lifeView.Data.stamina.Current + recoverAmount);
                    
                    // 시각적 효과 (위로 뿅 튀어오르는 효과로 휴식 인지)
                    lifeView.transform.DOPunchScale(new Vector3(0.2f, 0.4f, 0.2f), 0.3f, 2, 1);
                    
                    _prototype_PlayerUIView.Instance.UpdatePlayerInfo();
                    
                    // 재생 완료까지 잠시 대기
                    await UniTask.Delay(300);
                }).Forget();
            }
        }

        public void DiscardCard(_prototype_CardData cardToDiscard)
        {
            if (_prototype_TickManager.IsTickProcessing) return;

            var playerLife = _controlledEntityView as _prototype_LifeView;
            if (playerLife?.Data?.cardDeck != null)
            {
                // 일반 버리기 처리 (버린 카드 더미로 이동)
                playerLife.Data.cardDeck.handedCardDatas.Remove(cardToDiscard);
                playerLife.Data.cardDeck.discardedCardDatas.Add(cardToDiscard);

                // 시각적 효과 (아래로 납작해졌다가 돌아옴)
                playerLife.transform.DOPunchScale(new Vector3(0.2f, -0.4f, 0.2f), 0.3f, 5, 1);
                
                _prototype_PlayerUIView.Instance.UpdatePlayerCardDeck();
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
                _prototype_GridVisualManager.Instance?.ShowCastRange(_currentCastRange);
            }
            _prototype_PlayerUIView.Instance.ShowTargetingUI(_targetingCard);
        }

        public void CancelTargeting()
        {
            _targetingCard = null;
            _isMovable = true;
            _currentCastRange = null;
            _prototype_GridVisualManager.Instance?.ClearRanges();
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
                _prototype_GridVisualManager.Instance?.ShowTargetRange(targetRange);

                // Execute on Left Click
                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    ExecuteCardCast(targetRange);
                }
            }
            else
            {
                _prototype_GridVisualManager.Instance?.ShowTargetRange(null); // clear target range
                
                // 맨땅 좌클릭 시 취소
                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    CancelTargeting();
                }
            }
        }

        private void ExecuteCardCast(List<_prototype_Point> targetRange)
        {
            if (CheckAndHandleStun()) 
            {
                CancelTargeting();
                return;
            }

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
                        bool found = false;
                        foreach (var entityView in pointView.PlacedEntityViews)
                        {
                            targets.Add(entityView.EntityData);
                            found = true;
                        }
                        if (!found && cardToCast.targetRange != null && cardToCast.targetRange.IncludeEmptyPoints)
                        {
                            targets.Add(new _prototype_EmptyPointData(pt));
                        }
                    }
                }

                if (cardToCast.actionList != null)
                {
                    foreach (var action in cardToCast.actionList)
                    {
                        var filteredTargets = targets;
                        if (!action.includeSelf)
                        {
                            filteredTargets = targets.Where(t => t != _controlledEntityView.EntityData).ToList();
                        }
                        await action.ExecuteCardAction(_controlledEntityView.EntityData, filteredTargets, null);
                    }
                }

                _prototype_PlayerUIView.Instance.UpdatePlayerInfo();
                _prototype_PlayerUIView.Instance.UpdatePlayerCardDeck();
            }).Forget();
        }
    }

}