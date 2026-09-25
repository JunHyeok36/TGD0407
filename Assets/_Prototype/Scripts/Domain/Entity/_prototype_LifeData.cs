using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using TDG0407.Domain;
using UnityEngine;

namespace TDG0407._prototype
{

    [Serializable]
    public class _prototype_LifeData : _prototype_EntityData
    {

        public _prototype_Side side = _prototype_Side.None;
        public _prototype_CardDeck cardDeck = new();
        public _prototype_LifeStat lifeStat = new();
        public List<_prototype_StatusEffect> statusEffects = new();
        public List<_prototype_ShieldData> shields = new();
        public int CurrentShield => shields != null ? shields.Where(s => s != null && !s.IsExpired).Sum(s => s.amount) : 0;
        public _prototype_IInventoryData inventory = null;

        [SerializeReference, SubclassSelector] public _prototype_EnemyAILogic aiLogic;

        // ─── 패시브 시스템 ──────────────────────────────────────────────────────
        /// <summary>직렬화용 패시브 ID 목록 (저장/복원에 사용)</summary>
        public List<string> passiveIds = new();

        /// <summary>런타임 선택 패시브 인스턴스 목록 (비직렬화)</summary>
        [NonSerialized] private List<_prototype_LifePassiveData> _passives = new();
        public IReadOnlyList<_prototype_LifePassiveData> Passives => _passives;

        /// <summary>고유 패시브 (항상 활성, 직렬화 없음, LifeDataModel에서 설정)</summary>
        [NonSerialized] public _prototype_LifePassiveData uniquePassive = null;

        /// <summary>패시브 ID 목록 → 인스턴스 목록 복원 (씬 로드 후 호출)</summary>
        public void RestorePassives()
        {
            _passives = _passives ?? new List<_prototype_LifePassiveData>();
            _passives.Clear();
            if (passiveIds == null) return;
            foreach (var id in passiveIds)
            {
                var p = _prototype_LifePassiveRegistry.Create(id);
                if (p != null) _passives.Add(p);
            }
        }

        /// <summary>패시브를 획득합니다. 이미 보유하고 있으면 false를 반환합니다.</summary>
        public bool AddPassive(string id)
        {
            if (string.IsNullOrEmpty(id)) return false;
            if (passiveIds != null && passiveIds.Contains(id)) return false;
            var p = _prototype_LifePassiveRegistry.Create(id);
            if (p == null) return false;
            passiveIds = passiveIds ?? new List<string>();
            _passives = _passives ?? new List<_prototype_LifePassiveData>();
            passiveIds.Add(id);
            _passives.Add(p);
            return true;
        }

        public bool HasPassive(string id) => passiveIds != null && passiveIds.Contains(id);

        public IEnumerable<StatusDisplayData> GetDisplayStatuses(_prototype_StatusVisualDatabase db)
        {
            // 1. 일반 상태 이상 
            var groups = statusEffects.GroupBy(s => s.type);
            foreach (var group in groups)
            {
                var type = group.Key;
                int count = group.Count();
                var tickBased = group.Where(s => s.duration.TickDurationType == TickDurationType.TickBased && s.duration.Value != null).ToList();
                int maxTicks = tickBased.Count > 0 ? tickBased.Max(s => s.duration.Value.Current) : 0;
                bool isForever = group.Any(s => s.duration.TickDurationType == TickDurationType.Forever);

                string sym = "?";
                Color color = Color.white;
                Sprite icon = null;
                string dName = type.ToString();
                string dDesc = "";

                if (db != null)
                {
                    var entry = db.GetEntry(type);
                    if (entry != null)
                    {
                        sym = entry.symbolChar;
                        color = entry.themeColor;
                        icon = entry.icon;
                        dName = !string.IsNullOrEmpty(entry.displayName) ? entry.displayName : type.ToString();
                        dDesc = entry.description;
                    }
                    else
                    {
                        var def = _prototype_StatusVisualDatabase.GetDefaultEntry(type);
                        sym = def.symbol; color = def.color; dName = def.name; dDesc = def.desc;
                    }
                }
                else
                {
                    var def = _prototype_StatusVisualDatabase.GetDefaultEntry(type);
                    sym = def.symbol; color = def.color; dName = def.name; dDesc = def.desc;
                }

                yield return new StatusDisplayData
                {
                    id = type.ToString(),
                    displayName = dName,
                    symbolChar = sym,
                    icon = icon,
                    themeColor = color,
                    description = dDesc,
                    stackCount = count,
                    durationTicks = maxTicks,
                    isForever = isForever
                };
            }

            // 2. 패시브
            if (uniquePassive != null)
            {
                foreach (var d in uniquePassive.GetDisplayStatuses(db)) yield return d;
            }
            if (_passives != null)
            {
                foreach (var p in _passives)
                {
                    foreach (var d in p.GetDisplayStatuses(db)) yield return d;
                }
            }
        }

