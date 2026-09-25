using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TDG0407._prototype
{
    /// <summary>
    /// 렌 고유 패시브: 기류 축적 (Flow Gauge) + 칼바람 (Biting Wind)
    ///
    /// [기류 축적]
    ///  - 적을 적중시켜 피해를 입힐 때마다 스택 중첩 (기본 최대 3스택)
    ///  - 스택이 최대치 도달 시 초기화, 다음 Attack 카드 사용 시 칼바람 발동
    ///  - 스택이 중첩될 때마다 지속 시간 5틱으로 갱신 (만료 시 스택 모두 초기화)
    ///
    /// [칼바람]
    ///  - 지속 시간 10틱 (중첩 시 지속 시간만 갱신)
    ///  - 다음 Attack 카드 사용 시 발동: SP 소모 0으로 변경
    ///  - 발동 시 적중 대상과 1칸 반경 내 모든 적에게 [60 + 공격력 75%] 물리 피해 + 1틱 에어본
    /// </summary>
    [Serializable]
    public class _prototype_FlowGaugePassiveData : _prototype_LifePassiveData
    {
        public override string PassiveId => "passive_flow_gauge_unique";

        // ─── 설정값 (파생 클래스에서 변경 가능) ──────────────────────────────
        public virtual int MaxFlowStack => 3;
        public virtual int FlowTimerDuration => 5;
        public virtual int BitingWindDuration => 10;

        // ─── 칼바람 피해 계수 ───────────────────────────────────────────────
        public virtual float BitingWindBase => 60f;
        public virtual float BitingWindRedPowerCoeff => 0.75f;

        // ─── 내부 상태 ──────────────────────────────────────────────────────
        [NonSerialized] private int _flowStack = 0;
        [NonSerialized] private int _flowTimer = 0;
        [NonSerialized] private bool _bitingWindActive = false;
        [NonSerialized] private int _bitingWindTimer = 0;

        // 외부 읽기용 프로퍼티
        public int FlowStack => _flowStack;
        public bool BitingWindActive => _bitingWindActive;

        [NonSerialized] private static Sprite s_flowIcon;
        [NonSerialized] private static Sprite s_windIcon;

        private static Sprite GetFlowIcon()
        {
            if (s_flowIcon == null) s_flowIcon = Resources.Load<Sprite>("HUD/Icon_Buff");
            return s_flowIcon;
        }

        private static Sprite GetWindIcon()
        {
            if (s_windIcon == null) s_windIcon = Resources.Load<Sprite>("HUD/Icon_Sword");
            return s_windIcon;
        }

        public override System.Collections.Generic.IEnumerable<StatusDisplayData> GetDisplayStatuses(_prototype_StatusVisualDatabase db)
        {
            if (_flowStack > 0 && !_bitingWindActive)
            {
                yield return new StatusDisplayData
                {
                    id = "FlowGauge",
                    displayName = "기류 축적",
                    symbolChar = "기",
                    icon = GetFlowIcon(),
                    themeColor = new Color(0.6f, 0.9f, 1f),
                    description = "적중 시 기류 스택을 쌓습니다. (최대 3스택)",
                    stackCount = _flowStack,
                    durationTicks = _flowTimer,
                    isForever = false
                };
            }
            if (_bitingWindActive)
            {
                yield return new StatusDisplayData
                {
                    id = "BitingWind",
                    displayName = "칼바람",
                    symbolChar = "칼",
                    icon = GetWindIcon(),
                    themeColor = new Color(0.4f, 0.7f, 1f),
                    description = "다음 Attack 카드 사용 시 칼바람이 발동합니다.",
                    stackCount = 1,
                    durationTicks = _bitingWindTimer,
                    isForever = false
                };
            }
        }

        public void ResetFlowGauge()
        {
            _flowStack = 0;
            _flowTimer = 0;
            _bitingWindActive = false;
            _bitingWindTimer = 0;
        }

        // ─── OnDamageDealt: 기류 스택 누적 ──────────────────────────────────
        public override void OnDamageDealt(_prototype_LifeData owner, _prototype_DamageContext context)
        {
            // 실제로 피해가 들어간 경우만 스택 증가
            if (!context.finalDamage.HasValue || context.finalDamage.Value <= 0) return;
            // 아군 공격 제외
            if (context.target is _prototype_LifeData targetLife && owner.side == targetLife.side) return;

            AddFlowStack(owner);
        }

        public void AddFlowStack(_prototype_LifeData owner)
        {
            // 스택 중첩 시 지속 시간 갱신
            _flowTimer = FlowTimerDuration;
            _flowStack++;

            SpawnFloatingOnOwner(owner, $"기류 {_flowStack}!", new Color(0.6f, 0.9f, 1f));

            if (_flowStack >= MaxFlowStack)
            {
                _flowStack = 0;
                _flowTimer = 0;
                ActivateBitingWind(owner);
            }
        }

        // ─── 칼바람 활성화 ───────────────────────────────────────────────────
        protected virtual void ActivateBitingWind(_prototype_LifeData owner)
        {
            _bitingWindTimer = BitingWindDuration; // 중첩 시 지속 시간만 갱신
            _bitingWindActive = true;
            SpawnFloatingOnOwner(owner, "칼바람!", new Color(0.4f, 0.85f, 1f), 1.2f);
        }

        // ─── OnBeforeCardUse: SP 소모 0 오버라이드 ──────────────────────────
        public override int OnBeforeCardUse(_prototype_LifeData owner, _prototype_BattleCardData card)
        {
            if (!_bitingWindActive) return -1;
            // Attack 유형 카드에만 적용
            if (card.cardType != _prototype_CardType.Attack) return -1;
            return 0; // SP 소모 0
        }

        // ─── IsCardEmpowered: 카드 강조 표시 ──────────────────────────────────
        public override bool IsCardEmpowered(_prototype_LifeData owner, _prototype_CardData card)
        {
            if (_bitingWindActive && card is _prototype_BattleCardData bCard && bCard.cardType == _prototype_CardType.Attack)
            {
                return true;
            }
            if (owner.HasStatusEffect(_prototype_StatusType.EnhanceStab) && card.id == "icd_ren_stab")
            {
                return true;
            }
            return base.IsCardEmpowered(owner, card);
        }

        // ─── OnCardUsed: 칼바람 발동 및 소비 ────────────────────────────────
        public override void OnCardUsed(_prototype_LifeData owner, _prototype_BattleCardData card)
        {
            if (!_bitingWindActive) return;
            if (card.cardType != _prototype_CardType.Attack) return;

            _bitingWindActive = false;
            _bitingWindTimer = 0;

            SpawnFloatingOnOwner(owner, "칼바람 발동!", new Color(0.2f, 0.7f, 1f), 1.3f);
            TriggerBitingWindEffect(owner).Forget();
        }

        // ─── 칼바람 효과 실행 ────────────────────────────────────────────────
        protected virtual async UniTask TriggerBitingWindEffect(_prototype_LifeData owner)
        {
            if (_prototype_GridManager.Instance == null) return;

            int bitingWindDamage = Mathf.RoundToInt(BitingWindBase + owner.RedPower * BitingWindRedPowerCoeff);

            // 1칸 반경 이내 적 탐색
            var targets = GetEnemiesInRadius(owner, 1);
            if (targets.Count == 0) return;

            // 1. 각 타겟 위치를 중심으로 3×3 칼바람 폭풍 VFX 소환
            foreach (var target in targets)
            {
                if (target == null) continue;
                var pv = _prototype_GridManager.Instance.GetPointView(target.point);
                Vector3 spawnPos = pv != null ? pv.transform.position : Vector3.zero;
                _prototype_BitingWindVFX.Spawn(spawnPos, 3);
            }

            // 2. Step 1 임팩트 대기 (약 0.15초 지면 참격 폭발 시점)
            await UniTask.Delay(150);

            // 3. Step 2 피해 적용 및 에어본 부여
            var tasks = new List<UniTask>();
            foreach (var target in targets)
            {
                if (target == null || target.IsDead) continue;

                var ctx = new _prototype_DamageContext(
                    owner,
                    target,
                    _prototype_DamageType.Physical,
                    bitingWindDamage,
                    bitingWindDamage,
                    isCritical: false,
                    spDamageMultiplier: 0.5f
                );
                tasks.Add(_prototype_InteractionManager.ApplyDamage(ctx));
            }

            await UniTask.WhenAll(tasks);

            // 에어본 적용 (1틱)
            foreach (var target in targets)
            {
                if (target == null || target.IsDead) continue;
                if (target is _prototype_LifeData targetLife)
                {
                    var airbornEffect = new _prototype_StatusEffect(_prototype_StatusType.Airborne, 1);
                    targetLife.ApplyStatusEffect(airbornEffect);
                }
            }

            // 4. 회오리가 상승하여 적을 공중에 체공시키는 동안 딜레이 (0.35초)
            await UniTask.Delay(350);
        }

        // ─── 범위 내 적 목록 반환 ────────────────────────────────────────────
        protected List<_prototype_LifeData> GetEnemiesInRadius(_prototype_LifeData owner, int radius)
        {
            var result = new List<_prototype_LifeData>();
            if (_prototype_GridManager.Instance == null) return result;

            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = -radius; dy <= radius; dy++)
                {
                    var pt = new _prototype_Point(owner.point.x + dx, owner.point.y + dy);
                    var pv = _prototype_GridManager.Instance.GetPointView(pt);
                    if (pv == null) continue;
                    foreach (var ev in pv.PlacedEntityViews)
                    {
                        if (ev?.EntityData is _prototype_LifeData ld
                            && ld != owner
                            && !owner.IsSameSide(ld)
                            && !ld.IsDead)
                        {
                            result.Add(ld);
                        }
                    }
                }
            }
            return result.Distinct().ToList();
        }

        // ─── OnTick: 스택/칼바람 타이머 감소 ────────────────────────────────
        public override UniTask OnTick(_prototype_LifeData owner)
        {
            // 기류 만료 카운트
            if (_flowStack > 0)
            {
                _flowTimer--;
                if (_flowTimer <= 0)
                {
                    _flowStack = 0;
                    _flowTimer = 0;
                }
            }

            // 칼바람 만료 카운트
            if (_bitingWindActive)
            {
                _bitingWindTimer--;
                if (_bitingWindTimer <= 0)
                {
                    _bitingWindActive = false;
                    _bitingWindTimer = 0;
                }
            }

            return UniTask.CompletedTask;
        }

        // ─── FloatingText 헬퍼 ───────────────────────────────────────────────
        protected void SpawnFloatingOnOwner(_prototype_LifeData owner, string text, Color color, float sizeMul = 1f)
        {
            if (_prototype_GridManager.Instance == null) return;
            var pv = _prototype_GridManager.Instance.GetPointView(owner.point);
            if (pv == null) return;
            var ev = pv.PlacedEntityViews.FirstOrDefault(v => v?.EntityData == owner);
            if (ev != null)
                _prototype_FloatingText.SpawnOnEntity(ev, text, color, sizeMul);
        }

        public override _prototype_LifePassiveData Clone() => new _prototype_FlowGaugePassiveData();
    }
}
