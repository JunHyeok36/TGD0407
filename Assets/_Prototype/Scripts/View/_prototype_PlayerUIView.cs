using System.Collections.Generic;
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
        private Label _label_remainsCount;
        private Label _label_discardedCount;
        private VisualElement _cardDragArea;
        private VisualElement _handCardContainer;

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

        public bool IsCardHovered { get; private set; }

        private List<VisualElement> handCardViews = new();

        private void Awake()
        {
            if (_instance == null) _instance = this;
            else if (_instance != this) Destroy(gameObject);
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
        }

        private void OnUIReload(PanelRenderer panelRenderer, VisualElement root)
        {
            _label_point = root.Q<Label>("Point");
            _label_health = root.Q<Label>("Health");
            _label_stamina = root.Q<Label>("Stamina");
            _label_remainsCount = root.Q<Label>("RemainsCount");
            _label_discardedCount = root.Q<Label>("DiscardedCount");

            _cardDragArea = root.Q<VisualElement>("CardDragArea");
            _handCardContainer = root.Q<VisualElement>("HandCardContainer");

            _targetingHint = root.Q<VisualElement>("TargetingHint");
            _targetingTooltip = root.Q<VisualElement>("TargetingTooltip");
            _tooltipCardName = root.Q<Label>("TooltipCardName");
            _tooltipCardCost = root.Q<Label>("TooltipCardCost");
            _tooltipCardDesc = root.Q<Label>("TooltipCardDescription");

            _warningMessageContainer = root.Q<VisualElement>("WarningMessageContainer");
            _warningMessageText = root.Q<Label>("WarningMessageText");

            _gameOverContainer = root.Q<VisualElement>("GameOverContainer");

            _modeIndicatorBanner = root.Q<VisualElement>("ModeIndicatorBanner");
            _modeIndicatorText = root.Q<Label>("ModeIndicatorText");

            UpdatePlayerInfo();
            UpdatePlayerCardDeck();

            // UI 리로드 후 현재 모드 표시 갱신
            _prototype_PlayMode currentMode = _prototype_PlayModeManager.Instance != null
                ? _prototype_PlayModeManager.Instance.CurrentMode
                : _prototype_PlayMode.Battle;
            UpdateModeIndicator(currentMode);

            BuildHazardInfoTooltip(root);
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
            if (_targetingHint != null) _targetingHint.style.display = DisplayStyle.Flex;
            if (_targetingTooltip != null)
            {
                _targetingTooltip.style.display = DisplayStyle.Flex;
                if (_tooltipCardName != null) _tooltipCardName.text = cardData.id;
                if (_tooltipCardCost != null)
                {
                    if (cardData is _prototype_BattleCardData battleCard)
                        _tooltipCardCost.text = $"Cost: {battleCard.costValue?.value ?? 0}";
                    else
                        _tooltipCardCost.text = "Cost: -";
                }
                if (_tooltipCardDesc != null) _tooltipCardDesc.text = cardData.description;
            }
        }

        public void HideTargetingUI()
        {
            if (_targetingHint != null) _targetingHint.style.display = DisplayStyle.None;
            if (_targetingTooltip != null) _targetingTooltip.style.display = DisplayStyle.None;
        }

        public void UpdateTargetingTooltipPosition(Vector2 screenPosition)
        {
            if (_targetingTooltip != null && _targetingTooltip.style.display == DisplayStyle.Flex)
            {
                if (_targetingTooltip.panel != null)
                {
                    // Convert Mouse Position (Bottom-Left origin) to Screen Position (Top-Left origin) expected by ScreenToPanel
                    Vector2 screenTopLeft = new Vector2(screenPosition.x, Screen.height - screenPosition.y);
                    Vector2 panelPos = UnityEngine.UIElements.RuntimePanelUtils.ScreenToPanel(_targetingTooltip.panel, screenTopLeft);

                    _targetingTooltip.style.left = panelPos.x;
                    _targetingTooltip.style.top = panelPos.y;
                }
                else
                {
                    // Fallback
                    float y = Screen.height - screenPosition.y;
                    _targetingTooltip.style.left = screenPosition.x;
                    _targetingTooltip.style.top = y;
                }
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

                    if (_label_point != null) _label_point.text = $"> POS : {controlledEntityView.Point}";
                    if (_label_health != null) _label_health.text = $"> HP  : {displayHp}/{entityData.health.Max}";
                    if (_label_stamina != null) _label_stamina.text = $"> SP  : {displaySp}/{entityData.stamina.Max}";
                }
                else
                {
                    if (_label_point != null) _label_point.text = string.Empty;
                    if (_label_health != null) _label_health.text = string.Empty;
                    if (_label_stamina != null) _label_stamina.text = string.Empty;
                }

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

                        var lblDesc = cardViewInstance.Q<Label>("CardDesc");
                        if (lblDesc != null)
                        {
                            lblDesc.text = string.IsNullOrEmpty(cardData.description) ? "No description available." : cardData.description;
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
                        // 1. Play Burn Animation
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
                        card.pickingMode = PickingMode.Ignore;

                        // 2. Execute Burn after animation
                        card.schedule.Execute(() =>
                        {
                            // 우클릭으로 카드 버리기
                            _prototype_PlayerController.Instance.DiscardCard(cardData);
                        }).ExecuteLater(300);
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
                    card.style.display = DisplayStyle.None;

                    // 남은 핸드 카드들을 재정렬
                    handCardViews.Remove(card);
                    LayoutCardViews();

                    if (_prototype_PlayerController.Instance != null)
                    {
                        _prototype_PlayerController.Instance.StartTargeting(cardData);
                    }
                    evt.StopPropagation();
                    return;
                }

                card.style.translate = new Translate(0, 0, 0);

                int safeIndex = Mathf.Clamp(originalIndex, 0, _handCardContainer.childCount);
                _handCardContainer.Insert(safeIndex, card);

                if (_prototype_PlayerController.Instance != null) _prototype_PlayerController.Instance.IsMovable = true;

                LayoutCardViews();

                evt.StopPropagation();
            });

            card.RegisterCallback<PointerEnterEvent>(evt =>
            {
                if (!isDragging)
                {
                    IsCardHovered = true;
                    card.style.translate = new Translate(0, Length.Percent(-50), 100);
                    UpdateZOrder(card);
                }
            });

            card.RegisterCallback<PointerLeaveEvent>(evt =>
            {
                IsCardHovered = false; // 드래그 중이든 아니든 호버 상태는 해제해야 함
                if (!isDragging)
                {
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
    }

}