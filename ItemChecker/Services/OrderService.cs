using ItemChecker.MVVM.Model;
using ItemChecker.Net;
using ItemChecker.Properties;
using System;
using System.Linq;
using System.Threading;
using System.Web;

namespace ItemChecker.Services
{
    public class OrderService
    {
        public static void PlaceOrder(string itemName)
        {
            ItemBaseService baseService = new();
            baseService.UpdateSteamItem(itemName, SteamAccount.CurrencyId);
            var item = SteamBase.ItemList.FirstOrDefault(x => x.ItemName == itemName).Steam;

            if (SteamAccount.Balance > item.HighestBuyOrder)
            {
                string market_hash_name = HttpUtility.UrlEncode(itemName);
                Post.CreateBuyOrder(SteamAccount.Cookies, market_hash_name, item.HighestBuyOrder, SteamAccount.CurrencyId);
                ItemsList.Favorite.Add(new(itemName, ParserCheckConfig.CheckedConfig.ServiceTwo));
            }
        }
        public static Boolean IsAllow(DataParser item)
        {
            return (SteamMarket.Orders.Select(x => x.OrderPrice).Sum() + item.Purchase) > SteamMarket.Orders.GetAvailableAmount() &&
                !SteamMarket.Orders.Any(n => n.ItemName == item.ItemName) &&
                item.Precent > SettingsProperties.Default.MinPrecent;
        }
        public static Boolean PushItems(DataOrder order)
        {
            ItemBaseService baseService = new();
            baseService.UpdateSteamItem(order.ItemName, SteamAccount.CurrencyId);
            var item = SteamBase.ItemList.FirstOrDefault(x => x.ItemName == order.ItemName).Steam;

            if (item.HighestBuyOrder > order.OrderPrice & SteamAccount.Balance >= item.HighestBuyOrder & (item.HighestBuyOrder - order.OrderPrice) <= SteamMarket.Orders.GetAvailableAmount())
            {
                string market_hash_name = HttpUtility.UrlEncode(order.ItemName);
                Post.CancelBuyOrder(SteamAccount.Cookies, market_hash_name, order.Id);
                Thread.Sleep(1500);
                Post.CreateBuyOrder(SteamAccount.Cookies, market_hash_name, item.HighestBuyOrder, SteamAccount.CurrencyId);
                Thread.Sleep(1500);
                return true;
            }
            return false;
        }
    }
}