using ItemChecker.MVVM.Model;
using ItemChecker.Net;
using ItemChecker.Properties;
using ItemChecker.Support;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net;
using System.Threading;

namespace ItemChecker.Services
{
    public class QuickSellService : BaseService
    {
        public void checkInventory()
        {
            var json = Get.Request(SettingsProperties.Default.SteamCookies, "http://steamcommunity.com/my/inventory/json/730/2");
            JObject rgInventory = (JObject)JObject.Parse(json)["rgInventory"];
            JObject rgDescriptions = (JObject)JObject.Parse(json)["rgDescriptions"];

            DataSell.SellItems = new();
            foreach (var jObject in rgInventory)
            {
                string assetid = jObject.Value["id"].ToString();
                string classid = jObject.Value["classid"].ToString();
                string instanceid = jObject.Value["instanceid"].ToString();

                JObject item = (JObject)rgDescriptions[$"{classid}_{instanceid}"];
                string name = item["market_name"].ToString();
                bool marketable = (int)item["marketable"] != 0;
                bool nameTag = item.ContainsKey("fraudwarnings");
                string descriptions = item["descriptions"].ToString();
                bool stickers = descriptions.Contains("sticker_info");

                if (!marketable | stickers | nameTag)
                    continue;

                decimal price;
                if (!DataSell.SellItems.Any(c => c.ItemName == name))
                {
                    price = checkPrice(name);
                    if (price == -1)
                        continue;
                }
                else
                    price = DataSell.SellItems.First(i => i.ItemName == name).Price;

                DataSell.SellItems.Add( new DataSell() 
                {
                    AssetId = assetid,
                    ItemName = name,
                    Price = price
                });
            }
        }
        public void sellItems(DataSell item)
        {
            int sell_price = (int)((item.Price * 100 - 0.01m) * Calculator.CommissionSteam);

            Post.SellItem(SettingsProperties.Default.SteamCookies, SteamAccount.User, item.AssetId, sell_price);
        }
        Decimal checkPrice(string name)
        {
            decimal lowest_price = 0;
            decimal median_price = 0;
            bool check = true;
            do
            {
                try
                {
                    Tuple<Decimal, Decimal> prices = Get.PriceOverview(Edit.MarketHashName(name));
                    lowest_price = prices.Item1;
                    median_price = prices.Item2;
                    check = false;
                }
                catch
                {
                    Thread.Sleep(10000);
                }
            } while (check);
            

            decimal price = lowest_price;
            if (lowest_price < median_price)
                price = median_price;
            if (price > HomeProperties.Default.MaxPrice | price == 0)
                return -1;

            return price;
        }
    }
}