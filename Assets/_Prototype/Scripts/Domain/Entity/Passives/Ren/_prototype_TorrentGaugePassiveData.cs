using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TDG0407._prototype
{
    /// <summary>
    /// 선택 패시브: 폭류 축적
    /// - 기류 축적의 최대 스택이 5스택으로, 지속 시간이 10틱으로 변경
    /// - 기류 축적을 소모할 때(최대 스택 도달 포함)마다 3틱간 피해 증가 +10% 효과 (중첩 가능)
    /// - 칼바람이 [80 + 공격력 120% + 주문력 300%] 물리 피해로 변경
    ///
    /// 구현 방식: LifeData 초기화 시 고유 패시브(FlowGaugePassiveData)를 TorrentFlowGaugePassiveData로 교체합니다.
    /// 이 패시브가 장착되면 OnTick에서 고유 패시브를 업그레이드합니다.
    /// </summary>
    [Serializable]
    public class _prototype_TorrentGaugePassiveData : _prototype_LifePassiveData
    {
        public override string PassiveId => "passive_torrent_gauge";

        [NonSerialized] private bool _applied = false;

        // 피해 증가 버프 스택 (각 스택은 남은 틱 수)
        [NonSerialized] private int _damageBonusStacks = 0;
        [NonSerialized] private int[] _stackTimers = new int[10]; // 최대 10개 스택 추적

        static _prototype_TorrentGaugePassiveData()
            => _prototype_LifePassiveRegistry.Register("passive_torrent_gauge", () => new _prototype_TorrentGaugePassiveData());

        /// <summary>고유 패시브를 TorrentFlowGaugePassiveData로 업그레이드합니다.</summary>
        private void EnsureApplied(_prototype_LifeData owner)
        {
            if (_applied) return;
            if (owner.uniquePassive is _prototype_FlowGaugePassiveData flowGauge
                && flowGauge is not _prototype_TorrentFlowGaugePassiveData)
            {
                owner.uniquePassive = new _prototype_TorrentFlowGaugePassiveData(this);
                _applied = true;
            }
        }

        public override Cysharp.Threading.Tasks.UniTask OnTick(_prototype_LifeData owner)
        {
            EnsureApplied(owner);

            // 피해 증가 스택 타이머 감소
            for (int i = 0; i < _stackTimers.Length; i++)
            {
                if (_stackTimers[i] > 0) _stackTimers[i]--;
            }
            _damageBonusStacks = 0;
            for (int i = 0; i < _stackTimers.Length; i++)
                if (_stackTimers[i] > 0) _damageBonusStacks++;

            return Cysharp.Threading.Tasks.UniTask.CompletedTask;
        }

        /// <summary>기류 소모 시 피해 증가 스택 추가 (TorrentFlowGaugePassiveData에서 호출)</summary>
        public void OnFlowConsumed(_prototype_LifeData owner)
        {
            // 빈 슬롯에 3틱 스택 추가
            for (int i = 0; i < _stackTimers.Length; i++)
            {
                if (_stackTimers[i] <= 0)
                {
                    _stackTimers[i] = 3;
                    _damageBonusStacks++;
                    break;
                }
            }
            SpawnFloating(owner, $"폭류: 피해 +{_damageBonusStacks * 10}%!", new Color(1f, 0.6f, 0.1f));
        }

        /// <summary>현재 피해 증가 배율 (0.0 ~ N*0.1)</summary>
        public float GetDamageBonusRatio() => _damageBonusStacks * 0.1f;

        private void SpawnFloating(_prototype_LifeData owner, string text, Color color)
        {
            if (_prototype_GridManager.Instance == null) return;
            var pv = _prototype_GridManager.Instance.GetPointView(owner.point);
            if (pv == null) return;
            var ev = pv.PlacedEntityViews.FirstOrDefault(v => v?.EntityData == owner);
            if (ev != null) _prototype_FloatingText.SpawnOnEntity(ev, text, color);
        }

        public override _prototype_LifePassiveData Clone() => new _prototype_TorrentGaugePassiveData();
    }

    /// <summary>
    /// 폭류 축적 적용 시 고유 패시브를 대체하는 업그레이드 버전 FlowGaugePassiveData.
    /// </summary>
    [Serializable]
    public class _prototype_TorrentFlowGaugePassiveData : _prototype_FlowGaugePassiveData
    {
        // 폭류 축적 패시브 참조 (피해 증가 스택 추가를 위해)
        [NonSerialized] private _prototype_TorrentGaugePassiveData _torrentPassive;

        public override int MaxFlowStack => 5;
        public override int FlowTimerDuration => 10;
        public override float BitingWindBase => 80f;
        public override float BitingWindRedPowerCoeff => 1.2f;

        // 주문력 계수 추가
        public float BitingWindBluePowerCoeff => 3.0f;

        public _prototype_TorrentFlowGaugePassiveData() { }
        public _prototype_TorrentFlowGaugePassiveData(_prototype_TorrentGaugePassiveData torrentPassive)
        {
            _torrentPassive = torrentPassive;
        }

        protected override void ActivateBitingWind(_prototype_LifeData owner)
        {
            base.ActivateBitingWind(owner);
            // 기류 소모 시 피해 증가 스택 부여
            _torrentPassive?.OnFlowConsumed(owner);
        }

        protected override UniTask TriggerBitingWindEffect(_prototype_LifeData owner)
        {
            return TriggerBitingWindEffect(owner, GetEnemiesAroundPoint(owner, owner.point, 1));
        }

        protected override async UniTask TriggerBitingWindEffect(_prototype_LifeData owner, List<_prototype_LifeData> hitTargets)
        {
            if (_prototype_GridManager.Instance == null) return;
            if (hitTargets == null || hitTargets.Count == 0) return;

            int damage = UnityEngine.Mathf.RoundToInt(
                BitingWindBase
                + owner.RedPower * BitingWindRedPowerCoeff
                + owner.BluePower * BitingWindBluePowerCoeff);

            // 1. 각 적중 대상의 위치를 중심으로 3×3 칼바람 폭풍 VFX 소환
            foreach (var hitTarget in hitTargets)
            {
                if (hitTarget == null) continue;
                var pv = _prototype_GridManager.Instance.GetPointView(hitTarget.point);
                Vector3 spawnPos = pv != null ? pv.transform.position : Vector3.zero;
                _prototype_BitingWindVFX.Spawn(spawnPos, 3);
            }

            // 2. Step 1 임팩트 대기 (약 0.15초 지면 참격 폭발 시점)
            await UniTask.Delay(150);

            // 3. Step 2 피해 적용: 각 적중 대상 주변 1칸(3×3, 적중 대상 본인 포함) 내 모든 적에게 피해
            var tasks = new List<UniTask>();
            var allAffectedEnemies = new HashSet<_prototype_LifeData>();

            foreach (var hitTarget in hitTargets)
            {
                if (hitTarget == null) continue;
                var splashEnemies = GetEnemiesAroundPoint(owner, hitTarget.point, 1);
                foreach (var enemy in splashEnemies)
                {
                    if (enemy == null || enemy.IsDead) continue;
                    allAffectedEnemies.Add(enemy);

                    var ctx = new _prototype_DamageContext(
                        owner,
                        enemy,
                        _prototype_DamageType.Physical,
                        damage,
                        damage,
                        isCritical: false,
                        spDamageMultiplier: 0.5f
                    )
                    {
                        isBitingWindDamage = true // 칼바람 폭풍 추가 피해로 인한 기류 무한 스택 방지
                    };
                    tasks.Add(_prototype_InteractionManager.ApplyDamage(ctx));
                }
            }

            await UniTask.WhenAll(tasks);

            // 에어본 적용 (1틱)
            foreach (var enemy in allAffectedEnemies)
            {
                if (enemy != null && !enemy.IsDead)
                    enemy.ApplyStatusEffect(new _prototype_StatusEffect(_prototype_StatusType.Airborne, 1));
            }
        }

        public override _prototype_LifePassiveData Clone() => new _prototype_TorrentFlowGaugePassiveData(_torrentPassive);
    }
}
