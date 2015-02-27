using System.Collections.Generic;
using FoodPlan2ShoppingList.Definitions;

namespace FoodPlan2ShoppingList.Model
{
    public class Recipe
    {
        public string Name { get; set; }
        public MealType MealType { get; set; }
        public string Path { get; set; }
        public List<Ingredient> Ingredients { get; set; } 
    }
}