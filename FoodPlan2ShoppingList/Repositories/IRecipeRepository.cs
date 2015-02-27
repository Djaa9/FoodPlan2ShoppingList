using System.Collections.Generic;
using FoodPlan2ShoppingList.Model;

namespace FoodPlan2ShoppingList.Repositories
{
    public interface IRecipeRepository
    {
        List<Recipe> GetRecipesForWeek(uint weekNo);
    }
}