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
        #region prop
        decimal stmPrice { get; set; } = 0;
        decimal orderPrice { get; set; } = 0;
        decimal servicePrice { get; set; } = 0;
        decimal serviceGive { get; set; } = 0;
        decimal precent { get; set; } = 0;
        decimal difference { get; set; } = 0;
        #endregion

        public void SteamOrders(bool isUpdateService)
        {
            List<DataOrder> dataOrders = new();

            HtmlDocument htmlDoc = new();
            Thread.Sleep(500);
            htmlDoc.LoadHtml(Get.Request(SteamCookies, "https://steamcommunity.com/market/"));
            int index = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='my_listing_section market_content_block market_home_listing_table']/h3/span[1]").InnerText.Trim() != "My listings awaiting confirmation" ? 1 : 2;
            HtmlNodeCollection items = htmlDoc.DocumentNode.SelectNodes("//div[@class='my_listing_section market_content_block market_home_listing_table'][" + index + "]/div[@class='market_listing_row market_recent_listing_row']");
            if (items != null)
            {
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
                            baseService.UpdateBuffInfo(0, int.MaxValue);
                            break;
                    }
                }

                List<DataOrder> cancelList = new();
                foreach (HtmlNode item in items)
                {
                    string name = item.SelectSingleNode(".//div[4]/span/a").InnerText.Trim();
                    string id = item.Attributes["id"].Value;
                    string price = item.SelectSingleNode(".//div[2]/span/span[@class='market_listing_price']").InnerText.Trim();
                    price = price[3..].Trim();
                    Tuple<DataOrder, bool> response = CheckOrder(name, id, price);
                    if (response.Item2)
                        cancelList.Add(response.Item1);
                    else
                        dataOrders.Add(response.Item1);
                }
                CancelOrders(cancelList);
                foreach (DataOrder item in dataOrders)
                    if (!HomeFavorite.FavoriteList.Contains(item.ItemName) && HomeFavorite.FavoriteList.Count < 200)
                    {
                        HomeFavorite.FavoriteList.Add(item.ItemName);
                        BaseService.SaveList("FavoriteList", HomeFavorite.FavoriteList.ToList());
                    }
            }
            DataOrder.Orders = dataOrders;
        }
        Tuple<DataOrder, Boolean> CheckOrder(string itemName, string order_id, string order_price)
        {
            ItemBase item = ItemBase.SkinsBase.FirstOrDefault(x => x.ItemName == itemName);
            JObject json = Get.ItemOrdersHistogram(item.SteamInfo.Id);

            orderPrice = Edit.GetPrice(order_price);
            string sell_order = json["lowest_sell_order"].ToString();
            stmPrice = !String.IsNullOrEmpty(sell_order) ? Convert.ToDecimal(sell_order) / 100 : 0;

            SetService(item);

            DataOrder data = new()
            {
                Type = item.Type,
                ItemName = itemName,
                OrderId = order_id.Replace("mybuyorder_", string.Empty),
                StmPrice = stmPrice,
                OrderPrice = orderPrice,
                ServicePrice = servicePrice,
                ServiceGive = serviceGive,
                Precent = precent,
                Difference = difference
            };
            return Tuple.Create(data, CheckConditions(data, orderPrice));
        }
        void SetService(ItemBase itemBase)
        {
            int serviceId = SettingsProperties.Default.ServiceId;
            switch (serviceId)
            {
                case 0:
                    servicePrice = 0;
                    serviceGive = 0;
                    precent = -100;
                    difference = 0;
                    break;
                case 1:
                    servicePrice = stmPrice;
                    serviceGive = Math.Round(stmPrice * Calculator.CommissionSteam, 2);
                    precent = Edit.Precent(orderPrice, serviceGive);
                    difference = Edit.Difference(serviceGive, orderPrice);
                    break;
                case 2:
                    servicePrice = itemBase.CsmInfo.Price;
                    serviceGive = Math.Round(itemBase.CsmInfo.Price * Calculator.CommissionCsm, 2);
                    break;
                case 3:
                    servicePrice = itemBase.LfmInfo.Price;
                    serviceGive = Math.Round(itemBase.LfmInfo.Price * Calculator.CommissionLf, 2);
                    break;
                case 4:
                    servicePrice = itemBase.BuffInfo.BuyOrder;
                    serviceGive = Math.Round(itemBase.BuffInfo.BuyOrder * Calculator.CommissionBuff, 2);
                    break;
            }
            decimal currencyValue = SettingsProperties.Default.RUB;
            if (serviceId > 1)
            {
                decimal my_order_usd = Math.Round(orderPrice / currencyValue, 2);
                precent = Edit.Precent(my_order_usd, serviceGive);
                difference = Edit.Difference(serviceGive, my_order_usd);

                if (SettingsProperties.Default.CurrencyId == 1)
                {
                    servicePrice = Edit.ConverterFromUsd(servicePrice, currencyValue);
                    serviceGive = Edit.ConverterFromUsd(serviceGive, currencyValue);
                    difference = Edit.ConverterFromUsd(difference, currencyValue);
                }
            }
        }
    }
}