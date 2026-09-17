using Cysharp.Threading.Tasks;

namespace TDG0407._prototype
{
    /// <summary>
    /// 상호작용 가능한 엔티티 또는 컴포넌트가 구현하는 인터페이스입니다.
    /// 상호작용 카드(_prototype_InteractionCardData) 등에서 대상 엔티티와 상호작용할 때 호출됩니다.
    /// </summary>
    public interface _prototype_IInteractable
    {
        /// <summary>
        /// 상호작용을 실행합니다.
        /// </summary>
        /// <param name="caster">상호작용을 시도한 시전자 엔티티</param>
        /// <param name="interactionKey">실행할 상호작용 식별 키 (예: "Disarm", "Detonate", "TurnOn", "Break" 등)</param>
        UniTask Interact(_prototype_EntityData caster, string interactionKey);
    }
}
