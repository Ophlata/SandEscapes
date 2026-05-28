using UnityEngine;
using UnityEngine.UI;
using SandEscapes.Crafting;

namespace SandEscapes.Crafting
{
    public class CraftingUI : MonoBehaviour
    {
        public static CraftingUI Instance { get; private set; }

        [SerializeField] private GameObject panel;
        [SerializeField] private Transform recipeListParent;
        [SerializeField] private RecipeSlot recipeSlotPrefab;
        [SerializeField] private Button craftButton;

        private RecipeSO selectedRecipe;

        void Awake()
        {
            Instance = this;
            craftButton.onClick.AddListener(OnCraftPressed);
            craftButton.interactable = false;
        }

        void Start() => Refresh();

        public void Toggle() => panel.SetActive(!panel.activeSelf);

        public void Refresh()
        {
            foreach (Transform t in recipeListParent)
                Destroy(t.gameObject);

            foreach (var recipe in CraftingManager.Instance.GetAllRecipes())
            {
                var slot = Instantiate(recipeSlotPrefab, recipeListParent);
                bool available = CraftingManager.Instance.CanCraft(recipe);
                slot.Setup(recipe, available, OnRecipeSelected);
            }

            craftButton.interactable = selectedRecipe != null &&
                                       CraftingManager.Instance.CanCraft(selectedRecipe);
        }

        void OnRecipeSelected(RecipeSO recipe)
        {
            selectedRecipe = recipe;
            craftButton.interactable = CraftingManager.Instance.CanCraft(recipe);
        }

        void OnCraftPressed()
        {
            if (selectedRecipe != null && CraftingManager.Instance.Craft(selectedRecipe))
                Refresh();
        }
    }
}