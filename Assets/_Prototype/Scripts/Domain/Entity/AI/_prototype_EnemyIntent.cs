using System;
using UnityEngine;

namespace TDG0407._prototype
{
    /// <summary>
    /// 적 엔티티가 다음 턴에 취할 행동 의도의 유형을 나타냅니다.
    /// LifeHUD의 머리 위 배지 아이콘 선택에 사용됩니다.
    /// </summary>
    public enum _prototype_EnemyIntentType
    {
        None = 0,

        /// <summary>일반 근거리 공격</summary>
        AttackMelee = 1,

        /// <summary>원거리 투사체 공격</summary>
        AttackRanged = 2,

        /// <summary>넉백/밀치기를 포함한 공격</summary>
        AttackKnockback = 3,

        /// <summary>상태이상을 부여하는 공격 (출혈, 독, 화상 등)</summary>
        AttackDebuff = 4,

        /// <summary>이동 (플레이어 쪽으로 추격)</summary>
        Move = 5,

        /// <summary>스태미나 회복 (대기/휴식)</summary>
        Rest = 6,

        /// <summary>스턴/침묵 상태로 행동 불가</summary>
        Stunned = 7,

        /// <summary>도주 (공포 등 효과로 도망)</summary>
        Flee = 8,
    }

    /// <summary>
    /// 적 엔티티의 턴 의도를 담는 데이터 구조체.
    /// EnemyAILogic 서브클래스들이 EvaluateIntent() 내에서 설정합니다.
    /// </summary>
    public class _prototype_EnemyIntent
    {
        public _prototype_EnemyIntentType intentType = _prototype_EnemyIntentType.None;

        /// <summary>공격 카드가 있다면 그 데이터 참조</summary>
        public _prototype_BattleCardData targetCard;

        /// <summary>공격 예정 타겟 포인트 (있을 경우)</summary>
        public _prototype_Point? targetPoint;

        /// <summary>넉백 의도일 때 예상 착지 포인트</summary>
        public _prototype_Point? knockbackLandingPoint;

        public int estimatedDamage = 0;

        // ─── 팩토리 메서드 ───
        public static _prototype_EnemyIntent None() => new() { intentType = _prototype_EnemyIntentType.None };
        public static _prototype_EnemyIntent ForStunned() => new() { intentType = _prototype_EnemyIntentType.Stunned };
        public static _prototype_EnemyIntent ForRest() => new() { intentType = _prototype_EnemyIntentType.Rest };
        public static _prototype_EnemyIntent ForMove() => new() { intentType = _prototype_EnemyIntentType.Move };
        public static _prototype_EnemyIntent ForFlee() => new() { intentType = _prototype_EnemyIntentType.Flee };

        public static _prototype_EnemyIntent ForAttack(
            _prototype_BattleCardData card,
            _prototype_Point target,
            _prototype_EntityData attacker = null,
            _prototype_EntityData defender = null)
        {
            var intent = new _prototype_EnemyIntent
            {
                targetCard = card,
                targetPoint = target,
            };

            // 1. 예상 데미지 계산
            if (card != null)
            {
                var dmgAction = _prototype_CardDescriptionFormatter.FindDamageAction(card.actionList);
                if (dmgAction != null)
                {
                    intent.estimatedDamage = dmgAction.CalculateEstimatedDamage(attacker, defender);
                }
            }

            // 2. 카드 액션 분석으로 세부 공격 유형 분류 및 넉백 착지 타일 계산
            if (card?.actionList != null)
            {
                var kbAction = card.actionList.Find(a => a is _prototype_KnockbackEntityAction) as _prototype_KnockbackEntityAction;
                bool hasStatus = card.actionList.Exists(a => a is _prototype_ApplyStatusEffectEntityAction);
                bool hasProjectile = card.actionList.Exists(a =>
                    a is _prototype_SpawnTargetedProjectileEntityAction ||
                    a is _prototype_SpawnDirectionalProjectileEntityAction);

                if (hasProjectile)
                {
                    intent.intentType = _prototype_EnemyIntentType.AttackRanged;
                }
                else if (kbAction != null)
                {
                    intent.intentType = _prototype_EnemyIntentType.AttackKnockback;

                    // 넉백 착지점 사전 시뮬레이션
                    if (attacker != null)
                    {
                        _prototype_Point dir = _prototype_Point.zero;
                        if (kbAction.directionMode == _prototype_KnockbackDirectionType.AwayFromSource)
                        {
                            int dx = target.x - attacker.point.x;
                            int dy = target.y - attacker.point.y;
                            int signX = dx == 0 ? 0 : (dx > 0 ? 1 : -1);
                            int signY = dy == 0 ? 0 : (dy > 0 ? 1 : -1);
                            dir = new _prototype_Point(signX, signY);
                        }
                        else
                        {
                            dir = kbAction.fixedDirection;
                        }

                        if (dir != _prototype_Point.zero)
                        {
                            _prototype_Point curr = target;
                            for (int s = 0; s < kbAction.distance; s++)
                            {
                                _prototype_Point next = curr + dir;
                                if (_prototype_GridManager.Instance != null &&
                                    _prototype_GridManager.Instance.IsWithinBounds(next))
                                {
                                    var pv = _prototype_GridManager.Instance.GetPointView(next);
                                    if (pv != null && pv.CanPlaceEntity(defender))
                                    {
                                        curr = next;
                                        continue;
                                    }
                                }
                                break;
                            }
                            if (curr != target)
                            {
                                intent.knockbackLandingPoint = curr;
                            }
                        }
                    }
                }
                else if (hasStatus)
                {
                    intent.intentType = _prototype_EnemyIntentType.AttackDebuff;
                }
                else
                {
                    intent.intentType = _prototype_EnemyIntentType.AttackMelee;
                }
            }
            else
            {
                intent.intentType = _prototype_EnemyIntentType.AttackMelee;
            }

            return intent;
        }

