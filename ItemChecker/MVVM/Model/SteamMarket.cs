using HtmlAgilityPack;
using ItemChecker.Net;
using ItemChecker.Properties;
using ItemChecker.Support;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ItemChecker.MVVM.Model
{
    internal class SteamMarket
    {
        public static string StatusCommunity
        {
            get
            {
                try
                {
                    JObject res = SteamRequest.Get.GameServersStatus();
                    return res["result"]["services"]["SteamCommunity"].ToString();
                }
                catch
                {
                    return "error";
                }
            }
        }
    }
    public class OrderCheckService
    {
        public static void SteamOrders(bool isUpdateService)
        {
            SteamAccount.Orders.Clear();
            int canceled = 0;

            HtmlDocument htmlDoc = new();
            Thread.Sleep(500);
            htmlDoc.LoadHtml(SteamRequest.Get.Request("https://steamcommunity.com/market/"));
            int index = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='my_listing_section market_content_block market_home_listing_table']/h3/span[1]").InnerText.Trim() != "My listings awaiting confirmation" ? 1 : 2;
            HtmlNodeCollection items = htmlDoc.DocumentNode.SelectNodes("//div[@class='my_listing_section market_content_block market_home_listing_table'][" + index + "]/div[@class='market_listing_row market_recent_listing_row']");

            if (items != null)
            {
                foreach (HtmlNode item in items)
                {
                    DataOrder data = CheckOrder(item);
                    data = SetService(data, isUpdateService);

                    canceled += SteamAccount.Orders.Add(data) ? 0 : 1;
                }
                canceled += SteamAccount.Orders.CancelMinPrecent() ? 1 : 0;
                if (canceled > 0)
                    Main.Notifications.Add(new()
                    {
                        Title = "Orders",
                        Message = $"{canceled} orders were canceled."
                    });
            }
        }

        static DataOrder CheckOrder(HtmlNode item)
        {
            string itemName = item.SelectSingleNode(".//div[4]/span/a").InnerText.Trim();
            string order_id = item.Attributes["id"].Value;
            string order_price = item.SelectSingleNode(".//div[2]/span/span[@class='market_listing_price']").InnerText.Trim();
            decimal orderPrice = Edit.GetDecimal(order_price[3..].Trim());

            DataOrder data = new()
            {
                ItemName = itemName,
                Id = order_id.Replace("mybuyorder_", string.Empty),
                OrderPrice = orderPrice
            };
            return data;
        }
        static DataOrder SetService(DataOrder data, bool isUpdateService)
        {
            int serviceId = HomeProperties.Default.ServiceId;
            switch (serviceId)
            {
                case 0:
                    {
                        data.ServicePrice = 0;
                        data.ServiceGive = 0;
                        data.Precent = -100;
                        data.Difference = 0;
                        break;
                    }
                case 1:
                    {
                        if (isUpdateService)
                            ItemBaseService.UpdateSteamItem(data.ItemName);
                        var Item = ItemsBase.List.FirstOrDefault(x => x.ItemName == data.ItemName).Steam;
                        data.ServicePrice = Item.LowestSellOrder;
                        data.ServiceGive = Math.Round(Item.LowestSellOrder * Calculator.CommissionSteam, 2);
                        break;
                    }
                case 2:
                    {
                        if (isUpdateService)
                            ItemBaseService.UpdateCsmItem(data.ItemName, false);
                        var Item = ItemsBase.List.FirstOrDefault(x => x.ItemName == data.ItemName).Csm;
                        data.ServicePrice = Item.Price;
                        data.ServiceGive = Math.Round(Item.Price * Calculator.CommissionCsm, 2);
                        break;
                    }
                case 3:
                    {
                        if (isUpdateService)
                            ItemBaseService.UpdateLfm();
                        var Item = ItemsBase.List.FirstOrDefault(x => x.ItemName == data.ItemName).Lfm;
                        data.ServicePrice = Item.Price;
                        data.ServiceGive = Math.Round(Item.Price * Calculator.CommissionLf, 2);
                        break;
                    }
                case 4:
                    {
                        if (isUpdateService)
                            ItemBaseService.UpdateBuffItem(data.ItemName);
                        var Item = ItemsBase.List.FirstOrDefault(x => x.ItemName == data.ItemName).Buff;
                        data.ServicePrice = Item.BuyOrder;
                        data.ServiceGive = Math.Round(Item.BuyOrder * Calculator.CommissionBuff, 2);
                        break;
                    }
                case 5:
                    {
                        if (isUpdateService)
                            ItemBaseService.UpdateBuffItem(data.ItemName);
                        var Item = ItemsBase.List.FirstOrDefault(x => x.ItemName == data.ItemName).Buff;
                        data.ServicePrice = Item.Price;
                        data.ServiceGive = Math.Round(Item.Price * Calculator.CommissionBuff, 2);
                        break;
                    }
            }
            if (serviceId != 0)
            {
                if (SteamAccount.Currency.Id != 1)
                {
                    var id = SteamAccount.Currency.Id;
                    data.ServicePrice = Currency.ConverterFromUsd(data.ServicePrice, id);
                    data.ServiceGive = Currency.ConverterFromUsd(data.ServiceGive, id);
                    data.Difference = Currency.ConverterFromUsd(data.Difference, id);
                }
                data.Precent = Edit.Precent(data.OrderPrice, data.ServiceGive);
                data.Difference = Edit.Difference(data.ServiceGive, data.OrderPrice);
            }
            return data;
        }

        public static void PlaceOrderFromReserve()
        {
            if (SteamAccount.Orders.GetAvailableAmount() < SteamAccount.MaxOrderAmount * 0.15m || !SavedItems.Reserve.Any())
                return;

            int count = 0;
            decimal sum = 0m;
            foreach (var item in SavedItems.Reserve.Where(x => x.ServiceId == HomeProperties.Default.ServiceId))
            {
                try
                {
                    decimal orderPrice = ToolPlaceOrder.PlaceOrder(item.ItemName, HomeProperties.Default.ServiceId);
                    sum += orderPrice;
                    count += orderPrice > 0 ? 1 : 0;
                    if (sum >= SteamAccount.Orders.GetAvailableAmount())
                        break;
                }
                catch (Exception exp)
                {
                    BaseModel.ErrorLog(exp, false);
                    continue;
                }
            }
            if (count > 0)
                Main.Notifications.Add(new()
                {
                    Title = "Orders",
                    Message = $"{count} orders placed from Reserve.",
                });
        }
    }

    public class DataListingItem
    {
        public string ListingId { get; set; } = "0";
        public decimal Fee { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Total { get; set; }
    }
    public class DataOrder
    {
        public string ItemName { get; set; }
        public string Id { get; set; }
        public decimal OrderPrice { get; set; }
        public decimal ServicePrice { get; set; }
        public decimal ServiceGive { get; set; }
        public decimal Precent { get; set; }
        public decimal Difference { get; set; }
    }
    public class Orders<T> : List<T>
    {
        public bool CancelMinPrecent()
        {
            var currentList = this as Orders<DataOrder>;
            decimal availableAmount = GetAvailableAmount();

            if (availableAmount < (SteamAccount.MaxOrderAmount * 0.01m))
            {
                currentList.Cancel(currentList.FirstOrDefault(x => x.Precent == currentList.Min(x => x.Precent)));
                return true;
            }
            return false;
        }
        public decimal GetAvailableAmount()
        {
            var currentList = this as Orders<DataOrder>;

            if (currentList.Any())
                return Math.Round(SteamAccount.MaxOrderAmount - currentList.Sum(s => s.OrderPrice), 2);
            return SteamAccount.MaxOrderAmount;
        }
        public void Cancel(T order)
        {
            var item = order as DataOrder;
            SteamRequest.Post.CancelBuyOrder(item.ItemName, item.Id);
            base.Remove(order);
            var reservItem = SavedItems.Reserve.FirstOrDefault(x => x.ItemName == item.ItemName && x.ServiceId == HomeProperties.Default.ServiceId);
            if (reservItem != null)
                SavedItems.Reserve.Remove(reservItem);
        }
        public new bool Add(T order)
        {
            if (IsAllow(order))
            {
                base.Add(order);
                var item = order as DataOrder;
                SavedItems.Reserve.Add(new(item.ItemName, HomeProperties.Default.ServiceId));
                return true;
            }
            Cancel(order);
            return false;
        }
        bool IsAllow(T order)
        {
            var itemOrder = order as DataOrder;
            var item = ItemsBase.List.FirstOrDefault(x => x.ItemName == itemOrder.ItemName);

            bool isAllow = item != null;
            if (isAllow)
                isAllow = SteamAccount.Balance > itemOrder.OrderPrice;
            if (isAllow)
                isAllow = HomeProperties.Default.ServiceId == 0 || (HomeProperties.Default.ServiceId != 0 && HomeProperties.Default.MinPrecent < itemOrder.Precent);
            if (isAllow)
            {
                switch (HomeProperties.Default.ServiceId)
                {
                    case 2:
                        isAllow = item.Csm.Status != ItemStatus.Overstock && item.Csm.Status != ItemStatus.Unavailable;
                        break;
                    case 3:
                        isAllow = item.Lfm.Status != ItemStatus.Overstock && item.Lfm.Status != ItemStatus.Unavailable;
                        break;
                }
            }

            return isAllow;
        }
    }
}
