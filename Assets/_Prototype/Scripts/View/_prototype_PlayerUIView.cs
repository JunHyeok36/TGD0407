using System.Collections.Generic;
using System.Linq;
using TDG0407.Domain;
using UnityEngine;
using UnityEngine.UIElements;

namespace TDG0407._prototype
{

    [RequireComponent(typeof(PanelRenderer))]
    public class _prototype_PlayerUIView : MonoBehaviour
    {
        private static _prototype_PlayerUIView _instance;
        public static _prototype_PlayerUIView Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindAnyObjectByType<_prototype_PlayerUIView>(FindObjectsInactive.Include);
                }
                return _instance;
            }
            private set => _instance = value;
        }

        [SerializeField] private PanelRenderer _panelRenderer;
        [SerializeField] private VisualTreeAsset _cardViewTemplate;

        [Header("Floating UI & HUD Prefabs")]
        [SerializeField] private GameObject _floatingTextPrefab;
        [SerializeField] private GameObject _lifeHudPrefab;

        private Label _label_point;
        private Label _label_health;
        private Label _label_stamina;
        private Label _label_actionPoints;
        private VisualElement _playerStatusTray;
        private Label _label_remainsCount;
        private Label _label_discardedCount;
        private Label _label_destroyedCount;
        private VisualElement _cardDragArea;
        private VisualElement _handCardContainer;

        // Play Mode UI Layers
        private VisualElement _battleModeUI;
        private VisualElement _explorationModeUI;

        // Status Tooltip UI Elements
        private VisualElement _statusTooltip;
        private Label _statusTooltipTitle;
        private Label _statusTooltipDesc;
        private System.IDisposable _playerStatusChangeSub;
        private System.IDisposable _playerShieldChangeSub;

        // Targeting UI Elements
        private VisualElement _targetingHint;
        private VisualElement _targetingTooltip;
        private Label _tooltipCardName;
        private Label _tooltipCardCost;
        private Label _tooltipCardDesc;

        // Hazard Info Tooltip UI Elements
        private VisualElement _hazardInfoTooltip;
        private Label _hazardTooltipHeaderTitle;
        private Label _hazardTooltipPinBadge;
        private VisualElement _hazardTooltipItemsContainer;
        public bool IsHazardTooltipPinned { get; private set; }
        private _prototype_Point? _pinnedHazardPoint;
        public _prototype_Point? PinnedHazardPoint => _pinnedHazardPoint;

        private VisualElement _warningMessageContainer;
        private Label _warningMessageText;
        private VisualElement _gameOverContainer;

        // Mode indicator
        private VisualElement _modeIndicatorBanner;
        private Label _modeIndicatorText;
        private System.IDisposable _playModeSub;
        private System.IDisposable _floatingDamagedSub;
        private System.IDisposable _floatingStatusSub;

        // Reward Notification Modal Elements
        private VisualElement _rewardContainer;
        private Label _rewardHeader;
        private Label _rewardBadge;
        private Label _rewardSource;
        private Label _rewardTitle;
        private Label _rewardDescription;
        private Label _rewardDestinationHint;
        private Button _rewardConfirmBtn;
        private System.IDisposable _cardAcquiredSub;
        private System.IDisposable _itemAcquiredSub;
        private System.Action _onRewardModalClosed;
        public bool IsRewardModalOpen => _rewardContainer != null && _rewardContainer.style.display == DisplayStyle.Flex;

        // Left Acquisition Toast Container
        private VisualElement _acquisitionToastContainer;

        // Card Description Detailed Mode (Alt/Shift)
        private bool _isDetailedDescriptionMode = false;
        private _prototype_CardData _currentTargetingCard;
        private _prototype_EntityData _currentHoveredTarget;

        private VisualElement _cardFxContainer;
        public bool IsCardHovered { get; private set; }

        // Active Card Wide Banner UI Elements (방안 A)
        private VisualElement _activeCardWideBanner;
        private Label _wideCardCost;
        private Label _wideCardName;
        private Label _wideCardType;
        private Label _wideCardDestroyBadge;
        private Label _wideCardDesc;

        private List<VisualElement> handCardViews = new();

        public GameObject LifeHudPrefab => _lifeHudPrefab;

        private void Awake()
        {
            if (_instance == null) _instance = this;
            else if (_instance != this) Destroy(gameObject);

            RegisterFloatingUIPrefabs();
        }

        private void Update()
        {
            bool isAltOrShift = false;
            if (UnityEngine.InputSystem.Keyboard.current != null)
            {
                var kb = UnityEngine.InputSystem.Keyboard.current;
                isAltOrShift = kb.leftAltKey.isPressed || kb.rightAltKey.isPressed ||
                               kb.leftShiftKey.isPressed || kb.rightShiftKey.isPressed;
            }

            if (isAltOrShift != _isDetailedDescriptionMode)
            {
                _isDetailedDescriptionMode = isAltOrShift;
                RefreshAllCardDescriptions();
            }
        }

        /// <summary>
        /// BootStrapper Step 0(선등록 단계)에서 호출되어, 엔티티/HUD 생성 전에 프리팹을 정적 등록합니다.
        /// </summary>
        public void RegisterFloatingUIPrefabs()
        {
            if (_floatingTextPrefab != null)
            {
                _prototype_FloatingText.SetPrefab(_floatingTextPrefab);
            }
            if (_lifeHudPrefab != null)
            {
                _prototype_LifeHUD.SetDefaultPrefab(_lifeHudPrefab);
            }
        }

        public void Initialize()
        {
            if (_panelRenderer == null)
            {
                _panelRenderer = GetComponent<PanelRenderer>();
            }
            if (_panelRenderer != null)
            {
                _panelRenderer.RegisterUIReloadCallback(OnUIReload);
            }

            // 모드 변경 이벤트 구독
            EnsurePlayModeSubscription();

            // FloatingText 피격 및 상태이상 이벤트 리스너 구독
            _floatingDamagedSub?.Dispose();
            _floatingDamagedSub = _prototype_EventBus.Listen<EntityDamagedEvent>(_prototype_FloatingText.OnEntityDamaged);
            _floatingStatusSub?.Dispose();
            _floatingStatusSub = _prototype_EventBus.Listen<EntityStatusChangedEvent>(_prototype_FloatingText.OnEntityStatusChanged);

            // 획득 알림 모달 이벤트 리스너 구독
            _cardAcquiredSub?.Dispose();
            _cardAcquiredSub = _prototype_EventBus.Listen<EntityCardAcquiredEvent>(OnEntityCardAcquired);
            _itemAcquiredSub?.Dispose();
            _itemAcquiredSub = _prototype_EventBus.Listen<EntityItemAcquiredEvent>(OnEntityItemAcquired);

            // 플레이어 상태변화 이벤트 리스너 구독
            _playerStatusChangeSub?.Dispose();
            _playerStatusChangeSub = _prototype_EventBus.Listen<EntityStatusChangedEvent>(evt =>
            {
                var playerView = _prototype_PlayerController.Instance?.ControlledEntityView;
                if (playerView != null && evt.Target == playerView.EntityData)
                {
                    UpdatePlayerInfo();
                }
            });

            // 플레이어 보호막변화 이벤트 리스너 구독
            _playerShieldChangeSub?.Dispose();
            _playerShieldChangeSub = _prototype_EventBus.Listen<EntityShieldChangedEvent>(evt =>
            {
                var playerView = _prototype_PlayerController.Instance?.ControlledEntityView;
                if (playerView != null && evt.Entity == playerView.EntityData)
                {
                    UpdatePlayerInfo();
                }
            });

            // 초기 UI 갱신
            UpdatePlayerInfo();
            UpdatePlayerCardDeck();
        }

        public void EnsurePlayModeSubscription()
        {
            _playModeSub?.Dispose();
            _playModeSub = _prototype_EventBus.Listen<PlayModeChangedEvent>(OnPlayModeChanged);
        }

        private void OnDestroy()
        {
            _panelRenderer.UnregisterUIReloadCallback(OnUIReload);
            _playModeSub?.Dispose();
            _floatingDamagedSub?.Dispose();
            _floatingStatusSub?.Dispose();
            _cardAcquiredSub?.Dispose();
            _itemAcquiredSub?.Dispose();
            _playerStatusChangeSub?.Dispose();
            _playerShieldChangeSub?.Dispose();
        }

        private void OnUIReload(PanelRenderer panelRenderer, VisualElement root)
        {
            _label_point = root.Q<Label>("Point");
            _label_health = root.Q<Label>("Health");
            _label_stamina = root.Q<Label>("Stamina");
            _label_actionPoints = root.Q<Label>("ActionPoints");
            _playerStatusTray = root.Q<VisualElement>("PlayerStatusTray");
            _label_remainsCount = root.Q<Label>("RemainsCount");
            _label_discardedCount = root.Q<Label>("DiscardedCount");
            _label_destroyedCount = root.Q<Label>("DestroyedCount");

            _cardDragArea = root.Q<VisualElement>("CardDragArea");
            _handCardContainer = root.Q<VisualElement>("HandCardContainer");
            _battleModeUI = root.Q<VisualElement>("BattleModeUI");
            _explorationModeUI = root.Q<VisualElement>("ExplorationModeUI");

            if (_cardFxContainer != null && _cardFxContainer.parent != null)
            {
                _cardFxContainer.RemoveFromHierarchy();
            }
            _cardFxContainer = new VisualElement();
            _cardFxContainer.name = "CardFxContainer";
            _cardFxContainer.pickingMode = PickingMode.Ignore;
            _cardFxContainer.style.position = Position.Absolute;
            _cardFxContainer.style.left = 0;
            _cardFxContainer.style.top = 0;
            _cardFxContainer.style.right = 0;
            _cardFxContainer.style.bottom = 0;
            root.Add(_cardFxContainer);

            _targetingHint = root.Q<VisualElement>("TargetingHint");
            _targetingTooltip = root.Q<VisualElement>("TargetingTooltip");
            _tooltipCardName = root.Q<Label>("TooltipCardName");
            _tooltipCardCost = root.Q<Label>("TooltipCardCost");
            _tooltipCardDesc = root.Q<Label>("TooltipCardDescription");

            // Active Card Wide Banner 바인딩
            _activeCardWideBanner = root.Q<VisualElement>("ActiveCardWideBanner");
            _wideCardCost = root.Q<Label>("WideCardCost");
            _wideCardName = root.Q<Label>("WideCardName");
            _wideCardType = root.Q<Label>("WideCardType");
            _wideCardDestroyBadge = root.Q<Label>("WideCardDestroyBadge");
            _wideCardDesc = root.Q<Label>("WideCardDesc");

            _warningMessageContainer = root.Q<VisualElement>("WarningMessageContainer");
            _warningMessageText = root.Q<Label>("WarningMessageText");

            _gameOverContainer = root.Q<VisualElement>("GameOverContainer");

            _modeIndicatorBanner = root.Q<VisualElement>("ModeIndicatorBanner");
            _modeIndicatorText = root.Q<Label>("ModeIndicatorText");

            _rewardContainer = root.Q<VisualElement>("RewardNotificationContainer");
            _acquisitionToastContainer = root.Q<VisualElement>("AcquisitionToastContainer");
            _acquisitionToastContainer?.Clear();
            _rewardHeader = root.Q<Label>("RewardHeader");
            _rewardBadge = root.Q<Label>("RewardBadge");
            _rewardSource = root.Q<Label>("RewardSource");
            _rewardTitle = root.Q<Label>("RewardTitle");
            _rewardDescription = root.Q<Label>("RewardDescription");
            _rewardDestinationHint = root.Q<Label>("RewardDestinationHint");
            _rewardConfirmBtn = root.Q<Button>("RewardConfirmButton");

            if (_rewardConfirmBtn != null)
            {
                _rewardConfirmBtn.clicked -= HideRewardModal;
                _rewardConfirmBtn.clicked += HideRewardModal;
            }
            if (_rewardContainer != null)
            {
                _rewardContainer.RegisterCallback<PointerDownEvent>(evt =>
                {
                    if (evt.target == _rewardContainer)
                    {
                        HideRewardModal();
                        evt.StopPropagation();
                    }
                });
            }

            UpdatePlayerInfo();
            UpdatePlayerCardDeck();

            // UI 리로드 후 현재 모드 표시 갱신
            _prototype_PlayMode currentMode = _prototype_PlayModeManager.Instance != null
                ? _prototype_PlayModeManager.Instance.CurrentMode
                : _prototype_PlayMode.Battle;
            UpdateModeIndicator(currentMode);

            BuildHazardInfoTooltip(root);
            BuildStatusTooltip(root);
        }

        private void OnEntityCardAcquired(EntityCardAcquiredEvent evt)
        {
            if (_prototype_PlayerController.Instance != null &&
                _prototype_PlayerController.Instance.ControlledEntityView != null &&
                evt.Entity == _prototype_PlayerController.Instance.ControlledEntityView.EntityData)
            {
                string cardName = evt.Card != null ? evt.Card.id : "카드";
                ShowAcquisitionToast(null, cardName, 1, isCard: true);
                ShowCardAcquiredModal(evt.Card, evt.SourceName);
            }
        }

        private void OnEntityItemAcquired(EntityItemAcquiredEvent evt)
        {
            if (_prototype_PlayerController.Instance != null &&
                _prototype_PlayerController.Instance.ControlledEntityView != null &&
                evt.Entity == _prototype_PlayerController.Instance.ControlledEntityView.EntityData)
            {
                string itemName = evt.Item != null ? (!string.IsNullOrEmpty(evt.Item.displayName) ? evt.Item.displayName : evt.Item.id) : "아이템";
                Sprite itemIcon = evt.Item != null ? evt.Item.icon : null;
                ShowAcquisitionToast(itemIcon, itemName, evt.Quantity, isCard: false);
                ShowItemAcquiredModal(evt.Item, evt.Quantity, evt.SourceName);
            }
        }

        /// <summary>
        /// 화면 좌측에 아이템/카드 획득 알림 토스트(TYPE A: CMD 레트로 터미널)를 띄웁니다.
        /// </summary>
        public void ShowAcquisitionToast(Sprite icon, string name, int count = 1, bool isCard = true, string customTag = null)
        {
            if (_acquisitionToastContainer == null) return;

            // 최대 4개 초과 시 가장 오래된 항목 제거
            while (_acquisitionToastContainer.childCount >= 4)
            {
                _acquisitionToastContainer.RemoveAt(0);
            }

            var toast = new VisualElement();
            toast.AddToClassList("cmd-toast-item");

            // 1. Icon Box
            var iconBox = new VisualElement();
            iconBox.AddToClassList("cmd-toast-icon-box");
            if (icon != null)
            {
                var iconImg = new VisualElement();
                iconImg.AddToClassList("cmd-toast-icon");
                iconImg.style.backgroundImage = new StyleBackground(icon);
                iconBox.Add(iconImg);
            }
            else
            {
                var fallbackLabel = new Label(isCard ? "🎴" : "🧪");
                fallbackLabel.AddToClassList("cmd-toast-fallback-icon");
                iconBox.Add(fallbackLabel);
            }
            toast.Add(iconBox);

            // 2. Content (Tag + Name)
            var content = new VisualElement();
            content.AddToClassList("cmd-toast-content");

            string tagText = !string.IsNullOrEmpty(customTag)
                ? customTag
                : (isCard ? "> [CARD]" : "> [ITEM]");
            var tagLabel = new Label(tagText);
            tagLabel.AddToClassList("cmd-toast-tag");
            if (!isCard)
            {
                tagLabel.style.color = new Color(0.3f, 0.85f, 1f); // 아이템은 청록색
            }
            content.Add(tagLabel);

            var nameLabel = new Label(name);
            nameLabel.AddToClassList("cmd-toast-name");
            content.Add(nameLabel);
            toast.Add(content);

            // 3. Count Badge
            var badge = new VisualElement();
            badge.AddToClassList("cmd-toast-badge");
            var badgeText = new Label($"+{count}");
            badgeText.AddToClassList("cmd-toast-badge-text");
            badge.Add(badgeText);
            toast.Add(badge);

            _acquisitionToastContainer.Add(toast);

            // 슬라이드 인 애니메이션 (1프레임 뒤 클래스 추가)
            toast.schedule.Execute(() =>
            {
                toast.AddToClassList("cmd-toast-item-show");
            }).StartingIn(10);

            // 3초 후 슬라이드 아웃 및 제거
            toast.schedule.Execute(() =>
            {
                if (toast.panel != null)
                {
                    toast.RemoveFromClassList("cmd-toast-item-show");
                    toast.AddToClassList("cmd-toast-item-hide");

                    toast.schedule.Execute(() =>
                    {
                        if (toast.parent != null)
                        {
                            toast.RemoveFromHierarchy();
                        }
                    }).StartingIn(300);
                }
            }).StartingIn(3000);
        }

        public void ShowCardAcquiredModal(_prototype_CardData card, string sourceName = "상자", System.Action onClose = null)
        {
            if (_rewardContainer == null) return;

            _onRewardModalClosed = onClose;
            if (_rewardHeader != null) _rewardHeader.text = "★ NEW CARD ACQUIRED ★";
            if (_rewardBadge != null)
            {
                _rewardBadge.text = card is _prototype_InteractionCardData ? "[상호작용 카드]" : "[전투 카드]";
                _rewardBadge.style.color = card is _prototype_InteractionCardData ? new StyleColor(new Color(0.2f, 0.8f, 1f)) : new StyleColor(new Color(0.2f, 1f, 0.2f));
            }
            if (_rewardSource != null) _rewardSource.text = string.IsNullOrEmpty(sourceName) ? "" : $"출처: {sourceName}";
            if (_rewardTitle != null) _rewardTitle.text = card != null ? card.id : "카드";
            if (_rewardDescription != null)
            {
                if (card != null)
                {
                    var playerView = _prototype_PlayerController.Instance?.ControlledEntityView as _prototype_LifeView;
                    var playerLife = playerView?.Data;
                    _rewardDescription.text = _prototype_CardDescriptionFormatter.FormatDescription(card, playerLife, _isDetailedDescriptionMode);
                }
                else
                {
                    _rewardDescription.text = "";
                }
            }
            if (_rewardDestinationHint != null) _rewardDestinationHint.text = "> 덱(allCardDatas & remainedCards)에 추가되었습니다.";

            _rewardContainer.style.display = DisplayStyle.Flex;
            _rewardContainer.BringToFront();

            UpdatePlayerCardDeck();
        }

        public void ShowItemAcquiredModal(_prototype_ItemDataModel item, int quantity = 1, string sourceName = "상자", System.Action onClose = null)
        {
            if (_rewardContainer == null) return;

            _onRewardModalClosed = onClose;
            if (_rewardHeader != null) _rewardHeader.text = "★ NEW ITEM ACQUIRED ★";
            if (_rewardBadge != null)
            {
                _rewardBadge.text = "[아이템]";
                _rewardBadge.style.color = new StyleColor(new Color(1f, 0.84f, 0f));
            }
            if (_rewardSource != null) _rewardSource.text = string.IsNullOrEmpty(sourceName) ? "" : $"출처: {sourceName}";
            if (_rewardTitle != null) _rewardTitle.text = item != null ? $"{item.name} x{quantity}" : $"아이템 x{quantity}";
            if (_rewardDescription != null) _rewardDescription.text = item != null ? item.description : "";
            if (_rewardDestinationHint != null) _rewardDestinationHint.text = "> 인벤토리에 추가되었습니다.";

            _rewardContainer.style.display = DisplayStyle.Flex;
            _rewardContainer.BringToFront();
        }

        public void HideRewardModal()
        {
            if (_rewardContainer != null)
            {
                _rewardContainer.style.display = DisplayStyle.None;
            }

            var callback = _onRewardModalClosed;
            _onRewardModalClosed = null;
            callback?.Invoke();
        }

        public void ShowWarning(string message, float duration = 2.0f)
        {
            if (_warningMessageContainer == null || _warningMessageText == null) return;

            _warningMessageText.text = message;
            _warningMessageContainer.style.display = DisplayStyle.Flex;

            // 일정 시간 후 숨기기
            _warningMessageContainer.schedule.Execute(() =>
            {
                _warningMessageContainer.style.display = DisplayStyle.None;
            }).ExecuteLater((long)(duration * 1000));
        }

        public void ShowGameOver()
        {
            if (_gameOverContainer != null)
            {
                _gameOverContainer.style.display = DisplayStyle.Flex;
                _gameOverContainer.pickingMode = PickingMode.Position;
            }

            if (_prototype_PlayerController.Instance != null)
            {
                _prototype_PlayerController.Instance.enabled = false;
            }
        }

        // 모드 변경 이벤트 핸들러
        private void OnPlayModeChanged(PlayModeChangedEvent evt)
        {
            UpdateModeIndicator(evt.NewMode);
            UpdatePlayerCardDeck(); // 카드 딤 처리 갱신
        }

        /// <summary>
        /// PlayModeManager 등 외부에서 직접 모드 UI를 강제 갱신할 때 호출합니다.
        /// </summary>
        public void UpdateModeIndicatorDirect(_prototype_PlayMode mode)
        {
            UpdateModeIndicator(mode);
            UpdatePlayerCardDeck();
        }

        public void UpdateModeIndicator(_prototype_PlayMode mode)
        {
            if (_modeIndicatorBanner != null)
            {
                _modeIndicatorBanner.style.display = DisplayStyle.Flex;
            }

            // 모드별 전용 UI 레이어 분리 제어 (전투 모드: REMAIN/DISCARD 덱 박스 표시, 탐색 모드: 덱 박스 은닉)
            if (_battleModeUI != null)
            {
                _battleModeUI.style.display = (mode == _prototype_PlayMode.Battle) ? DisplayStyle.Flex : DisplayStyle.None;
            }
            if (_explorationModeUI != null)
            {
                _explorationModeUI.style.display = (mode == _prototype_PlayMode.Exploration) ? DisplayStyle.Flex : DisplayStyle.None;
            }

            if (_modeIndicatorText == null) return;

            if (mode == _prototype_PlayMode.Battle)
            {
                _modeIndicatorText.text = "⚔ BATTLE";
                _modeIndicatorText.style.color = new UnityEngine.Color(1f, 0.31f, 0.31f);
                _modeIndicatorText.style.borderTopColor = new UnityEngine.Color(1f, 0.31f, 0.31f);
                _modeIndicatorText.style.borderBottomColor = new UnityEngine.Color(1f, 0.31f, 0.31f);
                _modeIndicatorText.style.borderLeftColor = new UnityEngine.Color(1f, 0.31f, 0.31f);
                _modeIndicatorText.style.borderRightColor = new UnityEngine.Color(1f, 0.31f, 0.31f);
                _modeIndicatorText.style.backgroundColor = new UnityEngine.Color(0.12f, 0f, 0f, 0.80f);
            }
            else
            {
                _modeIndicatorText.text = "✦ EXPLORE";
                _modeIndicatorText.style.color = new UnityEngine.Color(0.4f, 0.9f, 0.4f);
                _modeIndicatorText.style.borderTopColor = new UnityEngine.Color(0.4f, 0.9f, 0.4f);
                _modeIndicatorText.style.borderBottomColor = new UnityEngine.Color(0.4f, 0.9f, 0.4f);
                _modeIndicatorText.style.borderLeftColor = new UnityEngine.Color(0.4f, 0.9f, 0.4f);
                _modeIndicatorText.style.borderRightColor = new UnityEngine.Color(0.4f, 0.9f, 0.4f);
                _modeIndicatorText.style.backgroundColor = new UnityEngine.Color(0f, 0.12f, 0f, 0.80f);
            }
        }

        public void ShowTargetingUI(_prototype_CardData cardData)
        {
            _currentTargetingCard = cardData;
            _currentHoveredTarget = null;
            SetHandTargetingLock(true);
            if (_targetingHint != null) _targetingHint.style.display = DisplayStyle.Flex;
            if (_targetingTooltip != null)
            {
                _targetingTooltip.style.display = DisplayStyle.None;
            }

            // 가로형 와이드 Active 카드 HUD 바 표시 (방안 A)
            if (_activeCardWideBanner != null)
            {
                var playerLife = _prototype_PlayerController.Instance?.ControlledEntityView?.EntityData as _prototype_LifeData;
                
                if (_wideCardName != null) _wideCardName.text = cardData.id;
                if (_wideCardCost != null)
                {
                    if (cardData is _prototype_BattleCardData battleCard)
                        _wideCardCost.text = $"{battleCard.costValue?.value ?? 0}";
                    else
                        _wideCardCost.text = "-";
                }
                if (_wideCardType != null)
                {
                    if (cardData is _prototype_BattleCardData battleCard)
                        _wideCardType.text = $"[{battleCard.cardType}]";
                    else
                        _wideCardType.text = "[Interaction]";
                }

                bool isDestroyCard = false;
                if (cardData is _prototype_BattleCardData bCard)
                {
                    isDestroyCard = bCard.isDestroyOnUse || bCard.isDestroyOnDiscard;
                }
                if (_wideCardDestroyBadge != null)
                {
                    _wideCardDestroyBadge.style.display = isDestroyCard ? DisplayStyle.Flex : DisplayStyle.None;
                }

                if (_wideCardDesc != null)
                {
                    _wideCardDesc.text = _prototype_CardDescriptionFormatter.FormatDescription(cardData, playerLife, _isDetailedDescriptionMode, _currentHoveredTarget);
                }

                // 배너 초기화 및 부드러운 슬라이드 업 등장
                _activeCardWideBanner.style.display = DisplayStyle.Flex;
                _activeCardWideBanner.style.backgroundColor = new StyleColor(new Color(0.05f, 0.07f, 0.10f, 0.95f));
                _activeCardWideBanner.style.borderTopColor = new StyleColor(new Color(0.2f, 1f, 0.2f, 1f));
                _activeCardWideBanner.style.borderBottomColor = new StyleColor(new Color(0.2f, 1f, 0.2f, 1f));
                _activeCardWideBanner.style.borderLeftColor = new StyleColor(new Color(0.2f, 1f, 0.2f, 1f));
                _activeCardWideBanner.style.borderRightColor = new StyleColor(new Color(0.2f, 1f, 0.2f, 1f));
                _activeCardWideBanner.style.scale = new StyleScale(Vector2.one);
                _activeCardWideBanner.style.rotate = new Rotate(Angle.Degrees(0f));
                _activeCardWideBanner.style.translate = new Translate(0, 30, 0);
                _activeCardWideBanner.style.opacity = 0f;

                _activeCardWideBanner.schedule.Execute(() =>
                {
                    _activeCardWideBanner.style.translate = new Translate(0, 0, 0);
                    _activeCardWideBanner.style.opacity = 1f;
                }).ExecuteLater(16);
            }
        }

        public void HideTargetingUI()
        {
            _currentTargetingCard = null;
            _currentHoveredTarget = null;
            SetHandTargetingLock(false);
            if (_activeCardWideBanner != null)
            {
                _activeCardWideBanner.style.display = DisplayStyle.None;
            }
            if (_targetingHint != null) _targetingHint.style.display = DisplayStyle.None;
            if (_targetingTooltip != null) _targetingTooltip.style.display = DisplayStyle.None;
        }

        /// <summary>
        /// 타겟팅 중 핸드 바를 화면 아래로 완전히 접고, 카드들의 마우스 호버 팝업 간섭을 원천 차단합니다.
        /// </summary>
        private void SetHandTargetingLock(bool isTargeting)
        {
            if (_handCardContainer != null)
            {
                _handCardContainer.pickingMode = isTargeting ? PickingMode.Ignore : PickingMode.Position;
                if (isTargeting)
                {
                    _handCardContainer.RemoveFromClassList("hand-container-active");
                    _handCardContainer.AddToClassList("hand-container-targeting");
                }
                else
                {
                    _handCardContainer.RemoveFromClassList("hand-container-targeting");
                }
            }

            if (handCardViews != null)
            {
                foreach (var c in handCardViews)
                {
                    if (c != null)
                    {
                        c.pickingMode = isTargeting ? PickingMode.Ignore : PickingMode.Position;
                        if (isTargeting)
                        {
                            c.style.translate = new Translate(0, 0, 0); // 돌출된 호버 애니메이션 리셋
                        }
                    }
                }
            }

            IsCardHovered = false;
        }

        public void UpdateTargetingHover(_prototype_EntityData hoveredEntity)
        {
            if (_currentHoveredTarget == hoveredEntity) return;
            _currentHoveredTarget = hoveredEntity;
            if (_currentTargetingCard != null)
            {
                var playerLife = _prototype_PlayerController.Instance?.ControlledEntityView?.EntityData as _prototype_LifeData;
                string formattedDesc = _prototype_CardDescriptionFormatter.FormatDescription(_currentTargetingCard, playerLife, _isDetailedDescriptionMode, _currentHoveredTarget);
                if (_wideCardDesc != null) _wideCardDesc.text = formattedDesc;
            }
        }

        public void UpdateTargetingTooltipPosition(Vector2 screenPosition)
        {
            // 마우스 커서 툴팁은 와이드 HUD 바로 대체되었으므로 항상 숨김 처리
            if (_targetingTooltip != null && _targetingTooltip.style.display != DisplayStyle.None)
            {
                _targetingTooltip.style.display = DisplayStyle.None;
            }
        }

        public Vector2 GetPanelResolution()
        {
            if (_cardDragArea != null && _cardDragArea.panel != null)
            {
                var root = _cardDragArea.panel.visualTree;
                if (root != null && !float.IsNaN(root.resolvedStyle.width) && root.resolvedStyle.width > 0)
                {
                    return new Vector2(root.resolvedStyle.width, root.resolvedStyle.height);
                }
            }
            return new Vector2(1920f, 1080f);
        }

        /// <summary>
        /// 카드가 사용되어 사라질 때 가로형 와이드 HUD 바에서 시각적 애니메이션을 재생합니다.
        /// - 기존의 카드를 공중에 띄우던 연출은 삭제되고, 가로형 와이드 HUD 바에서 일원화되어 재생됩니다.
        /// - isDestroyed == false: 일반 소멸 (청록빛 테두리, 부드럽게 아래로 슬라이드 다운 및 페이드아웃)
        /// - isDestroyed == true: 파괴 소멸 (강렬한 자주/보랏빛 발광, "✦ 파괴 ✦" 뱃지 펄스 확대, 좌우 진동 후 중심으로 수축/소멸)
        /// </summary>
        public void PlayCardCastDisappearAnimation(_prototype_CardData cardData, bool isDestroyed)
        {
            if (_activeCardWideBanner == null || _activeCardWideBanner.style.display == DisplayStyle.None)
            {
                SetHandTargetingLock(false);
                return;
            }

            if (isDestroyed)
            {
                // [파괴 소멸 연출]: 짙은 보라색 배경, 핫핑크 테두리, 파괴 뱃지 펄스, 좌우 진동 후 중심으로 급격히 수축 소멸
                _activeCardWideBanner.style.backgroundColor = new StyleColor(new Color(0.22f, 0.04f, 0.28f, 0.98f));
                _activeCardWideBanner.style.borderTopColor = new StyleColor(new Color(0.96f, 0.25f, 0.86f, 1f));
                _activeCardWideBanner.style.borderBottomColor = new StyleColor(new Color(0.96f, 0.25f, 0.86f, 1f));
                _activeCardWideBanner.style.borderLeftColor = new StyleColor(new Color(0.96f, 0.25f, 0.86f, 1f));
                _activeCardWideBanner.style.borderRightColor = new StyleColor(new Color(0.96f, 0.25f, 0.86f, 1f));

                if (_wideCardDestroyBadge != null)
                {
                    _wideCardDestroyBadge.style.display = DisplayStyle.Flex;
                    _wideCardDestroyBadge.style.scale = new StyleScale(new Vector2(1.3f, 1.3f));
                }

                _activeCardWideBanner.schedule.Execute(() =>
                {
                    _activeCardWideBanner.style.rotate = new Rotate(Angle.Degrees(-3f));
                    _activeCardWideBanner.style.scale = new StyleScale(new Vector2(1.04f, 1.04f));
                }).ExecuteLater(16);

                _activeCardWideBanner.schedule.Execute(() =>
                {
                    _activeCardWideBanner.style.rotate = new Rotate(Angle.Degrees(3f));
                }).ExecuteLater(100);

                _activeCardWideBanner.schedule.Execute(() =>
                {
                    _activeCardWideBanner.style.transitionDuration = new List<TimeValue> {
                        new TimeValue(0.28f, TimeUnit.Second),
                        new TimeValue(0.28f, TimeUnit.Second),
                        new TimeValue(0.28f, TimeUnit.Second),
                        new TimeValue(0.28f, TimeUnit.Second),
                        new TimeValue(0.28f, TimeUnit.Second)
                    };
                    _activeCardWideBanner.style.rotate = new Rotate(Angle.Degrees(0f));
                    _activeCardWideBanner.style.scale = new StyleScale(new Vector2(0.05f, 0.05f));
                    _activeCardWideBanner.style.opacity = 0f;
                }).ExecuteLater(180);

                _activeCardWideBanner.schedule.Execute(() =>
                {
                    _activeCardWideBanner.style.display = DisplayStyle.None;
                    SetHandTargetingLock(false);
                }).ExecuteLater(480);
            }
            else
            {
                // [일반 소멸 연출]: 청록빛 테두리, 부드럽게 아래로 슬라이드 다운되며 페이드아웃
                _activeCardWideBanner.style.backgroundColor = new StyleColor(new Color(0.05f, 0.08f, 0.12f, 0.95f));
                _activeCardWideBanner.style.borderTopColor = new StyleColor(new Color(0.25f, 0.75f, 1f, 1f));
                _activeCardWideBanner.style.borderBottomColor = new StyleColor(new Color(0.25f, 0.75f, 1f, 1f));
                _activeCardWideBanner.style.borderLeftColor = new StyleColor(new Color(0.25f, 0.75f, 1f, 1f));
                _activeCardWideBanner.style.borderRightColor = new StyleColor(new Color(0.25f, 0.75f, 1f, 1f));

                _activeCardWideBanner.schedule.Execute(() =>
                {
                    _activeCardWideBanner.style.transitionDuration = new List<TimeValue> {
                        new TimeValue(0.25f, TimeUnit.Second),
                        new TimeValue(0.25f, TimeUnit.Second),
                        new TimeValue(0.25f, TimeUnit.Second),
                        new TimeValue(0.25f, TimeUnit.Second),
                        new TimeValue(0.25f, TimeUnit.Second)
                    };
                    _activeCardWideBanner.style.translate = new Translate(0, 35, 0);
                    _activeCardWideBanner.style.opacity = 0f;
                }).ExecuteLater(16);

                _activeCardWideBanner.schedule.Execute(() =>
                {
                    _activeCardWideBanner.style.display = DisplayStyle.None;
                    SetHandTargetingLock(false);
                }).ExecuteLater(280);
            }
        }

        private void BuildHazardInfoTooltip(VisualElement root)
        {
            if (root == null) return;

            // Remove existing one if any
            var existing = root.Q<VisualElement>("HazardInfoTooltip");
            if (existing != null) existing.RemoveFromHierarchy();

            _hazardInfoTooltip = new VisualElement();
            _hazardInfoTooltip.name = "HazardInfoTooltip";
            _hazardInfoTooltip.pickingMode = PickingMode.Ignore;
            _hazardInfoTooltip.style.position = Position.Absolute;
            _hazardInfoTooltip.style.backgroundColor = new Color(0.04f, 0.08f, 0.05f, 0.95f);
            _hazardInfoTooltip.style.borderTopColor = new Color(1.0f, 0.45f, 0.05f, 0.95f);
            _hazardInfoTooltip.style.borderBottomColor = new Color(1.0f, 0.45f, 0.05f, 0.95f);
            _hazardInfoTooltip.style.borderLeftColor = new Color(1.0f, 0.45f, 0.05f, 0.95f);
            _hazardInfoTooltip.style.borderRightColor = new Color(1.0f, 0.45f, 0.05f, 0.95f);
            _hazardInfoTooltip.style.borderTopWidth = 2;
            _hazardInfoTooltip.style.borderBottomWidth = 2;
            _hazardInfoTooltip.style.borderLeftWidth = 2;
            _hazardInfoTooltip.style.borderRightWidth = 2;
            _hazardInfoTooltip.style.borderTopLeftRadius = 8;
            _hazardInfoTooltip.style.borderTopRightRadius = 8;
            _hazardInfoTooltip.style.borderBottomLeftRadius = 8;
            _hazardInfoTooltip.style.borderBottomRightRadius = 8;
            _hazardInfoTooltip.style.paddingTop = 12;
            _hazardInfoTooltip.style.paddingBottom = 12;
            _hazardInfoTooltip.style.paddingLeft = 14;
            _hazardInfoTooltip.style.paddingRight = 14;
            _hazardInfoTooltip.style.width = 360;
            _hazardInfoTooltip.style.display = DisplayStyle.None;

            // Header Container (Title + Pin Badge)
            var header = new VisualElement();
            header.style.flexDirection = FlexDirection.Row;
            header.style.justifyContent = Justify.SpaceBetween;
            header.style.alignItems = Align.Center;
            header.style.marginBottom = 8;
            header.style.borderBottomWidth = 1;
            header.style.borderBottomColor = new Color(1f, 0.45f, 0.05f, 0.35f);
            header.style.paddingBottom = 6;

            _hazardTooltipHeaderTitle = new Label("⚠ 공격 예고");
            _hazardTooltipHeaderTitle.style.fontSize = 19;
            _hazardTooltipHeaderTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
            _hazardTooltipHeaderTitle.style.color = new Color(1f, 0.75f, 0.2f);
            header.Add(_hazardTooltipHeaderTitle);

            _hazardTooltipPinBadge = new Label("[PINNED]");
            _hazardTooltipPinBadge.style.fontSize = 13;
            _hazardTooltipPinBadge.style.unityFontStyleAndWeight = FontStyle.Bold;
            _hazardTooltipPinBadge.style.color = new Color(0.2f, 1f, 0.4f);
            _hazardTooltipPinBadge.style.backgroundColor = new Color(0f, 0.3f, 0.1f, 0.7f);
            _hazardTooltipPinBadge.style.paddingLeft = 6;
            _hazardTooltipPinBadge.style.paddingRight = 6;
            _hazardTooltipPinBadge.style.paddingTop = 2;
            _hazardTooltipPinBadge.style.paddingBottom = 2;
            _hazardTooltipPinBadge.style.borderTopLeftRadius = 4;
            _hazardTooltipPinBadge.style.borderTopRightRadius = 4;
            _hazardTooltipPinBadge.style.borderBottomLeftRadius = 4;
            _hazardTooltipPinBadge.style.borderBottomRightRadius = 4;
            _hazardTooltipPinBadge.style.display = DisplayStyle.None;
            header.Add(_hazardTooltipPinBadge);

            _hazardInfoTooltip.Add(header);

            // Items Container
            _hazardTooltipItemsContainer = new VisualElement();
            _hazardTooltipItemsContainer.style.flexDirection = FlexDirection.Column;
            _hazardInfoTooltip.Add(_hazardTooltipItemsContainer);

            var container = root.Q<VisualElement>("Wrapper") ?? root;
            container.Add(_hazardInfoTooltip);
        }

        public void ShowHazardInfoTooltip(List<_prototype_HazardAttackInfo> infos, Vector2 screenPosition, _prototype_Point point, bool isPinned)
        {
            if (_hazardInfoTooltip == null || infos == null || infos.Count == 0)
            {
                HideHazardInfoTooltip(true);
                return;
            }

            IsHazardTooltipPinned = isPinned;
            _pinnedHazardPoint = isPinned ? point : null;

            if (_hazardTooltipHeaderTitle != null)
            {
                _hazardTooltipHeaderTitle.text = infos.Count > 1
                    ? $"⚠ 중첩 공격 ({infos.Count}개) - ({point.x}, {point.y})"
                    : $"⚠ 공격 예고 - ({point.x}, {point.y})";
            }

            if (_hazardTooltipPinBadge != null)
            {
                _hazardTooltipPinBadge.style.display = isPinned ? DisplayStyle.Flex : DisplayStyle.None;
            }

            if (_hazardTooltipItemsContainer != null)
            {
                _hazardTooltipItemsContainer.Clear();

                for (int i = 0; i < infos.Count; i++)
                {
                    var info = infos[i];
                    if (i > 0)
                    {
                        var divider = new VisualElement();
                        divider.style.height = 1;
                        divider.style.backgroundColor = new Color(1f, 1f, 1f, 0.15f);
                        divider.style.marginTop = 5;
                        divider.style.marginBottom = 5;
                        _hazardTooltipItemsContainer.Add(divider);
                    }

                    var cardBox = new VisualElement();
                    cardBox.style.flexDirection = FlexDirection.Column;

                    // Row 1: Attacker Name & Card ID
                    var row1 = new VisualElement();
                    row1.style.flexDirection = FlexDirection.Row;
                    row1.style.justifyContent = Justify.SpaceBetween;
                    row1.style.alignItems = Align.Center;

                    var nameLabel = new Label($"🗡 {info.AttackerName}");
                    nameLabel.style.fontSize = 20;
                    nameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                    nameLabel.style.color = new Color(1f, 0.45f, 0.45f);
                    row1.Add(nameLabel);

                    string attackTitle = !string.IsNullOrEmpty(info.AttackTitle)
                        ? info.AttackTitle
                        : (info.Card != null ? info.Card.id : "공격");
                    var cardLabel = new Label(attackTitle);
                    cardLabel.style.fontSize = 17;
                    cardLabel.style.color = new Color(0.9f, 0.9f, 0.9f);
                    row1.Add(cardLabel);
                    cardBox.Add(row1);

                    // Row 2: Estimated Damage
                    var row2 = new VisualElement();
                    row2.style.flexDirection = FlexDirection.Row;
                    row2.style.justifyContent = Justify.SpaceBetween;
                    row2.style.alignItems = Align.Center;
                    row2.style.marginTop = 4;

                    var dmgTypeStr = info.DamageType == _prototype_DamageType.Physical ? "물리" : "마법";
                    var dmgLabel = new Label($"예상 피해: {info.EstimatedDamage} ({dmgTypeStr})");
                    dmgLabel.style.fontSize = 18;
                    dmgLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                    dmgLabel.style.color = new Color(1f, 0.35f, 0.35f);
                    row2.Add(dmgLabel);
                    cardBox.Add(row2);

                    // Row 3: Additional Effects
                    if (info.AdditionalEffects != null && info.AdditionalEffects.Count > 0)
                    {
                        var effectBox = new VisualElement();
                        effectBox.style.flexDirection = FlexDirection.Column;
                        effectBox.style.marginTop = 4;

                        foreach (var effect in info.AdditionalEffects)
                        {
                            var effectLabel = new Label($"• {effect}");
                            effectLabel.style.fontSize = 16;
                            effectLabel.style.color = new Color(0.4f, 0.88f, 1f);
                            effectBox.Add(effectLabel);
                        }
                        cardBox.Add(effectBox);
                    }

                    _hazardTooltipItemsContainer.Add(cardBox);
                }
            }

            _hazardInfoTooltip.style.display = DisplayStyle.Flex;
            UpdateHazardTooltipPosition(screenPosition);
        }

        public void HideHazardInfoTooltip(bool force = false)
        {
            if (!force && IsHazardTooltipPinned) return;

            IsHazardTooltipPinned = false;
            _pinnedHazardPoint = null;
            if (_hazardInfoTooltip != null)
            {
                _hazardInfoTooltip.style.display = DisplayStyle.None;
            }
        }

        public void UpdateHazardTooltipPosition(Vector2 screenPosition)
        {
            if (_hazardInfoTooltip == null || _hazardInfoTooltip.style.display != DisplayStyle.Flex) return;

            Vector2 offset = new Vector2(24, 24);
            Vector2 targetScreen = screenPosition + offset;

            if (_hazardInfoTooltip.panel != null)
            {
                Vector2 screenTopLeft = new Vector2(targetScreen.x, Screen.height - targetScreen.y);
                Vector2 panelPos = UnityEngine.UIElements.RuntimePanelUtils.ScreenToPanel(_hazardInfoTooltip.panel, screenTopLeft);

                float maxLeft = _hazardInfoTooltip.panel.visualTree.layout.width - 340;
                float maxTop = _hazardInfoTooltip.panel.visualTree.layout.height - 240;
                if (maxLeft > 0 && panelPos.x > maxLeft) panelPos.x = panelPos.x - 360;
                if (maxTop > 0 && panelPos.y > maxTop) panelPos.y = maxTop;

                _hazardInfoTooltip.style.left = Mathf.Max(10, panelPos.x);
                _hazardInfoTooltip.style.top = Mathf.Max(10, panelPos.y);
            }
            else
            {
                float y = Screen.height - targetScreen.y;
                _hazardInfoTooltip.style.left = targetScreen.x;
                _hazardInfoTooltip.style.top = y;
            }
        }

        public void SetHandViewModeActive(bool isActive)
        {
            if (_handCardContainer == null) return;

            // active 상태인 카드가 있을 때(타겟팅 모드 중)만 핸드 컨테이너를 화면 아래로 완전히 내림
            if (_currentTargetingCard != null)
            {
                _handCardContainer.RemoveFromClassList("hand-container-active");
                if (!_handCardContainer.ClassListContains("hand-container-targeting"))
                {
                    _handCardContainer.AddToClassList("hand-container-targeting");
                }
                return;
            }

            // 일반적인 상태: 타겟팅 클래스 해제 후 이전 상태 그대로 마우스 호버에 따라 노출/수납 동작
            if (_handCardContainer.ClassListContains("hand-container-targeting"))
            {
                _handCardContainer.RemoveFromClassList("hand-container-targeting");
            }

            if (isActive)
            {
                _handCardContainer.AddToClassList("hand-container-active");
            }
            else
            {
                _handCardContainer.RemoveFromClassList("hand-container-active");
            }
        }

        public void UpdatePlayerInfo()
        {
            _prototype_EntityView controlledEntityView = _prototype_PlayerController.Instance.ControlledEntityView;
            if (controlledEntityView != null)
            {
                _prototype_EntityData entityData = controlledEntityView.EntityData;
                if (entityData != null)
                {
                    int displayHp = Mathf.Max(0, entityData.health.Current);
                    int displaySp = Mathf.Max(0, entityData.stamina.Current);
                    int currentShield = (entityData is _prototype_LifeData pld) ? pld.CurrentShield : 0;
                    string shieldSuffix = currentShield > 0 ? $" (+{currentShield})" : "";

                    if (_label_point != null) _label_point.text = $"> POS : {controlledEntityView.Point}";
                    if (_label_health != null) _label_health.text = $"> HP  : {displayHp}/{entityData.health.Max}{shieldSuffix}";
                    if (_label_stamina != null) _label_stamina.text = $"> SP  : {displaySp}/{entityData.stamina.Max}";

                    if (_label_actionPoints != null)
                    {
                        if (entityData is _prototype_LifeData playerLifeData && playerLifeData.Speed >= 2)
                        {
                            _label_actionPoints.style.display = DisplayStyle.Flex;
                            _label_actionPoints.text = $"> AP  : {playerLifeData.remainingActions}/{playerLifeData.Speed}";
                        }
                        else
                        {
                            _label_actionPoints.style.display = DisplayStyle.None;
                        }
                    }

                    UpdatePlayerStatusTray(entityData as _prototype_LifeData);
                }
                else
                {
                    if (_label_point != null) _label_point.text = string.Empty;
                    if (_label_health != null) _label_health.text = string.Empty;
                    if (_label_stamina != null) _label_stamina.text = string.Empty;
                    if (_label_actionPoints != null) _label_actionPoints.style.display = DisplayStyle.None;
                    _playerStatusTray?.Clear();
                }

            }
        }

        private void BuildStatusTooltip(VisualElement root)
        {
            if (root == null) return;
            var existing = root.Q<VisualElement>("StatusTooltip");
            if (existing != null) existing.RemoveFromHierarchy();

            _statusTooltip = new VisualElement();
            _statusTooltip.name = "StatusTooltip";
            _statusTooltip.pickingMode = PickingMode.Ignore;
            _statusTooltip.style.position = Position.Absolute;
            _statusTooltip.style.backgroundColor = new Color(0.02f, 0.05f, 0.02f, 0.95f);
            _statusTooltip.style.borderTopColor = new Color(0.2f, 0.9f, 0.2f, 0.9f);
            _statusTooltip.style.borderBottomColor = new Color(0.2f, 0.9f, 0.2f, 0.9f);
            _statusTooltip.style.borderLeftColor = new Color(0.2f, 0.9f, 0.2f, 0.9f);
            _statusTooltip.style.borderRightColor = new Color(0.2f, 0.9f, 0.2f, 0.9f);
            _statusTooltip.style.borderTopWidth = 1;
            _statusTooltip.style.borderBottomWidth = 1;
            _statusTooltip.style.borderLeftWidth = 1;
            _statusTooltip.style.borderRightWidth = 1;
            _statusTooltip.style.borderTopLeftRadius = 6;
            _statusTooltip.style.borderTopRightRadius = 6;
            _statusTooltip.style.borderBottomLeftRadius = 6;
            _statusTooltip.style.borderBottomRightRadius = 6;
            _statusTooltip.style.paddingTop = 8;
            _statusTooltip.style.paddingBottom = 8;
            _statusTooltip.style.paddingLeft = 12;
            _statusTooltip.style.paddingRight = 12;
            _statusTooltip.style.minWidth = 180;
            _statusTooltip.style.maxWidth = 280;
            _statusTooltip.style.display = DisplayStyle.None;

            _statusTooltipTitle = new Label("상태효과");
            _statusTooltipTitle.style.fontSize = 17;
            _statusTooltipTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
            _statusTooltipTitle.style.color = new Color(0.4f, 1f, 0.4f);
            _statusTooltipTitle.style.marginBottom = 4;
            _statusTooltip.Add(_statusTooltipTitle);

            _statusTooltipDesc = new Label("효과 설명");
            _statusTooltipDesc.style.fontSize = 14;
            _statusTooltipDesc.style.color = new Color(0.85f, 0.95f, 0.85f);
            _statusTooltipDesc.style.whiteSpace = WhiteSpace.Normal;
            _statusTooltip.Add(_statusTooltipDesc);

            var container = root.Q<VisualElement>("Wrapper") ?? root;
            container.Add(_statusTooltip);
        }

        private void UpdatePlayerStatusTray(_prototype_LifeData playerLife)
        {
            if (_playerStatusTray == null) return;
            _playerStatusTray.Clear();
            if (playerLife == null) return;
            var displayData = playerLife.GetDisplayStatuses(_prototype_StatusVisualDatabase.Instance).ToList();
            if (displayData.Count == 0) return;

            foreach (var data in displayData)
            {
                var badge = new VisualElement();
                badge.AddToClassList("status-badge");
                badge.style.borderTopColor = new StyleColor(data.themeColor);
                badge.style.borderBottomColor = new StyleColor(data.themeColor);
                badge.style.borderLeftColor = new StyleColor(data.themeColor);
                badge.style.borderRightColor = new StyleColor(data.themeColor);

                if (data.icon != null)
                {
                    var iconEl = new VisualElement();
                    iconEl.AddToClassList("status-badge-icon");
                    iconEl.style.backgroundImage = new StyleBackground(data.icon);
                    iconEl.style.unityBackgroundImageTintColor = new StyleColor(Color.white);
                    badge.Add(iconEl);
                }
                else
                {
                    var symbol = new Label(data.symbolChar);
                    symbol.AddToClassList("status-badge-symbol");
                    symbol.style.color = new StyleColor(data.themeColor);
                    badge.Add(symbol);
                }

                string countText = data.stackCount > 1 ? $"x{data.stackCount} " : "";
                string tickText = (data.durationTicks > 0 && !data.isForever) ? $"{data.durationTicks}t" : "";
                string fullBadgeText = $"{countText}{tickText}".Trim();
                if (!string.IsNullOrEmpty(fullBadgeText))
                {
                    var textLabel = new Label(fullBadgeText);
                    textLabel.AddToClassList("status-badge-text");
                    badge.Add(textLabel);
                }

                string tooltipTitle = data.stackCount > 1 ? $"{data.displayName} x{data.stackCount}" : data.displayName;
                string tooltipBody = string.IsNullOrEmpty(data.description) ? $"{data.id}" : data.description;
                if (data.durationTicks > 0 && !data.isForever) tooltipBody += $"\n지속시간: {data.durationTicks}틱 남음";
                else if (data.isForever) tooltipBody += "\n지속시간: 영구";

                badge.RegisterCallback<PointerEnterEvent>(evt =>
                {
                    ShowStatusTooltip(tooltipTitle, tooltipBody, data.themeColor, evt.position);
                });
                badge.RegisterCallback<PointerMoveEvent>(evt =>
                {
                    UpdateStatusTooltipPosition(evt.position);
                });
                badge.RegisterCallback<PointerLeaveEvent>(evt =>
                {
                    HideStatusTooltip();
                });

                _playerStatusTray.Add(badge);
            }
        }

        private void ShowStatusTooltip(string title, string desc, Color themeColor, Vector2 screenPos)
        {
            if (_statusTooltip == null) return;
            if (_statusTooltipTitle != null)
            {
                _statusTooltipTitle.text = title;
                _statusTooltipTitle.style.color = new StyleColor(themeColor);
            }
            if (_statusTooltipDesc != null)
            {
                _statusTooltipDesc.text = desc;
            }
            _statusTooltip.style.display = DisplayStyle.Flex;
            UpdateStatusTooltipPosition(screenPos);
        }

        private void UpdateStatusTooltipPosition(Vector2 screenPos)
        {
            if (_statusTooltip == null || _statusTooltip.style.display != DisplayStyle.Flex) return;
            Vector2 offset = new Vector2(16, 16);
            Vector2 target = screenPos + offset;
            if (_statusTooltip.panel != null)
            {
                Vector2 panelPos = UnityEngine.UIElements.RuntimePanelUtils.ScreenToPanel(_statusTooltip.panel, target);
                _statusTooltip.style.left = Mathf.Max(10, panelPos.x);
                _statusTooltip.style.top = Mathf.Max(10, panelPos.y);
            }
        }

        private void HideStatusTooltip()
        {
            if (_statusTooltip != null)
            {
                _statusTooltip.style.display = DisplayStyle.None;
            }
        }

        public void UpdatePlayerCardDeck()
        {
            if (_handCardContainer == null) return;

            _handCardContainer.Clear();
            _cardDragArea.Clear();
            handCardViews.Clear();
            IsCardHovered = false;

            _prototype_EntityView controlledEntityView = _prototype_PlayerController.Instance.ControlledEntityView;
            if (controlledEntityView != null && controlledEntityView is _prototype_LifeView lifeView)
            {
                _prototype_LifeData lifeData = lifeView.Data;
                if (lifeData != null)
                {
                    if (_label_remainsCount != null) _label_remainsCount.text = lifeData.cardDeck.remainedCardDatas.Count.ToString();
                    if (_label_discardedCount != null) _label_discardedCount.text = lifeData.cardDeck.discardedCardDatas.Count.ToString();
                    if (_label_destroyedCount != null) _label_destroyedCount.text = lifeData.cardDeck.destroyedCardDatas.Count.ToString();

                    bool isSilenced = lifeData.HasStatusEffect(_prototype_StatusType.Silence);

                    var availableCards = lifeView.GetAvailableCards();
                    foreach (_prototype_CardData cardData in availableCards)
                    {
                        VisualElement cardViewInstance = _cardViewTemplate.Instantiate();

                        var lblName = cardViewInstance.Q<Label>("CardName");
                        if (lblName != null) lblName.text = cardData.id;

                        var lblCost = cardViewInstance.Q<Label>("CardCost");
                        var lblType = cardViewInstance.Q<Label>("CardType");
                        var cooldownOverlay = cardViewInstance.Q<VisualElement>("CooldownOverlay");
                        var cooldownText = cardViewInstance.Q<Label>("CooldownText");

                        if (cardData is _prototype_BattleCardData battleCard)
                        {
                            if (lblCost != null) lblCost.text = battleCard.costValue != null ? battleCard.costValue.value.ToString() : "0";
                            if (lblType != null) lblType.text = battleCard.cardType.ToString();

                            // 쿨타임 시각적 피드백
                            if (battleCard.currentCoolTicks > 0)
                            {
                                if (cooldownOverlay != null) cooldownOverlay.style.display = DisplayStyle.Flex;
                                if (cooldownText != null) cooldownText.text = battleCard.currentCoolTicks.ToString();
                            }
                            else
                            {
                                if (cooldownOverlay != null) cooldownOverlay.style.display = DisplayStyle.None;
                            }
                        }
                        else if (cardData is _prototype_InteractionCardData interactionCard)
                        {
                            if (lblCost != null) lblCost.text = "-";
                            if (lblType != null) lblType.text = "Interact";
                            if (cooldownOverlay != null) cooldownOverlay.style.display = DisplayStyle.None;
                        }

                        bool isEmpowered = cardData.IsEmpowered(lifeData);
                        if (!isEmpowered && lifeData.uniquePassive != null)
                        {
                            isEmpowered = lifeData.uniquePassive.IsCardEmpowered(lifeData, cardData);
                        }
                        if (!isEmpowered && lifeData.Passives != null)
                        {
                            isEmpowered = lifeData.Passives.Any(p => p.IsCardEmpowered(lifeData, cardData));
                        }

                        if (isEmpowered)
                        {
                            cardViewInstance.AddToClassList("card-empowered");
                        }
                        else
                        {
                            cardViewInstance.RemoveFromClassList("card-empowered");
                        }

                        cardViewInstance.userData = cardData;
                        var lblDesc = cardViewInstance.Q<Label>("CardDesc");
                        if (lblDesc != null)
                        {
                            lblDesc.text = _prototype_CardDescriptionFormatter.FormatDescription(cardData, lifeView.Data, _isDetailedDescriptionMode);
                        }

                        // 침묵 시각적 피드백 (보라색 틴트 + 침묵 텍스트 오버레이)
                        if (isSilenced)
                        {
                            cardViewInstance.style.opacity = 0.75f;
                            var silenceOverlay = new VisualElement();
                            silenceOverlay.name = "SilenceOverlay";
                            silenceOverlay.style.position = Position.Absolute;
                            silenceOverlay.style.left = 0;
                            silenceOverlay.style.top = 0;
                            silenceOverlay.style.right = 0;
                            silenceOverlay.style.bottom = 0;
                            silenceOverlay.style.backgroundColor = new StyleColor(new Color(0.45f, 0.1f, 0.65f, 0.5f));
                            silenceOverlay.style.alignItems = Align.Center;
                            silenceOverlay.style.justifyContent = Justify.Center;
                            silenceOverlay.style.borderTopLeftRadius = 12;
                            silenceOverlay.style.borderTopRightRadius = 12;
                            silenceOverlay.style.borderBottomLeftRadius = 12;
                            silenceOverlay.style.borderBottomRightRadius = 12;
                            silenceOverlay.pickingMode = PickingMode.Ignore;

                            var lblSilence = new Label("침묵");
                            lblSilence.style.color = new StyleColor(new Color(1f, 0.85f, 1f, 1f));
                            lblSilence.style.fontSize = 28;
                            lblSilence.style.unityFontStyleAndWeight = FontStyle.Bold;
                            lblSilence.pickingMode = PickingMode.Ignore;
                            silenceOverlay.Add(lblSilence);

                            cardViewInstance.Add(silenceOverlay);
                        }
                        else
                        {
                            cardViewInstance.style.opacity = 1f;
                        }

                        cardViewInstance.style.position = Position.Absolute;
                        cardViewInstance.AddToClassList("card-container-item");
                        cardViewInstance.style.transitionProperty = new List<StylePropertyName> { new StylePropertyName("translate") };
                        cardViewInstance.style.transitionDuration = new List<TimeValue> { new TimeValue(0.18f, TimeUnit.Second) };
                        cardViewInstance.style.transitionTimingFunction = new List<EasingFunction> { new EasingFunction(EasingMode.EaseOut) };

                        // 드래그 기능 등록
                        RegisterDragEvents(cardViewInstance, cardData);

                        _handCardContainer.Add(cardViewInstance);
                        handCardViews.Add(cardViewInstance);
                    }

                    LayoutCardViews();
                }
            }
        }

        private void RegisterDragEvents(VisualElement card, _prototype_CardData cardData)
        {
            bool isDragging = false;
            Vector2 dragOffset = Vector2.zero;
            int originalIndex = -1;

            card.RegisterCallback<PointerDownEvent>(evt =>
            {
                if (evt.button == 1) // Right click to Burn
                {
                    if (_prototype_PlayerController.Instance != null &&
                        _prototype_PlayerController.Instance.ControlledEntityView is _prototype_LifeView lifeViewStunCheck &&
                        lifeViewStunCheck.Data != null &&
                        (lifeViewStunCheck.Data.HasStatusEffect(_prototype_StatusType.Stun) || _prototype_PlayerController.Instance.IsStunAutoProgressing))
                    {
                        evt.StopPropagation();
                        return;
                    }

                    if (!evt.shiftKey)
                    {
                        ShowWarning("카드를 소각하려면 Shift 키를 누른 상태로 우클릭하세요!");
                        evt.StopPropagation();
                        return;
                    }

                    if (_prototype_PlayerController.Instance != null)
                    {
                        bool isDestroyOnDiscard = (cardData is _prototype_BattleCardData bCard && bCard.isDestroyOnDiscard);
                        card.pickingMode = PickingMode.Ignore;

                        if (isDestroyOnDiscard)
                        {
                            // 파괴 버리기 애니메이션: 보라빛 발광 + 위로 솟구치며 회전 축소/소멸
                            card.style.transitionProperty = new List<StylePropertyName> {
                                new StylePropertyName("translate"),
                                new StylePropertyName("scale"),
                                new StylePropertyName("opacity"),
                                new StylePropertyName("background-color"),
                                new StylePropertyName("rotate")
                            };
                            card.style.transitionDuration = new List<TimeValue> {
                                new TimeValue(0.35f, TimeUnit.Second),
                                new TimeValue(0.35f, TimeUnit.Second),
                                new TimeValue(0.35f, TimeUnit.Second),
                                new TimeValue(0.35f, TimeUnit.Second),
                                new TimeValue(0.35f, TimeUnit.Second)
                            };

                            card.style.translate = new Translate(0, -60, 0);
                            card.style.scale = new StyleScale(new Vector2(0.05f, 0.05f));
                            card.style.rotate = new Rotate(Angle.Degrees(15f));
                            card.style.opacity = 0f;
                            card.style.backgroundColor = new StyleColor(new Color(0.6f, 0.1f, 0.7f, 1f));

                            card.schedule.Execute(() =>
                            {
                                _prototype_PlayerController.Instance.DiscardCard(cardData);
                            }).ExecuteLater(350);
                        }
                        else
                        {
                            // 일반 소각/버리기 애니메이션
                            card.style.transitionProperty = new List<StylePropertyName> {
                                new StylePropertyName("scale"),
                                new StylePropertyName("opacity"),
                                new StylePropertyName("background-color")
                            };
                            card.style.transitionDuration = new List<TimeValue> {
                                new TimeValue(0.3f, TimeUnit.Second),
                                new TimeValue(0.3f, TimeUnit.Second),
                                new TimeValue(0.3f, TimeUnit.Second)
                            };

                            card.style.scale = new StyleScale(new Vector2(0.1f, 0.1f));
                            card.style.opacity = 0f;
                            card.style.backgroundColor = new StyleColor(new Color(1f, 0.2f, 0.2f, 1f));

                            card.schedule.Execute(() =>
                            {
                                _prototype_PlayerController.Instance.DiscardCard(cardData);
                            }).ExecuteLater(300);
                        }
                    }
                    evt.StopPropagation();
                    return;
                }

                if (evt.button != 0) return;

                if (_prototype_PlayerController.Instance != null &&
                    _prototype_PlayerController.Instance.ControlledEntityView is _prototype_LifeView lifeViewCheck &&
                    lifeViewCheck.Data != null)
                {
                    if (lifeViewCheck.Data.HasStatusEffect(_prototype_StatusType.Stun) || _prototype_PlayerController.Instance.IsStunAutoProgressing)
                    {
                        evt.StopPropagation();
                        return;
                    }

                    if (lifeViewCheck.Data.HasStatusEffect(_prototype_StatusType.Silence))
                    {
                        ShowWarning("침묵 상태에서는 카드를 사용할 수 없습니다!");
                        evt.StopPropagation();
                        return;
                    }
                }

                if (cardData is _prototype_BattleCardData battleCard)
                {
                    if (battleCard.currentCoolTicks > 0)
                    {
                        ShowWarning("현재 사용할 수 없습니다! (대기 중)");
                        evt.StopPropagation();
                        return;
                    }

                    bool hasEnoughCost = true;
                    if (_prototype_PlayerController.Instance != null && _prototype_PlayerController.Instance.ControlledEntityView is _prototype_LifeView lifeView)
                    {
                        var cost = battleCard.costValue;
                        if (cost != null)
                        {
                            if (cost.costType == _prototype_CostType.FixedStamina && lifeView.Data.stamina.Current < cost.value) hasEnoughCost = false;
                            else if (cost.costType == _prototype_CostType.FixedHealth && lifeView.Data.health.Current < cost.value) hasEnoughCost = false;
                        }
                    }

                    if (!hasEnoughCost)
                    {
                        ShowWarning("소모 자원이 부족합니다!");
                        return;
                    }
                }

                isDragging = true;
                IsCardHovered = false; // 드래그 시작 시 호버 상태 강제 해제
                dragOffset = evt.localPosition;

                originalIndex = _handCardContainer.IndexOf(card);

                // 드래그 중에는 애니메이션 끄기 및 translate 리셋
                card.style.transitionDuration = new List<TimeValue> { new TimeValue(0f, TimeUnit.Second) };
                card.style.translate = new Translate(0, 0, 0);

                // 드래그 시 부모(FlexContainer)를 떠나 드래그 영역으로 이동
                var worldPos = card.worldBound;
                card.style.position = Position.Absolute;
                card.style.left = worldPos.x;
                card.style.top = worldPos.y;

                _cardDragArea.Add(card); // 드래그 영역으로 이동하여 레이아웃 흔들림 방지

                card.CapturePointer(evt.pointerId);
                if (_prototype_PlayerController.Instance != null) _prototype_PlayerController.Instance.IsMovable = false;

                evt.StopPropagation();
            });

            card.RegisterCallback<PointerMoveEvent>(evt =>
            {
                if (!isDragging || !card.HasPointerCapture(evt.pointerId)) return;

                Vector2 pointerPos = evt.position;
                card.style.left = pointerPos.x - dragOffset.x;
                card.style.top = pointerPos.y - dragOffset.y;

                evt.StopPropagation();
            });

            card.RegisterCallback<PointerUpEvent>(evt =>
            {
                if (!isDragging) return;
                isDragging = false;

                card.ReleasePointer(evt.pointerId);

                // 화면 일정 높이 이상 올렸는지 확인하여 타겟팅 모드로 돌입
                float thresholdY = _handCardContainer.worldBound.yMin - 50f;
                if (evt.position.y < thresholdY)
                {
                    // 카드는 화면에 띄우지 않고 숨김 처리 (가로형 와이드 HUD 바가 내용을 표시함)
                    card.style.display = DisplayStyle.None;
                    card.pickingMode = PickingMode.Ignore;

                    if (_prototype_PlayerController.Instance != null)
                    {
                        _prototype_PlayerController.Instance.StartTargeting(cardData);
                    }
                    evt.StopPropagation();
                    return;
                }

                card.style.translate = new Translate(0, 0, 0);
                card.style.transitionDuration = new List<TimeValue> { new TimeValue(0.18f, TimeUnit.Second) };

                int safeIndex = Mathf.Clamp(originalIndex, 0, _handCardContainer.childCount);
                _handCardContainer.Insert(safeIndex, card);

                if (_prototype_PlayerController.Instance != null) _prototype_PlayerController.Instance.IsMovable = true;

                LayoutCardViews();

                evt.StopPropagation();
            });

            card.RegisterCallback<PointerEnterEvent>(evt =>
            {
                // 타겟팅 모드 중일 때는 호버 팝업을 발생시키지 않음
                if (_currentTargetingCard != null) return;

                if (!isDragging)
                {
                    IsCardHovered = true;
                    // 하단에 빈공간(갭)이 생기지 않도록 -125px만큼 부드럽게 팝업 (바닥 모서리가 화면 하단선 아래 15px에 머묾)
                    card.style.transitionDuration = new List<TimeValue> { new TimeValue(0.18f, TimeUnit.Second) };
                    card.style.translate = new Translate(0, -125, 0);
                    UpdateZOrder(card);
                }
            });

            card.RegisterCallback<PointerLeaveEvent>(evt =>
            {
                IsCardHovered = false; // 드래그 중이든 아니든 호버 상태는 해제해야 함
                if (!isDragging)
                {
                    card.style.transitionDuration = new List<TimeValue> { new TimeValue(0.18f, TimeUnit.Second) };
                    card.style.translate = new Translate(0, 0, 0);
                    UpdateZOrder(null);
                }
            });
        }

        private void UpdateZOrder(VisualElement hoveredCard)
        {
            _handCardContainer.Sort((a, b) =>
            {
                int indexA = handCardViews.IndexOf(a);
                int indexB = handCardViews.IndexOf(b);

                if (hoveredCard == null)
                    return indexA.CompareTo(indexB);

                int hoverIndex = handCardViews.IndexOf(hoveredCard);
                int distA = Mathf.Abs(indexA - hoverIndex);
                int distB = Mathf.Abs(indexB - hoverIndex);

                if (distA != distB)
                    return distB.CompareTo(distA); // 거리가 멀수록 앞에 위치 (낮은 인덱스 = 먼저 그려져서 아래에 깔림)

                return indexA.CompareTo(indexB);
            });
        }

        private void LayoutCardViews()
        {
            float containerWidth = _handCardContainer.resolvedStyle.width;
            int count = handCardViews.Count;
            if (count == 0) return;

            // 아직 폭이 계산되지 않았다면 스케줄러로 조금 뒤에 다시 호출 (UI Toolkit의 NaN 버그 방지)
            if (float.IsNaN(containerWidth) || containerWidth <= 0)
            {
                _handCardContainer.schedule.Execute(LayoutCardViews).StartingIn(10);
                return;
            }

            float defaultCardWidth = 240f; // USS에 정의된 카드 너비
            float cardWidth = (!float.IsNaN(handCardViews[0].resolvedStyle.width) && handCardViews[0].resolvedStyle.width > 0) ? handCardViews[0].resolvedStyle.width : defaultCardWidth;

            // 카드 사이 최대 간격 설정 (살짝 떨어져 있거나 겹칠 수 있음)
            float maxSpacing = cardWidth * 1.05f;
            float spacing = Mathf.Min(maxSpacing, containerWidth / count);

            float totalWidth = spacing * (count - 1) + cardWidth;
            float startX = (containerWidth - totalWidth) / 2f;

            for (int i = 0; i < count; i++)
            {
                VisualElement card = handCardViews[i];
                card.style.left = startX + (i * spacing);
                card.style.top = 10f; // 컨테이너 상단에서 약간 떨어뜨림
            }
        }

        public void RefreshAllCardDescriptions()
        {
            _prototype_EntityView controlledEntityView = _prototype_PlayerController.Instance != null ? _prototype_PlayerController.Instance.ControlledEntityView : null;
            _prototype_LifeData playerLife = (controlledEntityView is _prototype_LifeView lifeView) ? lifeView.Data : null;
            if (playerLife == null && controlledEntityView != null)
            {
                playerLife = controlledEntityView.EntityData as _prototype_LifeData;
            }

            foreach (var cardView in handCardViews)
            {
                if (cardView == null) continue;
                if (cardView.userData is _prototype_CardData cardData)
                {
                    var lblDesc = cardView.Q<Label>("CardDesc");
                    if (lblDesc != null)
                    {
                        lblDesc.text = _prototype_CardDescriptionFormatter.FormatDescription(cardData, playerLife, _isDetailedDescriptionMode);
                    }
                }
            }

            if (_currentTargetingCard != null && _tooltipCardDesc != null)
            {
                _tooltipCardDesc.text = _prototype_CardDescriptionFormatter.FormatDescription(_currentTargetingCard, playerLife, _isDetailedDescriptionMode, _currentHoveredTarget);
            }
        }
    }

}