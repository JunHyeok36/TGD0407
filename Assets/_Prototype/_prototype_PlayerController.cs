using Cysharp.Threading.Tasks;
using System;
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
        [SerializeField] private int _initialDrawCount = 5;

        //[Header("Settings")] 

        public _prototype_Point ControlledEntityLastPoint => _controlledEntityLastPoint ?? _controlledEntityView.Point;
        public _prototype_EntityView ControlledEntityView => _controlledEntityView;
        public _prototype_LifeView ControlledLifeView => _controlledEntityView as _prototype_LifeView;
        
        private _prototype_Point? _controlledEntityLastPoint = null; 

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }
        private void Update()
        {
            if (Mouse.current == null) return;
            _prototype_Point? mousePoint = GetIsometricMousePoint();
            bool isCardTargeting = _prototype_PlayerUIView.Instance != null && _prototype_PlayerUIView.Instance.IsCardTargeting;

            if (Mouse.current.rightButton.isPressed)
            {    
                if (mousePoint.HasValue)
                {
                    _prototype_GridVisualManager.Instance.UpdatePointHoverIndicatorColor(isCardTargeting ? Color.yellow : Color.red);
                }
                if (!isCardTargeting && mousePoint.HasValue) HandleMovementInput(mousePoint.Value);
            }
            else
            {
                _prototype_GridVisualManager.Instance.UpdatePointHoverIndicatorColor(Color.white);
            }
            
            _prototype_GridVisualManager.Instance.HighlightPoint(mousePoint);
        }

        public void InitializeRuntime()
        {
            _prototype_LifeView lifeView = ControlledLifeView;
            if (lifeView == null) return;
            if (lifeView.Data == null) return;

            lifeView.Data.cardDeck.InitializeRuntimeState(true);
            lifeView.Data.cardDeck.Draw(Mathf.Max(1, _initialDrawCount));
        }

        public _prototype_Point? GetIsometricMousePoint()
        {
            if (Mouse.current == null || _cameraController == null || _cameraController.MainCamera == null) return null;
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

            if (Mouse.current.rightButton.wasPressedThisFrame)
                ExecuteSingleMoveStep(targetPoint.Value);

            void ExecuteSingleMoveStep(_prototype_Point targetPoint)
            {
                if (_controlledEntityView.Point == targetPoint) return;

                List<_prototype_PointView> path = _prototype_GridManager.Instance.FindPath(_controlledEntityView.Point, targetPoint);
                if (path != null && path.Count > 0)
                {
                    _prototype_PointView nextStep = path[0];

                    _controlledEntityLastPoint = _controlledEntityView.Point;
                    _prototype_TickManager.AdvanceTick(async () => {  
                        await _prototype_InteractionManager.MoveEntity(_controlledEntityView, _prototype_GridManager.Instance.GetPointView(_controlledEntityView.Point), nextStep);
                        if (ControlledLifeView != null) _prototype_CardManager.TickDeck(ControlledLifeView.Data.cardDeck);
                        if (_prototype_PlayerUIView.Instance != null) _prototype_PlayerUIView.Instance.UpdatePlayerInfo();
                    }).Forget();
                    //_prototype_GridVisualManager.Instance.PlayClickEffect(nextStep);
                }
            }
        }

        public async UniTask<bool> TryPlayCardFromHand(_prototype_CardData cardData, _prototype_Point targetPoint)
        {
            _prototype_LifeView lifeView = ControlledLifeView;
            if (lifeView == null) throw new Exception("Controlled entity must be a life view to cast cards.");
            if (!_prototype_CardManager.CanCast(lifeView.Data, cardData, targetPoint)) return false;

            bool casted = false;
            await _prototype_TickManager.AdvanceTick(async () =>
            {
                casted = await _prototype_CardManager.TryCastCard(lifeView, cardData, targetPoint);
            });

            if (casted)
            {
                _prototype_CardManager.TickDeck(lifeView.Data.cardDeck);
            }

            return casted;
        }

    }

}