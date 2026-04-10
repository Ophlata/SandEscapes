using SandEscapes.Inventory;
using UnityEngine;

namespace SandEscapes.Items
{
    /// <summary>
    /// Shows the item in the active hotbar slot in front of the camera and throws one unit with <see cref="throwKey"/>.
    /// </summary>
    public class HeldItemPresenter : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] HotbarSystem hotbar;
        [SerializeField] InventorySystem inventory;
        [SerializeField] Camera viewCamera;
        [Tooltip("Parent for the held model. If empty, a child is created on the camera.")]
        [SerializeField] Transform handAnchor;

        [Header("Hand pose (local to hand anchor)")]
        [SerializeField] Vector3 handLocalPosition = new Vector3(0.35f, -0.28f, 0.55f);
        [SerializeField] Vector3 handLocalEulerAngles = new Vector3(0f, 15f, 0f);
        [SerializeField] Vector3 handLocalScale = new Vector3(0.32f, 0.32f, 0.32f);

        [Header("Throw")]
        [SerializeField] KeyCode throwKey = KeyCode.G;
        [SerializeField] float throwImpulse = 7f;
        [SerializeField] float throwUpwardImpulse = 1.25f;
        [SerializeField] float spawnForwardOffset = 1.1f;
        [SerializeField] float spawnUpwardOffset = 0.05f;
        [SerializeField] PrimitiveType fallbackHandPrimitive = PrimitiveType.Cube;
        [SerializeField] PrimitiveType fallbackDropPrimitive = PrimitiveType.Cube;
        [SerializeField] float fallbackDropScale = 0.45f;

        GameObject currentHandInstance;

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
            EnsureHandAnchor();
        }

        void OnEnable()
        {
            if (inventory != null)
                inventory.OnInventoryChanged += RefreshHandVisual;
            if (hotbar != null)
                hotbar.OnSelectionChanged += RefreshHandVisual;
        }

        void OnDisable()
        {
            if (inventory != null)
                inventory.OnInventoryChanged -= RefreshHandVisual;
            if (hotbar != null)
                hotbar.OnSelectionChanged -= RefreshHandVisual;
        }

        void Start()
        {
            RefreshHandVisual();
        }

        void Update()
        {
            if (Input.GetKeyDown(throwKey))
                TryThrowFromSelectedSlot();
        }

        void EnsureHandAnchor()
        {
            if (handAnchor != null)
                return;
            if (viewCamera == null)
                return;

            var anchorGo = new GameObject("HandAnchor");
            anchorGo.transform.SetParent(viewCamera.transform, false);
            anchorGo.transform.localPosition = Vector3.zero;
            anchorGo.transform.localRotation = Quaternion.identity;
            anchorGo.transform.localScale = Vector3.one;
            handAnchor = anchorGo.transform;
        }

        void RefreshHandVisual()
        {
            if (inventory == null || hotbar == null || handAnchor == null)
                return;

            ClearHandVisual();

            var slot = inventory.GetSlot(hotbar.SelectedHotbarIndex);
            if (slot.IsEmpty || slot.Item == null)
                return;

            currentHandInstance = BuildHandInstance(slot.Item);
        }

        void ClearHandVisual()
        {
            if (currentHandInstance == null)
                return;
            Destroy(currentHandInstance);
            currentHandInstance = null;
        }

        GameObject BuildHandInstance(ItemData item)
        {
            GameObject root;
            if (item.WorldPickupPrefab != null)
            {
                root = Instantiate(item.WorldPickupPrefab, handAnchor);
                ApplyHandTransform(root.transform);
            }
            else
            {
                root = GameObject.CreatePrimitive(fallbackHandPrimitive);
                root.name = $"Hand_{item.ItemId}";
                root.transform.SetParent(handAnchor, false);
                ApplyHandTransform(root.transform);
            }

            PrepareAsHandVisual(root);
            return root;
        }

        void ApplyHandTransform(Transform t)
        {
            t.localPosition = handLocalPosition;
            t.localRotation = Quaternion.Euler(handLocalEulerAngles);
            t.localScale = handLocalScale;
        }

        static void PrepareAsHandVisual(GameObject root)
        {
            foreach (var rb in root.GetComponentsInChildren<Rigidbody>())
                Destroy(rb);

            foreach (var col in root.GetComponentsInChildren<Collider>())
                col.enabled = false;

            foreach (var pickup in root.GetComponentsInChildren<WorldItemPickup>())
                pickup.enabled = false;
        }

        void TryThrowFromSelectedSlot()
        {
            if (inventory == null || hotbar == null || viewCamera == null)
                return;

            var idx = hotbar.SelectedHotbarIndex;
            if (!inventory.TryRemoveOneFromSlot(idx, out var item) || item == null)
                return;

            SpawnThrownPickup(item, 1);
        }

        void SpawnThrownPickup(ItemData item, int amount)
        {
            var camTx = viewCamera.transform;
            var spawnPos = camTx.position + camTx.forward * spawnForwardOffset + Vector3.up * spawnUpwardOffset;
            var rotation = Quaternion.LookRotation(camTx.forward, Vector3.up);
            var v = camTx.forward * throwImpulse + Vector3.up * throwUpwardImpulse;
            ItemPickupSpawn.SpawnWorldPickup(item, amount, spawnPos, rotation, v, fallbackDropPrimitive, fallbackDropScale);
        }
    }
}
