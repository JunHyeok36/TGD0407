using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TDG0407._prototype
{

    [Serializable]
    public class _prototype_DamageEntityAction : _prototype_EntityAction
    {

        public _prototype_DamageType damageType = _prototype_DamageType.Physical;
        public _prototype_CoefficientValue[] damageCoefficients;

        public static float GetStatValue(_prototype_EntityData entity, _prototype_Stat stat)
        {
            if (entity == null) return 0f;
            var life = entity as _prototype_LifeData;
            return stat switch
            {
                _prototype_Stat.RedPower => life != null ? life.RedPower : 0f,
                _prototype_Stat.BluePower => life != null ? life.BluePower : 0f,
                _prototype_Stat.Health => entity.health != null ? entity.health.Max : 0f,
                _prototype_Stat.MaxHealth => entity.health != null ? entity.health.Max : 0f,
                _prototype_Stat.CurrentHealth => entity.health != null ? entity.health.Current : 0f,
                _prototype_Stat.MissingHealth => entity.health != null ? Mathf.Max(0, entity.health.Max - entity.health.Current) : 0f,
                _prototype_Stat.Stamina => entity.stamina != null ? entity.stamina.Current : 0f,
                _prototype_Stat.Shield => 0f,
                _prototype_Stat.Avoidance => (life != null && life.lifeStat != null) ? life.lifeStat.dodgeProb : 0f,
                _prototype_Stat.CriticalProb => (life != null && life.lifeStat != null) ? life.lifeStat.criticalProb : 0f,
                _prototype_Stat.CriticalWeight => (life != null && life.lifeStat != null) ? life.lifeStat.criticalWeight : 0,
                _prototype_Stat.RedResist => (life != null && life.lifeStat != null) ? life.lifeStat.redResist : 0,
                _prototype_Stat.BlueResist => (life != null && life.lifeStat != null) ? life.lifeStat.blueResist : 0,
                _ => 1f
            };
        }

        public override async UniTask ExecuteAction(
            _prototype_EntityData source,
            IEnumerable<_prototype_EntityData> targets,
            _prototype_IActionParams @params)
        {
            if (source == null) return;
            var sourceLife = source as _prototype_LifeData;

            // 1. 시전자(Caster) 및 고정 계수 피해량 사전 계산
            float casterCalculatedDamage = 0f;
            if (damageCoefficients != null && damageCoefficients.Length > 0)
            {
                foreach (var coeff in damageCoefficients)
                {
                    if (coeff == null) continue;
                    if (coeff.source == _prototype_StatSource.Caster)
                    {
                        float statValue = (coeff.stat == _prototype_Stat.None || coeff.stat == _prototype_Stat.NULL)
                            ? 1f
                            : GetStatValue(source, coeff.stat);
                        casterCalculatedDamage += statValue * coeff.coefficient;
                    }
                }
            }
            int baseCasterDamage = Mathf.RoundToInt(casterCalculatedDamage);

            var sourceView = _prototype_GridManager.Instance?.GetPointView(source.point)?.PlacedEntityViews.Find(v => v.EntityData == source);
            var cardParams = @params as _prototype_CardActionParams;

            // 2. 공격 시도(Action Execution)에 따른 공격 모션 및 방향 회전 (피해 유무와 무관하게 실행)
            UniTask attackAnimTask = UniTask.CompletedTask;
            if (cardParams != null && sourceView != null)
            {
                bool hasTargetPoint = false;
                _prototype_Point targetPoint = _prototype_Point.zero;

                if (cardParams.TargetPoints != null && cardParams.TargetPoints.Count > 0)
                {
                    targetPoint = cardParams.TargetPoints[0];
                    hasTargetPoint = true;
                }
                else if (cardParams.Direction != _prototype_Point.zero)
                {
                    targetPoint = source.point + cardParams.Direction;
                    hasTargetPoint = true;
                }
                else if (cardParams.TargetedPoint != source.point)
                {
                    targetPoint = cardParams.TargetedPoint;
                    hasTargetPoint = true;
                }

                if (!hasTargetPoint && targets != null)
                {
                    var firstTarget = targets.FirstOrDefault(t => t != null && t != source);
                    if (firstTarget != null)
                    {
                        targetPoint = firstTarget.point;
                        hasTargetPoint = true;
                    }
                }

                if (hasTargetPoint && targetPoint != source.point)
                {
                    sourceView.FaceTowards(targetPoint, 0.2f);
                }
                else if (cardParams.Direction != _prototype_Point.zero)
                {
                    sourceView.FaceTowards(source.point + cardParams.Direction, 0.2f);
                }

                Vector3 attackDirWorld = Vector3.zero;
                if (hasTargetPoint && targetPoint != source.point)
                {
                    var targetPointView = _prototype_GridManager.Instance?.GetPointView(targetPoint);
                    if (targetPointView != null)
                    {
                        attackDirWorld = targetPointView.transform.position - sourceView.transform.position;
                        attackDirWorld.y = 0;
                    }
                }
                else if (cardParams.Direction != _prototype_Point.zero)
                {
                    var dirPointView = _prototype_GridManager.Instance?.GetPointView(source.point + cardParams.Direction);
                    if (dirPointView != null)
                    {
                        attackDirWorld = dirPointView.transform.position - sourceView.transform.position;
                        attackDirWorld.y = 0;
                    }
                }

                if (attackDirWorld.sqrMagnitude > 0.001f)
                {
                    attackDirWorld.Normalize();
                }
                else
                {
                    attackDirWorld = sourceView.transform.forward;
                    attackDirWorld.y = 0;
                    if (attackDirWorld.sqrMagnitude > 0.001f)
                        attackDirWorld.Normalize();
                    else
                        attackDirWorld = Vector3.forward;
                }

                if (sourceView is _prototype_LifeView lifeView)
                {
                    attackAnimTask = lifeView.PlayUniqueAnimation(_prototype_EntityAnimationType.Attack, attackDirWorld);
                }
            }

            // 3. 피격 대상들에 대한 데미지 적용 (타겟별 계수 합산)
            List<UniTask> damageTasks = new();
            if (targets != null)
            {
                foreach (var target in targets)
                {
                    if (target == null || target == source) continue;

                    // 팀킬 및 아군 투사체 피격 방지 (같은 side를 갖는 엔티티/투사체는 공격 대상에서 제외)
                    if (source.IsSameSide(target))
                    {
                        continue;
                    }

                    // 대상(Target) 기준 계수 추가 계산
                    float targetCalculatedBonus = 0f;
                    if (damageCoefficients != null && damageCoefficients.Length > 0)
                    {
                        foreach (var coeff in damageCoefficients)
                        {
                            if (coeff != null && coeff.source == _prototype_StatSource.Target)
                            {
                                float statValue = (coeff.stat == _prototype_Stat.None || coeff.stat == _prototype_Stat.NULL)
                                    ? 1f
                                    : GetStatValue(target, coeff.stat);
                                targetCalculatedBonus += statValue * coeff.coefficient;
                            }
                        }
                    }

                    int finalBaseDamage = baseCasterDamage + Mathf.RoundToInt(targetCalculatedBonus);

                    // 크리티컬 판정 및 데미지 계산
                    bool isCritical = false;
                    int currentDamage = finalBaseDamage;
                    if (sourceLife != null && sourceLife.lifeStat.criticalProb > .0f)
                    {
                        if (UnityEngine.Random.value < sourceLife.lifeStat.criticalProb)
                        {
                            isCritical = true;
                            float critMultiplier = 1f + sourceLife.lifeStat.criticalWeight / 100f;
                            currentDamage = UnityEngine.Mathf.RoundToInt(finalBaseDamage * critMultiplier);
                        }
                    }

                    var damageContext = new _prototype_DamageContext(
                        source,
                        target,
                        damageType,
                        finalBaseDamage,
                        currentDamage,
                        isCritical
                    );
                    damageTasks.Add(_prototype_InteractionManager.ApplyDamage(damageContext));
                }
            }

            // 4. 공격 모션과 데미지 태스크 모두 완료될 때까지 대기
            if (damageTasks.Count > 0)
            {
                await UniTask.WhenAll(attackAnimTask, UniTask.WhenAll(damageTasks));
            }
            else
            {
                await attackAnimTask;
            }
        }

        public int CalculateEstimatedDamage(_prototype_EntityData source, _prototype_EntityData target)
        {
            float casterTotal = 0f;
            if (damageCoefficients != null && damageCoefficients.Length > 0)
            {
                foreach (var coeff in damageCoefficients)
                {
                    if (coeff == null) continue;
                    if (coeff.source == _prototype_StatSource.Caster)
                    {
                        float statValue = (coeff.stat == _prototype_Stat.None || coeff.stat == _prototype_Stat.NULL)
                            ? 1f
                            : GetStatValue(source, coeff.stat);
                        casterTotal += statValue * coeff.coefficient;
                    }
                    else if (coeff.source == _prototype_StatSource.Target && target != null)
                    {
                        float statValue = (coeff.stat == _prototype_Stat.None || coeff.stat == _prototype_Stat.NULL)
                            ? 1f
                            : GetStatValue(target, coeff.stat);
                        casterTotal += statValue * coeff.coefficient;
                    }
                }
            }
            return Mathf.RoundToInt(casterTotal);
        }
    }

}
