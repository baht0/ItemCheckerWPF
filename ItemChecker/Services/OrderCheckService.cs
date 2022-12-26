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
            decimal orderPrice = Edit.GetPrice(order_price[3..].Trim());

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
            ItemBaseService baseService = new();
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
                            baseService.UpdateSteamItem(data.ItemName);
                        var Item = SteamBase.ItemList.FirstOrDefault(x => x.ItemName == data.ItemName).Steam;
                        data.ServicePrice = Item.LowestSellOrder;
                        data.ServiceGive = Math.Round(Item.LowestSellOrder * Calculator.CommissionSteam, 2);
                        break;
                    }
                case 2:
                    {
                        if (isUpdateService)
                            baseService.UpdateCsmItem(data.ItemName, false);
                        var Item = SteamBase.ItemList.FirstOrDefault(x => x.ItemName == data.ItemName).Csm;
                        data.ServicePrice = Item.Price;
                        data.ServiceGive = Math.Round(Item.Price * Calculator.CommissionCsm, 2);
                        break;
                    }
                case 3:
                    {
                        if (isUpdateService)
                            baseService.UpdateLfm();
                        var Item = SteamBase.ItemList.FirstOrDefault(x => x.ItemName == data.ItemName).Lfm;
                        data.ServicePrice = Item.Price;
                        data.ServiceGive = Math.Round(Item.Price * Calculator.CommissionLf, 2);
                        break;
                    }
                case 4:
                    {
                        if (isUpdateService)
                            baseService.UpdateBuffItem(data.ItemName);
                        var Item = SteamBase.ItemList.FirstOrDefault(x => x.ItemName == data.ItemName).Buff;
                        data.ServicePrice = Item.BuyOrder;
                        data.ServiceGive = Math.Round(Item.BuyOrder * Calculator.CommissionBuff, 2);
                        break;
                    }
                case 5:
                    {
                        if (isUpdateService)
                            baseService.UpdateBuffItem(data.ItemName);
                        var Item = SteamBase.ItemList.FirstOrDefault(x => x.ItemName == data.ItemName).Buff;
                        data.ServicePrice = Item.Price;
                        data.ServiceGive = Math.Round(Item.Price * Calculator.CommissionBuff, 2);
                        break;
                    }
            }
            if (serviceId != 0)
            {
                if (SteamAccount.Currency.Id != 1)
                {
                    decimal currencyValue = SteamAccount.Currency.Value;
                    data.ServicePrice = Edit.ConverterFromUsd(data.ServicePrice, currencyValue);
                    data.ServiceGive = Edit.ConverterFromUsd(data.ServiceGive, currencyValue);
                    data.Difference = Edit.ConverterFromUsd(data.Difference, currencyValue);
                }
                data.Precent = Edit.Precent(data.OrderPrice, data.ServiceGive);
                data.Difference = Edit.Difference(data.ServiceGive, data.OrderPrice);
            }
            return data;
        }
    }
}