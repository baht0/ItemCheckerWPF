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
            try
            {
                if (!Account.MyOrders.Any(n => n.ItemName == itemName) & price <= Account.BalanceUsd & !OrderPlace.Queue.Contains(itemName))
                {
                    OrderPlace.AmountRub += Math.Round(price * GeneralProperties.Default.CurrencyValue, 2);
                    OrderPlace.Queue.Add(itemName);
                }
            }
            catch (Exception exp)
            {
                errorMessage(exp);
                errorLog(exp);
            }
        }
        public static void RemoveQueue(string itemName)
        {
            try
            {
                decimal price = ParserData.ParserItems.First(p => p.ItemName == itemName).Price2;
                if (!Account.MyOrders.Any(n => n.ItemName == itemName) & price <= Account.BalanceUsd & OrderPlace.Queue.Contains(itemName))
                {
                    OrderPlace.AmountRub -= Math.Round(price * GeneralProperties.Default.CurrencyValue, 2);
                    OrderPlace.Queue.Remove(itemName);
                }
            }
            catch (Exception exp)
            {
                errorMessage(exp);
                errorLog(exp);
            }
        }
        public void PlaceOrder(string itemName)
        {
            var market_hash_name = Edit.MarketHashName(itemName);
            Main.Browser.Navigate().GoToUrl("https://steamcommunity.com/market/listings/730/" + market_hash_name);
            var item_nameid = Edit.ItemNameId(Main.Browser.PageSource);
            var highest_buy_order = Get.ItemOrdersHistogram(item_nameid);

            if (Account.Balance > highest_buy_order)
                Main.Browser.ExecuteJavaScript(Post.CreateBuyOrder(market_hash_name, highest_buy_order, Main.SessionId));
        }
    }
}
