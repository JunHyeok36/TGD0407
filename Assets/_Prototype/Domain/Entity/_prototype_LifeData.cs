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
    }

}
