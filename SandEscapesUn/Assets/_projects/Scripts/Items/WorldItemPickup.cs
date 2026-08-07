using UnityEngine;
using SandEscapes.Interaction;
using SandEscapes.Inventory;

namespace SandEscapes.Items
{
    /// <summary>
    /// Pickup in the world. Requires a collider on this object (or child) for camera raycasts.
    /// </summary>
    public class WorldItemPickup : MonoBehaviour, IInteractable, IInteractablePromptProvider
    {
        [SerializeField] ItemData itemData;
        [SerializeField] [Min(1)] int pickupAmount = 1;
        [SerializeField] string promptPickUp = "E - Pick up";

        public ItemData ItemData => itemData;
        public int PickupAmount => pickupAmount;

        public void Initialize(ItemData data, int amount)
        {
            itemData = data;
            pickupAmount = Mathf.Max(1, amount);
        }

        public string GetPromptText() => promptPickUp;

        public void OnInteract(GameObject interactor)
        {
            Debug.Log("1. Начало подбора");

            if (itemData == null || pickupAmount <= 0)
                return;

            var inventory = interactor != null
                ? interactor.GetComponent<InventorySystem>()
                : null;

            if (inventory == null && interactor != null)
                inventory = interactor.GetComponentInParent<InventorySystem>();

            if (inventory == null)
            {
                Debug.Log("2. Inventory не найден");
                return;
            }

            if (!inventory.TryAddItem(itemData, pickupAmount))
            {
                Debug.Log("3. Не удалось добавить предмет");
                return;
            }

            Debug.Log("4. Предмет добавлен");
            Debug.Log("ItemID = " + itemData.ItemId);

            QuestManager.Instance?.AddProgress(itemData.ItemId);

            Destroy(gameObject);
        }
    }
}