        // ─── 패시브 훅 내부 호출 헬퍼 ──────────────────────────────────────────
        internal void FirePassiveOnTick() => _ = FirePassiveOnTickAsync();
        private async UniTask FirePassiveOnTickAsync()
        {
            if (uniquePassive != null) await uniquePassive.OnTick(this);
            if (_passives != null)
                foreach (var p in _passives) await p.OnTick(this);
        }

        internal void FirePassiveOnDamageDealt(_prototype_DamageContext ctx)
        {
            uniquePassive?.OnDamageDealt(this, ctx);
            if (_passives != null) foreach (var p in _passives) p.OnDamageDealt(this, ctx);
        }

        internal void FirePassiveOnDamageReceived(_prototype_DamageContext ctx)
        {
            uniquePassive?.OnDamageReceived(this, ctx);
            if (_passives != null) foreach (var p in _passives) p.OnDamageReceived(this, ctx);
        }

        internal void FirePassiveOnKill(_prototype_EntityData killed)
        {
            uniquePassive?.OnKillConfirmed(this, killed);
            if (_passives != null) foreach (var p in _passives) p.OnKillConfirmed(this, killed);
        }

        internal void FirePassiveOnCardUsed(_prototype_BattleCardData card)
        {
            uniquePassive?.OnCardUsed(this, card);
            if (_passives != null) foreach (var p in _passives) p.OnCardUsed(this, card);
        }

        /// <summary>
        /// 카드 비용 오버라이드를 질의합니다.
        /// 고유 패시브 → 선택 패시브 순으로 확인하며, 처음 0 이상 값을 반환한 패시브의 값을 사용합니다.
        /// 모두 -1이면 -1 반환 (오버라이드 없음).
        /// </summary>
        internal int QueryPassiveCostOverride(_prototype_BattleCardData card)
        {
            if (uniquePassive != null)
            {
                int ov = uniquePassive.OnBeforeCardUse(this, card);
                if (ov >= 0) return ov;
            }
            if (_passives != null)
                foreach (var p in _passives)
                {
                    int ov = p.OnBeforeCardUse(this, card);
                    if (ov >= 0) return ov;
                }
            return -1;
        }

        public _prototype_LifeData() : base() { }

        public _prototype_LifeData(
            string name,
            _prototype_BoundedValue<int> health,
            _prototype_BoundedValue<int> stamina,
            _prototype_Point point,
            _prototype_CardDeck cardDeck,
            _prototype_Side side = _prototype_Side.None,
            _prototype_IInventoryData inventory = null)
            : base(name, health, stamina, point)
        {
            this.cardDeck = cardDeck;
            this.side = side;
            this.lifeStat = new _prototype_LifeStat();
            this.statusEffects = new List<_prototype_StatusEffect>();
            this.inventory = inventory;
        }
        public _prototype_LifeData(_prototype_LifeData other)
            : base(other)
        {
            this.cardDeck = new _prototype_CardDeck(other.cardDeck);
            this.side = other.side;
            this.lifeStat = new _prototype_LifeStat(other.lifeStat);
            this.statusEffects = new List<_prototype_StatusEffect>(other.statusEffects);
            this.inventory = other.inventory?.Clone();
        }

