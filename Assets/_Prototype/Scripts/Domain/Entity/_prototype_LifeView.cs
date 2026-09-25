using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace TDG0407._prototype
{

    public sealed class _prototype_LifeView : _prototype_EntityView
    {

        [SerializeField] private _prototype_Side side;

        public _prototype_LifeData Data => _entityData as _prototype_LifeData;
        public _prototype_ChannelingController ChannelingController { get; private set; }

        public override void Initialize(
            _prototype_EntityData entityData,
            _prototype_PointView pointView)
        {
            base.Initialize(entityData, pointView);

            if (!TryGetComponent<_prototype_LifeHUD>(out var hud))
            {
                hud = gameObject.AddComponent<_prototype_LifeHUD>();
            }
            hud.Initialize();

            if (!TryGetComponent<_prototype_ChannelingController>(out var chCtrl))
            {
                chCtrl = gameObject.AddComponent<_prototype_ChannelingController>();
            }
            ChannelingController = chCtrl;
            ChannelingController.Initialize(this);

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
                Data.stamina.OnValueChanged += CheckStaminaForGroggy;
                Data.OnDied += HandleDeath;
                Data.OnChannelCancelRequested += OnChannelCancelRequested;
            }

            _deathsDoorEnteredSub = _prototype_EventBus.Listen<EntityDeathsDoorEnteredEvent>(evt =>
            {
                if (evt.Target == Data)
                {
                    _prototype_FloatingText.SpawnOnEntity(this, "죽음의 문턱!", new Color(0.9f, 0.15f, 0.15f), 1.2f, 1.2f, 1.5f);
                }
            });

            _deathResistedSub = _prototype_EventBus.Listen<EntityDeathResistedEvent>(evt =>
            {
                if (evt.Target == Data)
                {
                    _prototype_FloatingText.SpawnOnEntity(this, $"죽음 저항! ({evt.CurrentStack}스택)", new Color(1f, 0.85f, 0.1f), 1.2f, 1.2f, 1.5f);
                }
            });

            _deathsDoorClearedSub = _prototype_EventBus.Listen<EntityDeathsDoorClearedEvent>(evt =>
            {
                if (evt.Target == Data)
                {
                    _prototype_FloatingText.SpawnOnEntity(this, "문턱 극복!", new Color(0.2f, 0.9f, 0.4f), 1.2f, 1.2f, 1.5f);
                }
            });

            _statusChangedSub = _prototype_EventBus.Listen<EntityStatusChangedEvent>(evt =>
            {
                if (evt.Target == Data && evt.Effect.type == _prototype_StatusType.Airborne)
                {
                    if (evt.IsAdded)
                    {
                        transform.DOLocalMoveY(0.6f, 0.2f).SetEase(Ease.OutQuad).SetLink(gameObject);
                    }
                    else
                    {
                        transform.DOLocalMoveY(0f, 0.25f).SetEase(Ease.OutBounce).SetLink(gameObject);
                    }
                }
            });

            _prototype_TickManager.RegisterTick(ProcessLifeTick);
        }

        private IDisposable _deathsDoorEnteredSub;
        private IDisposable _deathResistedSub;
        private IDisposable _deathsDoorClearedSub;
        private IDisposable _statusChangedSub;

        private void OnChannelCancelRequested(string reason)
        {
            ChannelingController?.CancelChanneling(reason);
        }

        protected override void OnDestroy()
        {
            if (Data != null)
            {
                Data.health.OnValueChanged -= CheckDeath;
                Data.stamina.OnValueChanged -= CheckStaminaForGroggy;
                Data.OnDied -= HandleDeath;
                Data.OnChannelCancelRequested -= OnChannelCancelRequested;
            }
            _deathsDoorEnteredSub?.Dispose();
            _deathResistedSub?.Dispose();
            _deathsDoorClearedSub?.Dispose();
            _statusChangedSub?.Dispose();
            _prototype_TickManager.UnregisterTick(ProcessLifeTick);
        }

        private void CheckStaminaForGroggy()
        {
            if (Data != null)
            {
                Data.CheckStaminaForGroggy();
            }
        }

        private void CheckDeath()
        {
            if (Data != null && Data.IsDead)
            {
                HandleDeath();
            }
        }

        private bool _isDeathHandled = false;
        private async void HandleDeath()
        {
            if (_isDeathHandled) return;
            _isDeathHandled = true;

            if (Data != null)
            {
                Data.health.OnValueChanged -= CheckDeath;
                Data.stamina.OnValueChanged -= CheckStaminaForGroggy;
                Data.OnDied -= HandleDeath;

                // 전리품 드랍 이벤트 발행 (보유한 아이템이 있을 경우)
                if (Data.inventory != null && Data.inventory.TotalItemCount > 0)
                {
                    var droppedItems = Data.inventory.GetAllItems();
                    _prototype_EventBus.Fire(new _prototype_EntityLootDroppedEvent(Data, Data.point, droppedItems));
                }
                if (_prototype_PlayerController.Instance != null && _prototype_PlayerController.Instance.ControlledEntityView == this)
                {
                    if (_prototype_PlayerUIView.Instance != null)
                        _prototype_PlayerUIView.Instance.ShowGameOver();
                }
            }

            // 1. Play death animation
            await transform.DOScale(0, 0.5f).SetEase(Ease.InBack).AsyncWaitForCompletion();

            // 2. Unregister from Grid
            var pointView = _prototype_GridManager.Instance != null && Data != null ? _prototype_GridManager.Instance.GetPointView(Data.point) : null;
            if (pointView != null) pointView.RemoveEntity(this);

            // 3. Destroy GameObject
            Destroy(gameObject);
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
                            bool hasGroggy = Data.HasStatusEffect(_prototype_StatusType.Groggy);
                            foreach (var card in Data.cardDeck.handedCardDatas)
                            {
                                if (card is _prototype_BattleCardData battleCard)
                                {
                                    int reduction = 1 + Data.lifeStat.drawQuickness;
                                    if (hasGroggy)
                                    {
                                        battleCard.currentCoolTicks = Mathf.Min(battleCard.coolTicks.Max, battleCard.currentCoolTicks + reduction);
                                    }
                                    else if (battleCard.currentCoolTicks > 0)
                                    {
                                        battleCard.currentCoolTicks = Mathf.Max(0, battleCard.currentCoolTicks - reduction);
                                    }
                                }
                            }
                        }

                        if (ChannelingController != null && ChannelingController.IsChanneling)
                        {
                            await ChannelingController.ProcessTick();
                            if (Data.IsDead) return;
                        }

                        if (Data.statusEffects != null)
                        {
                            for (int i = Data.statusEffects.Count - 1; i >= 0; i--)
                            {
                                var effect = Data.statusEffects[i];

                                // 도발 시전자가 사망했거나 없는 경우 도발 자동 해제
                                if (effect.type == _prototype_StatusType.Provocation && (effect.sourceEntity == null || effect.sourceEntity.IsDead))
                                {
                                    Data.statusEffects.RemoveAt(i);
                                    _prototype_EventBus.Fire(new EntityStatusChangedEvent(Data, effect, false));
                                    continue;
                                }

                                if (_prototype_TickManager.CurrentTick > effect.appliedTick)
                                {
                                    if (effect.type == _prototype_StatusType.Bleeding)
                                    {
                                        int dmg = (int)effect.value;
                                        if (dmg > 0)
                                        {
                                            await Data.TakeDamage(new _prototype_DamageContext(Data, Data, _prototype_DamageType.Physical, dmg, dmg));
                                            _prototype_FloatingText.SpawnOnEntity(this, dmg.ToString(), new Color(1f, 0.28f, 0.1f), 0.8f, 0.85f, 1.5f);
                                            if (Data.IsDead) break;
                                        }
                                    }
                                    else if (effect.type == _prototype_StatusType.Burning)
                                    {
                                        int dmg = (int)effect.value;
                                        if (dmg > 0)
                                        {
                                            await Data.TakeDamage(new _prototype_DamageContext(Data, Data, _prototype_DamageType.Physical, dmg, dmg));
                                            _prototype_FloatingText.SpawnOnEntity(this, dmg.ToString(), new Color(1f, 0.45f, 0.05f), 0.8f, 0.85f, 1.5f);
                                            if (Data.IsDead) break;
                                        }
                                    }
                                    else if (effect.type == _prototype_StatusType.Poisoning)
                                    {
                                        int poisonDmg = (int)(effect.durationTicks * effect.value);
                                        if (poisonDmg > 0)
                                        {
                                            await Data.TakeDamage(new _prototype_DamageContext(Data, Data, _prototype_DamageType.Magical, poisonDmg, poisonDmg));
                                            _prototype_FloatingText.SpawnOnEntity(this, poisonDmg.ToString(), new Color(0.45f, 0.35f, 1f), 0.8f, 0.85f, 1.5f);
                                            if (Data.IsDead) break;
                                        }
                                    }

                                    effect.duration.OnTick();
                                    if (effect.IsExpired)
                                    {
                                        Data.statusEffects.RemoveAt(i);
                                        _prototype_EventBus.Fire(new EntityStatusChangedEvent(Data, effect, false));
                                    }

                                }
                            }
                        }

                        Data.TickShields();
                    }
                    await UniTask.Yield();
                }
            };
            return UniTask.FromResult(intent);
        }
        public System.Collections.Generic.List<_prototype_CardData> GetAvailableCards()
        {
            var result = new System.Collections.Generic.List<_prototype_CardData>();
            
            var playMode = _prototype_PlayModeManager.Instance != null
                ? _prototype_PlayModeManager.Instance.CurrentMode
                : _prototype_PlayMode.Exploration;
            var context = new _prototype_ConditionContext(Data, null, playMode);

            if (Data != null && Data.cardDeck != null)
            {
                if (playMode == _prototype_PlayMode.Battle)
                {
                    foreach (var card in Data.cardDeck.handedCardDatas)
                    {
                        if (card != null && card.IsVisible(context))
                        {
                            card.sourceProvider = null;
                            result.Add(card);
                        }
                    }
                }
            }

            if (_prototype_GridManager.Instance != null && Data != null)
            {
                if (playMode == _prototype_PlayMode.Exploration)
                {
                    // search for CardProviderComponentData in nearby entities (e.g., radius 1)
                    foreach (var pointView in _prototype_GridManager.Instance.PointViews)
                    {
                        if (pointView == null) continue;
                        int dist = Mathf.Abs(pointView.Point.x - Data.point.x) + Mathf.Abs(pointView.Point.y - Data.point.y);
                        if (dist > 1) continue; // Only adjacent or self

                        foreach (var entityView in pointView.PlacedEntityViews)
                        {
                            if (entityView == null || entityView.EntityData == null || entityView.EntityData.components == null) continue;
                            
                            // we provide a specific context where Target is the providing entity
                            var targetContext = new _prototype_ConditionContext(Data, entityView.EntityData, playMode);

                            foreach (var comp in entityView.EntityData.components)
                            {
                                if (comp is _prototype_CardProviderComponentData provider)
                                {
                                    if (provider.providedCards != null)
                                    {
                                        foreach (var c in provider.providedCards)
                                        {
                                            if (c != null && c.IsVisible(targetContext))
                                            {
                                                c.sourceProvider = entityView.EntityData;
                                                result.Add(c);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return result;
        }

    }

}