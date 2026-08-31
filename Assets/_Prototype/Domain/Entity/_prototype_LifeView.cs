using System;
using DG.Tweening;
using UnityEngine;

namespace TDG0407._prototype
{

    public sealed class _prototype_LifeView : _prototype_EntityView
    {

        [SerializeField] private _prototype_Side side;

        public _prototype_LifeData Data => _entityData as _prototype_LifeData;

        public override void Initialize(_prototype_PointView pointView)
        {
            base.Initialize(pointView);

            _prototype_LifeHUD hud = GetComponent<_prototype_LifeHUD>();
            if (hud == null)
            {
                hud = gameObject.AddComponent<_prototype_LifeHUD>();
            }
            hud.SetupEvents();

            // 카드 덱 초기화 (플레이어 및 적 AI 공통)
            if (Data != null && Data.cardDeck != null)
            {
                if (Data.cardDeck.remainedCardDatas.Count == 0 &&
                    Data.cardDeck.discardedCardDatas.Count == 0 &&
                    Data.cardDeck.allCardDatas.Count > 0)
                {
                    Data.cardDeck.InitializeDeck();
                }
            }

            if (Data != null)
            {
                Data.side = side;
                Data.health.OnValueChanged += CheckDeath;
            }

            _prototype_TickManager.RegisterTick(ProcessLifeTick);
        }

        private void OnDestroy()
        {
            if (Data != null)
            {
                Data.health.OnValueChanged -= CheckDeath;
            }
            _prototype_TickManager.UnregisterTick(ProcessLifeTick);
        }

        private async void CheckDeath()
        {
            if (Data.health.Current <= 0)
            {
                Data.health.OnValueChanged -= CheckDeath;

                // 1. Play death animation
                await transform.DOScale(0, 0.5f).SetEase(DG.Tweening.Ease.InBack).AsyncWaitForCompletion();

                // 2. Unregister from Grid
                var pointView = _prototype_GridManager.Instance.GetPointView(Data.point);
                if (pointView != null) pointView.RemoveEntity(this);

                // 3. Destroy game object
                if (gameObject != null) Destroy(gameObject);

                // 4. Check if player
                if (_prototype_PlayerController.Instance != null && _prototype_PlayerController.Instance.ControlledEntityView == this)
                {
                    if (_prototype_PlayerUIView.Instance != null)
                        _prototype_PlayerUIView.Instance.ShowGameOver();
                }
            }
        }

        private Cysharp.Threading.Tasks.UniTask ProcessLifeTick()
        {
            if (Data != null && Data.cardDeck != null)
            {
                foreach (var card in Data.cardDeck.handedCardDatas)
                {
                    if (card.currentCoolTicks > 0)
                    {
                        // drawQuickness를 이용해 쿨타임을 더 빠르게 줄일 수 있음
                        int reduction = 1 + Data.lifeStat.drawQuickness;
                        card.currentCoolTicks = UnityEngine.Mathf.Max(0, card.currentCoolTicks - reduction);
                    }
                }
            }
            return Cysharp.Threading.Tasks.UniTask.CompletedTask;
        }
    }

}