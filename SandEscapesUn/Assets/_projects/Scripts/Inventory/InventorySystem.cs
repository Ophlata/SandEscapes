using System;
using UnityEngine;
using SandEscapes.Items;

namespace SandEscapes.Inventory
{
    /// <summary>
    /// Single inventory grid. The first <see cref="hotbarSlotCount"/> indices are mirrored by the hotbar UI and <see cref="HotbarSystem"/>.
    /// </summary>
    public class InventorySystem : MonoBehaviour
    {
        [SerializeField] [Min(1)] int totalSlotCount = 24;
        [SerializeField] [Min(1)] int hotbarSlotCount = 4;

        InventorySlot[] slots;

        public event Action OnInventoryChanged;

        public int TotalSlotCount => slots != null ? slots.Length : 0;
        public int HotbarSlotCount => hotbarSlotCount;

        void Awake()
        {
            ApplySlotCapacity();
        }

        void OnValidate()
        {
            hotbarSlotCount = Mathf.Max(1, hotbarSlotCount);
            totalSlotCount = Mathf.Max(hotbarSlotCount, totalSlotCount);
        }

        void ApplySlotCapacity()
        {
            totalSlotCount = Mathf.Max(hotbarSlotCount, totalSlotCount);
            slots = new InventorySlot[totalSlotCount];
        }

        public InventorySlot GetSlot(int index)
        {
            if (slots == null || index < 0 || index >= slots.Length)
                return default;
            return slots[index];
        }

        public int GetTotalQuantity(ItemData item)
        {
            if (item == null || slots == null)
                return 0;

            var sum = 0;
            for (var i = 0; i < slots.Length; i++)
            {
                if (!slots[i].IsEmpty && slots[i].Item == item)
                    sum += slots[i].Quantity;
            }

            return sum;
        }

        /// <summary>
        /// Adds the full amount in one transaction, or fails without changing the inventory.
        /// </summary>
        public bool TryAddItem(ItemData item, int amount)
        {
            if (item == null || amount <= 0 || slots == null)
                return false;
            if (!CanFitEntireAmount(item, amount))
                return false;

            var remaining = amount;

            for (var i = 0; i < slots.Length && remaining > 0; i++)
            {
                if (slots[i].IsEmpty)
                    continue;
                if (slots[i].Item != item)
                    continue;

                var cap = item.MaxStack;
                if (cap <= 1)
                    continue;

                var space = cap - slots[i].Quantity;
                if (space <= 0)
                    continue;

                var add = Mathf.Min(space, remaining);
                slots[i].Quantity += add;
                remaining -= add;
            }

            for (var i = 0; i < slots.Length && remaining > 0; i++)
            {
                if (!slots[i].IsEmpty)
                    continue;

                var stackLimit = item.MaxStack <= 1 ? 1 : item.MaxStack;
                var add = Mathf.Min(stackLimit, remaining);
                slots[i].Item = item;
                slots[i].Quantity = add;
                remaining -= add;
            }

            if (remaining != 0)
                return false;

            RaiseChanged();
            return true;
        }

        /// <summary>
        /// Removes up to <paramref name="amount"/> across stacks; returns how much was removed.
        /// </summary>
        public int TryRemoveItem(ItemData item, int amount)
        {
            if (item == null || amount <= 0 || slots == null)
                return 0;

            var toRemove = amount;
            var removed = 0;

            for (var i = 0; i < slots.Length && toRemove > 0; i++)
            {
                if (slots[i].IsEmpty || slots[i].Item != item)
                    continue;

                var take = Mathf.Min(slots[i].Quantity, toRemove);
                slots[i].Quantity -= take;
                toRemove -= take;
                removed += take;

                if (slots[i].Quantity <= 0)
                    slots[i] = default;
            }

            if (removed > 0)
                RaiseChanged();

            return removed;
        }

        public bool HasAtLeast(ItemData item, int amount)
        {
            return GetTotalQuantity(item) >= amount;
        }

        /// <summary>
        /// Removes a single unit from a specific slot (e.g. active hotbar slot when throwing).
        /// </summary>
        public bool TryRemoveOneFromSlot(int slotIndex, out ItemData removedItem)
        {
            removedItem = null;
            if (slots == null || slotIndex < 0 || slotIndex >= slots.Length)
                return false;

            var s = slots[slotIndex];
            if (s.IsEmpty || s.Item == null)
                return false;

            removedItem = s.Item;
            s.Quantity--;
            if (s.Quantity <= 0)
                s = default;
            slots[slotIndex] = s;
            RaiseChanged();
            return true;
        }

        void RaiseChanged()
        {
            OnInventoryChanged?.Invoke();
        }

        bool CanFitEntireAmount(ItemData item, int amount)
        {
            var remaining = amount;

            for (var i = 0; i < slots.Length && remaining > 0; i++)
            {
                if (slots[i].IsEmpty)
                    continue;
                if (slots[i].Item != item)
                    continue;

                var cap = item.MaxStack;
                if (cap <= 1)
                    continue;

                var space = cap - slots[i].Quantity;
                if (space <= 0)
                    continue;

                remaining -= Mathf.Min(space, remaining);
            }

            for (var i = 0; i < slots.Length && remaining > 0; i++)
            {
                if (!slots[i].IsEmpty)
                    continue;

                var stackLimit = item.MaxStack <= 1 ? 1 : item.MaxStack;
                remaining -= Mathf.Min(stackLimit, remaining);
            }

            return remaining <= 0;
        }
    }
}
