using ItemChecker.MVVM.Model;
using ItemChecker.Net;
using ItemChecker.Properties;
using System;
using System.Linq;
using System.Web;

namespace ItemChecker.Services
{
    public class OrderService : BaseService
    {
        public static void CancelOrder(DataOrder order)
        {
            string market_hash_name = HttpUtility.UrlEncode(order.ItemName);

            Post.CancelBuyOrder(SteamAccount.Cookies, market_hash_name, order.OrderId);
            if (DataOrder.Orders.Contains(order))
                DataOrder.Orders.Remove(order);
        }
        protected Boolean IsCancel(DataOrder order)
        {
            var item = SteamBase.ItemList.FirstOrDefault(x => x.ItemName == order.ItemName);

            return SteamAccount.Balance < order.OrderPrice ||
                (SettingsProperties.Default.MinPrecent > order.Precent && SettingsProperties.Default.ServiceId != 0) || 
                (SettingsProperties.Default.ServiceId == 2 && (item.Csm.Overstock || item.Csm.Unavailable)) || 
                (SettingsProperties.Default.ServiceId == 3 && (item.Lfm.Overstock || item.Lfm.Unavailable));
        }
    }
}
