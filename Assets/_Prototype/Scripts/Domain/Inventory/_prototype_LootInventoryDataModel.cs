using System;
using System.Collections.Generic;
using UnityEngine;

namespace TDG0407._prototype
{
    [Serializable]
    public struct _prototype_LootDropEntry
    {
        public _prototype_ItemDataModel item;
        [Min(1)] public int minQuantity;
        [Min(1)] public int maxQuantity;
        [Range(0f, 1f)] public float dropChance;

        public _prototype_LootDropEntry(_prototype_ItemDataModel item, int minQuantity, int maxQuantity, float dropChance)
        {
            this.item = item;
            this.minQuantity = Math.Max(1, minQuantity);
            this.maxQuantity = Math.Max(this.minQuantity, maxQuantity);
            this.dropChance = Mathf.Clamp01(dropChance);
        }
    }

    /// <summary>
    /// 일반 몬스터용 초경량 전리품 인벤토리 모델 (ScriptableObject)
    /// 확률에 따라 1~2개의 전리품 아이템만 생성하여 메모리 낭비를 방지합니다.
    /// </summary>
    [CreateAssetMenu(fileName = "LootInventoryDataModel", menuName = "_Prototype/Inventory/LootInventoryDataModel", order = 1)]
    public class _prototype_LootInventoryDataModel : _prototype_InventoryDataModel
    {
        [Tooltip("최대 전리품 슬롯 수 (기본 2)")]
        public int maxDropSlots = 2;

        [Tooltip("드랍 가능한 전리품 항목 목록 (확률 및 수량 범위 설정)")]
        public List<_prototype_LootDropEntry> possibleDrops = new();

        public override _prototype_IInventoryData CreateInventoryData()
        {
            var inv = new _prototype_LootInventoryData(maxDropSlots);

            if (possibleDrops != null)
            {
                foreach (var entry in possibleDrops)
                {
                    if (entry.item != null && entry.dropChance > 0f)
                    {
                        if (UnityEngine.Random.value <= entry.dropChance)
                        {
                            int qty = UnityEngine.Random.Range(entry.minQuantity, entry.maxQuantity + 1);
                            inv.AddItem(entry.item, qty);

                            if (inv.lootItems.Count >= maxDropSlots)
                                break;
                        }
                    }
                }
            }

            return inv;
        }
    }
}
