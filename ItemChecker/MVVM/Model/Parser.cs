using ItemChecker.Net;
using ItemChecker.Support;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading;

namespace ItemChecker.MVVM.Model
{
    public class Parser
    {
        public List<string> Services = new();
        public int ServiceOne { get; set; }
        public int ServiceTwo { get; set; }

        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
        public decimal MinPrecent { get; set; }
        public decimal MaxPrecent { get; set; }
        public int SteamSales { get; set; }
        public string NameContains { get; set; }
        public bool Knife { get; set; }
        public bool Stattrak { get; set; }
        public bool Souvenir { get; set; }
        public bool Sticker { get; set; }

        public static void getList()
        {
            ParserData.ParserItems.Clear();
            ParserData.ParserItems.Add(new ParserData("White", "★ StatTrak™ Paracord Knife | Night Stripe (Battle-Scarred)", 10000, 20000, 30000, 40000, 45.67m, 7890.67m, "Unavailable"));
            ParserData.ParserItems.Add(new ParserData("White", "★ StatTrak™ Paracord Knife | Night Stripe (Battle-Scarred)", 10000, 20000, 30000.32m, 40000, 45.67m, 7890.67m, "Unavailable"));
            ParserData.ParserItems.Add(new ParserData("White", "★ StatTrak™ Paracord Knife | Night Stripe (Battle-Scarred)", 10000, 20000, 30000, 40000, 45.67m, 7890.67m, "Unavailable"));
            ParserData.ParserItems.Add(new ParserData("White", "★ StatTrak™ Paracord Knife | Night Stripe (Battle-Scarred)", 10000, 20000, 30000, 40000.76m, 45.67m, 7890.67m, "Unavailable"));
            ParserData.ParserItems.Add(new ParserData("White", "★ StatTrak™ Paracord Knife | Night Stripe (Battle-Scarred)", 10000, 20000, 30000, 40000, 45.67m, 7890.67m, "Unavailable"));
            ParserData.ParserItems.Add(new ParserData("White", "★ StatTrak™ Paracord Knife | Night Stripe (Battle-Scarred)", 10000, 20000, 30000, 40000, 45.67m, 7890.67m, "Unavailable"));
            ParserData.ParserItems.Add(new ParserData("White", "★ StatTrak™ Paracord Knife | Night Stripe (Battle-Scarred)", 10000, 20000, 30000, 40000, 45.67m, 7890.67m, "Unavailable"));
            ParserData.ParserItems.Add(new ParserData("White", "★ StatTrak™ Paracord Knife | Night Stripe (Battle-Scarred)", 10000, 20000, 30000, 40000, 45.67m, 7890.67m, "Unavailable"));
        }
        protected void checkItems(string itemName)
        {
            Tuple<String, Boolean> response = Tuple.Create("", false);
            do
            {
                response = Get.MrinkaRequest(Edit.MarketHashName(itemName));
                if (!response.Item2)
                {
                    Thread.Sleep(30000);
                }
            }
            while (!response.Item2);

            parseJson(response.Item1, itemName);
        }
        private void parseJson(string response, string itemName)
        {
            decimal steam_price = Convert.ToDecimal(JObject.Parse(response)["steam"]["buyOrder"].ToString());
            steam_price = Convert.ToDecimal(JObject.Parse(response)["steam"]["sellOrder"].ToString());
            decimal csm_sell = Convert.ToDecimal(JObject.Parse(response)["csm"]["sell"].ToString());
            decimal precent = Edit.Precent(steam_price, csm_sell);

            if (precent > 0)
            {
                //ItemName.Add(item_name);
                //sta.Add(steam_price);
                //csm.Add(csm_sell);
                //Precent.Add(precent);
                //Difference.Add(Edit.Difference(csm_sell, steam_price, GeneralProperties.Default.currency));
            }
        }
    }
}        