using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using FoodPlan2ShoppingList.Definitions;
using FoodPlan2ShoppingList.Model;

namespace FoodPlan2ShoppingList.Repositories
{
    public class RecipeRepository : IRecipeRepository
    {
        // Declarations
        private const string ArlaFoodplanUrl = "http://www.arla.dk/opskrifter/madplaner";
        private const string ArlaUrl = "http://www.arla.dk";
        private readonly List<string> _unitDefinitions;

        public RecipeRepository(IDefinitions measurementDefinitions)
        {
            _unitDefinitions = measurementDefinitions.Measurements;
        }

        public List<Recipe> GetRecipesForWeek(uint weekno)
        {
            var recipeSubUrLs = new List<string>();
            var listOfRecipes = new List<Recipe>();

            /* READ HTML FROM ARLA FOODPLAN */
            string htmlFrontpage;

            using (var client = new WebClient())
            {
                htmlFrontpage = client.DownloadString(ArlaFoodplanUrl);
            }

            var agilitypack = new HtmlAgilityPack.HtmlDocument();
            agilitypack.LoadHtml(htmlFrontpage);

            /* EXTRACT URLS FOR EACH RECIPE IN THE FOODPLAN*/
            var list = agilitypack.DocumentNode.Descendants("tbody");

            foreach (var dayinfo in list)
            {
                foreach (var descendant in dayinfo.Descendants("a").Where(x => x.Attributes.Contains("href")))
                {
                    if (descendant.InnerHtml == null) continue;

                    var pathOfRecipe = descendant.Attributes["href"].Value;

                    var mealTypeString = descendant.ChildNodes["h3"].ChildNodes["span"].InnerText;
                        
                    var mealType = MealType.Forret;
                    if (mealTypeString == "Forret:")
                        mealType = MealType.Forret;
                    if (mealTypeString == "Hovedret:")
                        mealType = MealType.Hovedret;
                    if (mealTypeString == "Dessert:")
                        mealType = MealType.Dessert;

                    listOfRecipes.Add(new Recipe()
                    {
                        Name = descendant.ChildNodes["h3"].LastChild.InnerText.Trim(),
                        Path = pathOfRecipe,
                        MealType = mealType,
                        Ingredients = ExtractingredientsFromrecipe(pathOfRecipe)
                    }
                        );
                }
            }

            return listOfRecipes;
        }

        private List<Ingredient> ExtractingredientsFromrecipe(string subUrl)
        {
            string html;

            using (var client = new WebClient())
            {
                html = client.DownloadString(ArlaUrl + subUrl);
            }

            var agilitypack = new HtmlAgilityPack.HtmlDocument();
            agilitypack.LoadHtml(html);

            var listOfIngredientInfo = agilitypack.DocumentNode.SelectNodes("//li[@itemprop=\"ingredients\"]");  // Extract <li> nodes with an attribute named "itemprop" with a value of \"ingredients\"

            var ingredients = listOfIngredientInfo.Select(ingredientInfo => ingredientInfo.InnerText).ToList();  // Extract string ingredients information from <li> nodes

            var listOfIngredientsToReturn = new List<Ingredient>();
            foreach (var ingredient in ingredients)
            {
                var words = ingredient.Split(' ').ToList();
                words.Add("");
                words.Add("");

                /*If Amount is in fraction -> Convert to decimal*/

                if (words[0].Contains("&frasl;"))
                {
                    var fractions = (words[0].Replace("&frasl;", "&")).Split('&');
                    words[0] = (Convert.ToDouble(fractions[0]) / Convert.ToDouble(fractions[1])).ToString();
                }

                if (words[1].Contains("&frasl;"))
                {
                    var fractions = (words[1].Replace("&frasl;", "&")).Split('&');
                    words[0] = (Convert.ToDouble(words[0]) + (Convert.ToDouble(fractions[0]) / Convert.ToDouble(fractions[1]))).ToString();
                    words.RemoveAt(1);
                }

                /*If no numeric Amount -> Set to 0*/

                double n;
                if (!double.TryParse(words[0], out n)) // true if numeric
                {
                    words.Insert(0, "0");
                }

                /*If no valid Unit -> Set to "" */
                bool isGoodUnit = false;
                foreach (var unitDefition in _unitDefinitions)
                {
                    if (unitDefition == words[1])
                    {
                        isGoodUnit = true;
                        break;
                    }
                }

                if (!isGoodUnit)
                    words.Insert(1, "");

                /*Insert ingredient in the list we return*/
                listOfIngredientsToReturn.Add(new Ingredient
                {
                    Amount = Convert.ToDouble(words[0]),
                    Unit = words[1],
                    Name = String.Join(" ", words.Where(x => x != words[0] && x != words[1]))
                });
            }

            return listOfIngredientsToReturn;

        }
    }
}
