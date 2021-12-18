using ItemChecker.Net;
using ItemChecker.Properties;
using ItemChecker.Services;
using ItemChecker.Support;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ItemChecker.MVVM.Model
{
    public class OrderCheckService : OrderService
    {
        public void SteamOrders()
        {
            DataOrder.Orders.Clear();

            Browser.Navigate().GoToUrl("https://steamcommunity.com/market/");

            IWebElement table = WebDriverWait.Until(e => e.FindElement(By.XPath("//div[@class='my_listing_section market_content_block market_home_listing_table']/h3/span[1]")));
            int table_index = table.Text != "My listings awaiting confirmation" ? table_index = 1 : table_index = 2;

            List<IWebElement> items = Browser.FindElements(By.XPath("//div[@class='my_listing_section market_content_block market_home_listing_table'][" + table_index + "]/div[@class='market_listing_row market_recent_listing_row']")).ToList();
            foreach (IWebElement item in items)
                parseOrder(item);

            Account.GetAvailableAmount();
        }
        void parseOrder(IWebElement item)
        {
            string[] str = item.Text.Split("\n");

            string itemName = str[2].Trim();
            string order_id = Edit.buyOrderId(item.GetAttribute("id"));
            decimal order_price = Edit.removeRub(str[0].Trim());

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
            decimal csm_buy = Math.Round(itemBase.PriceCsm * 0.95m, 2);

            decimal my_order = Edit.removeRub(str[0].Trim());
            decimal my_order_usd = Math.Round(my_order / GeneralProperties.Default.CurrencyValue, 2);
            decimal precent = Edit.Precent(my_order_usd, csm_sell);
            decimal different = Edit.Difference(csm_sell, my_order_usd);

            if (GeneralProperties.Default.Currency == 1)
            {
                csm_buy = Edit.ConverterToRub(csm_buy, GeneralProperties.Default.CurrencyValue);
                csm_sell = Edit.ConverterToRub(csm_sell, GeneralProperties.Default.CurrencyValue);
                different = Edit.ConverterToRub(different, GeneralProperties.Default.CurrencyValue);
            }

            DataOrder.Orders.Add(new DataOrder(type, itemName, order_id, stm_sell, order_price, csm_buy, csm_sell, precent, different));
            CheckConditions(DataOrder.Orders.Last(), order_price);
        }
    }
}