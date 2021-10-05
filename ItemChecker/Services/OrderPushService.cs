using ItemChecker.MVVM.Model;
using ItemChecker.Net;
using ItemChecker.Support;
using OpenQA.Selenium.Support.Extensions;
using System;
using System.Threading;

namespace ItemChecker.Services
{
    public class OrderPushService : OrderService
    {
        public void preparationPush()
        {
            try
            {
                Account.GetInformations();
                OrderCheckService order = new();
                order.SteamOrders();
            }
            catch (Exception exp)
            {
                errorLog(exp);
            }
        }
        public void PushItems(OrderData order)
        {
            string market_hash_name = Edit.MarketHashName(order.ItemName);
            Browser.Navigate().GoToUrl("https://steamcommunity.com/market/listings/730/" + market_hash_name);
            Thread.Sleep(500);
            int item_nameid = Edit.ItemNameId(Browser.PageSource);
            decimal highest_buy_order = Get.ItemOrdersHistogram(item_nameid);

            if (highest_buy_order > order.OrderPrice & Account.Balance >= highest_buy_order & (highest_buy_order - order.OrderPrice) <= Account.AvailableAmount)
            {
                Browser.ExecuteJavaScript(Post.CancelBuyOrder(order.OrderId, SessionId));
                Thread.Sleep(2000);
                Browser.ExecuteJavaScript(Post.CreateBuyOrder(market_hash_name, highest_buy_order, SessionId));
                Thread.Sleep(1500);
                OrderStatistic.Push++;
            }
            CheckConditions(order, highest_buy_order);
        }
    }
}
