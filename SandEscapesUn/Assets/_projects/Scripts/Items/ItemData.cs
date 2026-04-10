using UnityEngine;
using SandEscapes.Items.Effects;

namespace SandEscapes.Items
{
    [CreateAssetMenu(fileName = "NewItem", menuName = "Sand Escapes/Items/Item Data", order = 0)]
    public class ItemData : ScriptableObject
    {
        [SerializeField] string itemId;
        [SerializeField] string itemName;
        [SerializeField] Sprite icon;
        [TextArea(2, 6)]
        [SerializeField] string description;
        [SerializeField] [Min(1)] int maxStack = 64;
        [SerializeField] ItemType itemType;
        [Header("World / hand")]
        [Tooltip("Optional root prefab for pickups and held view. Should include collider; Rigidbody optional (added on throw if missing).")]
        [SerializeField] GameObject worldPickupPrefab;

        [Header("Use (consumables)")]
        [Tooltip("Applied in order when the player uses this item from the hotbar. Leave empty for pure resources.")]
        [SerializeField] ItemUseEffect[] useEffects;

        public string ItemId => itemId;
        public string ItemName => itemName;
        public Sprite Icon => icon;
        public string Description => description;
        public int MaxStack => maxStack;
        public ItemType ItemType => itemType;
        public GameObject WorldPickupPrefab => worldPickupPrefab;
        public ItemUseEffect[] UseEffects => useEffects;
    }
}