        [NonSerialized] public bool isDead = false;
        [NonSerialized] private int _baseMaxHealth = -1;
        [NonSerialized] private int _baseMaxStamina = -1;

        public int BaseMaxHealth => _baseMaxHealth > 0 ? _baseMaxHealth : health.Max;
        public int BaseMaxStamina => _baseMaxStamina > 0 ? _baseMaxStamina : stamina.Max;

        public bool CanEnterDeathsDoor => lifeStat != null && lifeStat.deathResistProp > 0f;
        public bool IsAtDeathsDoor => HasStatusEffect(_prototype_StatusType.DeathsDoor);

        public override bool IsDead
        {
            get
            {
                if (isDead) return true;
                if (CanEnterDeathsDoor) return false;
                return health.Current <= 0;
            }
        }

        public override void Die(_prototype_EntityData killer = null)
        {
            isDead = true;
            base.Die(killer);

            // 킬러의 패시브 OnKillConfirmed 훅 호출
            if (killer is _prototype_LifeData killerLife)
            {
                killerLife.FirePassiveOnKill(this);
            }
        }

        public int DeathsDoorStack
        {
            get
            {
                var dd = GetStatusEffect(_prototype_StatusType.DeathsDoor);
                return dd != null ? (int)dd.value : 0;
            }
        }

        public float GetEffectiveDeathResistProp()
        {
            if (lifeStat == null || lifeStat.deathResistProp <= 0f) return 0f;
            int stack = DeathsDoorStack;
            if (stack <= 0) return lifeStat.deathResistProp;
            // 스택당 -0.08f (-8%p), 5스택 시 최소 0.10f (10%) 캡 보장
            return Mathf.Max(0.10f, lifeStat.deathResistProp - (stack * 0.08f));
        }

        public float GetEffectiveDeathResist() => GetEffectiveDeathResistProp();

        // ─── Speed 및 행동 횟수(Action) 관리 ──────────────────────────────
        [NonSerialized] private int _speedModifier = 0;
        [NonSerialized] public int remainingActions = 1;
        [NonSerialized] private bool _actionsInitialized = false;

        public int Speed => Mathf.Max(0, (lifeStat != null ? lifeStat.speed : 1) + _speedModifier);

        public void ResetActions()
        {
            remainingActions = Speed;
            _actionsInitialized = true;
        }

        public bool ConsumeAction(int count = 1)
        {
            if (!_actionsInitialized) ResetActions();
            remainingActions = Mathf.Max(0, remainingActions - count);
            return remainingActions <= 0;
        }

        public void AddSpeedModifier(int mod)
        {
            _speedModifier += mod;
        }

        public void RemoveSpeedModifier(int mod)
        {
            _speedModifier -= mod;
        }

        public int RedPower
        {
            get
            {
                int basePower = lifeStat != null ? lifeStat.redPower : 0;
                int stack = DeathsDoorStack;
                if (stack <= 0) return basePower;
                // 스택당 -12% (최대 -60%), 최소 25% 및 1 보장
                float ratio = Mathf.Max(0.25f, 1f - stack * 0.12f);
                return Mathf.Max(1, Mathf.RoundToInt(basePower * ratio));
            }
        }

        public int BluePower
        {
            get
            {
                int basePower = lifeStat != null ? lifeStat.bluePower : 0;
                int stack = DeathsDoorStack;
                if (stack <= 0) return basePower;
                float ratio = Mathf.Max(0.25f, 1f - stack * 0.12f);
                return Mathf.Max(0, Mathf.RoundToInt(basePower * ratio));
            }
        }

        public event Action<string> OnChannelCancelRequested;
        public void CancelChanneling(string reason = "CC") => OnChannelCancelRequested?.Invoke(reason);

