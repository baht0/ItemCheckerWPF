using ItemChecker.MVVM.Model;
using ItemChecker.Net;
using ItemChecker.Properties;
using System.Linq;
using System.Threading;

namespace ItemChecker.Services
{
    public class OrderService
    {
        public static decimal PlaceOrder(string itemName)
        {
            ItemBaseService baseService = new();
            baseService.UpdateSteamItem(itemName, SteamAccount.Currency.Id);
            var item = ItemsBase.List.FirstOrDefault(x => x.ItemName == itemName).Steam;

            if (SteamAccount.Balance > item.HighestBuyOrder)
            {
                SteamRequest.Post.CreateBuyOrder(itemName, item.HighestBuyOrder, SteamAccount.Currency.Id);
                SavedItems.Reserve.Add(new(itemName, ParserCheckConfig.CheckedConfig.ServiceTwo));
                return item.HighestBuyOrder;
            }
            return 0;
        }
        public static bool IsAllow(DataParser item)
        {
            return 
                (SteamMarket.Orders.Select(x => x.OrderPrice).Sum() + item.Purchase) > SteamMarket.Orders.GetAvailableAmount()
                && !SteamMarket.Orders.Any(n => n.ItemName == item.ItemName)
                && item.Precent > HomeProperties.Default.MinPrecent;
        }
        public static bool PushItems(DataOrder order)
        {
            ItemBaseService baseService = new();
            baseService.UpdateSteamItem(order.ItemName, SteamAccount.Currency.Id);
            var item = ItemsBase.List.FirstOrDefault(x => x.ItemName == order.ItemName).Steam;

            if (item.HighestBuyOrder > order.OrderPrice && SteamAccount.Balance >= item.HighestBuyOrder && (item.HighestBuyOrder - order.OrderPrice) <= SteamMarket.Orders.GetAvailableAmount())
            {
                SteamRequest.Post.CancelBuyOrder(order.ItemName, order.Id);
                Thread.Sleep(1500);
                SteamRequest.Post.CreateBuyOrder(order.ItemName, item.HighestBuyOrder, SteamAccount.Currency.Id);
                Thread.Sleep(1500);
                return true;
            }
            return false;
        }
    }
}