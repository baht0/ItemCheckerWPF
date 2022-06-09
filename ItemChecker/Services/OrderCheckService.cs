using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using ItemChecker.Net;
using ItemChecker.Properties;
using ItemChecker.Services;
using ItemChecker.Support;
using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;

namespace ItemChecker.MVVM.Model
{
    public class OrderCheckService : OrderService
    {
        public void SteamOrders(bool isUpdateService)
        {
            DataOrder.Orders.Clear();
            List<DataOrder> dataOrders = new();

            HtmlDocument htmlDoc = new();
            Thread.Sleep(500);
            htmlDoc.LoadHtml(Get.Request(SteamAccount.Cookies, "https://steamcommunity.com/market/"));
            int index = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='my_listing_section market_content_block market_home_listing_table']/h3/span[1]").InnerText.Trim() != "My listings awaiting confirmation" ? 1 : 2;
            HtmlNodeCollection items = htmlDoc.DocumentNode.SelectNodes("//div[@class='my_listing_section market_content_block market_home_listing_table'][" + index + "]/div[@class='market_listing_row market_recent_listing_row']");
            if (items != null)
            {
                foreach (HtmlNode item in items)
                    dataOrders.Add(CheckOrder(item));
                if (isUpdateService)
                {
                    ItemBaseService baseService = new();
                    switch (SettingsProperties.Default.ServiceId)
                    {
                        case 2:
                            baseService.UpdateCsmInfo();
                            break;
                        case 3:
                            baseService.UpdateLfmInfo();
                            break;
                        case 4:
                            while (!BuffAccount.IsLogIn())
                                Thread.Sleep(200);
                            foreach (var data in dataOrders)
                                baseService.UpdateBuffInfoItem(data.ItemName);
                            break;
                    }
                }
                int canceled = 0;
                foreach (DataOrder data in dataOrders)
                {
                    DataOrder item = SetService(data);
                    if (!IsCancel(item))
                    {
                        DataOrder.Orders.Add(item);
                        DataSavedList.Add(item.ItemName, "favorite", SettingsProperties.Default.ServiceId);
                    }
                    else
                    {
                        CancelOrder(item);
                        canceled++;
                    }
                }
                if (SteamAccount.GetAvailableAmount() < (SteamAccount.Balance * 10 * 0.01m))
                {
                    CancelOrder(DataOrder.Orders.FirstOrDefault(x => x.Precent == DataOrder.Orders.Min(x => x.Precent)));
                    canceled++;
                }
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

            JObject json = Get.ItemOrdersHistogram(SteamBase.ItemList.FirstOrDefault(x => x.ItemName == itemName).Steam.Id, SteamAccount.CurrencyId);
            string sell_order = json["lowest_sell_order"].ToString();
            decimal stmPrice = !String.IsNullOrEmpty(sell_order) ? Convert.ToDecimal(sell_order) / 100 : 0;

            DataOrder data = new()
            {
                ItemName = itemName,
                OrderId = order_id.Replace("mybuyorder_", string.Empty),
                StmPrice = stmPrice,
                OrderPrice = orderPrice
            };
            return data;
        }
        static DataOrder SetService(DataOrder item)
        {
            var itemBase = SteamBase.ItemList.FirstOrDefault(x => x.ItemName == item.ItemName);
            int serviceId = SettingsProperties.Default.ServiceId;
            switch (serviceId)
            {
                case 0:
                    item.ServicePrice = 0;
                    item.ServiceGive = 0;
                    item.Precent = -100;
                    item.Difference = 0;
                    break;
                case 1:
                    item.ServicePrice = item.StmPrice;
                    item.ServiceGive = Math.Round(item.StmPrice * Calculator.CommissionSteam, 2);
                    item.Precent = Edit.Precent(item.OrderPrice, item.ServiceGive);
                    item.Difference = Edit.Difference(item.ServiceGive, item.OrderPrice);
                    break;
                case 2:
                    item.ServicePrice = itemBase.Csm.Price;
                    item.ServiceGive = Math.Round(itemBase.Csm.Price * Calculator.CommissionCsm, 2);
                    break;
                case 3:
                    item.ServicePrice = itemBase.Lfm.Price;
                    item.ServiceGive = Math.Round(itemBase.Lfm.Price * Calculator.CommissionLf, 2);
                    break;
                case 4:
                    item.ServicePrice = itemBase.Buff.BuyOrder;
                    item.ServiceGive = Math.Round(itemBase.Buff.BuyOrder * Calculator.CommissionBuff, 2);
                    break;
            }
            decimal currencyValue = SteamBase.CurrencyList.FirstOrDefault(x => x.Id == SteamAccount.CurrencyId).Value;
            if (serviceId > 1)
            {
                if (SteamAccount.CurrencyId != 1)
                {
                    item.ServicePrice = Edit.ConverterFromUsd(item.ServicePrice, currencyValue);
                    item.ServiceGive = Edit.ConverterFromUsd(item.ServiceGive, currencyValue);
                    item.Difference = Edit.ConverterFromUsd(item.Difference, currencyValue);
                }
                item.Precent = Edit.Precent(item.OrderPrice, item.ServiceGive);
                item.Difference = Edit.Difference(item.ServiceGive, item.OrderPrice);
            }
            return item;
        }
    }
}