        public override UniTask<int> TakeDamage(_prototype_DamageContext context)
        {
            if (IsDead) return UniTask.FromResult(0);

            // 무적 (Invincible) 상태 체크: 모든 피해 0 무효화 (CC/디버프는 정상 적용)
            if (HasStatusEffect(_prototype_StatusType.Invincible))
            {
                context.modifiedDamage = 0;
                context.finalDamage = 0;
                var pointView = _prototype_GridManager.Instance != null ? _prototype_GridManager.Instance.GetPointView(point) : null;
                if (pointView != null)
                {
                    _prototype_FloatingText.Spawn(pointView.transform.position, "무적!", new Color(1f, 0.95f, 0.4f), 1.15f, 1.2f, 2.2f);
                }
                return UniTask.FromResult(health.Current);
            }

            float damage = context.modifiedDamage;
            float resist = 0f;

            if (context.damageType == _prototype_DamageType.Physical)
                resist = lifeStat.redResist;
            else if (context.damageType == _prototype_DamageType.Magical)
                resist = lifeStat.blueResist;

            // 데미지 감소 비율 계산 (value / (100 + value))
            float damageReductionRatio = 0f;
            if (resist > 0)
                damageReductionRatio = resist / (100f + resist);
            else if (resist < 0)
                damageReductionRatio = resist / (100f - resist);

            // 그로기 상태 체크: 받는 피해 50% 증가
            if (HasStatusEffect(_prototype_StatusType.Groggy))
            {
                damage *= 1.5f;
            }

            // 데미지 적용
            int finalDamage = Mathf.Max(0, Mathf.RoundToInt(damage * (1f - damageReductionRatio)));

            context.modifiedDamage = finalDamage;
            context.finalDamage = finalDamage;

            if (finalDamage <= 0) return UniTask.FromResult(health.Current);

            // ─── 보호막(Shield) 피해 우선 흡수 ───────────────────────────────
            int hpDamage = finalDamage;
            if (CurrentShield > 0)
            {
                int absorbedByShield = AbsorbDamageWithShield(finalDamage);
                hpDamage = Mathf.Max(0, finalDamage - absorbedByShield);
            }

            // ─── 체력 피해 적용 ──────────────────────────────────────────────
            if (hpDamage > 0)
            {
                if (CanEnterDeathsDoor)
                {
                    if (health.Current <= 0 || IsAtDeathsDoor)
                    {
                        // 이미 죽음의 문턱(HP 0)인 상태에서 추가 피격 -> 사망 굴림 (Deathblow Check)
                        health.Current = 0;
                        float deathResistProp = GetEffectiveDeathResistProp();

                        if (UnityEngine.Random.value < deathResistProp)
                        {
                            // 죽음 저항 성공! 생존 & 디버프 스택 증가
                            AddDeathsDoorStack();
                            _prototype_EventBus.Fire(new EntityDeathResistedEvent(this, deathResistProp, DeathsDoorStack));
                        }
                        else
                        {
                            // 죽음 저항 실패! 최종 사망
                            Die(context.source);
                        }
                    }
                    else
                    {
                        // 아직 HP가 남아있는 상태
                        if (health.Current - hpDamage <= 0)
                        {
                            // 이번 타격으로 처음 HP 0 도달 -> 죽음의 문턱 진입 (사망 유예)
                            health.Current = 0;
                            AddDeathsDoorStack();
                            _prototype_EventBus.Fire(new EntityDeathsDoorEnteredEvent(this));
                        }
                        else
                        {
                            health.Current -= hpDamage;
                        }
                    }
                }
                else
                {
                    // 일반 엔티티: HP 0 이하 시 즉시 사망
                    health.Current -= hpDamage;
                    if (health.Current <= 0)
                    {
                        Die(context.source);
                    }
                }
            }

            // ─── 스테미나(체간) 감소 및 그로기 체크 ──────────────────────────
            // [중요 게임 디자인 규칙]: 보호막에 의해 모든 피해가 흡수되어 HP 피해가 0인 경우, SP 피해를 전혀 받지 않음!
            if (!IsDead && stamina != null && stamina.Max > 0 && hpDamage > 0)
            {
                int baseSpDamage = Mathf.RoundToInt(hpDamage * context.spDamageMultiplier);
                if (baseSpDamage > 0)
                {
                    float poise = lifeStat.poise;
                    float poiseReductionRatio = 0f;
                    if (poise > 0)
                        poiseReductionRatio = poise / (100f + poise);
                    else if (poise < 0)
                        poiseReductionRatio = poise / (100f - poise);

                    int spDamage = Mathf.Max(0, Mathf.RoundToInt(baseSpDamage * (1f - poiseReductionRatio)));
                    context.spDamage = spDamage;

                    if (spDamage > 0)
                    {
                        stamina.Current -= spDamage;
                        CheckStaminaForGroggy();
                    }
                }
            }
            else
            {
                context.spDamage = 0;
            }

            // ─── 패시브 훅: 공격자 OnDamageDealt / 피격자 OnDamageReceived ──
            if (context.finalDamage.HasValue && context.finalDamage.Value > 0)
            {
                if (context.source is _prototype_LifeData sourceLife)
                    sourceLife.FirePassiveOnDamageDealt(context);
                FirePassiveOnDamageReceived(context);
            }

            return UniTask.FromResult(health.Current);
        }


