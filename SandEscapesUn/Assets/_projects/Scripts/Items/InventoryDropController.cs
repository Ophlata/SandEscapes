using SandEscapes.Inventory;
using UnityEngine;

namespace SandEscapes.Items
{
    /// <summary>
    /// Drops one item from the selected hotbar slot into the world (weaker impulse than throw).
    /// </summary>
    public class InventoryDropController : MonoBehaviour
    {
        [SerializeField] InventorySystem inventory;
        [SerializeField] HotbarSystem hotbar;
        [SerializeField] Camera viewCamera;
        [SerializeField] KeyCode dropKey = KeyCode.X;
        [SerializeField] float forwardOffset = 0.65f;
        [SerializeField] float upwardOffset = 0.35f;
        [SerializeField] float forwardImpulse = 1.25f;
        [SerializeField] float upwardImpulse = 0.35f;
        [SerializeField] PrimitiveType fallbackPrimitive = PrimitiveType.Cube;
        [SerializeField] float fallbackScale = 0.45f;

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
            if (viewCamera == null)
                viewCamera = GetComponentInChildren<Camera>(true);
        }

        void Update()
        {
            if (!Input.GetKeyDown(dropKey))
                return;
            if (inventory == null || hotbar == null || viewCamera == null)
                return;

            var idx = hotbar.SelectedHotbarIndex;
            if (!inventory.TryRemoveOneFromSlot(idx, out var item) || item == null)
                return;

            var cam = viewCamera.transform;
            var pos = cam.position + cam.forward * forwardOffset + Vector3.up * upwardOffset;
            var rot = Quaternion.LookRotation(cam.forward, Vector3.up);
            var vel = cam.forward * forwardImpulse + Vector3.up * upwardImpulse;

            ItemPickupSpawn.SpawnWorldPickup(item, 1, pos, rot, vel, fallbackPrimitive, fallbackScale);
        }
    }
}
