using System;
using System.Collections.Generic;
using System.Linq;

namespace TDG0407._prototype
{
    /// <summary>
    /// 일반 몬스터를 위한 초경량 전리품 인벤토리
    /// 16개 슬롯 배열이나 KeyItems 리스트를 할당하지 않고, 오직 1~2개의 드랍 전리품만 가볍게 보관합니다.
    /// </summary>
    [Serializable]
    public class _prototype_LootInventoryData : _prototype_IInventoryData
    {
        public List<_prototype_ItemStack> lootItems;
        public int maxSlots = 2;

        public int TotalItemCount => lootItems != null ? lootItems.Sum(i => i.quantity) : 0;

        public _prototype_LootInventoryData(int maxSlots = 2)
        {
            this.maxSlots = Math.Max(1, maxSlots);
            this.lootItems = new List<_prototype_ItemStack>(this.maxSlots);
        }

        public _prototype_LootInventoryData(_prototype_LootInventoryData other)
        {
            if (other != null)
            {
                this.maxSlots = other.maxSlots;
                this.lootItems = other.lootItems != null
                    ? other.lootItems.Select(i => i.Clone()).ToList()
                    : new List<_prototype_ItemStack>(this.maxSlots);
            }
            else
            {
                this.maxSlots = 2;
                this.lootItems = new List<_prototype_ItemStack>(2);
            }
        }

        public IReadOnlyList<_prototype_ItemStack> GetAllItems()
        {
            return lootItems ?? (IReadOnlyList<_prototype_ItemStack>)Array.Empty<_prototype_ItemStack>();
        }

        public int CountOf(string itemId)
        {
            if (string.IsNullOrEmpty(itemId) || lootItems == null) return 0;
            return lootItems.Where(i => i.itemId == itemId).Sum(i => i.quantity);
        }

        public bool HasItem(string itemId, int quantity = 1)
        {
            if (quantity <= 0) return true;
            return CountOf(itemId) >= quantity;
        }

        public int AddItem(_prototype_ItemDataModel itemModel, int quantity)
        {
            if (itemModel == null || quantity <= 0) return 0;
            if (lootItems == null) lootItems = new List<_prototype_ItemStack>(maxSlots);

            // 기존 동일 아이템에 스택 누적
            var existing = lootItems.FirstOrDefault(i => i.itemId == itemModel.id);
            if (existing != null)
            {
                existing.AddQuantity(quantity);
                return quantity;
            }

            // 여유 슬롯이 있으면 추가
            if (lootItems.Count < maxSlots)
            {
                lootItems.Add(new _prototype_ItemStack(itemModel.id, quantity));
                return quantity;
            }

            return 0;
        }

        public bool ConsumeItem(string itemId, int quantity)
        {
            if (string.IsNullOrEmpty(itemId) || quantity <= 0 || lootItems == null) return false;
            if (!HasItem(itemId, quantity)) return false;

            int remaining = quantity;
            for (int i = lootItems.Count - 1; i >= 0 && remaining > 0; i--)
            {
                var item = lootItems[i];
                if (item.itemId == itemId)
                {
                    int removed = item.RemoveQuantity(remaining);
                    remaining -= removed;
                    if (item.quantity <= 0)
                    {
                        lootItems.RemoveAt(i);
                    }
                }
            }

            return remaining == 0;
        }

        public _prototype_IInventoryData Clone()
        {
            return new _prototype_LootInventoryData(this);
        }
    }
}
