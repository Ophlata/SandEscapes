using SandEscapes.Items;

namespace SandEscapes.Inventory
{
    [System.Serializable]
    public struct InventorySlot
    {
        public ItemData Item;
        public int Quantity;

        public bool IsEmpty => Item == null || Quantity <= 0;
    }
}
