using System.Collections.Generic;
using UnityEngine;
using SandEscapes.Interaction;
using SandEscapes.Inventory;
using SandEscapes.UI;

namespace SandEscapes.Items
{
    /// <summary>
    /// World container with loot stacks. Interact opens the loot UI (or take-all if no panel assigned).
    /// </summary>
    public class LootContainer : MonoBehaviour, IInteractable, IInteractablePromptProvider
    {
        [System.Serializable]
        public class LootStack
        {
            public ItemData item;
            [Min(0)] public int quantity;
        }

        [SerializeField] List<LootStack> contents = new List<LootStack>();
        [SerializeField] LootPanelUIView lootPanel;
        [SerializeField] string promptLoot = "E - Loot";
        [Tooltip("If true, a single press of Interact takes everything without opening the panel.")]
        [SerializeField] bool takeAllOnInteractWithoutPanel;

        [SerializeField] bool destroyWhenEmpty = true;

        public IReadOnlyList<LootStack> Contents => contents;

        public string GetPromptText() => promptLoot;

        public void OnInteract(GameObject interactor)
        {
            if (contents == null || contents.Count == 0)
                return;

            if (takeAllOnInteractWithoutPanel || lootPanel == null)
            {
                TryTakeAll(interactor);
                return;
            }

            lootPanel.Open(this, interactor);
        }

        /// <summary>
        /// Moves one unit at a time into inventory until full or loot empty.
        /// </summary>
        public void TryTakeAll(GameObject interactor)
        {
            var inventory = interactor != null
                ? interactor.GetComponent<InventorySystem>()
                : null;
            if (inventory == null && interactor != null)
                inventory = interactor.GetComponentInParent<InventorySystem>();
            if (inventory == null)
                return;

            for (var i = contents.Count - 1; i >= 0; i--)
            {
                var entry = contents[i];
                if (entry == null || entry.item == null || entry.quantity <= 0)
                {
                    contents.RemoveAt(i);
                    continue;
                }

                while (entry.quantity > 0)
                {
                    if (!inventory.TryAddItem(entry.item, 1))
                        return;
                    entry.quantity--;
                }

                contents.RemoveAt(i);
            }

            if (contents.Count == 0 && destroyWhenEmpty)
                Destroy(gameObject);
        }

        public void NotifyContentsChanged()
        {
            if (contents == null || contents.Count == 0)
            {
                if (destroyWhenEmpty)
                    Destroy(gameObject);
            }
        }
    }
}
