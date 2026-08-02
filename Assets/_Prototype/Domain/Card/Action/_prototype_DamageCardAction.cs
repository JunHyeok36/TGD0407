using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace TDG0407._prototype
{

    [Serializable]
    public class _prototype_DamageCardAction : _prototype_ICardAction
    {

        public _prototype_CoefficientValue[] damageCoefficients;

        public async UniTask ExecuteCardAction(
            _prototype_EntityData source, 
            IEnumerable<_prototype_EntityData> targets,
            _prototype_CardActionParams @params)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (targets == null) throw new ArgumentNullException(nameof(targets));

            foreach (var target in targets)
            {
                if (target == null) continue;
                int baseDamage = EvaluateDamage(source);
                _prototype_DamageContext context = new(source, target, baseDamage);
                await _prototype_InteractionManager.ApplyDamage(context);
            }
        }

        private int EvaluateDamage(_prototype_EntityData source)
        {
            if (damageCoefficients == null || damageCoefficients.Length == 0) return 0;

            float total = 0f;
            for (int i = 0; i < damageCoefficients.Length; i++)
            {
                _prototype_CoefficientValue coefficientValue = damageCoefficients[i];
                if (coefficientValue == null) continue;

                int statValue = GetStatValue(source, coefficientValue.stat);
                total += statValue * coefficientValue.coefficient;
            }

            return Mathf.Max(0, Mathf.RoundToInt(total));
        }

        private static int GetStatValue(_prototype_EntityData source, _prototype_Stat stat)
        {
            return stat switch
            {
                _prototype_Stat.Health => source.health.Current,
                _prototype_Stat.Stamina => source.stamina.Current,
                _ => 0,
            };
        }

    }

}