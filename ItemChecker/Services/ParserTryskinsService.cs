using ItemChecker.Properties;
using ItemChecker.Support;
using OpenQA.Selenium;
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
            DataParser response = CheckList(item_name);

            DataParser.ParserItems.Add(response);
        }

        void LoginTryskins()
        {
            if (BaseModel.token.IsCancellationRequested)
                return;

            BaseModel.Browser.Navigate().GoToUrl("https://table.altskins.com/site/items");
            if (BaseModel.Browser.Url.Contains("items"))
                return;

            BaseModel.Browser.Navigate().GoToUrl("https://table.altskins.com/login/steam");

            IWebElement account = BaseModel.WebDriverWait.Until(e => e.FindElement(By.XPath("//div[@class='OpenID_loggedInAccount']")));
            IWebElement signins = BaseModel.WebDriverWait.Until(e => e.FindElement(By.XPath("//input[@class='btn_green_white_innerfade']")));
            signins.Click();
            Thread.Sleep(300);

            LoginTryskins();
        }
        public List<IWebElement> GetItems()
        {
            try
            {
                LoginTryskins();
                decimal minPrice = ParserProperties.Default.MinPrice;
                int bal = 15;
                while (bal < Account.BalanceUsd & ParserProperties.Default.MinPrice == 0)
                {
                    bal += 15;
                    minPrice += 5;
                }

                decimal maxPrice = Account.BalanceUsd;
                if (ParserProperties.Default.MaxPrice != 0)
                    maxPrice = ParserProperties.Default.MaxPrice;
                string sales = string.Empty;
                if (ParserProperties.Default.SteamSales != 0)
                    sales = ParserProperties.Default.SteamSales.ToString();
                string minPrecent = string.Empty;
                if (ParserProperties.Default.MinPrecent != 0)
                    minPrecent = ParserProperties.Default.MinPrecent.ToString();
                string maxPrecent = string.Empty;
                if (ParserProperties.Default.MaxPrecent != 0)
                    maxPrecent = ParserProperties.Default.MaxPrecent.ToString();

                int knife = getType(ParserProperties.Default.KnifeTS);
                int stattrak = getType(ParserProperties.Default.StattrakTS);
                int souvenir = getType(ParserProperties.Default.SouvenirTS);
                int sticker = getType(ParserProperties.Default.StickerTS);
                string serviceOne = getService(ParserProperties.Default.ServiceOne);
                string serviceTwo = getService(ParserProperties.Default.ServiceTwo);
                string marketHashName = Edit.MarketHashName(ParserProperties.Default.NameContains);

                List<IWebElement> items = new();
                string url = "https://table.altskins.com/site/items?ItemsFilter%5Bknife%5D=0&ItemsFilter%5Bknife%5D=" + knife + "&ItemsFilter%5Bstattrak%5D=0&ItemsFilter%5Bstattrak%5D=" + stattrak + "&ItemsFilter%5Bsouvenir%5D=0&ItemsFilter%5Bsouvenir%5D=" + souvenir + "&ItemsFilter%5Bsticker%5D=0&ItemsFilter%5Bsticker%5D=" + sticker + "&ItemsFilter%5Btype%5D=1&ItemsFilter%5Bservice1%5D=" + serviceOne + "&ItemsFilter%5Bservice2%5D=" + serviceTwo + "&ItemsFilter%5Bunstable1%5D=1&ItemsFilter%5Bunstable2%5D=1&ItemsFilter%5Bhours1%5D=192&ItemsFilter%5Bhours2%5D=192&ItemsFilter%5BpriceFrom1%5D=" + minPrice + "&ItemsFilter%5BpriceTo1%5D=" + maxPrice + "&ItemsFilter%5BpriceFrom2%5D=&ItemsFilter%5BpriceTo2%5D=&ItemsFilter%5BsalesBS%5D=&ItemsFilter%5BsalesTM%5D=&ItemsFilter%5BsalesST%5D=" + sales + "&ItemsFilter%5Bname%5D=" + marketHashName + "&ItemsFilter%5Bservice1Minutes%5D=&ItemsFilter%5Bservice2Minutes%5D=&ItemsFilter%5BpercentFrom1%5D=" + minPrecent + "&ItemsFilter%5BpercentFrom2%5D=&ItemsFilter%5Btimeout%5D=5&ItemsFilter%5Bservice1CountFrom%5D=1&ItemsFilter%5Bservice1CountTo%5D=&ItemsFilter%5Bservice2CountFrom%5D=1&ItemsFilter%5Bservice2CountTo%5D=&ItemsFilter%5BpercentTo1%5D=" + maxPrecent + "&ItemsFilter%5BpercentTo2%5D=&page=1&per-page=30";
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

                    if (BaseModel.token.IsCancellationRequested)
                        break;

                } while (items.Count > last);

                return checkList(items);
            }
            catch (Exception exp)
            {
                BaseModel.errorLog(exp);
                return new List<IWebElement>();
            }
        }
        String getService(int serviceInt)
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
        Int32 getType(bool type)
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
        List<IWebElement> checkList(List<IWebElement> items)
        {
            for (int i = 0; i < items.Count; i++)
            {
                string[] str = items[i].Text.Split("\n");
                string item_name = str[0].Trim();

                if (item_name.Contains("Или криво настроили фильтры") | item_name.Contains("Or poorly configured filters"))
                    items.Clear();
                else if (ItemBase.Overstock.Any(x => x.ItemName == item_name) | ItemBase.Unavailable.Any(x => x.ItemName == item_name) | DataOrder.Orders.Any(i => i.ItemName == item_name))
                    items.RemoveAt(i);
            }
            return items;
        }        
    }
}