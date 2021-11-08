using ItemChecker.Properties;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ItemChecker.MVVM.Model
{
    internal class ParserTryskinsService : ParserService
    {
        public void Tryskins(IWebElement item)
        {
            string[] str = item.Text.Split("\n");
            string item_name = str[0].Trim();
            ParserData response = CheckItems(item_name);

            ParserData.ParserItems.Add(response);
        }
        public List<IWebElement> GetItems()
        {
            try
            {
                decimal min_sta = 2;
                if (ParserProperties.Default.minPrice == 0)
                {
                    int j = 15;
                    do
                    {
                        j += 15;
                        min_sta += 5;
                    }
                    while (j < Account.BalanceUsd);
                    min_sta -= 2;
                }
                else min_sta = ParserProperties.Default.minPrice;
                decimal max_sta = Account.BalanceUsd;
                if (ParserProperties.Default.maxPrice != 0)
                    max_sta = ParserProperties.Default.maxPrice;

                string serviceOne = getService(ParserProperties.Default.serviceOne);
                string serviceTwo = getService(ParserProperties.Default.serviceTwo);
                int knife = getType(ParserProperties.Default.knife);
                int stattrak = getType(ParserProperties.Default.stattrak);
                int souvenir = getType(ParserProperties.Default.souvenir);
                int sticker = getType(ParserProperties.Default.sticker);

                List<IWebElement> items = new();
                string url = "https://table.altskins.com/site/items?ItemsFilter%5Bknife%5D=0&ItemsFilter%5Bknife%5D=" + knife + "&ItemsFilter%5Bstattrak%5D=0&ItemsFilter%5Bstattrak%5D=" + stattrak + "&ItemsFilter%5Bsouvenir%5D=0&ItemsFilter%5Bsouvenir%5D=" + souvenir + "&ItemsFilter%5Bsticker%5D=0&ItemsFilter%5Bsticker%5D=" + sticker + "&ItemsFilter%5Btype%5D=1&ItemsFilter%5Bservice1%5D=" + serviceOne + "&ItemsFilter%5Bservice2%5D=" + serviceTwo + "&ItemsFilter%5Bunstable1%5D=1&ItemsFilter%5Bunstable2%5D=1&ItemsFilter%5Bhours1%5D=192&ItemsFilter%5Bhours2%5D=192&ItemsFilter%5BpriceFrom1%5D=" + min_sta + "&ItemsFilter%5BpriceTo1%5D=" + max_sta + "&ItemsFilter%5BpriceFrom2%5D=&ItemsFilter%5BpriceTo2%5D=&ItemsFilter%5BsalesBS%5D=&ItemsFilter%5BsalesTM%5D=&ItemsFilter%5BsalesST%5D=" + ParserProperties.Default.steamSales + "&ItemsFilter%5Bname%5D=&ItemsFilter%5Bservice1Minutes%5D=&ItemsFilter%5Bservice2Minutes%5D=&ItemsFilter%5BpercentFrom1%5D=" + ParserProperties.Default.minPrecent + "&ItemsFilter%5BpercentFrom2%5D=&ItemsFilter%5Btimeout%5D=5&ItemsFilter%5Bservice1CountFrom%5D=1&ItemsFilter%5Bservice1CountTo%5D=&ItemsFilter%5Bservice2CountFrom%5D=1&ItemsFilter%5Bservice2CountTo%5D=&ItemsFilter%5BpercentTo1%5D=" + ParserProperties.Default.maxPrecent + "&ItemsFilter%5BpercentTo2%5D=&page=1&per-page=30";
                BaseModel.Browser.Navigate().GoToUrl(url);
                int last;
                do
                {
                    last = items.Count();
                    IWebElement element = BaseModel.WebDriverWait.Until(e => e.FindElement(By.XPath("//table[@class='table table-bordered']/tbody/tr[last()]")));
                    Thread.Sleep(1000);
                    IJavaScriptExecutor jse = (IJavaScriptExecutor)BaseModel.Browser;
                    jse.ExecuteScript("arguments[0].scrollIntoView(true); window.scrollBy(0, -window.innerHeight / 4);", element);
                    Thread.Sleep(1000);
                    items = BaseModel.Browser.FindElements(By.XPath("//table[@class='table table-bordered']/tbody/tr")).ToList();
                } while (items.Count > last);

                return checkList(items);
            }
            catch (Exception exp)
            {
                errorLog(exp);
                return new List<IWebElement>();
            }            
        }
        private String getService(int serviceInt)
        {
            string service;
            switch (serviceInt)
            {
                case 0:
                    service = "showsteama";
                    break;
                case 1:
                    service = "showcsmoney";
                    break;
                case 2:
                    service = "showlootfarm";
                    break;
                default:
                    goto case 0;
            }
            return service;
        }
        private Int32 getType(bool type)
        {
            int typeStatus;
            switch (type)
            {
                case true:
                    typeStatus = 1;
                    break;
                case false:
                    typeStatus = 0;
                    break;
            }
            return typeStatus;
        }
        private List<IWebElement> checkList(List<IWebElement> items)
        {
            for (int i = 0; i < items.Count; i++)
            {
                string[] str = items[i].Text.Split("\n");
                string item_name = str[0].Trim();

                if (item_name.Contains("Или криво настроили фильтры") | item_name.Contains("Or poorly configured filters"))
                    items.Clear();
                else if (Main.Overstock.Contains(item_name) | Main.Unavailable.Contains(item_name) | Account.MyOrders.Any(i => i.ItemName == item_name))
                    items.RemoveAt(i);
            }
            return items;
        }        
    }
}