        public void CheckStaminaForGroggy()
        {
            if (!IsDead && stamina != null && stamina.Current <= 0)
            {
                ApplyStatusEffect(new _prototype_StatusEffect(_prototype_StatusType.Groggy, 1));
            }
        }

        public void RecoverHealth(int amount)
        {
            var poisoning = statusEffects.Find(s => s.type == _prototype_StatusType.Poisoning);
            if (poisoning != null)
            {
                amount = Mathf.RoundToInt(amount * 0.5f);
            }
            health.Current += amount;
            CheckClearDeathsDoor();
        }

        public void ApplyStatusEffect(_prototype_StatusEffect effect)
        {
            if (effect == null || effect.type == _prototype_StatusType.None) return;

            bool isCC = effect.type == _prototype_StatusType.Stun ||
                        effect.type == _prototype_StatusType.Groggy ||
                        effect.type == _prototype_StatusType.Silence ||
                        effect.type == _prototype_StatusType.Fear ||
                        effect.type == _prototype_StatusType.Freeze ||
                        effect.type == _prototype_StatusType.Airborne ||
                        effect.type == _prototype_StatusType.Provocation;

            if (isCC && HasStatusEffect(_prototype_StatusType.Unstoppable))
            {
                // Unstoppable blocks all CC
                return;
            }

            float resist = 0f;
            switch (effect.type)
            {
                case _prototype_StatusType.Stun: resist = lifeStat.stunResist; break;
                case _prototype_StatusType.Groggy: resist = lifeStat.groggyResist; break;
                case _prototype_StatusType.Silence: resist = lifeStat.silenceResist; break;
                case _prototype_StatusType.Fear: resist = lifeStat.fearResist; break;
                case _prototype_StatusType.Curse: resist = GetCurseResist(); break;
                case _prototype_StatusType.Bleeding: resist = lifeStat.bleedingResist; break;
                case _prototype_StatusType.Burning: resist = lifeStat.burningResist; break;
                case _prototype_StatusType.Freeze: resist = lifeStat.freezeResist; break;
                case _prototype_StatusType.Poisoning: resist = lifeStat.poisoningResist; break;
                case _prototype_StatusType.Provocation: resist = lifeStat.provocationResist; break;
                case _prototype_StatusType.Airborne: resist = lifeStat.airborneResist; break;
            }

            float resistProb = resist > 0 ? resist / (resist + 100f) : 0f;
            if (UnityEngine.Random.value < resistProb)
            {
                // Resisted
                return;
            }

            // Hard CC cancels channeling
            if (effect.type == _prototype_StatusType.Stun ||
                effect.type == _prototype_StatusType.Airborne ||
                effect.type == _prototype_StatusType.Silence ||
                effect.type == _prototype_StatusType.Groggy)
            {
                CancelChanneling("CC_" + effect.type);
            }

            // 1. Check mutually exclusive Burning vs Freeze (All Clear on conflict)
            if (effect.type == _prototype_StatusType.Burning)
            {
                var freezes = statusEffects.FindAll(s => s.type == _prototype_StatusType.Freeze);
                if (freezes.Count > 0)
                {
                    foreach (var f in freezes)
                    {
                        statusEffects.Remove(f);
                        _prototype_EventBus.Fire(new EntityStatusChangedEvent(this, f, false));
                    }
                    return; // Cancel each other out
                }
            }
            else if (effect.type == _prototype_StatusType.Freeze)
            {
                var burnings = statusEffects.FindAll(s => s.type == _prototype_StatusType.Burning);
                if (burnings.Count > 0)
                {
                    foreach (var b in burnings)
                    {
                        statusEffects.Remove(b);
                        _prototype_EventBus.Fire(new EntityStatusChangedEvent(this, b, false));
                    }
                    return; // Cancel each other out
                }
            }

            // 2. Crowd Control & Buffs (Stun, Groggy, Silence, Fear, Freeze, Airborne, Provocation, Unstoppable, Invincible) - Single instance, Max duration
            if (effect.type == _prototype_StatusType.Stun ||
                effect.type == _prototype_StatusType.Groggy ||
                effect.type == _prototype_StatusType.Silence ||
                effect.type == _prototype_StatusType.Fear ||
                effect.type == _prototype_StatusType.Freeze ||
                effect.type == _prototype_StatusType.Airborne ||
                effect.type == _prototype_StatusType.Provocation ||
                effect.type == _prototype_StatusType.Unstoppable ||
                effect.type == _prototype_StatusType.Invincible ||
                effect.type == _prototype_StatusType.EnhanceStab)
            {
                var existing = statusEffects.Find(s => s.type == effect.type);
                if (existing != null)
                {
                    if (existing.duration.TickDurationType == TickDurationType.Forever || effect.duration.TickDurationType == TickDurationType.Forever)
                    {
                        existing.duration = new TickDuration(TickDurationType.Forever);
                    }
                    else
                    {
                        int maxTicks = Mathf.Max(existing.durationTicks, effect.durationTicks);
                        existing.duration = new TickDuration(TickDurationType.TickBased, maxTicks);
                    }
                    existing.appliedTick = _prototype_TickManager.CurrentTick;
                    if (effect.type == _prototype_StatusType.Provocation && effect.sourceEntity != null)
                    {
                        existing.sourceEntity = effect.sourceEntity;
                    }
                    _prototype_EventBus.Fire(new EntityStatusChangedEvent(this, existing, true));
                    return;
                }

                statusEffects.Add(effect);
                _prototype_EventBus.Fire(new EntityStatusChangedEvent(this, effect, true));
                return;
            }

            // 3. Stat debuff (Curse) - Single instance, Additive value + Max duration
            if (effect.type == _prototype_StatusType.Curse)
            {
                var existing = statusEffects.Find(s => s.type == _prototype_StatusType.Curse);
                if (existing != null)
                {
                    existing.value += effect.value;
                    if (existing.duration.TickDurationType == TickDurationType.Forever || effect.duration.TickDurationType == TickDurationType.Forever)
                    {
                        existing.duration = new TickDuration(TickDurationType.Forever);
                    }
                    else
                    {
                        int maxTicks = Mathf.Max(existing.durationTicks, effect.durationTicks);
                        existing.duration = new TickDuration(TickDurationType.TickBased, maxTicks);
                    }
                    existing.appliedTick = _prototype_TickManager.CurrentTick;
                    _prototype_EventBus.Fire(new EntityStatusChangedEvent(this, existing, true));
                    return;
                }


                statusEffects.Add(effect);
                _prototype_EventBus.Fire(new EntityStatusChangedEvent(this, effect, true));
                return;
            }

            // 4. DoTs (Bleeding, Burning, Poisoning) - Independent instances
            statusEffects.Add(effect);
            _prototype_EventBus.Fire(new EntityStatusChangedEvent(this, effect, true));
        }

