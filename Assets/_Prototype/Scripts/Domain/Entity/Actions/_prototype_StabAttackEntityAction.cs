using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TDG0407._prototype
{
    /// <summary>
    /// 삼연격: 찌르기 (Stab) 액션
    /// - 기본 전방 3x3 영역에 [60 + 공격력 120%] 물리 피해
    /// - 투사체(Projectile)에게는 500% 피해
    /// - 시전자가 '삼연격: 찌르기 강화'(EnhanceStab) 보유 시:
    ///   - 피해 50% 추가 증가
    ///   - 적중 대상에게 1칸 넉백 및 벽/장애물 충돌 시 [80 + 공격력 200%] 물리 피해, 2틱간 기절
    ///   - 발동 후 시전자의 EnhanceStab 효과 소모(제거)
    /// </summary>
    [Serializable]
    public class _prototype_StabAttackEntityAction : _prototype_EntityAction
    {
        public int baseDamage = 60;
        public float redPowerRatio = 1.2f;

        // 벽 충돌 계수
        public int wallCollisionBaseDamage = 80;
        public float wallCollisionRedPowerRatio = 2.0f;
        public int stunDuration = 2;

        public override async UniTask ExecuteAction(
            _prototype_EntityData source,
            IEnumerable<_prototype_EntityData> targets,
            _prototype_IActionParams @params)
        {
            if (source == null) return;
            var sourceLife = source as _prototype_LifeData;

            int casterRedPower = sourceLife != null ? sourceLife.RedPower : 0;
            int calculatedDamage = baseDamage + Mathf.RoundToInt(casterRedPower * redPowerRatio);

            var sourceView = _prototype_GridManager.Instance?.GetPointView(source.point)?.PlacedEntityViews.Find(v => v.EntityData == source);

            // 강화 상태 확인 및 적중 여부 무관 시전 즉시 소모
            bool isEnhanced = sourceLife != null && sourceLife.HasStatusEffect(_prototype_StatusType.EnhanceStab);
            if (isEnhanced)
            {
                calculatedDamage = Mathf.RoundToInt(calculatedDamage * 1.5f); // 50% 증가
                sourceLife.RemoveStatusEffect(_prototype_StatusType.EnhanceStab);
                if (sourceView != null)
                {
                    _prototype_FloatingText.SpawnOnEntity(sourceView, "강화 찌르기 발동!", new Color(1f, 0.4f, 0.1f), 1.2f);
                }
            }
            _prototype_Point attackDir = _prototype_Point.zero;

            if (@params is _prototype_CardActionParams cardParams)
            {
                attackDir = cardParams.Direction;
                if (sourceView != null)
                {
                    if (attackDir != _prototype_Point.zero)
                    {
                        sourceView.FaceTowards(source.point + attackDir, 0.15f);
                    }
                    if (sourceView is _prototype_LifeView lifeView)
                    {
                        Vector3 dir3D = new Vector3(attackDir.x, 0, attackDir.y);
                        if (dir3D.sqrMagnitude < 0.01f) dir3D = sourceView.transform.forward;
                        lifeView.PlayUniqueAnimation(_prototype_EntityAnimationType.Attack, dir3D).Forget();
                    }
                }
            }

            List<UniTask> damageTasks = new();
            List<_prototype_EntityData> hitTargets = new();

            if (targets != null)
            {
                foreach (var target in targets)
                {
                    if (target == null || target == source) continue;
                    if (source.IsSameSide(target)) continue;

                    int targetDamage = calculatedDamage;

                    // 투사체에게는 500% 피해
                    if (target is _prototype_ProjectileData)
                    {
                        targetDamage = Mathf.RoundToInt(targetDamage * 5.0f);
                    }

                    // 크리티컬 판정
                    bool isCritical = false;
                    int finalExpectedDamage = targetDamage;
                    if (sourceLife != null && sourceLife.lifeStat.criticalProb > 0f)
                    {
                        if (UnityEngine.Random.value < sourceLife.lifeStat.criticalProb)
                        {
                            isCritical = true;
                            float critMultiplier = 1f + sourceLife.lifeStat.criticalWeight / 100f;
                            finalExpectedDamage = Mathf.RoundToInt(targetDamage * critMultiplier);
                        }
                    }

                    var context = new _prototype_DamageContext(
                        source,
                        target,
                        _prototype_DamageType.Physical,
                        targetDamage,
                        finalExpectedDamage,
                        isCritical,
                        0.5f
                    );

                    async UniTask DealDamageTask()
                    {
                        await _prototype_InteractionManager.ApplyDamage(context);
                        if (target is _prototype_ProjectileData projData)
                        {
                            // 투사체에게는 500% 피해 적용 후 즉시 파괴(소멸)
                            projData.Die(source);
                            var projView = _prototype_GridManager.Instance?.GetPointView(target.point)?.PlacedEntityViews.Find(v => v.EntityData == target) as _prototype_ProjectileView;
                            if (projView != null)
                            {
                                projView.DestroyProjectile();
                            }
                        }
                        if (context.finalDamage.HasValue && context.finalDamage.Value > 0)
                        {
                            lock (hitTargets)
                            {
                                hitTargets.Add(target);
                            }
                        }
                    }

                    damageTasks.Add(DealDamageTask());
                }
            }

            if (damageTasks.Count > 0)
            {
                await UniTask.WhenAll(damageTasks);
            }

            // 강화 상태였던 경우: 넉백 및 벽 충돌 효과 적용
            if (isEnhanced && hitTargets.Count > 0)
            {
                int wallDamage = wallCollisionBaseDamage + Mathf.RoundToInt(casterRedPower * wallCollisionRedPowerRatio);

                var pushAction = new _prototype_PushEntityAction
                {
                    distance = 1,
                    useAttackDirection = true,
                    onCollisionActions = new List<_prototype_EntityAction>
                    {
                        new _prototype_DamageEntityAction
                        {
                            damageType = _prototype_DamageType.Physical,
                            spDamageMultiplier = 0.5f,
                            damageCoefficients = new[]
                            {
                                new _prototype_CoefficientValue(_prototype_StatSource.Caster, _prototype_Stat.None, wallDamage)
                            }
                        },
                        new _prototype_ApplyStatusEffectEntityAction
                        {
                            statusType = _prototype_StatusType.Stun,
                            durationTicks = stunDuration
                        }
                    }
                };

                await pushAction.ExecuteAction(source, hitTargets, @params);
            }
        }
    }
}
