using System;
using UnityEngine;
using System.Linq;

namespace TDG0407._prototype
{
    /// <summary>
    /// 선택 패시브: 결의
    /// - 적을 처치할 때마다 스테미나를 2만큼 회복
    /// - 스테미나가 최대치일 경우, 물리 방어력과 마법 저항력이 +40%
    /// </summary>
    [Serializable]
    public class _prototype_ResolutePassiveData : _prototype_LifePassiveData
    {
        public override string PassiveId => "passive_resolute";

        private const int KillStaminaRecover = 20;
        private const float MaxStaminaDefenseBonus = 0.4f; // 40%

        // 방어 보너스 활성 여부 (틱마다 갱신)
        [NonSerialized] private bool _defenseBoostActive = false;

        static _prototype_ResolutePassiveData()
            => _prototype_LifePassiveRegistry.Register("passive_resolute", () => new _prototype_ResolutePassiveData());

        public override void OnKillConfirmed(_prototype_LifeData owner, _prototype_EntityData killed)
        {
            if (owner.stamina == null) return;
            int recovered = Mathf.Min(KillStaminaRecover, owner.stamina.Max - owner.stamina.Current);
            if (recovered > 0)
            {
                owner.stamina.Current += recovered;
                SpawnFloating(owner, $"+{recovered} 체간!", new Color(0.4f, 0.9f, 0.5f));
            }
        }

        public override Cysharp.Threading.Tasks.UniTask OnTick(_prototype_LifeData owner)
        {
            if (owner.stamina == null || owner.lifeStat == null)
                return Cysharp.Threading.Tasks.UniTask.CompletedTask;

            bool isMaxStamina = owner.stamina.Current >= owner.stamina.Max;
            if (isMaxStamina && !_defenseBoostActive)
            {
                // 보너스 적용
                int bonusRed = Mathf.RoundToInt(owner.lifeStat.redResist * MaxStaminaDefenseBonus);
                int bonusBlue = Mathf.RoundToInt(owner.lifeStat.blueResist * MaxStaminaDefenseBonus);
                owner.lifeStat.redResist += bonusRed;
                owner.lifeStat.blueResist += bonusBlue;
                _defenseBoostActive = true;
                SpawnFloating(owner, "결의: 방어 증가!", new Color(0.5f, 0.8f, 1f));
            }
            else if (!isMaxStamina && _defenseBoostActive)
            {
                // 보너스 제거: 원래 값으로 복원 (1/(1+ratio))
                owner.lifeStat.redResist = Mathf.RoundToInt(owner.lifeStat.redResist / (1f + MaxStaminaDefenseBonus));
                owner.lifeStat.blueResist = Mathf.RoundToInt(owner.lifeStat.blueResist / (1f + MaxStaminaDefenseBonus));
                _defenseBoostActive = false;
            }

            return Cysharp.Threading.Tasks.UniTask.CompletedTask;
        }

        private void SpawnFloating(_prototype_LifeData owner, string text, Color color)
        {
            if (_prototype_GridManager.Instance == null) return;
            var pv = _prototype_GridManager.Instance.GetPointView(owner.point);
            if (pv == null) return;
            var ev = pv.PlacedEntityViews.FirstOrDefault(v => v?.EntityData == owner);
            if (ev != null) _prototype_FloatingText.SpawnOnEntity(ev, text, color);
        }

        public override _prototype_LifePassiveData Clone() => new _prototype_ResolutePassiveData();
    }
}
