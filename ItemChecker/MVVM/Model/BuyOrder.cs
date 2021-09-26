using ItemChecker.Net;
using ItemChecker.Properties;
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
    public class BuyOrder
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
            if (Account.OrdersCount != 0)
            {
                getSteamlist();
                availableAmount();
                if (GeneralProperties.Default.proxy & ItemName.Count > 30)
                    checkOrdersProxy();
                else
                    checkOrders();

                for (int i = 0; i < ItemName.Count; i++)
                    Account.MyOrders.Add(new MyOrder(ItemType[i], ItemName[i], OrderId[i], StmPrice[i], OrderPrice[i], CsmPrice[i], CsmBuy[i], Precent[i], Difference[i]));
            }
        }
        private void getSteamlist()
        {
            Start.Browser.Navigate().GoToUrl("https://steamcommunity.com/market/");

            int table_index = 1;
            IWebElement table = Start.webDriverWait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//div[@class='my_listing_section market_content_block market_home_listing_table']/h3/span[1]")));
            if (table.Text == "My listings awaiting confirmation") table_index = 2;

            List<IWebElement> items = Start.Browser.FindElements(By.XPath("//div[@class='my_listing_section market_content_block market_home_listing_table'][" + table_index + "]/div[@class='market_listing_row market_recent_listing_row']")).ToList();
            int i = 2;
            foreach (IWebElement item in items)
            {
                string[] str = item.Text.Split("\n");
                IWebElement id = Start.webDriverWait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//div[@class='my_listing_section market_content_block market_home_listing_table'][" + table_index + "]/div[" + i + "]")));
                
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
        public void availableAmount()
        {
            if (this.ItemName.Any())
            {
                Account.OrderSum = 0;
                Account.AvailableAmount = 0;

                foreach (decimal item_price in this.StmPrice)
                    Account.OrderSum += item_price;
                Account.AvailableAmount = Math.Round(Account.Balance * 10 - Account.OrderSum, 2);
            }
        }
        private void checkOrders()
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
        private void checkOrdersProxy()
        {
            //int id = 0;

            //for (int i = 0; i < BuyOrder.item.Count; i++)
            //{
            //    try
            //    {
            //        var market_hash_name = Edit.MarketHashName(BuyOrder.item[i]);
            //        string url = @"http://188.166.72.201:8080/singleitem?i=" + market_hash_name;
            //        var response = Get.Request(url, Main.proxyList[id]);

            //        parseOrder(response, i);
            //    }
            //    catch
            //    {
            //        i--;
            //        if (Main.proxyList.Count > id)
            //            id++;
            //        else
            //            id = 0;
            //    }
            //}
        }
        private void parseOrder(string response, int i)
        {
            decimal my_order = Convert.ToDecimal(this.OrderPrice[i]);
            decimal buy_order = Math.Round(my_order / GeneralProperties.Default.currency, 2);
            decimal stm_sell = Convert.ToDecimal(JObject.Parse(response)["steam"]["sellOrder"].ToString());
            decimal csm_sell = Convert.ToDecimal(JObject.Parse(response)["csm"]["sell"].ToString());
            decimal csm_buy = Convert.ToDecimal(JObject.Parse(response)["csm"]["buy"]["0"].ToString());
            decimal precent = Edit.Precent(buy_order, csm_sell);
            decimal different = Edit.Difference(csm_sell * GeneralProperties.Default.currency, my_order);

            this.StmPrice.Add(stm_sell);
            this.CsmPrice.Add(csm_buy);
            this.CsmBuy.Add(csm_sell);
            this.Precent.Add(precent);
            this.Difference.Add(different);
        }
    }
}