        public float GetCurseResist()
        {
            float resist = lifeStat.curseResist;
            foreach (var s in statusEffects)
            {
                if (s.type == _prototype_StatusType.Curse) resist -= s.value;
            }
            return resist;
        }

        public bool HasStatusEffect(_prototype_StatusType type)
        {
            return statusEffects.Find(s => s.type == type) != null;
        }

        public _prototype_StatusEffect GetStatusEffect(_prototype_StatusType type)
        {
            return statusEffects.Find(s => s.type == type);
        }

        public bool RemoveStatusEffect(_prototype_StatusType type)
        {
            var effect = statusEffects.Find(s => s.type == type);
            if (effect != null)
            {
                statusEffects.Remove(effect);
                _prototype_EventBus.Fire(new EntityStatusChangedEvent(this, effect, false));
                return true;
            }
            return false;
        }

        #region Inventory Helpers

        public bool HasItem(string itemId, int quantity = 1)
        {
            return inventory != null && inventory.HasItem(itemId, quantity);
        }

        public int AddItem(_prototype_ItemDataModel itemModel, int quantity)
        {
            return inventory != null ? inventory.AddItem(itemModel, quantity) : 0;
        }

        public bool ConsumeItem(string itemId, int quantity)
        {
            return inventory != null && inventory.ConsumeItem(itemId, quantity);
        }