        // ─── HUD 표시용 데이터 ───

        /// <summary>
        /// HUD 의도 배지에 표시할 단일 라인 텍스트를 반환합니다. (예: "공격 14", "원거리 12", "이동", "대기")
        /// neodgm 등 픽셀 폰트에서도 깨지지 않는 안전한 한글/숫자 조합을 제공합니다.
        /// </summary>
        public string GetIntentDisplayText()
        {
            string dmgText = estimatedDamage > 0 ? $" {estimatedDamage}" : "";
            return intentType switch
            {
                _prototype_EnemyIntentType.AttackMelee => $"공격{dmgText}",
                _prototype_EnemyIntentType.AttackRanged => $"원거리{dmgText}",
                _prototype_EnemyIntentType.AttackKnockback => $"넉백{dmgText}",
                _prototype_EnemyIntentType.AttackDebuff => $"약화{dmgText}",
                _prototype_EnemyIntentType.Move => "이동",
                _prototype_EnemyIntentType.Rest => "",
                _prototype_EnemyIntentType.Stunned => "기절",
                _prototype_EnemyIntentType.Flee => "도주",
                _ => ""
            };
        }

        /// <summary>현재 intentType에 맞는 약어 심볼 문자를 반환합니다.</summary>
        public string GetSymbol()
        {
            return intentType switch
            {
                _prototype_EnemyIntentType.AttackMelee => "공",
                _prototype_EnemyIntentType.AttackRanged => "원",
                _prototype_EnemyIntentType.AttackKnockback => "넉",
                _prototype_EnemyIntentType.AttackDebuff => "약",
                _prototype_EnemyIntentType.Move => "이",
                _prototype_EnemyIntentType.Rest => "대",
                _prototype_EnemyIntentType.Stunned => "기",
                _prototype_EnemyIntentType.Flee => "도",
                _ => ""
            };
        }

        /// <summary>현재 intentType에 맞는 테마 컬러를 반환합니다.</summary>
        public Color GetColor()
        {
            return intentType switch
            {
                _prototype_EnemyIntentType.AttackMelee => new Color(1f, 0.35f, 0.35f),
                _prototype_EnemyIntentType.AttackRanged => new Color(1f, 0.65f, 0.2f),
                _prototype_EnemyIntentType.AttackKnockback => new Color(0.9f, 0.4f, 1f),
                _prototype_EnemyIntentType.AttackDebuff => new Color(0.4f, 0.9f, 0.3f),
                _prototype_EnemyIntentType.Move => new Color(0.35f, 0.85f, 1f),
                _prototype_EnemyIntentType.Rest => new Color(0.75f, 0.85f, 0.75f),
                _prototype_EnemyIntentType.Stunned => new Color(1f, 0.9f, 0.2f),
                _prototype_EnemyIntentType.Flee => new Color(0.8f, 0.5f, 1f),
                _ => Color.clear
            };
        }

        /// <summary>한국어 표시명을 반환합니다.</summary>
        public string GetDisplayName()
        {
            return intentType switch
            {
                _prototype_EnemyIntentType.AttackMelee => "근접 공격",
                _prototype_EnemyIntentType.AttackRanged => "원거리 공격",
                _prototype_EnemyIntentType.AttackKnockback => "넉백 공격",
                _prototype_EnemyIntentType.AttackDebuff => "약화 공격",
                _prototype_EnemyIntentType.Move => "이동",
                _prototype_EnemyIntentType.Rest => "대기",
                _prototype_EnemyIntentType.Stunned => "기절",
                _prototype_EnemyIntentType.Flee => "도주",
                _ => ""
            };
        }
    }
}
