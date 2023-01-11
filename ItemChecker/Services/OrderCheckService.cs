using HtmlAgilityPack;
using ItemChecker.Net;
using ItemChecker.Properties;
using ItemChecker.Services;
using ItemChecker.Support;
using System;
using System.Linq;
using System.Threading;

namespace ItemChecker.MVVM.Model
{
    public class OrderCheckService : OrderService
    {
        public static void SteamOrders(bool isUpdateService)
        {
            SteamMarket.Orders.Clear();
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

                    canceled += SteamMarket.Orders.Add(data) ? 0 : 1;
                }
                canceled += SteamMarket.Orders.CancelMinPrecent() ? 1 : 0;
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
            if (SteamMarket.Orders.GetAvailableAmount() < SteamMarket.MaxAmount * 0.15m || !SavedItems.Reserve.Any())
                return;

            int count = 0;
            decimal sum = 0m;
            foreach (var item in SavedItems.Reserve.Where(x => x.ServiceId == HomeProperties.Default.ServiceId))
            {
                try
                {
                    decimal orderPrice = PlaceOrder(item.ItemName);
                    sum += orderPrice;
                    count += orderPrice > 0 ? 1 : 0;
                    if (sum >= SteamMarket.Orders.GetAvailableAmount())
                        break;
                }
                catch (Exception exp)
                {
                    BaseService.errorLog(exp, false);
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
}