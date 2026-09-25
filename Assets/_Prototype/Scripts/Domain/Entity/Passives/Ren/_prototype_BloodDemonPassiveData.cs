using System;
using System.Linq;
using UnityEngine;

namespace TDG0407._prototype
{
    /// <summary>
    /// 선택 패시브: 혈귀 (Blood Demon)
    /// - 체력이 50% 미만일 때, 적을 처치할 때마다 [25 + 잃은 체력 30%] 만큼 체력 회복
    /// - 회복 후 체력이 최대치의 50%를 넘을 수 없음
    /// </summary>
    [Serializable]
    public class _prototype_BloodDemonPassiveData : _prototype_LifePassiveData
    {
        public override string PassiveId => "passive_blood_demon";

        static _prototype_BloodDemonPassiveData()
            => _prototype_LifePassiveRegistry.Register("passive_blood_demon", () => new _prototype_BloodDemonPassiveData());

        public override void OnKillConfirmed(_prototype_LifeData owner, _prototype_EntityData killed)
        {
            if (owner.health == null) return;

            float hpRatio = (float)owner.health.Current / owner.health.Max;
            if (hpRatio >= 0.5f) return; // 체력 50% 이상이면 발동 안 함

            int missing = owner.health.Max - owner.health.Current;
            int healAmount = Mathf.RoundToInt(25f + missing * 0.3f);

            // 회복 후 50% 캡 적용
            int cap = Mathf.FloorToInt(owner.health.Max * 0.5f);
            int newHp = Mathf.Min(owner.health.Current + healAmount, cap);
            int actualHeal = newHp - owner.health.Current;

            if (actualHeal > 0)
            {
                owner.health.Current = newHp;
                SpawnFloating(owner, $"혈귀: +{actualHeal}!", new Color(0.9f, 0.2f, 0.2f));
            }
        }

        private void SpawnFloating(_prototype_LifeData owner, string text, Color color)
        {
            if (_prototype_GridManager.Instance == null) return;
            var pv = _prototype_GridManager.Instance.GetPointView(owner.point);
            if (pv == null) return;
            var ev = pv.PlacedEntityViews.FirstOrDefault(v => v?.EntityData == owner);
            if (ev != null) _prototype_FloatingText.SpawnOnEntity(ev, text, color);
        }

        public override _prototype_LifePassiveData Clone() => new _prototype_BloodDemonPassiveData();
    }
}
