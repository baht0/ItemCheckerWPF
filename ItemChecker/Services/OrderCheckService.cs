using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
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
        public Decimal SteamOrders()
        {
            DataOrder.Orders.Clear();
            string html = Get.Request(SettingsProperties.Default.SteamCookies, "https://steamcommunity.com/market/");
            HtmlDocument htmlDoc = null;
            Thread.Sleep(500);
            htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            string table = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='my_listing_section market_content_block market_home_listing_table']/h3/span[1]").InnerText.Trim();
            int index = table != "My listings awaiting confirmation" ? 1 : 2;
            HtmlNodeCollection items = htmlDoc.DocumentNode.SelectNodes("//div[@class='my_listing_section market_content_block market_home_listing_table'][" + index + "]/div[@class='market_listing_row market_recent_listing_row']");
            if(items != null)
                foreach (HtmlNode item in items)
                {
                    string name = item.SelectSingleNode(".//div[4]/span/a").InnerText.Trim();
                    string id = item.Attributes["id"].Value;
                    string price = item.SelectSingleNode(".//div[2]/span/span[@class='market_listing_price']").InnerText.Trim();
                    price = price[3..].Trim();
                    CheckOrder(name, id, price);
                }
            var availableAmount = SteamAccount.GetAvailableAmount();
            if (availableAmount < (SteamAccount.Balance * 10 * 0.01m))
            {
                var min = DataOrder.Orders.Min(x => x.Precent);
                CancelOrder(DataOrder.Orders.Where(x => x.Precent == min).FirstOrDefault());
            }
            return SteamAccount.GetAvailableAmount();
        }
        void CheckOrder(string itemName, string order_id, string order_price)
        {
            order_id = order_id.Replace("mybuyorder_", "");
            decimal my_order = Edit.GetPrice(order_price);

            string type = "Normal";
            if (itemName.Contains("Souvenir"))
                type = "Souvenir";
            if (itemName.Contains("StatTrak"))
                type = "Stattrak";
            if (itemName.Contains("★ "))
                type = "KnifeGlove";
            if (itemName.Contains("★ StatTrak"))
                type = "KnifeGloveStattrak";

            ItemBase itemBase = ItemBase.SkinsBase.Where(x => x.ItemName == itemName).First();
            JObject json = Get.ItemOrdersHistogram(itemBase.SteamId);

            var sell_order = json["lowest_sell_order"].ToString();
            decimal stm_sell = 0;
            if (!String.IsNullOrEmpty(sell_order))
                stm_sell = Convert.ToDecimal(sell_order) / 100;
            decimal csm_sell = itemBase.PriceCsm;
            decimal csm_buy = Math.Round(itemBase.PriceCsm * Calculator.CommissionCsm, 2);

            decimal my_order_usd = Math.Round(my_order / SettingsProperties.Default.CurrencyValue, 2);
            decimal precent = Edit.Precent(my_order_usd, csm_buy);
            decimal different = Edit.Difference(csm_buy, my_order_usd);

            if (SettingsProperties.Default.Currency == 1)
            {
                csm_buy = Edit.ConverterToRub(csm_buy, SettingsProperties.Default.CurrencyValue);
                csm_sell = Edit.ConverterToRub(csm_sell, SettingsProperties.Default.CurrencyValue);
                different = Edit.ConverterToRub(different, SettingsProperties.Default.CurrencyValue);
            }

            DataOrder.Orders.Add(new DataOrder(type, itemName, order_id, stm_sell, my_order, csm_sell, csm_buy, precent, different));
            bool cancel = CheckConditions(DataOrder.Orders.Last(), my_order);
            if (cancel)
                CancelOrder(DataOrder.Orders.Last());
        }
    }
}