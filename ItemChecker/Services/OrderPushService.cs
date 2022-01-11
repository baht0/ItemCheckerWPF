using ItemChecker.MVVM.Model;
using ItemChecker.Net;
using ItemChecker.Properties;
using ItemChecker.Support;
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
                GetBase();
                SteamAccount.GetSteamBalance();
                OrderCheckService order = new();
                order.SteamOrders();
            }
            catch (Exception exp)
            {
                errorLog(exp);
            }
        }
        public Tuple<Int32, Boolean> PushItems(DataOrder order)
        {
            int push = 0;
            string market_hash_name = Edit.MarketHashName(order.ItemName);
            int item_nameid = ItemBase.SkinsBase.Where(x => x.ItemName == order.ItemName).Select(x => x.SteamId).FirstOrDefault();
            decimal highest_buy_order = Convert.ToDecimal(Get.ItemOrdersHistogram(item_nameid)["highest_buy_order"]) / 100;

            if (highest_buy_order > order.OrderPrice & SteamAccount.Balance >= highest_buy_order & (highest_buy_order - order.OrderPrice) <= SteamAccount.GetAvailableAmount())
            {
                Post.CancelBuyOrder(SettingsProperties.Default.SteamCookies, market_hash_name, order.OrderId);
                Thread.Sleep(1500);
                Post.CreateBuyOrder(SettingsProperties.Default.SteamCookies, market_hash_name, highest_buy_order);
                Thread.Sleep(1500);
                push++;
            }
            bool cancel = CheckConditions(order, highest_buy_order);

            return Tuple.Create(push, cancel);
        }
    }
}
