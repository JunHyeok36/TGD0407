using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;

namespace TDG0407._prototype
{
    /// <summary>
    /// 플레이어 및 상인/주요 NPC를 위한 풀 인벤토리 (슬롯 기반 가방 + 무제한 중요 아이템 보관)
    /// </summary>
    [Serializable]
    public class _prototype_PlayerInventoryData : _prototype_IInventoryData
    {
        public _prototype_Bag bag;
        public List<_prototype_ItemStack> keyItems;

        public _prototype_Bag Bag => bag;
        public IReadOnlyList<_prototype_ItemStack> KeyItems => keyItems;

        public int TotalItemCount
        {
            get
            {
                int bagCount = bag != null ? bag.GetAllItems().Sum(i => i.quantity) : 0;
                int keyCount = keyItems != null ? keyItems.Sum(i => i.quantity) : 0;
                return bagCount + keyCount;
            }
        }

        public _prototype_PlayerInventoryData(int bagSlotCount = 16)
        {
            bag = new _prototype_Bag(bagSlotCount);
            keyItems = new List<_prototype_ItemStack>();
        }

        public _prototype_PlayerInventoryData(_prototype_PlayerInventoryData other)
        {
            if (other != null)
            {
                bag = other.bag?.Clone() ?? new _prototype_Bag(16);
                keyItems = other.keyItems != null 
                    ? other.keyItems.Select(k => k.Clone()).ToList() 
                    : new List<_prototype_ItemStack>();
            }
            else
            {
                bag = new _prototype_Bag(16);
                keyItems = new List<_prototype_ItemStack>();
            }
        }

        public IReadOnlyList<_prototype_ItemStack> GetAllItems()
        {
            var list = new List<_prototype_ItemStack>();
            if (bag != null)
            {
                list.AddRange(bag.GetAllItems());
            }
            if (keyItems != null)
            {
                foreach (var k in keyItems)
                {
                    if (k != null && k.quantity > 0)
                    {
                        list.Add(k.Clone());
                    }
                }
            }
            return list;
        }

        public int CountOf(string itemId)
        {
            if (string.IsNullOrEmpty(itemId)) return 0;

            int count = 0;
            if (bag != null)
            {
                count += bag.CountOf(itemId);
            }
            if (keyItems != null)
            {
                var keyItem = keyItems.FirstOrDefault(k => k.itemId == itemId);
                if (keyItem != null)
                {
                    count += keyItem.quantity;
                }
            }
            return count;
        }

        public bool HasItem(string itemId, int quantity = 1)
        {
            if (quantity <= 0) return true;
            return CountOf(itemId) >= quantity;
        }

        public int AddItem(_prototype_ItemDataModel itemModel, int quantity)
        {
            if (itemModel == null || quantity <= 0) return 0;

            // 중요 아이템(KeyItem)은 슬롯 제한 없는 keyItems 목록으로 자동 분기
            if (itemModel.itemType == _prototype_ItemType.KeyItem)
            {
                var existing = keyItems.FirstOrDefault(k => k.itemId == itemModel.id);
                if (existing != null)
                {
                    existing.AddQuantity(quantity);
                }
                else
                {
                    keyItems.Add(new _prototype_ItemStack(itemModel.id, quantity));
                }
                return quantity;
            }

            // 그 외 일반 아이템(소모품, 재료, 잡화)은 가방 슬롯에 누적 배치
            if (bag != null)
            {
                return bag.Add(new _prototype_ItemStack(itemModel.id, quantity), itemModel.maxStack);
            }

            return 0;
        }

        public bool ConsumeItem(string itemId, int quantity)
        {
            if (string.IsNullOrEmpty(itemId) || quantity <= 0) return false;
            if (!HasItem(itemId, quantity)) return false;

            int remaining = quantity;

            // 1. KeyItems에서 먼저 차감 시도
            if (keyItems != null)
            {
                var keyItem = keyItems.FirstOrDefault(k => k.itemId == itemId);
                if (keyItem != null)
                {
                    int removed = keyItem.RemoveQuantity(remaining);
                    remaining -= removed;
                    if (keyItem.quantity <= 0)
                    {
                        keyItems.Remove(keyItem);
                    }
                }
            }

            // 2. 남은 수량을 Bag 슬롯들에서 순차 차감
            if (remaining > 0 && bag != null)
            {
                for (int i = 0; i < bag.SlotCount && remaining > 0; i++)
                {
                    var slot = bag.Slots[i];
                    if (!slot.IsEmpty && slot.item.itemId == itemId)
                    {
                        int removed = slot.item.RemoveQuantity(remaining);
                        remaining -= removed;
                        if (slot.item.quantity <= 0)
                        {
                            slot.Clear();
                        }
                    }
                }
            }

            return remaining == 0;
        }

        /// <summary>
        /// 특정 가방 슬롯의 소모품 아이템을 사용합니다.
        /// </summary>
        public async UniTask<bool> UseItem(int slotIndex, _prototype_LifeData user, _prototype_ItemDataModel itemModel)
        {
            if (bag == null || slotIndex < 0 || slotIndex >= bag.SlotCount || user == null)
                return false;

            var slot = bag.Slots[slotIndex];
            if (slot.IsEmpty || (itemModel != null && slot.item.itemId != itemModel.id))
                return false;

            if (itemModel != null && itemModel.isUsable)
            {
                bool success = await itemModel.ExecuteUseActions(user);
                if (success)
                {
                    bag.RemoveAt(slotIndex, 1);
                    return true;
                }
                return false;
            }

            return false;
        }

        public _prototype_IInventoryData Clone()
        {
            return new _prototype_PlayerInventoryData(this);
        }
    }
}
