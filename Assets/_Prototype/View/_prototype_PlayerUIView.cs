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
        private VisualElement _cardDragArea;
        private VisualElement _handCardContainer;

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
            _cardDragArea = root.Q<VisualElement>("CardDragArea");
            _handCardContainer = root.Q<VisualElement>("HandCardContainer");

            UpdatePlayerInfo();
            UpdatePlayerCardDeck();
        }

        public void UpdatePlayerInfo()
        {
            _prototype_EntityView controlledEntityView = _prototype_PlayerController.Instance.ControlledEntityView;
            if (controlledEntityView != null)
            {
                _prototype_EntityData entityData = controlledEntityView.EntityData;
                if (entityData != null)
                {
                    _label_point.text = $"Point: {controlledEntityView.Point}";
                    _label_health.text = $"Health: {entityData.health.Current}/{entityData.health.Max}";
                }
                else
                {
                    _label_point.text = string.Empty;
                    _label_health.text = string.Empty;
                }
                
            }   
        }

        public void UpdatePlayerCardDeck()
        {
            _handCardContainer.Clear();
            _prototype_EntityView controlledEntityView = _prototype_PlayerController.Instance.ControlledEntityView;
            if (controlledEntityView != null && controlledEntityView is _prototype_LifeView lifeView)
            {
                _prototype_LifeData lifeData = lifeView.Data;
                if (lifeData != null)
                {
                    foreach (_prototype_CardData cardData in lifeData.cardDeck.allCardDatas)
                    {
                        VisualElement cardViewInstance = _cardViewTemplate.Instantiate();

                        cardViewInstance.Q<Label>("CardName").text = cardData.id;
                        cardViewInstance.Q<Label>("CardCost").text = cardData.costValue.value.ToString();
                        cardViewInstance.Q<Label>("CardType").text = cardData.cardType.ToString();
                        
                        cardViewInstance.style.position = Position.Absolute;
                        // 드래그 기능 등록
                        RegisterDragEvents(cardViewInstance);
                        
                        _handCardContainer.Add(cardViewInstance);
                        handCardViews.Add(cardViewInstance);
                    }

                    LayoutCardViews();
                }
            }
        }

        private void RegisterDragEvents(VisualElement card)
        {
            bool isDragging = false;
            Vector2 dragOffset = Vector2.zero;
            int originalIndex = -1;
            
            card.RegisterCallback<PointerDownEvent>(evt =>
            {
                if (evt.button != 0) return;

                isDragging = true;
                dragOffset = evt.localPosition;

                originalIndex = _handCardContainer.IndexOf(card);

                // 드래그 시 부모(FlexContainer)를 떠나 드래그 영역으로 이동
                var worldPos = card.worldBound;
                card.style.position = Position.Absolute;
                card.style.left = worldPos.x;
                card.style.top = worldPos.y;
                
                _cardDragArea.Add(card); // 드래그 영역으로 이동하여 레이아웃 흔들림 방지

                card.CapturePointer(evt.pointerId);
                if (_prototype_PlayerController.Instance != null) _prototype_PlayerController.Instance.enabled = false;

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
                
                card.style.translate = new Translate(0, 0, 0);

                int safeIndex = Mathf.Clamp(originalIndex, 0, _handCardContainer.childCount);
                _handCardContainer.Insert(safeIndex, card);

                if (_prototype_PlayerController.Instance != null) _prototype_PlayerController.Instance.enabled = true;

                LayoutCardViews(); 
            
                evt.StopPropagation();
            });
            
            card.RegisterCallback<PointerEnterEvent>(evt =>
            {
                // 드래그 중이 아닐 때만 호버 효과 적용
                if (!isDragging)
                {
                    // 카드를 위쪽으로 15%만큼 올림 (UI Toolkit의 translate 속성 사용)
                    card.style.translate = new Translate(0, Length.Percent(-15), 100);
                }
                else
                {
                    int hoverIndex = _handCardContainer.IndexOf(card);

                    _handCardContainer.Sort((a, b) =>
                    {
                        int indexA = _handCardContainer.IndexOf(a);
                        int indexB = _handCardContainer.IndexOf(b);

                        int distA = Mathf.Abs(indexA - hoverIndex);
                        int distB = Mathf.Abs(indexB - hoverIndex);

                        if (distA != distB)
                            return distA.CompareTo(distB);
                        else
                            return indexA.CompareTo(indexB);
                    });
                }
            });

            card.RegisterCallback<PointerLeaveEvent>(evt =>
            {
                // 드래그 중이 아닐 때 원래 위치로 안전하게 복원
                if (!isDragging)
                {
                    card.style.translate = new Translate(0, 0, 0);
                }
                else
                {
                    _handCardContainer.Sort((a, b) =>
                    {
                        return handCardViews.IndexOf(a).CompareTo(handCardViews.IndexOf(b));
                    });
                }
            });
        }

    
        private void LayoutCardViews()
        {
            float containerWidth = _handCardContainer.resolvedStyle.width;
            float spacing = containerWidth / (handCardViews.Count + 1);
            for (int i = 0; i < handCardViews.Count; i++)
            {
                VisualElement card = handCardViews[i];
                float cardWidth = card.resolvedStyle.width;
                card.style.left = (spacing * (i + 1)) - (cardWidth / 2);
            }
        }
    }

}