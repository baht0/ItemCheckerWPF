using ItemChecker.MVVM.Model;
using ItemChecker.Net;
using ItemChecker.Properties;
using ItemChecker.Support;
using System;
using System.Linq;

namespace ItemChecker.Services
{
    public class OrderService : BaseService
    {
        //delete order
        public static void CancelOrder(DataOrder order)
        {
            string market_hash_name = Edit.MarketHashName(order.ItemName);

            Post.CancelBuyOrder(SettingsProperties.Default.SteamCookies, market_hash_name, order.OrderId);
            DataOrder.Orders.Remove(order);
        }
        protected Boolean CheckConditions(DataOrder order, decimal orderPrice)
        {
            bool cancel = false;
            if (SettingsProperties.Default.NotEnoughBalance & SteamAccount.Balance < orderPrice)
            {
                cancel = true;
            }
            else if (SettingsProperties.Default.CancelOrder > order.Precent & order.Precent != -100)
            {
                cancel = true;
            }
            else if (ItemBase.Overstock.Any(x => x.ItemName == order.ItemName) | ItemBase.Unavailable.Any(x => x.ItemName == order.ItemName))
            {
                cancel = true;
            }
            return cancel;
        }
    }
}