        #endregion

        #region Rest & Recovery

        /// <summary>
        /// 대기(휴식) 시 스태미나를 회복합니다. (방안 A: 대기 시 체력은 회복하지 않고 스태미나만 회복)
        /// </summary>
        /// <returns>실제 회복된 스태미나 양</returns>
        public virtual int Rest()
        {
            int recoverAmount = lifeStat != null && lifeStat.staminaRecoverAmount > 0 
                ? lifeStat.staminaRecoverAmount 
                : 3; // 에셋 기본값 미설정 시 안전 기본값(3) 적용

            // DeathsDoor 디버프: 스택당 -15%, 최소 1 보장
            int stack = DeathsDoorStack;
            if (stack > 0)
            {
                float ratio = Mathf.Max(0.25f, 1f - stack * 0.15f);
                recoverAmount = Mathf.Max(1, Mathf.RoundToInt(recoverAmount * ratio));
            }

            int prev = stamina.Current;
            stamina.Current = Mathf.Min(stamina.Max, stamina.Current + recoverAmount);
            return stamina.Current - prev;
        }

        /// <summary>
        /// 체력을 회복합니다. 최대 체력을 초과하지 않습니다. (물약, 치유 카드, 쉼터 등에서 사용)
        /// </summary>
        /// <returns>실제 회복된 체력 양</returns>
        public virtual int Heal(int amount)
        {
            if (amount <= 0) return 0;
            int prev = health.Current;
            health.Current = Mathf.Min(health.Max, health.Current + amount);
            CheckClearDeathsDoor();
            return health.Current - prev;
        }

        #region Shield Management

        /// <summary>
        /// 엔티티에 보호막을 부여합니다.
        /// </summary>
        /// <param name="amount">보호막 수치</param>
        /// <param name="duration">지속 시간 (TickDuration)</param>
        public void AddShield(int amount, TickDuration duration)
        {
            if (amount <= 0) return;
            shields = shields ?? new List<_prototype_ShieldData>();
            shields.Add(new _prototype_ShieldData(amount, duration));
            _prototype_EventBus.Fire(new EntityShieldChangedEvent(this, CurrentShield));
        }

        /// <summary>
        /// 엔티티에 보호막을 부여합니다.
        /// </summary>
        /// <param name="amount">보호막 수치</param>
        /// <param name="durationTicks">지속 틱 수 (-1이면 영구)</param>
        public void AddShield(int amount, int durationTicks = -1)
        {
            AddShield(amount, durationTicks < 0 ? new TickDuration(TickDurationType.Forever) : new TickDuration(TickDurationType.TickBased, durationTicks));
        }

