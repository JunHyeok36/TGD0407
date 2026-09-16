using System.Collections.Generic;

namespace TDG0407._prototype
{
    /// <summary>
    /// 엔티티가 사망하거나 전리품을 드랍할 때 발행되는 이벤트
    /// </summary>
    public struct _prototype_EntityLootDroppedEvent
    {
        public _prototype_LifeData DeadEntity { get; }
        public _prototype_Point DropPoint { get; }
        public IReadOnlyList<_prototype_ItemStack> DroppedItems { get; }

        public _prototype_EntityLootDroppedEvent(
            _prototype_LifeData deadEntity, 
            _prototype_Point dropPoint, 
            IReadOnlyList<_prototype_ItemStack> droppedItems)
        {
            DeadEntity = deadEntity;
            DropPoint = dropPoint;
            DroppedItems = droppedItems;
        }
    }
}
