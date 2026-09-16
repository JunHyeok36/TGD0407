using System.Collections.Generic;

namespace TDG0407._prototype
{
    /// <summary>
    /// 모든 인벤토리(플레이어 풀 인벤토리, 몬스터 경량 전리품 인벤토리 등)의 공통 인터페이스
    /// </summary>
    public interface _prototype_IInventoryData
    {
        /// <summary>
        /// 인벤토리에 보관된 총 아이템 수량의 합
        /// </summary>
        int TotalItemCount { get; }

        /// <summary>
        /// 보관 중인 모든 유효 아이템 스택 목록 반환
        /// </summary>
        IReadOnlyList<_prototype_ItemStack> GetAllItems();

        /// <summary>
        /// 특정 아이템의 총 보유 수량 반환
        /// </summary>
        int CountOf(string itemId);

        /// <summary>
        /// 특정 아이템을 지정 수량 이상 보유하고 있는지 확인
        /// </summary>
        bool HasItem(string itemId, int quantity = 1);

        /// <summary>
        /// 아이템 추가 (ItemType에 따른 자동 분기 및 스택 처리)
        /// 실제 추가된 수량 반환
        /// </summary>
        int AddItem(_prototype_ItemDataModel itemModel, int quantity);

        /// <summary>
        /// 아이템 차감/소모 (보유 수량이 충분할 때만 차감하고 true 반환)
        /// </summary>
        bool ConsumeItem(string itemId, int quantity);

        /// <summary>
        /// 인벤토리 깊은 복사
        /// </summary>
        _prototype_IInventoryData Clone();
    }
}
