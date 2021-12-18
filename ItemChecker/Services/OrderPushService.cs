using ItemChecker.MVVM.Model;
using ItemChecker.Net;
using ItemChecker.Support;
using OpenQA.Selenium.Support.Extensions;
using System;
using System.Linq;
using System.Threading;

namespace ItemChecker.Services
{
    public class OrderPushService : OrderService
    {
        public void preparationPush()
        {
            try
            {
                Account.GetBase();
                Account.GetSteamAccount();
                OrderCheckService order = new();
                order.SteamOrders();
            }
            catch (Exception exp)
            {
                errorLog(exp);
            }
        }
        public void PushItems(DataOrder order)
        {
            string market_hash_name = Edit.MarketHashName(order.ItemName);
            Browser.Navigate().GoToUrl("https://steamcommunity.com/market/listings/730/" + market_hash_name);
            int item_nameid = ItemBase.SkinsBase.Where(x => x.ItemName == order.ItemName).Select(x => x.SteamId).FirstOrDefault();
            decimal highest_buy_order = Convert.ToDecimal(Get.ItemOrdersHistogram(item_nameid)["highest_buy_order"]) / 100;

            if (highest_buy_order > order.OrderPrice & Account.Balance >= highest_buy_order & (highest_buy_order - order.OrderPrice) <= DataOrder.AvailableAmount)
            {
                Browser.ExecuteJavaScript(Post.CancelBuyOrder(order.OrderId, Account.SessionId));
                Thread.Sleep(2000);
                Browser.ExecuteJavaScript(Post.CreateBuyOrder(market_hash_name, highest_buy_order, Account.SessionId));
                Thread.Sleep(1500);
                Home.Push++;
            }
            CheckConditions(order, highest_buy_order);
        }
    }
}
