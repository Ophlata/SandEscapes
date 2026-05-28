using System.Collections.Generic;
using UnityEngine;
using SandEscapes.Items;

namespace SandEscapes.Crafting
{
    [System.Serializable]
    public class Ingredient
    {
        public ItemData item;
        public int amount;
    }

    [CreateAssetMenu(menuName = "SandEscapes/Crafting/Recipe")]
    public class RecipeSO : ScriptableObject
    {
        public string recipeName;
        public List<Ingredient> ingredients;
        public ItemData result;
        public int resultAmount = 1;
    }
}