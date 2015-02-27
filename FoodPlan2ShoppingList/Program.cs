using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using FoodPlan2ShoppingList.Model;

namespace FoodPlan2ShoppingList
{
    class Program
    {
        // Declarations
        private static readonly List<string> RecipeSubUrLs = new List<string>();
        private const string ArlaFoodplanUrl = "http://www.arla.dk/opskrifter/madplaner";
        private const string ArlaUrl = "http://www.arla.dk";

        private static bool errorInUnitConversion = false;
        private static readonly List<List<Ingredient>> ListOfIngredientsInrecipes = new List<List<Ingredient>>();
        private static List<Ingredient> _shoppingList = new List<Ingredient>();
        private static readonly List<string> UnitDefinitons = new List<string>
        {
            "mg","milligram","g","gram","kg","kilo","kilogram",
            "ml","milliliter","cl","centiliter","dl","deciliter","deciliter","l","liter",
            "knsp","knivspids","tsk","teske","teskeer","spsk","spiseskeer","spiseskefulde","håndfulde",
            "stk","stykker","dåse","dåser","bøtter","glas","kartonner","poser","bakker",
            "skiver","fileter","sider","bundt","bundter","stilk","stilke"
        };

        static void Main(string[] args)
        {
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
                    if (descendant.InnerHtml != null)
                    {
                        RecipeSubUrLs.Add(descendant.Attributes["href"].Value);
                    }
                }
            }

            foreach (var subUrL in RecipeSubUrLs)
            {
                ExtractingredientsFromrecipe(subUrL);
            }

            CollectListOfIngredientsForShoppingList();

            //SendShoppingList();
        }

        private static void ExtractingredientsFromrecipe(string subUrl)
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
                foreach (var unitDefition in UnitDefinitons)
                {
                    if (unitDefition == words[1])
                    {
                        isGoodUnit = true;
                        break;
                    }
                }

                if (!isGoodUnit)
                    words.Insert(1, "");

                /*Isert ingredient in the list we return*/
                listOfIngredientsToReturn.Add(new Ingredient
                {
                    Amount = Convert.ToDouble(words[0]),
                    Unit = words[1],
                    Name = String.Join(" ", words.Where(x => x != words[0] && x != words[1]))
                });
            }

            ListOfIngredientsInrecipes.Add(listOfIngredientsToReturn);

        }
        private static void CollectListOfIngredientsForShoppingList()
        {
            foreach (var dayList in ListOfIngredientsInrecipes)
            {
                foreach (var ingredient in dayList)
                {
                    ConvertUnitsOfIngredient(ingredient);

                    bool alreadyInList = false;
                    foreach (var shoppingListItem in _shoppingList)
                    {
                        if (shoppingListItem.Name == ingredient.Name)
                        {

                            if (shoppingListItem.Unit == ingredient.Unit)
                            {
                                shoppingListItem.Amount = shoppingListItem.Amount + ingredient.Amount;
                                alreadyInList = true;
                            }
                            else
                            {
                                errorInUnitConversion = true;
                            }
                        }
                    }

                    if (!alreadyInList)
                    {
                        _shoppingList.Add(ingredient);
                    }
                }
            }
        }

        private static void ConvertUnitsOfIngredient(Ingredient ingredient)
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

        private static void SendShoppingList()
        {
            string stringToSend = "Indkøbsliste: </br></br><table border=\"1\">";

            foreach (var shoppingListItem in _shoppingList)
            {
                stringToSend = stringToSend + "<tr><td>" + shoppingListItem.Amount + "</td><td>" +
                               shoppingListItem.Unit + "</td><td>" +
                               shoppingListItem.Name + "</td></tr>";
            }

            stringToSend = stringToSend + "</table>";

            if (errorInUnitConversion)
            {
                stringToSend = stringToSend + "</br></br></br>ERROR IN UNITCONVERSTION";
            }

            SmtpClient client = new SmtpClient();
            client.Port = 587;
            client.Host = "smtp.gmail.com";
            client.EnableSsl = true;
            client.Timeout = 10000;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.Credentials = new System.Net.NetworkCredential("madplansrobotten@gmail.com", "Jegerdenondemadplansrobot");

            var msg = new MailMessage("madplansrobotten@gmail.com", "jp@thepage.dk", "Indkøbsliste for den kommende uge", stringToSend)
            {
                IsBodyHtml = true,
            };

            client.Send(msg);

        }
    }
}