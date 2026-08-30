using System;

namespace TDG0407._prototype
{

    public sealed class _prototype_LifeView : _prototype_EntityView
    {

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

            _prototype_TickManager.RegisterTick(ProcessLifeTick);
        }

        private void OnDestroy()
        {
            _prototype_TickManager.UnregisterTick(ProcessLifeTick);
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