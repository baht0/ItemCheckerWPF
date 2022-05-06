using ItemChecker.MVVM.Model;
using ItemChecker.Net;
using ItemChecker.Properties;
using System;
using System.Linq;
using System.Threading;
using System.Web;

namespace ItemChecker.Services
{
    public class OrderPushService : OrderService
    {
        public Boolean PushItems(DataOrder order)
        {
            bool push = false;
            string market_hash_name = HttpUtility.UrlEncode(order.ItemName);
            int item_nameid = ItemBase.SkinsBase.FirstOrDefault(x => x.ItemName == order.ItemName).SteamInfo.Id;
            decimal highest_buy_order = Convert.ToDecimal(Get.ItemOrdersHistogram(item_nameid)["highest_buy_order"]) / 100;

            if (highest_buy_order > order.OrderPrice & SteamAccount.Balance >= highest_buy_order & (highest_buy_order - order.OrderPrice) <= SteamAccount.GetAvailableAmount())
            {
                Post.CancelBuyOrder(SteamAccount.Cookies, market_hash_name, order.OrderId);
                Thread.Sleep(1500);
                Post.CreateBuyOrder(SteamAccount.Cookies, market_hash_name, highest_buy_order);
                Thread.Sleep(1500);
                return true;
            }
            return false;
        }
    }
}
