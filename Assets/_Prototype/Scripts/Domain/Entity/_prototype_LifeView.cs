using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace TDG0407._prototype
{

    public sealed class _prototype_LifeView : _prototype_EntityView
    {

        [SerializeField] private _prototype_Side side;

        public _prototype_LifeData Data => _entityData as _prototype_LifeData;

        public async UniTask PlayAttackAnimation(Vector3 direction)
        {
            if (this == null || gameObject == null)
                return;

            await transform.DOPunchPosition(direction * 0.3f, 0.2f, 1, 0)
                .AsyncWaitForCompletion();
        }

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
                Data.stamina.OnValueChanged += CheckStaminaForKnockdown;
            }

            _prototype_TickManager.RegisterTick(ProcessLifeTick);
        }

        private void OnDestroy()
        {
            if (Data != null)
            {
                Data.health.OnValueChanged -= CheckDeath;
                Data.stamina.OnValueChanged -= CheckStaminaForKnockdown;
            }
            _prototype_TickManager.UnregisterTick(ProcessLifeTick);
        }

        private void CheckStaminaForKnockdown()
        {
            if (Data.stamina.Current <= 0)
            {
                Data.ApplyStatusEffect(new _prototype_StatusEffect(_prototype_StatusType.Knockdown, 5));
            }
        }

        private async void CheckDeath()
        {
            if (Data.health.Current <= 0)
            {
                Data.health.OnValueChanged -= CheckDeath;

                // 1. Play death animation
                await transform.DOScale(0, 0.5f).SetEase(Ease.InBack).AsyncWaitForCompletion();

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

        private UniTask<_prototype_TickIntent> ProcessLifeTick()
        {
            var intent = new _prototype_TickIntent
            {
                TargetsPlayer = false,
                Execute = async () =>
                {
                    if (Data != null)
                    {
                        if (Data.cardDeck != null)
                        {
                            bool hasKnockdown = Data.HasStatusEffect(_prototype_StatusType.Knockdown);
                            foreach (var card in Data.cardDeck.handedCardDatas)
                            {
                                int reduction = 1 + Data.lifeStat.drawQuickness;
                                if (hasKnockdown)
                                {
                                    card.currentCoolTicks = UnityEngine.Mathf.Min(card.coolTicks.Max, card.currentCoolTicks + reduction);
                                }
                                else if (card.currentCoolTicks > 0)
                                {
                                    card.currentCoolTicks = UnityEngine.Mathf.Max(0, card.currentCoolTicks - reduction);
                                }
                            }
                        }

                        if (Data.statusEffects != null)
                        {
                            for (int i = Data.statusEffects.Count - 1; i >= 0; i--)
                            {
                                var effect = Data.statusEffects[i];

                                if (_prototype_TickManager.CurrentTick > effect.appliedTick)
                                {
                                    if (effect.type == _prototype_StatusType.Bleeding)
                                    {
                                        Data.health.Current -= (int)effect.value;
                                    }
                                    else if (effect.type == _prototype_StatusType.Burning)
                                    {
                                        await Data.TakeDamage(new _prototype_DamageContext(Data, Data, _prototype_DamageType.Physical, (int)effect.value, (int)effect.value));
                                    }
                                    else if (effect.type == _prototype_StatusType.Poisoning)
                                    {
                                        int poisonDmg = (int)(effect.durationTicks * effect.value);
                                        await Data.TakeDamage(new _prototype_DamageContext(Data, Data, _prototype_DamageType.Magical, poisonDmg, poisonDmg));
                                    }

                                    effect.durationTicks--;
                                    if (effect.durationTicks <= 0)
                                    {
                                        Data.statusEffects.RemoveAt(i);
                                    }
                                }
                            }
                        }
                    }
                    await UniTask.Yield();
                }
            };
            return UniTask.FromResult(intent);
        }
    }

}