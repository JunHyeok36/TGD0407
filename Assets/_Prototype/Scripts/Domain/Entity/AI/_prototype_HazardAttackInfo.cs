using System.Collections.Generic;
using UnityEngine;

namespace TDG0407._prototype
{
    /// <summary>
    /// 공격 예정 하이라이터 타일을 노리는 적의 공격 상세 정보를 담는 DTO 클래스
    /// </summary>
    public class _prototype_HazardAttackInfo
    {
        public _prototype_EntityView AttackerView { get; set; }
        public string AttackerName { get; set; }
        public string AttackTitle { get; set; }
        public _prototype_CardData Card { get; set; }
        public int EstimatedDamage { get; set; }
        public _prototype_DamageType DamageType { get; set; }
        public List<string> AdditionalEffects { get; set; } = new();

        /// <summary>AI 카드 공격 기반 팩토리</summary>
        public static _prototype_HazardAttackInfo Create(_prototype_EntityView attacker, _prototype_CardData card)
        {
            if (attacker == null || card == null) return null;

            var info = new _prototype_HazardAttackInfo
            {
                AttackerView = attacker,
                AttackerName = !string.IsNullOrEmpty(attacker.EntityData?.ename) ? attacker.EntityData.ename : attacker.name,
                AttackTitle = card.id,
                Card = card,
                DamageType = _prototype_DamageType.Physical,
                EstimatedDamage = 0
            };

            var sourceLife = attacker.EntityData as _prototype_LifeData;
            var battleCard = card as _prototype_BattleCardData;
            ParseActions(info, battleCard?.actionList, sourceLife);
            return info;
        }

        /// <summary>적군 투사체 기반 팩토리 — onHitActions 파싱</summary>
        public static _prototype_HazardAttackInfo CreateFromProjectile(_prototype_ProjectileView projectileView)
        {
            if (projectileView == null || projectileView.Data == null) return null;

            var data = projectileView.Data;

            // 발사자 LifeData 참조
            var shooterLife = data.shooter as _prototype_LifeData;

            // 발사자 EntityView 검색 (씬에서 LifeView 탐색)
            _prototype_EntityView shooterView = null;
            if (shooterLife != null)
            {
                var allLifeViews = _prototype_GridManager.Instance.GetAllLifeViews();
                foreach (var lv in allLifeViews)
                {
                    if (lv.EntityData == shooterLife)
                    {
                        shooterView = lv;
                        break;
                    }
                }
            }

            string attackerName;
            if (shooterLife != null && !string.IsNullOrEmpty(shooterLife.ename))
                attackerName = shooterLife.ename;
            else if (shooterView != null)
                attackerName = shooterView.name;
            else
                attackerName = projectileView.name;

            string attackTitle = !string.IsNullOrEmpty(data.ename) ? data.ename : "투사체";

            var info = new _prototype_HazardAttackInfo
            {
                AttackerView = shooterView != null ? shooterView : projectileView,
                AttackerName = attackerName,
                AttackTitle = attackTitle,
                Card = null,
                DamageType = _prototype_DamageType.Physical,
                EstimatedDamage = 0
            };

            ParseActions(info, data.onHitActions, shooterLife);
            return info;
        }

        private static void ParseActions(
            _prototype_HazardAttackInfo info,
            List<_prototype_EntityAction> actions,
            _prototype_LifeData sourceLife)
        {
            if (actions == null) return;

            foreach (var action in actions)
            {
                if (action is _prototype_DamageEntityAction dmgAction)
                {
                    info.DamageType = dmgAction.damageType;
                    float calculated = 0f;
                    if (dmgAction.damageCoefficients != null)
                    {
                        foreach (var coeff in dmgAction.damageCoefficients)
                        {
                            float statVal = 1f;
                            if (sourceLife != null)
                            {
                                switch (coeff.stat)
                                {
                                    case _prototype_Stat.RedPower: statVal = sourceLife.RedPower; break;
                                    case _prototype_Stat.BluePower: statVal = sourceLife.BluePower; break;
                                    case _prototype_Stat.Health: statVal = sourceLife.health.Current; break;
                                    case _prototype_Stat.Stamina: statVal = sourceLife.stamina.Current; break;
                                    default: break;
                                }
                            }
                            calculated += statVal * coeff.coefficient;
                        }
                    }
                    info.EstimatedDamage += Mathf.RoundToInt(calculated);
                }
                else if (action is _prototype_PushEntityAction pushAction)
                {
                    info.AdditionalEffects.Add($"밀쳐내기 {pushAction.distance}칸");
                }
                else if (action is _prototype_KnockbackEntityAction)
                {
                    info.AdditionalEffects.Add("넉백 효과");
                }
                else if (action is _prototype_ApplyStatusEffectEntityAction statusAction)
                {
                    string statusName = statusAction.statusType.ToString();
                    switch (statusAction.statusType)
                    {
                        case _prototype_StatusType.Stun: statusName = "기절"; break;
                        case _prototype_StatusType.Silence: statusName = "침묵"; break;
                        case _prototype_StatusType.Bleeding: statusName = "출혈"; break;
                        case _prototype_StatusType.Burning: statusName = "화상"; break;
                        case _prototype_StatusType.Freeze: statusName = "빙결"; break;
                        case _prototype_StatusType.Curse: statusName = "저주"; break;
                        case _prototype_StatusType.SuperArmor: statusName = "슈퍼아머"; break;
                    }

                    if (statusAction.value > 0)
                        info.AdditionalEffects.Add($"{statusName} {statusAction.value:0} ({statusAction.durationTicks}틱)");
                    else
                        info.AdditionalEffects.Add($"{statusName} ({statusAction.durationTicks}틱)");
                }
            }
        }
    }
}
