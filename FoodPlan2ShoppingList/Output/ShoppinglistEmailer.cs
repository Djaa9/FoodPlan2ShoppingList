using System.Collections.Generic;
using System.Net.Mail;
using FoodPlan2ShoppingList.Model;

namespace FoodPlan2ShoppingList.Output
{
    public class ShoppinglistEmailer : IShoppingListOutputType
    {
        public void SendList(List<Ingredient> shoppingList)
        {
            string stringToSend = "Indkøbsliste: </br></br><table border=\"1\">";

            foreach (var shoppingListItem in shoppingList)
            {
                stringToSend = stringToSend + "<tr><td>" + shoppingListItem.Amount + "</td><td>" +
                               shoppingListItem.Unit + "</td><td>" +
                               shoppingListItem.Name + "</td></tr>";
            }

            stringToSend = stringToSend + "</table>";

            var client = new SmtpClient
            {
                Port = 587,
                Host = "smtp.gmail.com",
                EnableSsl = true,
                Timeout = 10000,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials =
                    new System.Net.NetworkCredential("madplansrobotten@gmail.com", "Jegerdenondemadplansrobot")
            };

            client.Send(new MailMessage("madplansrobotten@gmail.com", "jp@thepage.dk", "Indkøbsliste for den kommende uge", stringToSend){IsBodyHtml = true,});
            client.Send(new MailMessage("madplansrobotten@gmail.com", "KarenD@vidsen.com", "Indkøbsliste for den kommende uge", stringToSend){IsBodyHtml = true,});
        }
    }
}