using System;
using System.Collections.Generic;
using System.Linq;

namespace TDG0407._prototype
{
    /// <summary>
    /// 슬롯 배열 기반 일반 인벤토리 가방
    /// </summary>
    [Serializable]
    public class _prototype_Bag
    {
        public _prototype_ItemSlot[] slots;

        public int SlotCount => slots != null ? slots.Length : 0;
        public IReadOnlyList<_prototype_ItemSlot> Slots => slots;
        public int UsedSlotCount => slots != null ? slots.Count(s => !s.IsEmpty) : 0;
        public bool IsFull => UsedSlotCount >= SlotCount;

        public _prototype_Bag(int slotCount = 16)
        {
            slotCount = Math.Max(1, slotCount);
            slots = new _prototype_ItemSlot[slotCount];
            for (int i = 0; i < slotCount; i++)
            {
                slots[i] = new _prototype_ItemSlot(i);
            }
        }

        public _prototype_Bag(_prototype_Bag other)
        {
            if (other != null && other.slots != null)
            {
                slots = new _prototype_ItemSlot[other.slots.Length];
                for (int i = 0; i < other.slots.Length; i++)
                {
                    slots[i] = other.slots[i].Clone();
                }
            }
            else
            {
                slots = new _prototype_ItemSlot[0];
            }
        }

        /// <summary>
        /// 아이템 스택을 가방에 추가합니다.
        /// 기존 동일 ID 슬롯에 먼저 누적하고, 잔여 수량은 빈 슬롯에 배정합니다.
        /// 실제 추가된 수량을 반환합니다.
        /// </summary>
        public int Add(_prototype_ItemStack stack, int maxStack = 99)
        {
            if (stack == null || stack.quantity <= 0 || string.IsNullOrEmpty(stack.itemId))
                return 0;

            maxStack = Math.Max(1, maxStack);
            int remaining = stack.quantity;

            // 1단계: 기존 동일 아이템 슬롯에 누적 적재
            for (int i = 0; i < slots.Length && remaining > 0; i++)
            {
                var slot = slots[i];
                if (!slot.IsEmpty && slot.item.itemId == stack.itemId && slot.item.quantity < maxStack)
                {
                    int space = maxStack - slot.item.quantity;
                    int toAdd = Math.Min(space, remaining);
                    slot.item.AddQuantity(toAdd);
                    remaining -= toAdd;
                }
            }

            // 2단계: 빈 슬롯에 새 스택 배치
            for (int i = 0; i < slots.Length && remaining > 0; i++)
            {
                var slot = slots[i];
                if (slot.IsEmpty)
                {
                    int toAdd = Math.Min(maxStack, remaining);
                    slot.Set(new _prototype_ItemStack(stack.itemId, toAdd));
                    remaining -= toAdd;
                }
            }

            int added = stack.quantity - remaining;
            return added;
        }

        /// <summary>
        /// 특정 슬롯에서 지정한 수량만큼 아이템을 제거합니다.
        /// 실제 제거된 수량을 반환합니다.
        /// </summary>
        public int RemoveAt(int slotIndex, int quantity)
        {
            if (slotIndex < 0 || slotIndex >= slots.Length || quantity <= 0)
                return 0;

            var slot = slots[slotIndex];
            if (slot.IsEmpty)
                return 0;

            int removed = slot.item.RemoveQuantity(quantity);
            if (slot.item.quantity <= 0)
            {
                slot.Clear();
            }
            return removed;
        }

        /// <summary>
        /// 두 슬롯의 아이템을 서로 맞바꿉니다. (UI 드래그 & 드롭 지원)
        /// </summary>
        public void Swap(int slotA, int slotB)
        {
            if (slotA < 0 || slotA >= slots.Length || slotB < 0 || slotB >= slots.Length || slotA == slotB)
                return;

            var temp = slots[slotA].item;
            slots[slotA].Set(slots[slotB].item);
            slots[slotB].Set(temp);
        }

        /// <summary>
        /// 가방 내 특정 아이템의 총 보유 수량을 조회합니다.
        /// </summary>
        public int CountOf(string itemId)
        {
            if (string.IsNullOrEmpty(itemId) || slots == null)
                return 0;

            int total = 0;
            foreach (var slot in slots)
            {
                if (!slot.IsEmpty && slot.item.itemId == itemId)
                {
                    total += slot.item.quantity;
                }
            }
            return total;
        }

        /// <summary>
        /// 가방에 보관된 모든 유효 아이템 스택 목록을 반환합니다.
        /// </summary>
        public List<_prototype_ItemStack> GetAllItems()
        {
            var list = new List<_prototype_ItemStack>();
            if (slots == null) return list;

            foreach (var slot in slots)
            {
                if (!slot.IsEmpty)
                {
                    list.Add(slot.item.Clone());
                }
            }
            return list;
        }

        public _prototype_Bag Clone()
        {
            return new _prototype_Bag(this);
        }
    }
}
