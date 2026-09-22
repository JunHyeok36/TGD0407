using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Settings;

namespace TDG0407._prototype
{
    /// <summary>
    /// 카드의 Description을 다국어(ko/en) 템플릿 및 시전자/대상 스탯 기반으로 실시간 동적 포맷팅하는 유틸리티 엔진입니다.
    /// - 기본 모드: 시전자 스탯을 대입한 최종 계산 수치 (예: 84) 및 대상 비례 수식 (예: +대상 최대 체력 10%)
    /// - 대상 지정 모드: 호버 중인 대상의 실제 스탯을 대입한 완전 합산 수치
    /// - 상세 모드 (Alt/Shift 키 홀드): 계산 과정 계수 수식 (예: [25 + 공격력 50% + 대상 최대 체력 10%])
    /// - 스탯별 Rich Text 색상 하이라이트 적용
    /// </summary>
    public static class _prototype_CardDescriptionFormatter
    {
        // 스탯 테마 컬러
        public const string COLOR_PHYSICAL = "#FF6B4A"; // 주황/빨강 (물리 / 공격력)
        public const string COLOR_MAGICAL = "#4AA8FF";  // 시안/하늘색 (마법 / 주문력)
        public const string COLOR_TRUE = "#FACC15";     // 골드/노랑 (고정 피해)
        public const string COLOR_HEALTH = "#4ADE80";   // 에메랄드 그린 (체력 / 회복)
        public const string COLOR_STAMINA = "#FDE047";  // 밝은 노랑 (스태미나)
        public const string COLOR_SHIELD = "#C084FC";   // 보라색 (보호막)

