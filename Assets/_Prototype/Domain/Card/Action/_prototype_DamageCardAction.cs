using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace TDG0407._prototype
{

    [Serializable]
    public class _prototype_DamageCardAction : _prototype_CardAction
    {

        public _prototype_CoefficientValue[] damageCoefficients;

        public override async UniTask ExecuteCardAction(
            _prototype_EntityData source,
            IEnumerable<_prototype_EntityData> targets,
            _prototype_ICardActionParams @params)
        {
            var sourceLife = source as _prototype_LifeData;
            float calculatedDamage = 0f;
            _prototype_DamageType damageType = _prototype_DamageType.Physical;

            if (damageCoefficients != null && damageCoefficients.Length > 0)
            {
                foreach (var coeff in damageCoefficients)
                {
                    float statValue = 1f;
                    if (sourceLife != null)
                    {
                        switch (coeff.stat)
                        {
                            case _prototype_Stat.RedPower: statValue = sourceLife.lifeStat.redPower; damageType = _prototype_DamageType.Physical; break;
                            case _prototype_Stat.BluePower: statValue = sourceLife.lifeStat.bluePower; damageType = _prototype_DamageType.Magical; break;
                            case _prototype_Stat.Health: statValue = sourceLife.health.Current; break;
                            case _prototype_Stat.Stamina: statValue = sourceLife.stamina.Current; break;
                            default: break; // fallback
                        }
                    }
                    calculatedDamage += statValue * coeff.coefficient;
                }
            }
            int finalDamage = (int)calculatedDamage;

            List<UniTask> tasks = new();

            foreach (var target in targets)
            {
                var targetLife = target as _prototype_LifeData;

                // 팀킬 방지 (같은 팀끼리는 공격 불가, 단 None은 제외)
                if (sourceLife != null && targetLife != null)
                {
                    if (sourceLife.side != _prototype_Side.None && sourceLife.side == targetLife.side)
                    {
                        continue;
                    }
                }

                var damageContext = new _prototype_DamageContext(
                    source,
                    target,
                    damageType,
                    finalDamage,
                    finalDamage
                );
                tasks.Add(_prototype_InteractionManager.ApplyDamage(damageContext));
            }

            await UniTask.WhenAll(tasks);
        }

    }

}