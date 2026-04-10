using System;
using UnityEngine;

namespace SandEscapes.Inventory
{
    /// <summary>
    /// Selects among the first inventory slots (hotbar region). Selection is input-driven; item data always lives in <see cref="InventorySystem"/>.
    /// </summary>
    [RequireComponent(typeof(InventorySystem))]
    public class HotbarSystem : MonoBehaviour
    {
        [SerializeField] InventorySystem inventory;
        [SerializeField] KeyCode slot1Key = KeyCode.Alpha1;
        [SerializeField] KeyCode slot2Key = KeyCode.Alpha2;
        [SerializeField] KeyCode slot3Key = KeyCode.Alpha3;
        [SerializeField] KeyCode slot4Key = KeyCode.Alpha4;

        [SerializeField] [Min(0)] int selectedHotbarIndex;

        public event Action OnSelectionChanged;

        public int SelectedHotbarIndex => selectedHotbarIndex;
        public InventorySystem Inventory => inventory;

        void Reset()
        {
            inventory = GetComponent<InventorySystem>();
        }

        void Awake()
        {
            if (inventory == null)
                inventory = GetComponent<InventorySystem>();
        }

        void Start()
        {
            ClampSelection();
        }

        void Update()
        {
            if (Input.GetKeyDown(slot1Key))
                SetSelected(0);
            else if (Input.GetKeyDown(slot2Key))
                SetSelected(1);
            else if (Input.GetKeyDown(slot3Key))
                SetSelected(2);
            else if (Input.GetKeyDown(slot4Key))
                SetSelected(3);
        }

        public void SetSelected(int index)
        {
            if (inventory == null)
                return;
            if (index < 0 || index >= inventory.HotbarSlotCount)
                return;
            if (selectedHotbarIndex == index)
                return;

            selectedHotbarIndex = index;
            OnSelectionChanged?.Invoke();
        }

        void ClampSelection()
        {
            if (inventory == null)
                return;
            selectedHotbarIndex = Mathf.Clamp(selectedHotbarIndex, 0, Mathf.Max(0, inventory.HotbarSlotCount - 1));
        }
    }
}
