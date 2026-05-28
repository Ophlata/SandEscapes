using UnityEngine;
using SandEscapes.Inventory;
using SandEscapes.Crafting;

namespace SandEscapes.Crafting
{
    public class CraftingManager : MonoBehaviour
    {
        public static CraftingManager Instance { get; private set; }

        [SerializeField] private RecipeSO[] allRecipes;
        [SerializeField] private InventorySystem inventory;

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        void Start()
        {
            // Если не назначен вручную — ищем на сцене
            if (inventory == null)
                inventory = FindObjectOfType<InventorySystem>();

            inventory.OnInventoryChanged += OnInventoryChanged;
        }

        void OnDestroy()
        {
            if (inventory != null)
                inventory.OnInventoryChanged -= OnInventoryChanged;
        }

        public RecipeSO[] GetAllRecipes() => allRecipes;

        public bool CanCraft(RecipeSO recipe)
        {
            foreach (var ing in recipe.ingredients)
                if (!inventory.HasAtLeast(ing.item, ing.amount)) return false;
            return true;
        }

        public bool Craft(RecipeSO recipe)
        {
            if (!CanCraft(recipe)) return false;

            if (!inventory.TryAddItem(recipe.result, recipe.resultAmount))
            {
                Debug.LogWarning("Крафт отменён: нет места для результата.");
                return false;
            }

            foreach (var ing in recipe.ingredients)
                inventory.TryRemoveItem(ing.item, ing.amount);

            return true;
        }

        void OnInventoryChanged() => CraftingUI.Instance?.Refresh();
    }
}