        /// <summary>
        /// 21종 전체 카드에 대한 내장 다국어 템플릿 사전 (Localization 에셋 로드 전/폴백용 및 원천 데이터)
        /// </summary>
        private static readonly Dictionary<string, (string ko, string en)> CardLocalizationCatalog = new(StringComparer.OrdinalIgnoreCase)
        {
            // ─── 전투 카드 (Battle Cards - 14종) ───
            { "BasicAttack", ("단일 적에게 {damage}의 물리 피해를 입힙니다.", "Deals {damage} physical damage to a single target.") },
            { "CARD_BASIC_ATTACK_DESC", ("단일 적에게 {damage}의 물리 피해를 입힙니다.", "Deals {damage} physical damage to a single target.") },

            { "BloodStrike", ("체력을 소모하여 전방에 {damage}의 물리 피해를 입히고 출혈을 부여합니다.", "Consumes HP to deal {damage} physical damage to the front and inflict Bleed.") },
            { "CARD_BLOOD_STRIKE_DESC", ("체력을 소모하여 전방에 {damage}의 물리 피해를 입히고 출혈을 부여합니다.", "Consumes HP to deal {damage} physical damage to the front and inflict Bleed.") },

            { "Claw", ("날카로운 발톱으로 전방을 할퀴어 {damage}의 물리 피해를 입힙니다.", "Scratches the enemy in front with sharp claws, dealing {damage} physical damage.") },
            { "할퀴기 (Claw)", ("날카로운 발톱으로 전방을 할퀴어 {damage}의 물리 피해를 입힙니다.", "Scratches the enemy in front with sharp claws, dealing {damage} physical damage.") },
            { "CARD_CLAW_DESC", ("날카로운 발톱으로 전방을 할퀴어 {damage}의 물리 피해를 입힙니다.", "Scratches the enemy in front with sharp claws, dealing {damage} physical damage.") },

            { "DashAway", ("후방으로 1칸 도주하여 거리를 벌립니다.", "Dashes 1 tile backward to create distance.") },
            { "CARD_DASH_AWAY_DESC", ("후방으로 1칸 도주하여 거리를 벌립니다.", "Dashes 1 tile backward to create distance.") },

            { "Earthquake", ("지면을 내리쳐 범위 내 모든 적에게 {damage}의 물리 피해를 가합니다.", "Slams the ground, dealing {damage} physical damage to all enemies in the area.") },
            { "CARD_EARTHQUAKE_DESC", ("지면을 내리쳐 범위 내 모든 적에게 {damage}의 물리 피해를 가합니다.", "Slams the ground, dealing {damage} physical damage to all enemies in the area.") },

            { "HolyNova", ("신성한 빛을 방출하여 주변 1칸 내 모든 적에게 {damage}의 물리 피해를 입힙니다.", "Unleashes holy light, dealing {damage} physical damage to all surrounding enemies.") },
            { "CARD_HOLY_NOVA_DESC", ("신성한 빛을 방출하여 주변 1칸 내 모든 적에게 {damage}의 물리 피해를 입힙니다.", "Unleashes holy light, dealing {damage} physical damage to all surrounding enemies.") },

            { "LaserBeamCard", ("집중된 에너지를 발사하여 직선 경로의 모든 대상에게 {damage}의 마법 피해를 입힙니다.", "Fires a focused energy beam, dealing {damage} magic damage to all targets in a straight line.") },
            { "LaserBeam", ("집중된 에너지를 발사하여 직선 경로의 모든 대상에게 {damage}의 마법 피해를 입힙니다.", "Fires a focused energy beam, dealing {damage} magic damage to all targets in a straight line.") },
            { "CARD_LASER_BEAM_DESC", ("집중된 에너지를 발사하여 직선 경로의 모든 대상에게 {damage}의 마법 피해를 입힙니다.", "Fires a focused energy beam, dealing {damage} magic damage to all targets in a straight line.") },

            { "PiercingThrust", ("창을 깊숙이 찔러 일직선상의 적들에게 {damage}의 물리 피해를 입힙니다.", "Thrusts a spear forward, dealing {damage} physical damage to enemies in a line.") },
            { "CARD_PIERCING_THRUST_DESC", ("창을 깊숙이 찔러 일직선상의 적들에게 {damage}의 물리 피해를 입힙니다.", "Thrusts a spear forward, dealing {damage} physical damage to enemies in a line.") },

            { "ShootArrow", ("원거리의 적 1체를 겨냥하여 화살을 발사해 {damage}의 물리 피해를 입힙니다.", "Fires an arrow at a targeted ranged enemy, dealing {damage} physical damage.") },
            { "card_shoot_arrow", ("원거리의 적 1체를 겨냥하여 화살을 발사해 {damage}의 물리 피해를 입힙니다.", "Fires an arrow at a targeted ranged enemy, dealing {damage} physical damage.") },
            { "CARD_SHOOT_ARROW_DESC", ("원거리의 적 1체를 겨냥하여 화살을 발사해 {damage}의 물리 피해를 입힙니다.", "Fires an arrow at a targeted ranged enemy, dealing {damage} physical damage.") },

            { "Snipe", ("먼 거리의 단일 적을 정밀 조준하여 {damage}의 물리 피해를 입힙니다.", "Carefully aims at a distant enemy to deal {damage} physical damage.") },
            { "CARD_SNIPE_DESC", ("먼 거리의 단일 적을 정밀 조준하여 {damage}의 물리 피해를 입힙니다.", "Carefully aims at a distant enemy to deal {damage} physical damage.") },

            { "SplashAttack", ("지정 위치 주변에 충격파를 일으켜 {damage}의 물리 피해를 입힙니다.", "Causes a shockwave around the target area, dealing {damage} physical damage.") },
            { "CARD_SPLASH_ATTACK_DESC", ("지정 위치 주변에 충격파를 일으켜 {damage}의 물리 피해를 입힙니다.", "Causes a shockwave around the target area, dealing {damage} physical damage.") },

            { "StatusTestCard", ("대상에게 기절 및 상태이상을 부여하여 행동을 제한합니다.", "Inflicts Stun and status ailments on the target to restrict actions.") },
            { "CARD_STATUS_TEST_CARD_DESC", ("대상에게 기절 및 상태이상을 부여하여 행동을 제한합니다.", "Inflicts Stun and status ailments on the target to restrict actions.") },

            { "ThrowStone", ("돌을 던져 원거리의 적에게 {damage}의 물리 피해를 가합니다.", "Throws a stone, dealing {damage} physical damage to a distant enemy.") },
            { "CARD_THROW_STONE_DESC", ("돌을 던져 원거리의 적에게 {damage}의 물리 피해를 가합니다.", "Throws a stone, dealing {damage} physical damage to a distant enemy.") },

            { "knockback_test", ("대상을 1칸 밀쳐냅니다.", "Knocks back target by 1 tile.") },
            { "CARD_KNOCKBACK_TEST_DESC", ("대상을 1칸 밀쳐냅니다.", "Knocks back target by 1 tile.") },

            // ─── 상호작용 카드 (Interaction Cards - 7종) ───
            { "Chest_Open", ("상자를 열어 내용물을 확인하고 카드를 획득합니다.", "Opens the chest to inspect its contents and acquire cards.") },
            { "상자 열기 (Open)", ("상자를 열어 내용물을 확인하고 카드를 획득합니다.", "Opens the chest to inspect its contents and acquire cards.") },
            { "CARD_CHEST_OPEN_DESC", ("상자를 열어 내용물을 확인하고 카드를 획득합니다.", "Opens the chest to inspect its contents and acquire cards.") },

            { "InteractableTest_TurnOff", ("스위치를 조작하여 장치 전원을 끕니다.", "Flips the switch to turn off the mechanism.") },
            { "레버 끄기 (Turn Off)", ("스위치를 조작하여 장치 전원을 끕니다.", "Flips the switch to turn off the mechanism.") },
            { "CARD_LEVER_TURN_OFF_DESC", ("스위치를 조작하여 장치 전원을 끕니다.", "Flips the switch to turn off the mechanism.") },

            { "InteractableTest_TurnOn", ("스위치를 조작하여 장치 전원을 켭니다.", "Flips the switch to turn on the mechanism.") },
            { "레버 켜기 (Turn On)", ("스위치를 조작하여 장치 전원을 켭니다.", "Flips the switch to turn on the mechanism.") },
            { "CARD_LEVER_TURN_ON_DESC", ("스위치를 조작하여 장치 전원을 켭니다.", "Flips the switch to turn on the mechanism.") },

            { "Obstacle_Break", ("장애물을 강하게 가격하여 완전히 파괴합니다.", "Strikes the obstacle forcefully to completely destroy it.") },
            { "부수기 (Break)", ("장애물을 강하게 가격하여 완전히 파괴합니다.", "Strikes the obstacle forcefully to completely destroy it.") },
            { "CARD_OBSTACLE_BREAK_DESC", ("장애물을 강하게 가격하여 완전히 파괴합니다.", "Strikes the obstacle forcefully to completely destroy it.") },

            { "TESTCOM", ("특수 상호작용 컴포넌트를 동작시키는 테스트 카드입니다.", "Test interaction card used to trigger special component behaviors.") },
            { "CARD_TESTCOM_DESC", ("특수 상호작용 컴포넌트를 동작시키는 테스트 카드입니다.", "Test interaction card used to trigger special component behaviors.") },

            { "Trap_Detonate", ("원거리에서 함정의 격발 장치를 건드려 강제 폭발시킵니다.", "Triggers the trap mechanism from a distance to force detonation.") },
            { "원거리 격발 (Detonate)", ("원거리에서 함정의 격발 장치를 건드려 강제 폭발시킵니다.", "Triggers the trap mechanism from a distance to force detonation.") },
            { "CARD_TRAP_DETONATE_DESC", ("원거리에서 함정의 격발 장치를 건드려 강제 폭발시킵니다.", "Triggers the trap mechanism from a distance to force detonation.") },

            { "Trap_Disarm", ("함정을 안전하게 해체하여 위험 요소를 무력화합니다.", "Safely disarms the trap, neutralizing the hazard.") },
            { "해체 (Disarm)", ("함정을 안전하게 해체하여 위험 요소를 무력화합니다.", "Safely disarms the trap, neutralizing the hazard.") },
            { "CARD_TRAP_DISARM_DESC", ("함정을 안전하게 해체하여 위험 요소를 무력화합니다.", "Safely disarms the trap, neutralizing the hazard.") }
        };

