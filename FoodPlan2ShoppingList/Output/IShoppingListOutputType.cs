using System.Collections.Generic;
using FoodPlan2ShoppingList.Model;

namespace FoodPlan2ShoppingList
{
    public interface IShoppingListOutputType
    {
        void SendList(List<Ingredient> shoppingList);
    }
}