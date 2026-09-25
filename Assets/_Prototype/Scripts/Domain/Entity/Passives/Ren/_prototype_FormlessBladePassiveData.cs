using System;
using System.Linq;
using UnityEngine;

namespace TDG0407._prototype
{
    /// <summary>
    /// 선택 패시브: 무형의 검 (Formless Blade)
    /// - 칼바람 피해의 50%가 물리 피해로, 나머지 50%가 마법 피해로 변경
    /// - 칼바람 피해는 대상의 물리 방어력과 마법 저항력을 30% 무시
    ///
    /// 구현 방식: OnTick 시 고유 패시브를 FormlessFlowGaugePassiveData로 교체합니다.
    /// </summary>
    [Serializable]
    public class _prototype_FormlessBladePassiveData : _prototype_LifePassiveData
    {
        public override string PassiveId => "passive_formless_blade";

        [NonSerialized] private bool _applied = false;

        static _prototype_FormlessBladePassiveData()
            => _prototype_LifePassiveRegistry.Register("passive_formless_blade", () => new _prototype_FormlessBladePassiveData());

        private void EnsureApplied(_prototype_LifeData owner)
        {
            if (_applied) return;
            if (owner.uniquePassive is _prototype_FlowGaugePassiveData flowGauge
                && flowGauge is not _prototype_FormlessFlowGaugePassiveData)
            {
                owner.uniquePassive = new _prototype_FormlessFlowGaugePassiveData();
                _applied = true;
            }
        }

        public override Cysharp.Threading.Tasks.UniTask OnTick(_prototype_LifeData owner)
        {
            EnsureApplied(owner);
            return Cysharp.Threading.Tasks.UniTask.CompletedTask;
        }

        public override _prototype_LifePassiveData Clone() => new _prototype_FormlessBladePassiveData();
    }

    /// <summary>
    /// 무형의 검 적용 시 고유 패시브를 대체합니다.
    /// 칼바람 피해를 물리 50% + 마법 50%로 분리하고 방어력 30% 무시를 적용합니다.
    /// </summary>
    [Serializable]
    public class _prototype_FormlessFlowGaugePassiveData : _prototype_FlowGaugePassiveData
    {
        private const float ArmorIgnoreRatio = 0.3f; // 30% 방어 무시

        protected override async Cysharp.Threading.Tasks.UniTask TriggerBitingWindEffect(_prototype_LifeData owner)
        {
            if (_prototype_GridManager.Instance == null) return;

            int totalDamage = UnityEngine.Mathf.RoundToInt(BitingWindBase + owner.RedPower * BitingWindRedPowerCoeff);
            int physDamage = UnityEngine.Mathf.RoundToInt(totalDamage * 0.5f);
            int magicDamage = totalDamage - physDamage;

            var targets = GetEnemiesInRadius(owner, 1);
            var tasks = new System.Collections.Generic.List<Cysharp.Threading.Tasks.UniTask>();

            foreach (var target in targets)
            {
                if (target == null || target.IsDead) continue;

                // 방어력 30% 무시: 피해 계산 전 임시 방어력 감소
                int origRedResist = 0, origBlueResist = 0;
                if (target is _prototype_LifeData tl)
                {
                    origRedResist = tl.lifeStat?.redResist ?? 0;
                    origBlueResist = tl.lifeStat?.blueResist ?? 0;
                    if (tl.lifeStat != null)
                    {
                        tl.lifeStat.redResist = UnityEngine.Mathf.RoundToInt(origRedResist * (1f - ArmorIgnoreRatio));
                        tl.lifeStat.blueResist = UnityEngine.Mathf.RoundToInt(origBlueResist * (1f - ArmorIgnoreRatio));
                    }
                }

                var physCtx = new _prototype_DamageContext(owner, target, _prototype_DamageType.Physical, physDamage, physDamage, false, 0.25f);
                var magicCtx = new _prototype_DamageContext(owner, target, _prototype_DamageType.Magical, magicDamage, magicDamage, false, 0.25f);

                tasks.Add(_prototype_InteractionManager.ApplyDamage(physCtx));
                tasks.Add(_prototype_InteractionManager.ApplyDamage(magicCtx));

                // 방어력 복원
                if (target is _prototype_LifeData tl2 && tl2.lifeStat != null)
                {
                    tl2.lifeStat.redResist = origRedResist;
                    tl2.lifeStat.blueResist = origBlueResist;
                }
            }

            await Cysharp.Threading.Tasks.UniTask.WhenAll(tasks);

            foreach (var target in targets)
            {
                if (target is _prototype_LifeData ld && !ld.IsDead)
                    ld.ApplyStatusEffect(new _prototype_StatusEffect(_prototype_StatusType.Airborne, 1));
            }
        }

        public override _prototype_LifePassiveData Clone() => new _prototype_FormlessFlowGaugePassiveData();
    }
}
