using ItemChecker.MVVM.Model;
using ItemChecker.Net;
using ItemChecker.Properties;
using ItemChecker.Support;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium.Support.Extensions;
using System;
using System.Linq;

namespace ItemChecker.Services
{
    public class PlaceOrderService : BaseService
    {

        public static void AddQueue(string itemName, decimal price)
        {
            itemName = Edit.removeDoppler(itemName);
            if (ParserList.DataCurrency == "USD")
                price = Math.Round(price * GeneralProperties.Default.CurrencyValue, 2);

            if (DataOrder.Orders.Any(n => n.ItemName == itemName) | DataOrder.Queue.Contains(itemName) | price > Account.Balance | DataOrder.AmountRub + price > DataOrder.AvailableAmount)
                return;

            DataOrder.AmountRub += price;
            DataOrder.Queue.Add(itemName);
        }
        public static void RemoveQueue(string itemName)
        {
            if (!DataOrder.Queue.Contains(itemName))
                return;

            decimal price = DataParser.ParserItems.First(p => p.ItemName == itemName).Price2;

            if (ParserList.DataCurrency == "USD")
                price = Math.Round(price * GeneralProperties.Default.CurrencyValue, 2);
            DataOrder.AmountRub -= price;
            DataOrder.Queue.Remove(itemName);
        }
        public void PlaceOrder(string itemName)
        {
            var market_hash_name = Edit.MarketHashName(itemName);
            BaseModel.Browser.Navigate().GoToUrl("https://steamcommunity.com/market/listings/730/" + market_hash_name);
            var item_nameid = ItemBase.SkinsBase.Where(x => x.ItemName == itemName).Select(x => x.SteamId).FirstOrDefault();
            JObject json = Get.ItemOrdersHistogram(item_nameid);
            decimal highest_buy_order = Convert.ToDecimal(json["highest_buy_order"]) / 100;

            if (Account.Balance > highest_buy_order)
                BaseModel.Browser.ExecuteJavaScript(Post.CreateBuyOrder(market_hash_name, highest_buy_order, Account.SessionId));
        }
    }
}