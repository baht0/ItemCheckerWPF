using ItemChecker.MVVM.Model;
using ItemChecker.Net;
using ItemChecker.Properties;
using ItemChecker.Support;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.Extensions;
using OpenQA.Selenium.Support.UI;
using System;
using System.Linq;

namespace ItemChecker.Services
{
    public class QuickSellService : BaseService
    {
        public void checkInventory()
        {
            Browser.Navigate().GoToUrl("http://steamcommunity.com/my/inventory/json/730/2");
            IWebElement html = WebDriverWait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//pre")));
            string json = html.Text;
            JObject rgInventory = (JObject)JObject.Parse(json)["rgInventory"];
            JObject rgDescriptions = (JObject)JObject.Parse(json)["rgDescriptions"];

            QuickSell.SellItems = new();
            foreach (var jObject in rgInventory)
            {
                string assetid = jObject.Value["id"].ToString();
                string classid = jObject.Value["classid"].ToString();
                string instanceid = jObject.Value["instanceid"].ToString();

                JObject item = (JObject)rgDescriptions[$"{classid}_{instanceid}"];
                string name = item["market_name"].ToString();
                bool marketable = true;
                if ((int)item["marketable"] == 0)
                    marketable = false;
                bool nameTag = item.ContainsKey("fraudwarnings");
                string descriptions = item["descriptions"].ToString();
                bool stickers = descriptions.Contains("sticker_info");

                if (!marketable | stickers | nameTag)
                    continue;

                decimal price;
                decimal lowest_price;
                decimal median_price;
                if (!QuickSell.SellItems.Any(c => c.ItemName == name))
                {
                    try
                    {
                        Tuple<Decimal, Decimal> prices = Get.PriceOverview(Edit.MarketHashName(name));
                        lowest_price = prices.Item1;
                        median_price = prices.Item2;

                        price = lowest_price;
                        if (lowest_price < median_price)
                            price = median_price;
                        if (price > BuyOrderProperties.Default.MaxPrice | price == 0)
                            continue;
                    }
                    catch
                    {
                        break;
                    }
                }
                else
                    price = QuickSell.SellItems.First(i => i.ItemName == name).Price;

                QuickSell.SellItems.Add( new QuickSell() 
                {
                    AssetId = assetid,
                    ItemName = name,
                    Price = price
                });
            }
        }
        public void sellItems(QuickSell item)
        {
            var sell_price = item.Price;
            sell_price = Edit.CommissionSteam(sell_price - 0.01m);

            Browser.ExecuteJavaScript(Post.SellItem(item.AssetId, (sell_price * 100).ToString(), SessionId));
        }
    }
}
