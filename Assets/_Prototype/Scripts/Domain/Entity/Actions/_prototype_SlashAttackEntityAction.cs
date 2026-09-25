using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TDG0407._prototype
{
    /// <summary>
    /// 삼연격: 베기 (Slash) 액션
    /// - 전방 2칸 대상에게 [25 + 공격력 100%] 물리 피해
    /// - 적에게 실제 피해가 1 이상 적중했을 시, 시전자에게 '삼연격: 찌르기 강화' (EnhanceStab) 효과를 5틱 동안 부여
    /// </summary>
    [Serializable]
    public class _prototype_SlashAttackEntityAction : _prototype_EntityAction
    {
        public int baseDamage = 25;
        public float redPowerRatio = 1.0f;
        public int enhanceStabDuration = 5;

        public override async UniTask ExecuteAction(
            _prototype_EntityData source,
            IEnumerable<_prototype_EntityData> targets,
            _prototype_IActionParams @params)
        {
            if (source == null) return;
            var sourceLife = source as _prototype_LifeData;

            int casterRedPower = sourceLife != null ? sourceLife.RedPower : 0;
            int calculatedDamage = baseDamage + Mathf.RoundToInt(casterRedPower * redPowerRatio);

            // 공격 모션/방향 회전
            var sourceView = _prototype_GridManager.Instance?.GetPointView(source.point)?.PlacedEntityViews.Find(v => v.EntityData == source);
            if (@params is _prototype_CardActionParams cardParams && sourceView != null)
            {
                if (cardParams.Direction != _prototype_Point.zero)
                {
                    sourceView.FaceTowards(source.point + cardParams.Direction, 0.15f);
                }
                if (sourceView is _prototype_LifeView lifeView)
                {
                    Vector3 attackDir = new Vector3(cardParams.Direction.x, 0, cardParams.Direction.y);
                    if (attackDir.sqrMagnitude < 0.01f) attackDir = sourceView.transform.forward;
                    lifeView.PlayUniqueAnimation(_prototype_EntityAnimationType.Attack, attackDir).Forget();
                }
            }

            bool anyDamageDealt = false;
            List<UniTask> damageTasks = new();

            if (targets != null)
            {
                foreach (var target in targets)
                {
                    if (target == null || target == source) continue;
                    if (source.IsSameSide(target)) continue;

                    // 크리티컬 판정
                    bool isCritical = false;
                    int currentDamage = calculatedDamage;
                    if (sourceLife != null && sourceLife.lifeStat.criticalProb > 0f)
                    {
                        if (UnityEngine.Random.value < sourceLife.lifeStat.criticalProb)
                        {
                            isCritical = true;
                            float critMultiplier = 1f + sourceLife.lifeStat.criticalWeight / 100f;
                            currentDamage = Mathf.RoundToInt(calculatedDamage * critMultiplier);
                        }
                    }

                    var context = new _prototype_DamageContext(
                        source,
                        target,
                        _prototype_DamageType.Physical,
                        calculatedDamage,
                        currentDamage,
                        isCritical,
                        0.5f
                    );

                    async UniTask DealDamageTask()
                    {
                        await _prototype_InteractionManager.ApplyDamage(context);
                        if (context.finalDamage.HasValue && context.finalDamage.Value > 0)
                        {
                            anyDamageDealt = true;
                        }
                    }

                    damageTasks.Add(DealDamageTask());
                }
            }

            if (damageTasks.Count > 0)
            {
                await UniTask.WhenAll(damageTasks);
            }

            // 적중 성공 시 시전자에게 EnhanceStab 부여
            if (anyDamageDealt && sourceLife != null && !sourceLife.IsDead)
            {
                sourceLife.ApplyStatusEffect(new _prototype_StatusEffect(_prototype_StatusType.EnhanceStab, enhanceStabDuration));
                if (sourceView != null)
                {
                    _prototype_FloatingText.SpawnOnEntity(sourceView, "찌르기 강화 획득!", new Color(1f, 0.55f, 0.1f), 1.1f);
                }
            }
        }
    }
}
