using UnityEngine;

namespace TDG0407._prototype
{
    /// <summary>
    /// 인벤토리 데이터 생성을 위한 추상 ScriptableObject 기본 클래스
    /// </summary>
    public abstract class _prototype_InventoryDataModel : ScriptableObject
    {
        /// <summary>
        /// 런타임에 사용할 인벤토리 데이터 인스턴스를 생성합니다.
        /// </summary>
        public abstract _prototype_IInventoryData CreateInventoryData();
    }
}