        /// <summary>
        /// 카드 ID와 LocalizationKey 간의 기본 매핑 테이블
        /// </summary>
        private static readonly Dictionary<string, string> CardIdToLocKeyMap = new(StringComparer.OrdinalIgnoreCase)
        {
            { "BasicAttack", "CARD_BASIC_ATTACK_DESC" },
            { "BloodStrike", "CARD_BLOOD_STRIKE_DESC" },
            { "할퀴기 (Claw)", "CARD_CLAW_DESC" },
            { "Claw", "CARD_CLAW_DESC" },
            { "DashAway", "CARD_DASH_AWAY_DESC" },
            { "Earthquake", "CARD_EARTHQUAKE_DESC" },
            { "HolyNova", "CARD_HOLY_NOVA_DESC" },
            { "LaserBeam", "CARD_LASER_BEAM_DESC" },
            { "PiercingThrust", "CARD_PIERCING_THRUST_DESC" },
            { "card_shoot_arrow", "CARD_SHOOT_ARROW_DESC" },
            { "ShootArrow", "CARD_SHOOT_ARROW_DESC" },
            { "Snipe", "CARD_SNIPE_DESC" },
            { "SplashAttack", "CARD_SPLASH_ATTACK_DESC" },
            { "StatusTestCard", "CARD_STATUS_TEST_CARD_DESC" },
            { "ThrowStone", "CARD_THROW_STONE_DESC" },
            { "knockback_test", "CARD_KNOCKBACK_TEST_DESC" },
            { "상자 열기 (Open)", "CARD_CHEST_OPEN_DESC" },
            { "부수기 (Break)", "CARD_OBSTACLE_BREAK_DESC" },
            { "원거리 격발 (Detonate)", "CARD_TRAP_DETONATE_DESC" },
            { "해체 (Disarm)", "CARD_TRAP_DISARM_DESC" },
            { "레버 켜기 (Turn On)", "CARD_LEVER_TURN_ON_DESC" },
            { "레버 끄기 (Turn Off)", "CARD_LEVER_TURN_OFF_DESC" },
            { "TESTCOM", "CARD_TESTCOM_DESC" }
        };

