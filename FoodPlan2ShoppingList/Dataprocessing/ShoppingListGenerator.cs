using System.Collections.Generic;
using FoodPlan2ShoppingList.Model;

namespace FoodPlan2ShoppingList.Repositories
{
    public class ShoppingListGenerator
    {

        public List<Ingredient> CreateShoppingList(List<Recipe> listOfReciepies)
        {
            var shoppingList = new List<Ingredient>();

            foreach (var recipe in listOfReciepies)
            {
                foreach (var ingredient in recipe.Ingredients)
                {
                    ConvertUnitsOfIngredient(ingredient);

                    bool alreadyInList = false;
                    foreach (var shoppingListItem in shoppingList)
                    {
                        if (shoppingListItem.Name == ingredient.Name)
                        {

                            if (shoppingListItem.Unit == ingredient.Unit)
                            {
                                shoppingListItem.Amount = shoppingListItem.Amount + ingredient.Amount;
                                alreadyInList = true;
                            }
                        }
                    }

                    if (!alreadyInList)
                    {
                        shoppingList.Add(ingredient);
                    }
                }
            }

            return shoppingList;
        }

        private void ConvertUnitsOfIngredient(Ingredient ingredient)
        {
            if (ingredient.Unit == "mg" || ingredient.Unit == "milligram")
            {
                ingredient.Amount = ingredient.Amount / 10;
                ingredient.Unit = "g";
            }
            if (ingredient.Unit == "kg" || ingredient.Unit == "kilo" || ingredient.Unit == "kilogram")
            {
                ingredient.Amount = ingredient.Amount * 1000;
                ingredient.Unit = "g";
            }
            if (ingredient.Unit == "cl" || ingredient.Unit == "centiliter")
            {
                ingredient.Amount = ingredient.Amount * 10;
                ingredient.Unit = "ml";
            }
            if (ingredient.Unit == "dl" || ingredient.Unit == "deciliter")
            {
                ingredient.Amount = ingredient.Amount * 10;
                ingredient.Unit = "ml";
            }
            if (ingredient.Unit == "l" || ingredient.Unit == "liter")
            {
                ingredient.Amount = ingredient.Amount * 1000;
                ingredient.Unit = "ml";
            }
            if (ingredient.Unit == "knsp" || ingredient.Unit == "knivspids")
            {
                ingredient.Amount = ingredient.Amount * 0.25;
                ingredient.Unit = "ml";
            }
            if (ingredient.Unit == "tsk" || ingredient.Unit == "teske")
            {
                ingredient.Amount = ingredient.Amount * 5;
                ingredient.Unit = "ml";
            }
            if (ingredient.Unit == "spsk" || ingredient.Unit == "spiseskeer" || ingredient.Unit == "spiseskefulde")
            {
                ingredient.Amount = ingredient.Amount * 15;
                ingredient.Unit = "ml";
            }
        }
    }
}