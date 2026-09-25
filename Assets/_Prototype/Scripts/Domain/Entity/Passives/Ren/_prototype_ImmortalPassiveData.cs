using System;
using System.Linq;
using UnityEngine;

namespace TDG0407._prototype
{
    /// <summary>
    /// 선택 패시브: 불사
    /// - 죽음의 문턱 상태 효과 동안 받는 디버프가 100% 증가
    /// - 죽음의 문턱 상태 효과 동안 크리티컬 확률 +50%, 크리티컬 배율 +100% 증가
    /// </summary>
    [Serializable]
    public class _prototype_ImmortalPassiveData : _prototype_LifePassiveData
    {
        public override string PassiveId => "passive_immortal";

        private const float CritProbBonus = 0.5f;     // +50%
        private const int CritWeightBonus = 100;      // +100%

        [NonSerialized] private bool _bonusApplied = false;

        static _prototype_ImmortalPassiveData()
            => _prototype_LifePassiveRegistry.Register("passive_immortal", () => new _prototype_ImmortalPassiveData());

        public override Cysharp.Threading.Tasks.UniTask OnTick(_prototype_LifeData owner)
        {
            bool atDeathsDoor = owner.IsAtDeathsDoor;

            if (atDeathsDoor && !_bonusApplied)
            {
                ApplyBonus(owner);
            }
            else if (!atDeathsDoor && _bonusApplied)
            {
                RemoveBonus(owner);
            }

            return Cysharp.Threading.Tasks.UniTask.CompletedTask;
        }

        private void ApplyBonus(_prototype_LifeData owner)
        {
            if (owner.lifeStat == null) return;
            owner.lifeStat.criticalProb += CritProbBonus;
            owner.lifeStat.criticalWeight += CritWeightBonus;
            _bonusApplied = true;
            SpawnFloating(owner, "불사: 치명타 급증!", new Color(1f, 0.3f, 0.1f), 1.2f);
        }

        private void RemoveBonus(_prototype_LifeData owner)
        {
            if (owner.lifeStat == null) return;
            owner.lifeStat.criticalProb = Mathf.Max(0f, owner.lifeStat.criticalProb - CritProbBonus);
            owner.lifeStat.criticalWeight = Mathf.Max(0, owner.lifeStat.criticalWeight - CritWeightBonus);
            _bonusApplied = false;
        }

        /// <summary>
        /// 죽음의 문턱 상태에서 받는 디버프 확률을 2배로 적용.
        /// 이 메서드는 StatusEffect 적용 전에 외부에서 호출할 수 있습니다.
        /// (현재는 메모의 역할; 실제 디버프 증폭은 향후 TakeDamage 파이프라인에 통합 예정)
        /// </summary>
        public float GetDebuffResistMultiplier(_prototype_LifeData owner)
        {
            if (owner.IsAtDeathsDoor) return 2.0f; // 디버프 100% 증가 = 저항 0.5배
            return 1.0f;
        }

        private void SpawnFloating(_prototype_LifeData owner, string text, Color color, float sizeMul = 1f)
        {
            if (_prototype_GridManager.Instance == null) return;
            var pv = _prototype_GridManager.Instance.GetPointView(owner.point);
            if (pv == null) return;
            var ev = pv.PlacedEntityViews.FirstOrDefault(v => v?.EntityData == owner);
            if (ev != null) _prototype_FloatingText.SpawnOnEntity(ev, text, color, sizeMul);
        }

        public override _prototype_LifePassiveData Clone() => new _prototype_ImmortalPassiveData();
    }
}
