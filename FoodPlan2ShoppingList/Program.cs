using System.Linq;
using FoodPlan2ShoppingList.Definitions;
using FoodPlan2ShoppingList.Output;
using FoodPlan2ShoppingList.Repositories;

namespace FoodPlan2ShoppingList
{
    class Program
    {
        static void Main(string[] args)
        {
            var recipeRepository = new RecipeRepository(new Definitions.Definitions());
            var shoppinglistGenerator = new ShoppingListGenerator();
            var shoppinglistEmailer = new ShoppinglistEmailer();

            var listOfRecipes = recipeRepository.GetRecipesForWeek(1);
            var listOfRecipesToCook = listOfRecipes.Where(x => x.MealType == MealType.Hovedret).ToList();
            var shoppingList = shoppinglistGenerator.CreateShoppingList(listOfRecipesToCook);

            shoppinglistEmailer.SendList(shoppingList);
        }
    }
}