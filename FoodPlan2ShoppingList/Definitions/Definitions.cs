using System.Collections.Generic;

namespace FoodPlan2ShoppingList.Definitions
{
    public class Definitions : IDefinitions
    {
        private readonly List<string> _measurements;
        public List<string> Measurements { get { return _measurements; } }
        
        public Definitions()
        {
            _measurements = new List<string>
        {
            "mg","milligram","g","gram","kg","kilo","kilogram",
            "ml","milliliter","cl","centiliter","dl","deciliter","deciliter","l","liter",
            "knsp","knivspids","tsk","teske","teskeer","spsk","spiseskeer","spiseskefulde","håndfulde",
            "stk","stykker","dåse","dåser","bøtter","glas","kartonner","poser","bakker",
            "skiver","fileter","sider","bundt","bundter","stilk","stilke"
        };
        }

    }
}