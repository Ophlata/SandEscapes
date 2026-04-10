using SandEscapes.Inventory;
using UnityEngine;

namespace SandEscapes.Items
{
    /// <summary>
    /// Uses the selected hotbar stack when it is a consumable with <see cref="ItemData.UseEffects"/>.
    /// </summary>
    public class ConsumableUseController : MonoBehaviour
    {
        [SerializeField] InventorySystem inventory;
        [SerializeField] HotbarSystem hotbar;
        [SerializeField] KeyCode useKey = KeyCode.F;

        void Reset()
        {
            inventory = GetComponent<InventorySystem>();
            hotbar = GetComponent<HotbarSystem>();
        }

        void Awake()
        {
            if (inventory == null)
                inventory = GetComponent<InventorySystem>();
            if (hotbar == null)
                hotbar = GetComponent<HotbarSystem>();
        }

        void Update()
        {
            if (!Input.GetKeyDown(useKey))
                return;
            if (inventory == null || hotbar == null)
                return;

            var idx = hotbar.SelectedHotbarIndex;
            var slot = inventory.GetSlot(idx);
            if (slot.IsEmpty || slot.Item == null)
                return;
            if (slot.Item.ItemType != ItemType.Consumable)
                return;

            var effects = slot.Item.UseEffects;
            if (effects == null || effects.Length == 0)
                return;

            for (var i = 0; i < effects.Length; i++)
            {
                if (effects[i] != null)
                    effects[i].TryApply(gameObject, slot.Item);
            }

            inventory.TryRemoveOneFromSlot(idx, out _);
        }
    }
}
