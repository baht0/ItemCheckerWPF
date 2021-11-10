using ItemChecker.MVVM.Model;
using ItemChecker.Net;
using ItemChecker.Properties;
using ItemChecker.Support;
using OpenQA.Selenium.Support.Extensions;
using System;
using System.Linq;

namespace ItemChecker.Services
{
    public class PlaceOrderService : BaseService
    {

        public static void AddQueue(string itemName, decimal price)
        {
            if (Parser.DataCurrency == "USD")
                price = Math.Round(price * GeneralProperties.Default.CurrencyValue, 2);

            if (Account.MyOrders.Any(n => n.ItemName == itemName) | OrderPlace.Queue.Contains(itemName) | price > Account.Balance | OrderPlace.AmountRub + price > Account.AvailableAmount)
                return;

            OrderPlace.AmountRub += price;
            OrderPlace.Queue.Add(itemName);
        }
        public static void RemoveQueue(string itemName)
        {
            if (!OrderPlace.Queue.Contains(itemName))
                return;

            decimal price = ParserData.ParserItems.First(p => p.ItemName == itemName).Price2;

            if (Parser.DataCurrency == "USD")
                price = Math.Round(price * GeneralProperties.Default.CurrencyValue, 2);
            OrderPlace.AmountRub -= price;
            OrderPlace.Queue.Remove(itemName);
        }
        public void PlaceOrder(string itemName)
        {
            var market_hash_name = Edit.MarketHashName(itemName);
            Main.Browser.Navigate().GoToUrl("https://steamcommunity.com/market/listings/730/" + market_hash_name);
            var item_nameid = Edit.ItemNameId(Browser.PageSource);
            var highest_buy_order = Get.ItemOrdersHistogram(item_nameid);

            if (Account.Balance > highest_buy_order)
                Main.Browser.ExecuteJavaScript(Post.CreateBuyOrder(market_hash_name, highest_buy_order, Account.SessionId));
        }
    }
}