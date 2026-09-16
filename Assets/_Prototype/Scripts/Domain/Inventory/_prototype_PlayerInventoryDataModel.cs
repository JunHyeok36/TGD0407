using System;
using System.Collections.Generic;
using UnityEngine;

namespace TDG0407._prototype
{
    [Serializable]
    public struct _prototype_InitialItemEntry
    {
        public _prototype_ItemDataModel item;
        public int quantity;

        public _prototype_InitialItemEntry(_prototype_ItemDataModel item, int quantity)
        {
            this.item = item;
            this.quantity = quantity;
        }
    }

    /// <summary>
    /// 플레이어/주요 엔티티용 풀 인벤토리 모델 (ScriptableObject)
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerInventoryDataModel", menuName = "_Prototype/Inventory/PlayerInventoryDataModel", order = 0)]
    public class _prototype_PlayerInventoryDataModel : _prototype_InventoryDataModel
    {
        [Tooltip("기본 가방 슬롯 수 (기본 16)")]
        public int bagSlotCount = 16;

        [Tooltip("게임 시작 시 가방에 지급할 초기 아이템 목록")]
        public List<_prototype_InitialItemEntry> initialItems = new();

        [Tooltip("게임 시작 시 지급할 초기 중요 아이템(KeyItems) 목록")]
        public List<_prototype_InitialItemEntry> initialKeyItems = new();

        public override _prototype_IInventoryData CreateInventoryData()
        {
            var inv = new _prototype_PlayerInventoryData(bagSlotCount);

            if (initialItems != null)
            {
                foreach (var entry in initialItems)
                {
                    if (entry.item != null && entry.quantity > 0)
                    {
                        inv.AddItem(entry.item, entry.quantity);
                    }
                }
            }

            if (initialKeyItems != null)
            {
                foreach (var entry in initialKeyItems)
                {
                    if (entry.item != null && entry.quantity > 0)
                    {
                        inv.AddItem(entry.item, entry.quantity);
                    }
                }
            }

            return inv;
        }
    }
}
