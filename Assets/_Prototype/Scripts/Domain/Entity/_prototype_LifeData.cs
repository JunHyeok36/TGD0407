using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
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
        public _prototype_IInventoryData inventory = null;

        [SerializeReference, SubclassSelector] public _prototype_EnemyAILogic aiLogic;

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

        public override UniTask<int> TakeDamage(_prototype_DamageContext context)
        {
            if (IsDead) return UniTask.FromResult(0);

            float damage = context.modifiedDamage;
            float resist = 0f;

            if (context.damageType == _prototype_DamageType.Physical)
                resist = lifeStat.redResist;
            else if (context.damageType == _prototype_DamageType.Magical)
                resist = lifeStat.blueResist;

            // 데미지 감소 비율 계산 (value / (50 + value))
            float damageReductionRatio = 0f;
            if (resist > 0)
                damageReductionRatio = resist / (50f + resist);
            else if (resist < 0)
                damageReductionRatio = resist / (50f - resist);

            // 데미지 적용
            int finalDamage = Mathf.Max(0, Mathf.RoundToInt(damage * (1f - damageReductionRatio)));

            context.modifiedDamage = finalDamage;
            context.finalDamage = finalDamage;

            if (finalDamage <= 0) return UniTask.FromResult(health.Current);

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
                    if (health.Current - finalDamage <= 0)
                    {
                        // 이번 타격으로 처음 HP 0 도달 -> 죽음의 문턱 진입 (사망 유예)
                        health.Current = 0;
                        AddDeathsDoorStack();
                        _prototype_EventBus.Fire(new EntityDeathsDoorEnteredEvent(this));
                    }
                    else
                    {
                        health.Current -= finalDamage;
                    }
                }
            }
            else
            {
                // 일반 엔티티: HP 0 이하 시 즉시 사망
                health.Current -= finalDamage;
                if (health.Current <= 0)
                {
                    Die(context.source);
                }
            }

            return UniTask.FromResult(health.Current);
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

            float resist = 0f;
            switch (effect.type)
            {
                case _prototype_StatusType.Stun: resist = lifeStat.stunResist; break;
                case _prototype_StatusType.Knockdown: resist = lifeStat.knockdownResist; break;
                case _prototype_StatusType.Silence: resist = lifeStat.silenceResist; break;
                case _prototype_StatusType.Fear: resist = lifeStat.fearResist; break;
                case _prototype_StatusType.Curse: resist = GetCurseResist(); break;
                case _prototype_StatusType.Bleeding: resist = lifeStat.bleedingResist; break;
                case _prototype_StatusType.Burning: resist = lifeStat.burningResist; break;
                case _prototype_StatusType.Freeze: resist = lifeStat.freezeResist; break;
                case _prototype_StatusType.Poisoning: resist = lifeStat.poisoningResist; break;
            }

            float resistProb = resist > 0 ? resist / (resist + 100f) : 0f;
            if (UnityEngine.Random.value < resistProb)
            {
                // Resisted
                return;
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

            // 2. Crowd Control (Stun, Knockdown, Silence, Fear, Freeze) & Buff (SuperArmor) - Single instance, Max duration
            if (effect.type == _prototype_StatusType.Stun ||
                effect.type == _prototype_StatusType.Knockdown ||
                effect.type == _prototype_StatusType.Silence ||
                effect.type == _prototype_StatusType.Fear ||
                effect.type == _prototype_StatusType.Freeze ||
                effect.type == _prototype_StatusType.SuperArmor)
            {
                var existing = statusEffects.Find(s => s.type == effect.type);
                if (existing != null)
                {
                    existing.durationTicks = Mathf.Max(existing.durationTicks, effect.durationTicks);
                    existing.appliedTick = _prototype_TickManager.CurrentTick;
                    _prototype_EventBus.Fire(new EntityStatusChangedEvent(this, existing, true));
                    return;
                }

                // Knockdown initial damage (only on first application)
                if (effect.type == _prototype_StatusType.Knockdown)
                {
                    int dmg = Mathf.RoundToInt(health.Max * 0.3f);
                    if (health.Current - dmg <= 0)
                        health.Current = 1;
                    else
                        health.Current -= dmg;
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
                    existing.durationTicks = Mathf.Max(existing.durationTicks, effect.durationTicks);
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
                dd = new _prototype_StatusEffect(_prototype_StatusType.DeathsDoor, int.MaxValue, null, this, newStack);
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
