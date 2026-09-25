using Cysharp.Threading.Tasks;
using System;

namespace TDG0407._prototype
{
    /// <summary>
    /// Life 엔티티의 패시브 기반 클래스.
    /// 고유 패시브(항상 활성)와 선택 패시브(던전 탐험 중 획득) 모두 이 클래스를 상속합니다.
    /// </summary>
    [Serializable]
    public abstract class _prototype_LifePassiveData
    {
        /// <summary>패시브 고유 ID. 직렬화/레지스트리 복원에 사용됩니다.</summary>
        public abstract string PassiveId { get; }

        /// <summary>매 틱 호출 (스택 만료 카운트, 타이머 감소 등).</summary>
        public virtual UniTask OnTick(_prototype_LifeData owner) => UniTask.CompletedTask;

        /// <summary>
        /// 이 패시브 보유자가 적에게 데미지를 확정 입힌 후 호출.
        /// context.finalDamage 에 최종 피해량이 들어있습니다.
        /// </summary>
        public virtual void OnDamageDealt(_prototype_LifeData owner, _prototype_DamageContext context) { }

        /// <summary>
        /// 이 패시브 보유자가 데미지를 수신한 후 호출.
        /// context.finalDamage 에 최종 피해량이 들어있습니다.
        /// </summary>
        public virtual void OnDamageReceived(_prototype_LifeData owner, _prototype_DamageContext context) { }

        /// <summary>
        /// 이 패시브 보유자가 적을 처치한 후 호출.
        /// </summary>
        public virtual void OnKillConfirmed(_prototype_LifeData owner, _prototype_EntityData killed) { }

        /// <summary>
        /// 카드 사용 후(행동 소비 전) 호출. 카드 효과가 모두 실행된 다음 발동됩니다.
        /// </summary>
        public virtual void OnCardUsed(_prototype_LifeData owner, _prototype_BattleCardData card) { }

        /// <summary>
        /// 카드 비용을 차감하기 직전 호출.
        /// 반환값이 0 이상이면 원래 costValue 대신 반환값을 비용으로 사용합니다.
        /// -1을 반환하면 "개입 없음"으로 처리됩니다.
        /// </summary>
        public virtual int OnBeforeCardUse(_prototype_LifeData owner, _prototype_BattleCardData card) => -1;

        /// <summary>
        /// 지정된 카드가 이 패시브에 의해 강화 상태인지 여부를 반환합니다.
        /// </summary>
        public virtual bool IsCardEmpowered(_prototype_LifeData owner, _prototype_CardData card) => false;

        /// <summary>런타임 복제. LifeData 초기화 시 Clone() 을 통해 인스턴스를 생성합니다.</summary>
        public abstract _prototype_LifePassiveData Clone();
    }
}
