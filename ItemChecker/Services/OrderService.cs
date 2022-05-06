using ItemChecker.MVVM.Model;
using ItemChecker.Net;
using ItemChecker.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ItemChecker.Services
{
    public class OrderService : BaseService
    {
        protected void CancelOrders(List<DataOrder> cancelList)
        {
            int canceled = 0;
            if (cancelList.Any())
                foreach (DataOrder order in cancelList)
                {
                    CancelOrder(order);
                    canceled++;
                }
            if (SteamAccount.GetAvailableAmount() < (SteamAccount.Balance * 10 * 0.01m))
            {
                var min = DataOrder.Orders.Min(x => x.Precent);
                CancelOrder(DataOrder.Orders.FirstOrDefault(x => x.Precent == min));
                canceled++;
            }
            if (canceled > 0)
                Main.Notifications.Add(new()
                {
                    Title = "Orders",
                    Message = $"{canceled} orders were canceled."
                });
        }
        public static void CancelOrder(DataOrder order)
        {
            string market_hash_name = HttpUtility.UrlEncode(order.ItemName);

            Post.CancelBuyOrder(SteamAccount.Cookies, market_hash_name, order.OrderId);
            DataOrder.Orders.Remove(order);
        }
        protected Boolean CheckConditions(DataOrder order, decimal orderPrice)
        {
            ItemBase item = ItemBase.SkinsBase.FirstOrDefault(x => x.ItemName == order.ItemName);
            bool cancel =
                (SteamAccount.BalanceUsd < orderPrice && SettingsProperties.Default.CurrencyId == 0) ||
                (SteamAccount.Balance < orderPrice && SettingsProperties.Default.CurrencyId == 1) ||
                (SettingsProperties.Default.MinPrecent > order.Precent && order.Precent != -100) ||
                (SettingsProperties.Default.ServiceId == 2 && (item.CsmInfo.Overstock || item.CsmInfo.Unavailable)) ||
                (SettingsProperties.Default.ServiceId == 3 && (item.LfmInfo.Overstock || item.LfmInfo.Unavailable));

            return cancel;
        }
    }
}
