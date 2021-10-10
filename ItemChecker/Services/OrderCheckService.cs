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
using System.Threading;

namespace ItemChecker.MVVM.Model
{
    public class OrderCheckService : OrderService
    {
        List<string> ItemType = new();
        List<string> ItemName = new();
        List<string> OrderId = new();
        List<decimal> StmPrice = new();
        List<decimal> OrderPrice = new();
        List<decimal> CsmPrice = new();
        List<decimal> CsmBuy = new();
        List<decimal> Precent = new();
        List<decimal> Difference = new();

        public void SteamOrders()
        {
            getSteamlist();
            checkOrders();

            Account.MyOrders.Clear();
            for (int i = 0; i < ItemName.Count; i++)
            {
                decimal _stmPrice = StmPrice[i];
                decimal _csmPrice = CsmPrice[i];
                decimal _stmBuy = CsmBuy[i];
                decimal _difference = Difference[i];
                if (GeneralProperties.Default.Currency == 1)
                {
                    _stmPrice = Edit.Converter(_stmPrice, GeneralProperties.Default.CurrencyValue);
                    _csmPrice = Edit.Converter(_csmPrice, GeneralProperties.Default.CurrencyValue);
                    _stmBuy = Edit.Converter(_stmBuy, GeneralProperties.Default.CurrencyValue);
                    _difference = Edit.Converter(_difference, GeneralProperties.Default.CurrencyValue);
                }

                Account.MyOrders.Add(new OrderData(ItemType[i], ItemName[i], OrderId[i], _stmPrice, OrderPrice[i], _csmPrice, _stmBuy, Precent[i], _difference));
                CheckConditions(Account.MyOrders.Last(), OrderPrice[i]);
            }
            Account.GetAvailableAmount();
        }
        void getSteamlist()
        {
            BaseModel.Browser.Navigate().GoToUrl("https://steamcommunity.com/market/");

            int table_index = 1;
            IWebElement table = BaseModel.WebDriverWait.Until(ExpectedConditions.ElementExists(By.XPath("//div[@class='my_listing_section market_content_block market_home_listing_table']/h3/span[1]")));
            if (table.Text == "My listings awaiting confirmation") table_index = 2;

            List<IWebElement> items = BaseModel.Browser.FindElements(By.XPath("//div[@class='my_listing_section market_content_block market_home_listing_table'][" + table_index + "]/div[@class='market_listing_row market_recent_listing_row']")).ToList();
            int i = 2;
            foreach (IWebElement item in items)
            {
                string[] str = item.Text.Split("\n");
                IWebElement id = BaseModel.WebDriverWait.Until(ExpectedConditions.ElementExists(By.XPath("//div[@class='my_listing_section market_content_block market_home_listing_table'][" + table_index + "]/div[" + i + "]")));
                
                string itemName = str[2].Trim();
                string type = "White";
                if (itemName.Contains("Sticker") | itemName.Contains("Graffiti"))
                    type = "DeepSkyBlue";
                if (itemName.Contains("Souvenir"))
                    type = "Yellow";
                if (itemName.Contains("StatTrak"))
                    type = "Orange";
                if (itemName.Contains("★"))
                    type = "DarkViolet";

                ItemType.Add(type);

                this.ItemName.Add(itemName);
                this.OrderPrice.Add(Edit.removeRub(str[0].Trim()));
                this.OrderId.Add(Edit.buyOrderId(id.GetAttribute("id"))); 
                i++;
            }
        }
        void checkOrders()
        {
            for (int i = 0; i < this.ItemName.Count; i++)
            {
                Tuple<String, Boolean> response = Tuple.Create(string.Empty, false);
                do
                {
                    var market_hash_name = Edit.MarketHashName(this.ItemName[i]);
                    response = Get.MrinkaRequest(market_hash_name);
                    if (!response.Item2)
                    {
                        Thread.Sleep(30000);
                    }
                }
                while (!response.Item2);

                parseOrder(response.Item1, i);
            }
        }
        void parseOrder(string response, int i)
        {
            decimal my_order = Convert.ToDecimal(this.OrderPrice[i]);
            decimal my_order_usd = Math.Round(my_order / GeneralProperties.Default.CurrencyValue, 2);
            decimal stm_sell = Convert.ToDecimal(JObject.Parse(response)["steam"]["sellOrder"].ToString());
            decimal csm_sell = Convert.ToDecimal(JObject.Parse(response)["csm"]["sell"].ToString());
            decimal csm_buy = Convert.ToDecimal(JObject.Parse(response)["csm"]["buy"]["0"].ToString());
            decimal precent = Edit.Precent(my_order_usd, csm_sell);
            decimal different = Edit.Difference(csm_sell, my_order_usd);

            this.StmPrice.Add(stm_sell);
            this.CsmPrice.Add(csm_buy);
            this.CsmBuy.Add(csm_sell);
            this.Precent.Add(precent);
            this.Difference.Add(different);
        }
    }
}