        /// <summary>
        /// 카드의 설명을 동적으로 포맷팅하여 반환합니다.
        /// </summary>
        public static string FormatDescription(
            _prototype_CardData card,
            _prototype_LifeData caster = null,
            bool isDetailedMode = false,
            _prototype_EntityData target = null)
        {
            if (card == null) return string.Empty;

            // 1. 현재 로케일(언어) 판별
            bool isEnglish = IsCurrentLanguageEnglish();

            // 2. 키 확인 및 원본 템플릿 로드
            string locKey = card.descriptionLocalizationKey;
            if (string.IsNullOrEmpty(locKey) && !string.IsNullOrEmpty(card.id))
            {
                CardIdToLocKeyMap.TryGetValue(card.id, out locKey);
            }

            string rawTemplate = GetRawTemplate(locKey, isEnglish);
            if (string.IsNullOrEmpty(rawTemplate))
            {
                return string.Empty;
            }

            // 3. 시전자 컨텍스트 확인 (없을 경우 플레이어 기본 대입)
            if (caster == null)
            {
                caster = card.sourceProvider as _prototype_LifeData;
            }
            if (caster == null && _prototype_PlayerController.Instance != null && _prototype_PlayerController.Instance.ControlledEntityView != null)
            {
                var lifeView = _prototype_PlayerController.Instance.ControlledEntityView as _prototype_LifeView;
                caster = lifeView != null ? lifeView.Data : _prototype_PlayerController.Instance.ControlledEntityView.EntityData as _prototype_LifeData;
            }

            // 4. {damage} 토큰 치환
            if (rawTemplate.Contains("{damage}"))
            {
                string formattedDamage = BuildDamageString(card, caster, isDetailedMode, isEnglish, target);
                rawTemplate = rawTemplate.Replace("{damage}", formattedDamage);
            }

            return rawTemplate;
        }

