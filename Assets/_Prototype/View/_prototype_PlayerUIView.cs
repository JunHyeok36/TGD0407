using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
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
        private VisualElement _handCardContainer;
        private readonly List<VisualElement> _handCardViews = new();
        private _prototype_CardData _selectedCard;

        public bool IsCardTargeting => _selectedCard != null;

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
            _handCardContainer = root.Q<VisualElement>("HandCardContainer");

            UpdatePlayerInfo();
            UpdatePlayerCardDeck();
        }

        private void Update()
        {
            if (_selectedCard == null) return;
            if (_prototype_PlayerController.Instance == null) return;
            if (Mouse.current == null || Keyboard.current == null) return;

            if (Keyboard.current.escapeKey.wasPressedThisFrame || Mouse.current.rightButton.wasPressedThisFrame)
            {
                ClearCardSelection();
                return;
            }

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                _prototype_Point? castPoint = _prototype_PlayerController.Instance.GetIsometricMousePoint();
                if (!castPoint.HasValue) return;
                TryCastSelectedCard(castPoint.Value).Forget();
            }
        }

        public void UpdatePlayerInfo()
        {
            if (_label_point == null || _label_health == null) return;
            if (_prototype_PlayerController.Instance == null) return;
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
            if (_handCardContainer == null) return;
            if (_prototype_PlayerController.Instance == null) return;

            _handCardContainer.Clear();
            _handCardViews.Clear();
            _selectedCard = null;
            _prototype_EntityView controlledEntityView = _prototype_PlayerController.Instance.ControlledEntityView;
            if (controlledEntityView != null && controlledEntityView is _prototype_LifeView lifeView)
            {
                _prototype_LifeData lifeData = lifeView.Data;
                if (lifeData != null)
                {
                    foreach (_prototype_CardData cardData in lifeData.cardDeck.handedCardDatas)
                    {
                        VisualElement cardViewInstance = _cardViewTemplate.Instantiate();

                        cardViewInstance.Q<Label>("CardName").text = cardData.id;
                        cardViewInstance.Q<Label>("CardCost").text = cardData.costValue.value.ToString();
                        cardViewInstance.Q<Label>("CardType").text = cardData.cardType.ToString();
                        cardViewInstance.style.position = Position.Absolute;
                        cardViewInstance.userData = cardData;
                        RegisterCardEvents(cardViewInstance);
                        
                        _handCardContainer.Add(cardViewInstance);
                        _handCardViews.Add(cardViewInstance);
                    }

                    LayoutCardViews();
                }
            }
        }

        private void RegisterCardEvents(VisualElement card)
        {
            card.RegisterCallback<PointerDownEvent>(evt =>
            {
                if (evt.button != 0) return;
                _selectedCard = card.userData as _prototype_CardData;
                HighlightSelectedCard(card);
                evt.StopPropagation();
            });
        }

        private void LayoutCardViews()
        {
            float containerWidth = _handCardContainer.resolvedStyle.width;
            float spacing = containerWidth / (_handCardViews.Count + 1);
            for (int i = 0; i < _handCardViews.Count; i++)
            {
                VisualElement card = _handCardViews[i];
                float cardWidth = card.resolvedStyle.width;
                card.style.left = (spacing * (i + 1)) - (cardWidth / 2);
            }
        }

        private async UniTaskVoid TryCastSelectedCard(_prototype_Point castPoint)
        {
            if (_selectedCard == null) return;

            bool succeeded = await _prototype_PlayerController.Instance.TryPlayCardFromHand(_selectedCard, castPoint);
            if (!succeeded) return;

            UpdatePlayerInfo();
            UpdatePlayerCardDeck();
        }

        private void HighlightSelectedCard(VisualElement selectedCardView)
        {
            for (int i = 0; i < _handCardViews.Count; i++)
            {
                VisualElement card = _handCardViews[i];
                if (card == selectedCardView)
                {
                    card.style.translate = new Translate(0, Length.Percent(-20), 0);
                    card.style.unityBackgroundImageTintColor = new Color(0.85f, 1f, 0.85f);
                }
                else
                {
                    card.style.translate = new Translate(0, 0, 0);
                    card.style.unityBackgroundImageTintColor = Color.white;
                }
            }
        }

        private void ClearCardSelection()
        {
            _selectedCard = null;
            for (int i = 0; i < _handCardViews.Count; i++)
            {
                _handCardViews[i].style.translate = new Translate(0, 0, 0);
                _handCardViews[i].style.unityBackgroundImageTintColor = Color.white;
            }
        }
    }

}