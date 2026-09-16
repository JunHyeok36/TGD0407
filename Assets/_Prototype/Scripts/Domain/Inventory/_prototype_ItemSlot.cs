using System;

namespace TDG0407._prototype
{
    /// <summary>
    /// 가방의 개별 슬롯 (슬롯 번호 + 아이템 스택 보관)
    /// </summary>
    [Serializable]
    public class _prototype_ItemSlot
    {
        public int slotIndex;
        public _prototype_ItemStack item;

        public bool IsEmpty => item == null || item.quantity <= 0 || string.IsNullOrEmpty(item.itemId);

        public _prototype_ItemSlot()
        {
            this.slotIndex = 0;
            this.item = null;
        }

        public _prototype_ItemSlot(int slotIndex, _prototype_ItemStack item = null)
        {
            this.slotIndex = slotIndex;
            this.item = item;
        }

        public _prototype_ItemSlot(_prototype_ItemSlot other)
        {
            if (other != null)
            {
                this.slotIndex = other.slotIndex;
                this.item = other.item?.Clone();
            }
        }

        public void Set(_prototype_ItemStack newItem)
        {
            this.item = newItem;
            if (this.item != null && this.item.quantity <= 0)
            {
                this.item = null;
            }
        }

        public _prototype_ItemStack Pop()
        {
            var popped = item;
            item = null;
            return popped;
        }

        public void Clear()
        {
            item = null;
        }

        public _prototype_ItemSlot Clone()
        {
            return new _prototype_ItemSlot(this);
        }
    }
}
