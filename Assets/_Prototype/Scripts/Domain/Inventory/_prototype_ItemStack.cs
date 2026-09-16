using System;

namespace TDG0407._prototype
{
    /// <summary>
    /// 아이템 묶음 단위 (아이템 ID + 수량)
    /// </summary>
    [Serializable]
    public class _prototype_ItemStack
    {
        public string itemId;
        public int quantity;

        public _prototype_ItemStack()
        {
            this.itemId = string.Empty;
            this.quantity = 0;
        }

        public _prototype_ItemStack(string itemId, int quantity)
        {
            this.itemId = itemId;
            this.quantity = Math.Max(0, quantity);
        }

        public _prototype_ItemStack(_prototype_ItemStack other)
        {
            if (other != null)
            {
                this.itemId = other.itemId;
                this.quantity = other.quantity;
            }
        }

        public void AddQuantity(int amount)
        {
            if (amount > 0)
            {
                quantity += amount;
            }
        }

        public int RemoveQuantity(int amount)
        {
            if (amount <= 0) return 0;
            int removed = Math.Min(quantity, amount);
            quantity -= removed;
            return removed;
        }

        public _prototype_ItemStack Clone()
        {
            return new _prototype_ItemStack(this);
        }

        public override string ToString()
        {
            return $"{itemId} x{quantity}";
        }
    }
}
