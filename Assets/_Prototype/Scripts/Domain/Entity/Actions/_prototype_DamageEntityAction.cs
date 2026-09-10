using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace TDG0407._prototype
{

    [Serializable]
    public class _prototype_DamageEntityAction : _prototype_EntityAction
    {

        public _prototype_DamageType damageType = _prototype_DamageType.Physical;
        public _prototype_CoefficientValue[] damageCoefficients;

        public override async UniTask ExecuteAction(
            _prototype_EntityData source,
            IEnumerable<_prototype_EntityData> targets,
            _prototype_IActionParams @params)
        {
            var sourceLife = source as _prototype_LifeData;
            float calculatedDamage = 0f;

            if (damageCoefficients != null && damageCoefficients.Length > 0)
            {
                foreach (var coeff in damageCoefficients)
                {
                    float statValue = 1f;
                    if (sourceLife != null)
                    {
                        switch (coeff.stat)
                        {
                            case _prototype_Stat.RedPower:
                                statValue = sourceLife.lifeStat.redPower;
                                break;
                            case _prototype_Stat.BluePower:
                                statValue = sourceLife.lifeStat.bluePower;
                                break;
                            case _prototype_Stat.Health: statValue = sourceLife.health.Current; break;
                            case _prototype_Stat.Stamina: statValue = sourceLife.stamina.Current; break;
                            default: break; // fallback
                        }
                    }
                    calculatedDamage += statValue * coeff.coefficient;
                }
            }
            int baseDamage = (int)calculatedDamage;

            List<UniTask> tasks = new();

            var sourceView = _prototype_GridManager.Instance?.GetPointView(source.point)?.PlacedEntityViews.Find(v => v.EntityData == source);
            bool hasFaced = false;

            foreach (var target in targets)
            {
                if (!hasFaced && sourceView != null && target != source)
                {
                    sourceView.FaceTowards(target.point, 0.2f);
                    hasFaced = true;
                }

                var targetLife = target as _prototype_LifeData;
                // 팀킬 방지 (같은 팀끼리는 공격 불가, 단 None은 제외)
                if (sourceLife != null && targetLife != null)
                {
                    if (sourceLife.side != _prototype_Side.None && sourceLife.side == targetLife.side)
                    {
                        continue;
                    }
                }

                // 크리티컬 판정 및 데미지 계산
                bool isCritical = false;
                int currentDamage = baseDamage;
                if (sourceLife != null && sourceLife.lifeStat.criticalProb > .0f)
                {
                    if (UnityEngine.Random.value < sourceLife.lifeStat.criticalProb)
                    {
                        isCritical = true;
                        float critMultiplier = 1.5f;
                        if (sourceLife.lifeStat.criticalWeight > 0)
                        {
                            critMultiplier = 1f + sourceLife.lifeStat.criticalWeight / 100f;
                        }
                        currentDamage = UnityEngine.Mathf.RoundToInt(baseDamage * critMultiplier);
                    }
                }

                var damageContext = new _prototype_DamageContext(
                    source,
                    target,
                    damageType,
                    baseDamage,
                    currentDamage,
                    isCritical
                );
                tasks.Add(_prototype_InteractionManager.ApplyDamage(damageContext));
            }

            await UniTask.WhenAll(tasks);
        }

    }

}