        /// <summary>
        /// 들어온 피해를 보호막으로 우선 흡수합니다.
        /// </summary>
        /// <returns>보호막에 의해 실제 흡수된 피해량</returns>
        public int AbsorbDamageWithShield(int damage)
        {
            if (damage <= 0 || shields == null || shields.Count == 0) return 0;

            int remainingDamage = damage;
            int totalAbsorbed = 0;

            for (int i = 0; i < shields.Count; i++)
            {
                var s = shields[i];
                if (s == null || s.IsExpired) continue;

                if (s.amount <= remainingDamage)
                {
                    totalAbsorbed += s.amount;
                    remainingDamage -= s.amount;
                    s.amount = 0;
                }
                else
                {
                    totalAbsorbed += remainingDamage;
                    s.amount -= remainingDamage;
                    remainingDamage = 0;
                    break;
                }
            }

            shields.RemoveAll(s => s == null || s.IsExpired);
            _prototype_EventBus.Fire(new EntityShieldChangedEvent(this, CurrentShield));
            return totalAbsorbed;
        }

        /// <summary>
        /// 틱 경과에 따른 보호막 지속시간 감소 및 만료 처리.
        /// </summary>
        public void TickShields()
        {
            if (shields == null || shields.Count == 0) return;

            bool changed = false;
            for (int i = 0; i < shields.Count; i++)
            {
                var s = shields[i];
                if (s == null) continue;
                if (s.duration.TickDurationType == TickDurationType.TickBased && !s.IsExpired)
                {
                    s.duration.OnTick();
                    changed = true;
                }
            }

            int removed = shields.RemoveAll(s => s == null || s.IsExpired);
            if (changed || removed > 0)
            {
                _prototype_EventBus.Fire(new EntityShieldChangedEvent(this, CurrentShield));
            }
        }

        #endregion

        #endregion

        #region Death's Door Mechanics

        public void AddDeathsDoorStack()
        {
            if (_baseMaxHealth <= 0) _baseMaxHealth = health.Max;
            if (_baseMaxStamina <= 0) _baseMaxStamina = stamina.Max;

            var dd = GetStatusEffect(_prototype_StatusType.DeathsDoor);
            int currentStack = dd != null ? (int)dd.value : 0;
            int newStack = Mathf.Clamp(currentStack + 1, 1, 5);

            if (dd != null)
            {
                dd.value = newStack;
                _prototype_EventBus.Fire(new EntityStatusChangedEvent(this, dd, true));
            }
            else
            {
                dd = new _prototype_StatusEffect(_prototype_StatusType.DeathsDoor, new TickDuration(TickDurationType.Forever), null, this, newStack);
                statusEffects.Add(dd);
                _prototype_EventBus.Fire(new EntityStatusChangedEvent(this, dd, true));
            }

            ApplyDeathsDoorDebuffStats(newStack);
        }

        private void ApplyDeathsDoorDebuffStats(int stack)
        {
            if (stack <= 0) return;

            health.Max = Mathf.Max(1, Mathf.RoundToInt(BaseMaxHealth * Mathf.Max(0.5f, 1f - stack * 0.10f)));
            stamina.Max = Mathf.Max(1, Mathf.RoundToInt(BaseMaxStamina * Mathf.Max(0.75f, 1f - stack * 0.15f)));
        }

        public void CheckClearDeathsDoor()
        {
            if (IsAtDeathsDoor && health.Current >= Mathf.RoundToInt(BaseMaxHealth * 0.5f))
            {
                RemoveDeathsDoor();
            }
        }

        public void RemoveDeathsDoor()
        {
            var dd = GetStatusEffect(_prototype_StatusType.DeathsDoor);
            if (dd != null)
            {
                statusEffects.Remove(dd);
                _prototype_EventBus.Fire(new EntityStatusChangedEvent(this, dd, false));
                _prototype_EventBus.Fire(new EntityDeathsDoorClearedEvent(this));
            }

            // 스탯 원복
            if (_baseMaxHealth > 0)
            {
                health.Max = _baseMaxHealth;
                _baseMaxHealth = -1;
            }
            if (_baseMaxStamina > 0)
            {
                stamina.Max = _baseMaxStamina;
                _baseMaxStamina = -1;
            }
        }

        #endregion
    }

}
