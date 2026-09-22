using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using DG.Tweening;

namespace TDG0407._prototype
{

    public class _prototype_PlayerController : MonoBehaviour
    {
        private static _prototype_PlayerController _instance;
        public static _prototype_PlayerController Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindAnyObjectByType<_prototype_PlayerController>(FindObjectsInactive.Include);
                }
                return _instance;
            }
            private set => _instance = value;
        }

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

        private System.IDisposable _statusChangeSub;
        private bool _isStunAutoProgressing = false;
        public bool IsStunAutoProgressing => _isStunAutoProgressing;

        private void Awake()
        {
            if (_instance == null) _instance = this;
            else if (_instance != this) Destroy(gameObject);

            _statusChangeSub?.Dispose();
            _statusChangeSub = _prototype_EventBus.Listen<EntityStatusChangedEvent>(OnEntityStatusChanged);
        }
        private bool _initialDrawDone = false;

        public void Initialize()
        {
            _prototype_TickManager.RegisterPostTick(OnPostTick);

            if (_statusChangeSub == null)
            {
                _statusChangeSub = _prototype_EventBus.Listen<EntityStatusChangedEvent>(OnEntityStatusChanged);
            }

            if (!_initialDrawDone && _controlledEntityView != null && _controlledEntityView.EntityData != null)
            {
                _initialDrawDone = true;
                OnPostTick().Forget();
            }
        }

        private void OnDestroy()
        {
            _prototype_TickManager.UnregisterPostTick(OnPostTick);
            _statusChangeSub?.Dispose();
            _statusChangeSub = null;
        }

        private void OnEntityStatusChanged(EntityStatusChangedEvent evt)
        {
            if (_controlledEntityView == null || evt.Target != _controlledEntityView.EntityData) return;

            if (evt.Effect.type == _prototype_StatusType.Stun)
            {
                if (evt.IsAdded)
                {
                    CancelTargeting();
                    _prototype_GridVisualManager.Instance?.HideMovementPath();
                    _prototype_GridVisualManager.Instance?.HighlightPoint(null);
                    StartStunAutoProgression().Forget();
                }
            }
            else if (evt.Effect.type == _prototype_StatusType.Silence)
            {
                if (evt.IsAdded)
                {
                    CancelTargeting();
                }
                _prototype_PlayerUIView.Instance?.UpdatePlayerCardDeck();
            }
        }

        public async UniTaskVoid StartStunAutoProgression()
        {
            if (_isStunAutoProgressing) return;
            _isStunAutoProgressing = true;

            try
            {
                while (_controlledEntityView != null &&
                       _controlledEntityView.EntityData is _prototype_LifeData lifeData &&
                       !lifeData.IsDead &&
                       lifeData.HasStatusEffect(_prototype_StatusType.Stun))
                {
                    if (_prototype_TickManager.IsTickProcessing)
                    {
                        await UniTask.WaitWhile(() => _prototype_TickManager.IsTickProcessing);
                    }

                    if (_controlledEntityView == null ||
                        !(_controlledEntityView.EntityData is _prototype_LifeData currentLife) ||
                        currentLife.IsDead ||
                        !currentLife.HasStatusEffect(_prototype_StatusType.Stun))
                    {
                        break;
                    }

                    float startTime = Time.time;
                    _controlledEntityLastPoint = _controlledEntityView.Point;

                    await _prototype_TickManager.AdvanceTick(async () =>
                    {
                        if (_controlledEntityView != null)
                        {
                            _controlledEntityView.transform.DOShakePosition(0.3f, 0.1f, 10, 90f, false, true);
                        }
                        await UniTask.Delay(300);
                    });

                    float elapsed = Time.time - startTime;
                    int remainingMs = Mathf.Max(0, Mathf.RoundToInt((0.8f - elapsed) * 1000f));
                    if (remainingMs > 0)
                    {
                        await UniTask.Delay(remainingMs);
                    }
                }
            }
            finally
            {
                _isStunAutoProgressing = false;
            }
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

            if (playerLife?.Data != null && playerLife.Data.HasStatusEffect(_prototype_StatusType.Stun) && !_isStunAutoProgressing)
            {
                StartStunAutoProgression().Forget();
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

            var playerLife = _controlledEntityView as _prototype_LifeView;
            bool isStunned = _isStunAutoProgressing || (playerLife != null && playerLife.Data != null && playerLife.Data.HasStatusEffect(_prototype_StatusType.Stun));

            if (isStunned)
            {
                _prototype_GridVisualManager.Instance?.HideMovementPath();
                _prototype_GridVisualManager.Instance?.HighlightPoint(null);
                return;
            }

            if (_prototype_PlayerUIView.Instance != null && _prototype_PlayerUIView.Instance.IsRewardModalOpen)
            {
                if (Keyboard.current != null && (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame || Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame))
                {
                    _prototype_PlayerUIView.Instance.HideRewardModal();
                }
                _prototype_GridVisualManager.Instance?.HideMovementPath();
                _prototype_GridVisualManager.Instance?.HighlightPoint(null);
                return;
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
                _prototype_PlayerUIView.Instance?.HideHazardInfoTooltip(force: true);
                _prototype_GridVisualManager.Instance?.HighlightHazardAttacker(null, false);
                _prototype_GridVisualManager.Instance?.HighlightPoint(null); // Hide single white indicator while targeting
                _prototype_PlayerUIView.Instance.UpdateTargetingTooltipPosition(mousePos);
                HandleTargetingInput(mousePoint);
            }
            else
            {
                HandleHazardInteraction(mousePoint, mousePos);

                if (isHandViewActive)
                {
                    // 핸드 컨테이너가 활성화된 상태에서는 이동 불가 및 이동 경로 숨김
                    _prototype_GridVisualManager.Instance?.HideMovementPath();
                    _prototype_GridVisualManager.Instance?.HighlightPoint(null);
                    return;
                }

                if (!_isMovable || _prototype_TickManager.IsTickProcessing)
                {
                    // 카드 드래그 중 등 이동 불가 상태이거나 턴이 진행 중일 때 경로 표시 숨김
                    _prototype_GridVisualManager.Instance?.HideMovementPath();
                    _prototype_GridVisualManager.Instance?.HighlightPoint(null);
                    return;
                }

                // 평소에는 이동 모드로 동작: 현재 마우스 위치까지의 경로(Path)를 표시
                if (mousePoint.HasValue)
                {
                    List<_prototype_PointView> path = _prototype_GridManager.Instance.FindPath(_controlledEntityView.Point, mousePoint.Value, _controlledEntityView.EntityData);
                    if (path != null && path.Count > 0)
                    {
                        List<_prototype_Point> pathPoints = new();
                        pathPoints.Add(_controlledEntityView.Point); // 출발점 추가
                        foreach (var pv in path) pathPoints.Add(pv.Point);

                        // 경로 표시
                        _prototype_GridVisualManager.Instance?.ShowMovementPath(pathPoints);
                        _prototype_GridVisualManager.Instance?.ShowTargetRange(null); // 혹시 남아있을 수 있는 타겟 레인지 제거

                        if (Mouse.current.rightButton.wasPressedThisFrame)
                        {
                            HandleMovementInput(mousePoint.Value);
                        }
                    }
                    else if (mousePoint.Value == _controlledEntityView.Point)
                    {
                        // 자기 자신 위치를 호버 중일 때 클릭하면 대기(Rest)
                        _prototype_GridVisualManager.Instance?.HideMovementPath();
                        _prototype_GridVisualManager.Instance?.ShowTargetRange(null);

                        if (Mouse.current.rightButton.wasPressedThisFrame)
                        {
                            HandleMovementInput(mousePoint.Value);
                        }
                    }
                    else
                    {
                        _prototype_GridVisualManager.Instance?.HideMovementPath();
                        _prototype_GridVisualManager.Instance?.ShowTargetRange(null);
                    }
                }
                else
                {
                    _prototype_GridVisualManager.Instance?.HideMovementPath();
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
            var lifeData = _controlledEntityView != null ? _controlledEntityView.EntityData as _prototype_LifeData : null;
            if (lifeData != null && lifeData.HasStatusEffect(_prototype_StatusType.Stun))
            {
                if (!_isStunAutoProgressing)
                {
                    StartStunAutoProgression().Forget();
                }
                return true;
            }
            return false;
        }

        private void HandleMovementInput(_prototype_Point targetPoint)
        {
            if (!_isMovable || _prototype_TickManager.IsTickProcessing) return;

            _prototype_PlayerUIView.Instance?.HideHazardInfoTooltip(force: true);
            _prototype_GridVisualManager.Instance?.HighlightHazardAttacker(null, false);

            if (CheckAndHandleStun()) return;



            var lifeData = _controlledEntityView.EntityData as _prototype_LifeData;
            if (lifeData != null)
            {
                if (lifeData.HasStatusEffect(_prototype_StatusType.Freeze)) return;

                var fearEffect = lifeData.GetStatusEffect(_prototype_StatusType.Fear);
                if (fearEffect != null && fearEffect.sourceEntity != null)
                {
                    int oldDist = Mathf.Abs(lifeData.point.x - fearEffect.sourceEntity.point.x) + Mathf.Abs(lifeData.point.y - fearEffect.sourceEntity.point.y);
                    int newDist = Mathf.Abs(targetPoint.x - fearEffect.sourceEntity.point.x) + Mathf.Abs(targetPoint.y - fearEffect.sourceEntity.point.y);
                    if (newDist < oldDist) return;
                }
            }

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
                _prototype_TickManager.AdvanceTick(async () =>
                {
                    await _prototype_InteractionManager.MoveEntity(_controlledEntityView, _prototype_GridManager.Instance.GetPointView(_controlledEntityView.Point), nextStep);
                    _prototype_PlayerUIView.Instance.UpdatePlayerInfo();
                }).Forget();
            }
        }



        private void HandleRestInput()
        {
            _prototype_PlayerUIView.Instance?.HideHazardInfoTooltip(force: true);
            _prototype_GridVisualManager.Instance?.HighlightHazardAttacker(null, false);

            if (CheckAndHandleStun()) return;

            if (_controlledEntityView is _prototype_LifeView lifeView && lifeView.Data != null)
            {
                _controlledEntityLastPoint = _controlledEntityView.Point;
                _prototype_TickManager.AdvanceTick(async () =>
                {
                    lifeView.Data.Rest();

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
            if (cardToDiscard.sourceProvider != null) return; // Cannot discard provided cards

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
            if (cardData == null) return;

            var playerLife = _controlledEntityView as _prototype_LifeView;
            if (playerLife != null && playerLife.Data != null)
            {
                if (playerLife.Data.HasStatusEffect(_prototype_StatusType.Stun) || _isStunAutoProgressing)
                {
                    return;
                }
                if (playerLife.Data.HasStatusEffect(_prototype_StatusType.Silence))
                {
                    _prototype_PlayerUIView.Instance?.ShowWarning("침묵 상태에서는 카드를 사용할 수 없습니다!");
                    return;
                }
            }

            // 상호작용 카드는 별도의 범위/대상 선택(타겟팅) 과정 없이 즉시 발동
            if (cardData is _prototype_InteractionCardData interactionCard)
            {
                _targetingCard = interactionCard;
                List<_prototype_Point> instantTargets = interactionCard.sourceProvider != null
                    ? new() { interactionCard.sourceProvider.point }
                    : new();
                _prototype_Point targetPt = interactionCard.sourceProvider != null ? interactionCard.sourceProvider.point : _controlledEntityView.Point;
                ExecuteCardCast(instantTargets, targetPt);
                return;
            }

            _targetingCard = cardData;
            _isMovable = false;

            if (_targetingCard is _prototype_BattleCardData battleCard && battleCard.castRange != null)
            {
                _currentCastRange = battleCard.castRange.GetValidCastPoints(_controlledEntityView.Point);
                _prototype_GridVisualManager.Instance?.ShowCastRange(_currentCastRange, _controlledEntityView.Point);
            }
            _prototype_PlayerUIView.Instance.ShowTargetingUI(_targetingCard);
        }


        public void CancelTargeting()
        {
            _targetingCard = null;
            _isMovable = true;
            _currentCastRange = null;
            _prototype_GridVisualManager.Instance?.ClearRanges();
            _prototype_PlayerUIView.Instance?.UpdateTargetingHover(null);
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
                var ptView = _prototype_GridManager.Instance.GetPointView(mousePoint.Value);
                var hoveredEntity = ptView?.PlacedEntityViews?.FirstOrDefault(v => v != null && v.EntityData != null && v.EntityData != _controlledEntityView?.EntityData)?.EntityData;
                _prototype_PlayerUIView.Instance?.UpdateTargetingHover(hoveredEntity);

                var lifeData = _controlledEntityView.EntityData as _prototype_LifeData;
                if (lifeData != null)
                {
                    if (lifeData.HasStatusEffect(_prototype_StatusType.Silence)) return;

                    var fearEffect = lifeData.GetStatusEffect(_prototype_StatusType.Fear);
                    if (fearEffect != null && fearEffect.sourceEntity != null)
                    {
                        if (ptView != null && ptView.PlacedEntityViews.Exists(v => v.EntityData == fearEffect.sourceEntity))
                        {
                            return; // Cannot target feared entity
                        }
                    }
                }

                // Show Target Range
                List<_prototype_Point> targetRange = (_targetingCard is _prototype_BattleCardData bc && bc.targetRange != null)
                    ? bc.targetRange.GetValidTargetPoints(_controlledEntityView.Point, mousePoint.Value)
                    : new List<_prototype_Point> { mousePoint.Value };
                _prototype_GridVisualManager.Instance?.ShowTargetRange(targetRange, mousePoint.Value);

                // Execute on Left Click
                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    ExecuteCardCast(targetRange, mousePoint.Value);
                }
            }
            else
            {
                _prototype_PlayerUIView.Instance?.UpdateTargetingHover(null);
                _prototype_GridVisualManager.Instance?.ShowTargetRange(null); // clear target range

                // 맨땅 좌클릭 시 취소
                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    CancelTargeting();
                }
            }
        }

        private void ExecuteCardCast(List<_prototype_Point> targetRange, _prototype_Point targetedPoint = default)
        {
            if (CheckAndHandleStun())
            {
                CancelTargeting();
                return;
            }

            var playerLife = _controlledEntityView as _prototype_LifeView;
            if (playerLife?.Data?.HasStatusEffect(_prototype_StatusType.Silence) == true)
            {
                _prototype_PlayerUIView.Instance?.ShowWarning("침묵 상태에서는 카드를 사용할 수 없습니다!");
                CancelTargeting();
                return;
            }

            var cardToCast = _targetingCard;

            // 상호작용 카드 실행
            if (cardToCast is _prototype_InteractionCardData interactionCard)
            {
                CancelTargeting();
                _controlledEntityLastPoint = _controlledEntityView.Point;
                _prototype_TickManager.AdvanceTick(async () =>
                {
                    await interactionCard.ExecuteInteraction(_controlledEntityView.EntityData);
                    _prototype_PlayerUIView.Instance?.UpdatePlayerInfo();
                    _prototype_PlayerUIView.Instance?.UpdatePlayerCardDeck();
                }).Forget();
                return;
            }

            var battleCard = cardToCast as _prototype_BattleCardData;
            if (battleCard == null)
            {
                CancelTargeting();
                return;
            }

            // 1. Move card to discard pile and Deduct Cost
            if (playerLife?.Data?.cardDeck != null)
            {
                if (battleCard.sourceProvider == null)
                {
                    playerLife.Data.cardDeck.handedCardDatas.Remove(battleCard);

                    var burning = playerLife.Data.GetStatusEffect(_prototype_StatusType.Burning);
                    bool destroyed = false;
                    if (burning != null)
                    {
                        float destroyProb = burning.value / (burning.value + 200f);
                        if (UnityEngine.Random.value < destroyProb)
                        {
                            destroyed = true;
                        }
                    }

                    if (destroyed)
                    {
                        playerLife.Data.cardDeck.destroyedCardDatas.Add(battleCard);
                    }
                    else
                    {
                        playerLife.Data.cardDeck.discardedCardDatas.Add(battleCard);
                    }
                }

                var cost = battleCard.costValue;
                if (cost != null)
                {
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
            }

            CancelTargeting();

            _controlledEntityLastPoint = _controlledEntityView.Point;
            // 2. Advance Tick and Apply Action
            _prototype_TickManager.AdvanceTick(async () =>
            {
                List<_prototype_EntityData> targets = new();
                foreach (var pt in targetRange)
                {
                    var pointView = _prototype_GridManager.Instance.GetPointView(pt);
                    if (pointView != null)
                    {
                        foreach (var entityView in pointView.PlacedEntityViews)
                        {
                            targets.Add(entityView.EntityData);
                        }
                    }
                }
                targets = targets.Distinct().ToList();

                if (battleCard.actionList != null)
                {
                    foreach (var action in battleCard.actionList)
                    {
                        var filteredTargets = targets;
                        if (!action.includeSelf)
                        {
                            filteredTargets = targets.Where(t => t != _controlledEntityView.EntityData).ToList();
                        }
                        int dx = targetedPoint.x - _controlledEntityView.Point.x;
                        int dy = targetedPoint.y - _controlledEntityView.Point.y;
                        int normX = dx != 0 ? (int)Mathf.Sign(dx) : 0;
                        int normY = dy != 0 ? (int)Mathf.Sign(dy) : 0;
                        _prototype_Point attackDir = new _prototype_Point(normX, normY);
                        var actionParams = new _prototype_CardActionParams(battleCard, targetedPoint, attackDir, targetRange);
                        await action.ExecuteAction(_controlledEntityView.EntityData, filteredTargets, actionParams);
                    }
                }

                _prototype_PlayerUIView.Instance.UpdatePlayerInfo();
                _prototype_PlayerUIView.Instance.UpdatePlayerCardDeck();
            }).Forget();
        }

        private void HandleHazardInteraction(_prototype_Point? mousePoint, Vector2 mousePos)
        {
            if (_prototype_PlayerUIView.Instance == null || _prototype_GridVisualManager.Instance == null) return;

            // ESC 키 누르면 핀 고정 해제
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                _prototype_PlayerUIView.Instance.HideHazardInfoTooltip(force: true);
                _prototype_GridVisualManager.Instance.HighlightHazardAttacker(null, false);
                return;
            }

            bool leftClicked = Mouse.current.leftButton.wasPressedThisFrame;

            if (mousePoint.HasValue)
            {
                var hazardInfos = _prototype_GridVisualManager.Instance.GetHazardAttackInfosAtPoint(mousePoint.Value);
                if (hazardInfos != null && hazardInfos.Count > 0)
                {
                    if (leftClicked)
                    {
                        // 클릭 시 핀 고정 토글
                        bool willPin = !_prototype_PlayerUIView.Instance.IsHazardTooltipPinned ||
                                       _prototype_PlayerUIView.Instance.PinnedHazardPoint != mousePoint.Value;

                        if (willPin)
                        {
                            _prototype_PlayerUIView.Instance.ShowHazardInfoTooltip(hazardInfos, mousePos, mousePoint.Value, isPinned: true);
                            _prototype_GridVisualManager.Instance.HighlightHazardAttacker(hazardInfos[0].AttackerView, true);
                        }
                        else
                        {
                            _prototype_PlayerUIView.Instance.HideHazardInfoTooltip(force: true);
                            _prototype_GridVisualManager.Instance.HighlightHazardAttacker(null, false);
                        }
                    }
                    else
                    {
                        // 호버 동작 (현재 핀 고정 상태가 아니면 호버된 타일 정보 표시)
                        if (!_prototype_PlayerUIView.Instance.IsHazardTooltipPinned)
                        {
                            _prototype_PlayerUIView.Instance.ShowHazardInfoTooltip(hazardInfos, mousePos, mousePoint.Value, isPinned: false);
                            _prototype_GridVisualManager.Instance.HighlightHazardAttacker(hazardInfos[0].AttackerView, true);
                        }
                    }
                }
                else
                {
                    // Hazard가 없는 타일
                    if (leftClicked)
                    {
                        // 빈 곳 클릭 시 핀 고정 해제
                        _prototype_PlayerUIView.Instance.HideHazardInfoTooltip(force: true);
                        _prototype_GridVisualManager.Instance.HighlightHazardAttacker(null, false);
                    }
                    else if (!_prototype_PlayerUIView.Instance.IsHazardTooltipPinned)
                    {
                        _prototype_PlayerUIView.Instance.HideHazardInfoTooltip(force: false);
                        _prototype_GridVisualManager.Instance.HighlightHazardAttacker(null, false);
                    }
                }
            }
            else
            {
                // 타일 밖 마우스
                if (leftClicked)
                {
                    _prototype_PlayerUIView.Instance.HideHazardInfoTooltip(force: true);
                    _prototype_GridVisualManager.Instance.HighlightHazardAttacker(null, false);
                }
                else if (!_prototype_PlayerUIView.Instance.IsHazardTooltipPinned)
                {
                    _prototype_PlayerUIView.Instance.HideHazardInfoTooltip(force: false);
                    _prototype_GridVisualManager.Instance.HighlightHazardAttacker(null, false);
                }
            }

            // 핀 고정 상태가 아닐 때 마우스 위치에 따라 툴팁 위치 갱신
            if (!_prototype_PlayerUIView.Instance.IsHazardTooltipPinned)
            {
                _prototype_PlayerUIView.Instance.UpdateHazardTooltipPosition(mousePos);
            }
        }
    }

}