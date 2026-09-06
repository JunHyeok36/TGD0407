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

        [SerializeReference, SubclassSelector] public _prototype_EnemyAILogic aiLogic;

        public _prototype_LifeData() : base() { }
        public _prototype_LifeData(
            string name,
            _prototype_BoundedValue<int> health, 
            _prototype_BoundedValue<int> stamina, 
            _prototype_Point point, 
            _prototype_CardDeck cardDeck,
            _prototype_Side side = _prototype_Side.None) 
            : base(name, health, stamina, point)
        {
            this.cardDeck = cardDeck;
            this.side = side;
            this.lifeStat = new _prototype_LifeStat();
            this.statusEffects = new List<_prototype_StatusEffect>();
        }
        public _prototype_LifeData(_prototype_LifeData other) 
            : base(other)
        {
            this.cardDeck = new _prototype_CardDeck(other.cardDeck);
            this.side = other.side;
            this.lifeStat = new _prototype_LifeStat(other.lifeStat);
            this.statusEffects = new List<_prototype_StatusEffect>(other.statusEffects);
        }

        public override Cysharp.Threading.Tasks.UniTask<int> TakeDamage(_prototype_DamageContext context)
        {
            float damage = context.baseDamage;
            float resist = 0f;

            if (context.damageType == _prototype_DamageType.Physical)
                resist = lifeStat.redResist;
            else if (context.damageType == _prototype_DamageType.Magical)
                resist = lifeStat.blueResist;

            // 데미지 감소 비율 계산 (value / (100 + value))
            float damageReductionRatio = 0f;
            if (resist > 0)
                damageReductionRatio = resist / (100f + resist);
            
            // 데미지 적용
            int finalDamage = UnityEngine.Mathf.Max(0, UnityEngine.Mathf.RoundToInt(damage * (1f - damageReductionRatio)));
            
            context.modifiedDamage = finalDamage;
            health.Current -= finalDamage;

            return Cysharp.Threading.Tasks.UniTask.FromResult(health.Current);
        }

        public void RecoverHealth(int amount)
        {
            var poisoning = statusEffects.Find(s => s.type == _prototype_StatusType.Poisoning);
            if (poisoning != null)
            {
                amount = UnityEngine.Mathf.RoundToInt(amount * 0.5f);
            }
            health.Current += amount;
        }

        public void ApplyStatusEffect(_prototype_StatusEffect effect)
        {
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

            // Check mutually exclusive Burning/Freeze
            if (effect.type == _prototype_StatusType.Burning)
            {
                var freeze = statusEffects.Find(s => s.type == _prototype_StatusType.Freeze);
                if (freeze != null)
                {
                    statusEffects.Remove(freeze);
                    return; // Cancel each other out
                }
            }
            else if (effect.type == _prototype_StatusType.Freeze)
            {
                var burning = statusEffects.Find(s => s.type == _prototype_StatusType.Burning);
                if (burning != null)
                {
                    statusEffects.Remove(burning);
                    return; // Cancel each other out
                }
            }

            // Knockdown initial damage
            if (effect.type == _prototype_StatusType.Knockdown)
            {
                // check if we don't already have knockdown
                if (statusEffects.Find(s => s.type == _prototype_StatusType.Knockdown) == null)
                {
                    int dmg = UnityEngine.Mathf.RoundToInt(health.Max * 0.3f);
                    if (health.Current - dmg <= 0)
                        health.Current = 1;
                    else
                        health.Current -= dmg;
                }
            }

            statusEffects.Add(effect);
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
    }

}
