using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SandEscapes.Crafting;

namespace SandEscapes.Crafting
{
    public class RecipeSlot : MonoBehaviour
    {
        [SerializeField] private Image resultIcon;
        [SerializeField] private TMP_Text recipeNameText;
        [SerializeField] private TMP_Text ingredientsText;
        [SerializeField] private Button button;

        [SerializeField] private Color availableColor = Color.white;
        [SerializeField] private Color unavailableColor = new Color(1f, 1f, 1f, 0.4f);

        public void Setup(RecipeSO recipe, bool available, Action<RecipeSO> onSelect)
        {
            if (recipe.result.Icon != null)
                resultIcon.sprite = recipe.result.Icon; // замени Icon на поле из твоего ItemData

            recipeNameText.text = recipe.recipeName;

            var sb = new System.Text.StringBuilder();
            foreach (var ing in recipe.ingredients)
                sb.AppendLine($"• {ing.item.name} ×{ing.amount}");
            ingredientsText.text = sb.ToString().TrimEnd();

            GetComponent<CanvasGroup>().alpha = available ? 1f : 0.45f;
            button.onClick.AddListener(() => onSelect(recipe));
        }
    }
}