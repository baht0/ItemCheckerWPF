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
        public void SteamOrders()
        {
            List<DataOrder> cancelList = new();
            List<DataOrder> dataOrders = new();
            DataOrder.Orders.Clear();
            string html = Get.Request(SteamCookies, "https://steamcommunity.com/market/");
            HtmlDocument htmlDoc = new();
            Thread.Sleep(500);
            htmlDoc.LoadHtml(html);

            string table = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='my_listing_section market_content_block market_home_listing_table']/h3/span[1]").InnerText.Trim();
            int index = table != "My listings awaiting confirmation" ? 1 : 2;
            HtmlNodeCollection items = htmlDoc.DocumentNode.SelectNodes("//div[@class='my_listing_section market_content_block market_home_listing_table'][" + index + "]/div[@class='market_listing_row market_recent_listing_row']");
            if (items != null)
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
                if (!HomeProperties.Default.FavoriteList.Contains(item.ItemName) && HomeProperties.Default.FavoriteList.Count < 200)
                {
                    HomeProperties.Default.FavoriteList.Add(item.ItemName);
                    HomeProperties.Default.Save();
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
            
            var data = new DataOrder(item.Type, itemName, order_id.Replace("mybuyorder_", string.Empty), stmPrice, orderPrice, servicePrice, serviceGive, precent, difference);
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
            }
            decimal currencyValue = SettingsProperties.Default.CurrencyValue;
            if (serviceId > 1)
            {
                decimal my_order_usd = Math.Round(orderPrice / currencyValue, 2);
                precent = Edit.Precent(my_order_usd, serviceGive);
                difference = Edit.Difference(serviceGive, my_order_usd);

                if (SettingsProperties.Default.CurrencyId == 1)
                {
                    servicePrice = Edit.ConverterToRub(servicePrice, currencyValue);
                    serviceGive = Edit.ConverterToRub(serviceGive, currencyValue);
                    difference = Edit.ConverterToRub(difference, currencyValue);
                }
            }
        }
    }
}