using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace TDG0407._prototype
{
    
    [RequireComponent(typeof(PanelRenderer))]
    public class _prototype_PlayerUIView : MonoBehaviour
    {
        public static _prototype_PlayerUIView Instance { get; private set; }

        [SerializeField] private PanelRenderer _panelRenderer;
        [SerializeField] private VisualTreeAsset _cardViewTemplate;

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

        private VisualElement _warningMessageContainer;
        private Label _warningMessageText;
        private VisualElement _gameOverContainer;

        public bool IsCardHovered { get; private set; }

        private List<VisualElement> handCardViews = new();

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            _panelRenderer = GetComponent<PanelRenderer>();
            _panelRenderer.RegisterUIReloadCallback(OnUIReload);
        }

        private void OnDestroy()
        {
            _panelRenderer.UnregisterUIReloadCallback(OnUIReload);
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

            UpdatePlayerInfo();
            UpdatePlayerCardDeck();
        }

        public void ShowWarning(string message, float duration = 2.0f)
        {
            if (_warningMessageContainer == null || _warningMessageText == null) return;
            
            _warningMessageText.text = message;
            _warningMessageContainer.style.display = DisplayStyle.Flex;
            
            // 일정 시간 후 숨기기
            _warningMessageContainer.schedule.Execute(() => {
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

        public void ShowTargetingUI(_prototype_CardData cardData)
        {
            if (_targetingHint != null) _targetingHint.style.display = DisplayStyle.Flex;
            if (_targetingTooltip != null)
            {
                _targetingTooltip.style.display = DisplayStyle.Flex;
                if (_tooltipCardName != null) _tooltipCardName.text = cardData.id;
                if (_tooltipCardCost != null) _tooltipCardCost.text = $"Cost: {cardData.costValue.value}";
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

                    foreach (_prototype_CardData cardData in lifeData.cardDeck.handedCardDatas)
                    {
                        VisualElement cardViewInstance = _cardViewTemplate.Instantiate();

                        var lblName = cardViewInstance.Q<Label>("CardName");
                        if (lblName != null) lblName.text = cardData.id;

                        var lblCost = cardViewInstance.Q<Label>("CardCost");
                        if (lblCost != null) lblCost.text = cardData.costValue.value.ToString();

                        var lblType = cardViewInstance.Q<Label>("CardType");
                        if (lblType != null) lblType.text = cardData.cardType.ToString();

                        var lblDesc = cardViewInstance.Q<Label>("CardDesc");
                        if (lblDesc != null)
                        {
                            lblDesc.text = string.IsNullOrEmpty(cardData.description) ? "No description available." : cardData.description;
                        }
                        
                        var cooldownOverlay = cardViewInstance.Q<VisualElement>("CooldownOverlay");
                        var cooldownText = cardViewInstance.Q<Label>("CooldownText");

                        // 쿨타임 시각적 피드백
                        if (cardData.currentCoolTicks > 0)
                        {
                            if (cooldownOverlay != null) cooldownOverlay.style.display = DisplayStyle.Flex;
                            if (cooldownText != null) cooldownText.text = cardData.currentCoolTicks.ToString();
                        }
                        else
                        {
                            if (cooldownOverlay != null) cooldownOverlay.style.display = DisplayStyle.None;
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
                        card.schedule.Execute(() => {
                            // 우클릭으로 카드 버리기
                            _prototype_PlayerController.Instance.DiscardCard(cardData);
                        }).ExecuteLater(300);
                    }
                    evt.StopPropagation();
                    return;
                }

                if (evt.button != 0) return;

                if (cardData.currentCoolTicks > 0)
                {
                    ShowWarning("현재 사용할 수 없습니다! (대기 중)");
                    evt.StopPropagation();
                    return;
                }

                bool hasEnoughCost = true;
                if (_prototype_PlayerController.Instance != null && _prototype_PlayerController.Instance.ControlledEntityView is _prototype_LifeView lifeView)
                {
                    var cost = cardData.costValue;
                    if (cost.costType == _prototype_CostType.FixedStamina && lifeView.Data.stamina.Current < cost.value) hasEnoughCost = false;
                    else if (cost.costType == _prototype_CostType.FixedHealth && lifeView.Data.health.Current < cost.value) hasEnoughCost = false;
                }
                
                if (!hasEnoughCost)
                {
                    ShowWarning("소모 자원이 부족합니다!");
                    return;
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