        private static bool IsCurrentLanguageEnglish()
        {
            try
            {
                if (LocalizationSettings.SelectedLocale != null)
                {
                    string code = LocalizationSettings.SelectedLocale.Identifier.Code;
                    if (!string.IsNullOrEmpty(code) && code.StartsWith("en", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }
            catch
            {
                // Ignore exceptions during edit mode or when LocalizationSettings is uninitialized
            }
            return false;
        }

        private static string GetRawTemplate(string locKey, bool isEnglish)
        {
            if (!string.IsNullOrEmpty(locKey))
            {
                // Unity Localization StringDatabase 1차 시도
                try
                {
                    if (LocalizationSettings.StringDatabase != null)
                    {
                        var entry = LocalizationSettings.StringDatabase.GetLocalizedString("Cards_Table", locKey);
                        if (!string.IsNullOrEmpty(entry) && !entry.StartsWith("{") && !entry.EndsWith("}"))
                        {
                            return entry;
                        }
                    }
                }
                catch { }

                // 카탈로그 폴백 2차 시도 (직접 키)
                if (CardLocalizationCatalog.TryGetValue(locKey, out var pair))
                {
                    return isEnglish ? pair.en : pair.ko;
                }

                // 카탈로그 폴백 3차 시도 (CardIdToLocKeyMap 경유)
                if (CardIdToLocKeyMap.TryGetValue(locKey, out var mappedKey) && CardLocalizationCatalog.TryGetValue(mappedKey, out pair))
                {
                    return isEnglish ? pair.en : pair.ko;
                }
            }

            return string.Empty;
        }

        public static _prototype_DamageEntityAction FindDamageAction(IEnumerable<_prototype_EntityAction> actions)
        {
            if (actions == null) return null;
            foreach (var action in actions)
            {
                if (action == null) continue;
                if (action is _prototype_DamageEntityAction da)
                {
                    return da;
                }
                if (action is _prototype_SpawnTargetedProjectileEntityAction stp && stp.onHitActions != null)
                {
                    var found = FindDamageAction(stp.onHitActions);
                    if (found != null) return found;
                }
                if (action is _prototype_SpawnDirectionalProjectileEntityAction sdp && sdp.onHitActions != null)
                {
                    var found = FindDamageAction(sdp.onHitActions);
                    if (found != null) return found;
                }
                if (action is _prototype_SpawnAreaEffectEntityAction sae && sae.areaEffectModel != null && sae.areaEffectModel.onTriggerActions != null)
                {
                    var found = FindDamageAction(sae.areaEffectModel.onTriggerActions);
                    if (found != null) return found;
                }
            }
            return null;
        }

        private static string BuildDamageString(
            _prototype_CardData card,
            _prototype_LifeData caster,
            bool isDetailedMode,
            bool isEnglish,
            _prototype_EntityData target = null)
        {
            _prototype_DamageEntityAction dmgAction = null;

            if (card is _prototype_BattleCardData battleCard && battleCard.actionList != null)
            {
                dmgAction = FindDamageAction(battleCard.actionList);
            }

            if (dmgAction == null || dmgAction.damageCoefficients == null || dmgAction.damageCoefficients.Length == 0)
            {
                return isDetailedMode ? "<color=#888888>[0]</color>" : "<color=#888888>0</color>";
            }

            string themeColor = GetDamageColor(dmgAction.damageType);

            // 계수 분석 (고정 기본값 vs 시전자 스탯 계수 vs 대상 스탯 계수)
            float fixedBase = 0f;
            var casterStatList = new List<(_prototype_Stat stat, float coeff)>();
            var targetStatList = new List<(_prototype_Stat stat, float coeff)>();

            foreach (var coeff in dmgAction.damageCoefficients)
            {
                if (coeff == null) continue;
                if (coeff.stat == _prototype_Stat.None || coeff.stat == _prototype_Stat.NULL)
                {
                    fixedBase += coeff.coefficient;
                }
                else if (coeff.source == _prototype_StatSource.Target)
                {
                    targetStatList.Add((coeff.stat, coeff.coefficient));
                }
                else
                {
                    casterStatList.Add((coeff.stat, coeff.coefficient));
                }
            }

            // ─── [모드 1] 최종 계산 수치 모드 (기본) ───
            if (!isDetailedMode)
            {
                float casterTotal = fixedBase;
                if (caster != null)
                {
                    foreach (var item in casterStatList)
                    {
                        float statVal = GetStatValue(caster, item.stat);
                        casterTotal += statVal * item.coeff;
                    }
                }

                if (target != null)
                {
                    float targetTotal = 0f;
                    foreach (var item in targetStatList)
                    {
                        float statVal = GetStatValue(target, item.stat);
                        targetTotal += statVal * item.coeff;
                    }

                    int totalInt = Mathf.RoundToInt(casterTotal + targetTotal);
                    return $"<color={themeColor}><b>{totalInt}</b></color>";
                }
                else
                {
                    int casterInt = Mathf.RoundToInt(casterTotal);
                    if (targetStatList.Count == 0)
                    {
                        return $"<color={themeColor}><b>{casterInt}</b></color>";
                    }

                    // 대상 미지정 시: 시전자 피해 + (대상 스탯 비례식)
                    var targetParts = new List<string>();
                    foreach (var item in targetStatList)
                    {
                        string statName = GetStatName(item.stat, _prototype_StatSource.Target, isEnglish);
                        string statColor = GetStatColor(item.stat);
                        int percent = Mathf.RoundToInt(item.coeff * 100f);

                        string coeffPart = isEnglish
                            ? $"<color={statColor}>{percent}% {statName}</color>"
                            : $"<color={statColor}>{statName} {percent}%</color>";

                        targetParts.Add(coeffPart);
                    }

                    string targetFormula = string.Join(" + ", targetParts);
                    if (casterInt > 0)
                    {
                        return $"<color={themeColor}><b>{casterInt}</b></color> <color=#AAAAAA>(+{targetFormula})</color>";
                    }
                    else
                    {
                        return $"<b>{targetFormula}</b>";
                    }
                }
            }

            // ─── [모드 2] 상세 계수 수식 모드 (Alt/Shift 키 홀드) ───
            var parts = new List<string>();

            if (fixedBase > 0f || (casterStatList.Count == 0 && targetStatList.Count == 0))
            {
                parts.Add(Mathf.RoundToInt(fixedBase).ToString());
            }

            foreach (var item in casterStatList)
            {
                string statName = GetStatName(item.stat, _prototype_StatSource.Caster, isEnglish);
                string statColor = GetStatColor(item.stat);
                int percent = Mathf.RoundToInt(item.coeff * 100f);

                string coeffPart = isEnglish
                    ? $"<color={statColor}>{percent}% {statName}</color>"
                    : $"<color={statColor}>{statName} {percent}%</color>";

                parts.Add(coeffPart);
            }

            foreach (var item in targetStatList)
            {
                string statName = GetStatName(item.stat, _prototype_StatSource.Target, isEnglish);
                string statColor = GetStatColor(item.stat);
                int percent = Mathf.RoundToInt(item.coeff * 100f);

                string coeffPart = isEnglish
                    ? $"<color={statColor}>{percent}% {statName}</color>"
                    : $"<color={statColor}>{statName} {percent}%</color>";

                parts.Add(coeffPart);
            }

            string formulaText = string.Join(" + ", parts);
            return $"<color={themeColor}><b>[{formulaText}]</b></color>";
        }

        public static float GetStatValue(_prototype_EntityData entity, _prototype_Stat stat)
        {
            return _prototype_DamageEntityAction.GetStatValue(entity, stat);
        }

        public static string GetStatName(_prototype_Stat stat, _prototype_StatSource source, bool isEnglish)
        {
            if (source == _prototype_StatSource.Target)
            {
                return stat switch
                {
                    _prototype_Stat.RedPower => isEnglish ? "Target ATK" : "대상 공격력",
                    _prototype_Stat.BluePower => isEnglish ? "Target AP" : "대상 주문력",
                    _prototype_Stat.Health => isEnglish ? "Target Max HP" : "대상 최대 체력",
                    _prototype_Stat.MaxHealth => isEnglish ? "Target Max HP" : "대상 최대 체력",
                    _prototype_Stat.CurrentHealth => isEnglish ? "Target Current HP" : "대상 현재 체력",
                    _prototype_Stat.MissingHealth => isEnglish ? "Target Missing HP" : "대상 잃은 체력",
                    _prototype_Stat.Stamina => isEnglish ? "Target SP" : "대상 스태미나",
                    _prototype_Stat.Shield => isEnglish ? "Target Shield" : "대상 보호막",
                    _prototype_Stat.Avoidance => isEnglish ? "Target Evasion" : "대상 회피",
                    _prototype_Stat.CriticalProb => isEnglish ? "Target Crit Rate" : "대상 치명타 확률",
                    _prototype_Stat.CriticalWeight => isEnglish ? "Target Crit Dmg" : "대상 치명타 피해",
                    _prototype_Stat.RedResist => isEnglish ? "Target Physical Resist" : "대상 물리 저항",
                    _prototype_Stat.BlueResist => isEnglish ? "Target Magical Resist" : "대상 마법 저항",
                    _ => isEnglish ? "Target Stat" : "대상 스탯"
                };
            }

            return stat switch
            {
                _prototype_Stat.RedPower => isEnglish ? "ATK" : "공격력",
                _prototype_Stat.BluePower => isEnglish ? "AP" : "주문력",
                _prototype_Stat.Health => isEnglish ? "HP" : "체력",
                _prototype_Stat.MaxHealth => isEnglish ? "Max HP" : "최대 체력",
                _prototype_Stat.CurrentHealth => isEnglish ? "Current HP" : "현재 체력",
                _prototype_Stat.MissingHealth => isEnglish ? "Missing HP" : "잃은 체력",
                _prototype_Stat.Stamina => isEnglish ? "SP" : "스태미나",
                _prototype_Stat.Shield => isEnglish ? "Shield" : "보호막",
                _prototype_Stat.Avoidance => isEnglish ? "Evasion" : "회피",
                _prototype_Stat.CriticalProb => isEnglish ? "Crit Rate" : "치명타 확률",
                _prototype_Stat.CriticalWeight => isEnglish ? "Crit Dmg" : "치명타 피해",
                _prototype_Stat.RedResist => isEnglish ? "Physical Resist" : "물리 저항",
                _prototype_Stat.BlueResist => isEnglish ? "Magical Resist" : "마법 저항",
                _ => isEnglish ? "Stat" : "스탯"
            };
        }

        // Backward compatibility overload
        public static string GetStatName(_prototype_Stat stat, bool isEnglish) => GetStatName(stat, _prototype_StatSource.Caster, isEnglish);

        public static string GetStatColor(_prototype_Stat stat)
        {
            return stat switch
            {
                _prototype_Stat.RedPower => COLOR_PHYSICAL,
                _prototype_Stat.BluePower => COLOR_MAGICAL,
                _prototype_Stat.Health => COLOR_HEALTH,
                _prototype_Stat.MaxHealth => COLOR_HEALTH,
                _prototype_Stat.CurrentHealth => COLOR_HEALTH,
                _prototype_Stat.MissingHealth => "#FF8888",
                _prototype_Stat.Stamina => COLOR_STAMINA,
                _prototype_Stat.Shield => COLOR_SHIELD,
                _ => "#FFFFFF"
            };
        }

        public static string GetDamageColor(_prototype_DamageType damageType)
        {
            return damageType switch
            {
                _prototype_DamageType.Physical => COLOR_PHYSICAL,
                _prototype_DamageType.Magical => COLOR_MAGICAL,
                _prototype_DamageType.True => COLOR_TRUE,
                _ => COLOR_PHYSICAL
            };
        }

        /// <summary>
        /// 21종 전체 카드의 다국어 카탈로그를 반환합니다.
        /// </summary>
        public static IReadOnlyDictionary<string, (string ko, string en)> GetAllCatalogEntries() => CardLocalizationCatalog;

        /// <summary>
        /// 카드 ID에 매핑된 LocalizationKey를 반환합니다.
        /// </summary>
        public static string GetLocalizationKeyForCardId(string cardId)
        {
            if (string.IsNullOrEmpty(cardId)) return string.Empty;
            return CardIdToLocKeyMap.TryGetValue(cardId, out var key) ? key : string.Empty;
        }
    }
}
