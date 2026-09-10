using UnityEngine;

namespace TDG0407._prototype
{

    public class _prototype_BootStrapper : MonoBehaviour
    {

        public void Awake()
        {

            _prototype_EventBus.Reset();
            _prototype_EventBus.Listen<EntityDamagedEvent>(_prototype_DamageText.OnEntityDamaged);
            _prototype_TickManager.Initialize();
            _prototype_GridManager.Instance.Initialize();

            // PlayModeManager 초기화 (씬에 배치된 경우)
            if (_prototype_PlayModeManager.Instance != null)
            {
                _prototype_PlayModeManager.Instance.Initialize();
            }

            // 최초 시작 시 플레이어 덱에서 5장을 드로우
            if (_prototype_PlayerController.Instance != null &&
                _prototype_PlayerController.Instance.ControlledEntityView is _prototype_LifeView playerLife)
            {
                if (playerLife.Data != null && playerLife.Data.cardDeck != null)
                {
                    playerLife.Data.cardDeck.DrawCards(
                        playerLife.Data.lifeStat.handCardSlotCount,
                        playerLife.Data.lifeStat.handCardSlotCount
                    );

                    // 드로우 후 UI 갱신
                    if (_prototype_PlayerUIView.Instance != null)
                    {
                        //_prototype_PlayerUIView.Instance.UpdatePlayerCardDeck();
                    }
                }
            }
        }

        private void OnDestroy()
        {
            _prototype_EventBus.Reset();
        }
    }
}