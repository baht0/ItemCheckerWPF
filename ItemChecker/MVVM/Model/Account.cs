using ItemChecker.Net;
using ItemChecker.Properties;
using ItemChecker.Support;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ItemChecker.MVVM.Model
{
    public class Account : Main
    {
        //login
        public string Login { get; set; }
        public string Password { get; set; }
        public bool Remember { get; set; }
        public string Code2AF { get; set; }

        //informations
        public static string SessionId { get; set; }
        public static string Id64 { get; set; }
        public static string AccountName { get; set; }
        public static string User { get; set; }
        public static string SteamMarket { get; set; } = "Enabled";
        public static decimal Balance { get; set; } = 0.00m;
        public static decimal BalanceUsd { get; set; } = 0.00m;
        public static decimal BalanceCsm { get; set; } = 0.00m;
        public static decimal BalanceCsmUsd { get; set; } = 0.00m;
        public static string ApiKey { get; set; }

        //orders
        public static List<OrderData> MyOrders = new();
        public static decimal AvailableAmount { get; set; } = 0.00m;

        public static void GetInformations()
        {
            decimal course = Get.Course(GeneralProperties.Default.CurrencyApiKey);
            if (course != 0)
            {
                GeneralProperties.Default.CurrencyValue = course;
                GeneralProperties.Default.Save();
            }

            Get get = new();
            Overstock = get.Overstock();
            Unavailable = get.Unavailable();
        }
        public static void GetSteamAccount()
        {
            try
            {
                Browser.Navigate().GoToUrl("https://steamcommunity.com/market/");
                ICookieJar cookie = Browser.Manage().Cookies;
                SessionId = cookie.GetCookieNamed("sessionid").Value.ToString();
                string steamLoginSecure = cookie.GetCookieNamed("steamLoginSecure").Value.ToString();
                Id64 = steamLoginSecure.Substring(0, 17);
                Browser.Manage().Cookies.AddCookie(new Cookie("Steam_Language", "english", "steamcommunity.com", "/", DateTime.Now.AddYears(5), true, false, "None"));

                IWebElement balance = WebDriverWait.Until(e => e.FindElement(By.XPath("//a[@id='header_wallet_balance']")));
                IWebElement user = WebDriverWait.Until(e => e.FindElement(By.XPath("//span[@id='account_pulldown']")));
                IWebElement account = WebDriverWait.Until(e => e.FindElement(By.XPath("//span[@class='persona online']")));

                Balance = Edit.removeRub(balance.Text);
                BalanceUsd = Math.Round(Balance / GeneralProperties.Default.CurrencyValue, 2);
                User = user.Text;
                AccountName = account.GetAttribute("textContent");

                if (Browser.FindElement(By.XPath("//id[@class='market_headertip_container market_headertip_container_warning']")).Displayed)
                    SteamMarket = "Disabled";
            }
            catch (Exception exp)
            {
                errorLog(exp);
            }
        }
        public static void GetCsmBalance()
        {
            try
            {
                Browser.Navigate().GoToUrl("https://cs.money/personal-info/");
                IWebElement balance = WebDriverWait.Until(e => e.FindElement(By.XPath("//span[@class='styles_price__1m7op TradeBalance_balance__2Hxq3']/span")));
                BalanceCsmUsd = Edit.removeDol(balance.GetAttribute("textContent"));
                BalanceCsm = Math.Round(BalanceCsmUsd * GeneralProperties.Default.CurrencyValue, 2);
            }
            catch (Exception exp)
            {
                errorLog(exp);
            }
        }
        public static void GetSteamApiKey()
        {
            Browser.Navigate().GoToUrl("https://steamcommunity.com/dev/apikey");
            IWebElement register = WebDriverWait.Until(e => e.FindElement(By.XPath("//div[@id='bodyContents_ex']/h2")));

            if (register.Text == "Register for a new Steam Web API Key")
            {
                register = WebDriverWait.Until(e => e.FindElement(By.XPath("//input[@id='domain']")));
                register.SendKeys("localhost");
                register = WebDriverWait.Until(e => e.FindElement(By.XPath("//input[@id='agreeToTerms']")));
                register.Click();
                register = WebDriverWait.Until(e => e.FindElement(By.XPath("//input[@class='btn_green_white_innerfade btn_border_2px btn_medium']")));
                register.Click();
                Thread.Sleep(500);
            }

            IWebElement steam_api = WebDriverWait.Until(e => e.FindElement(By.XPath("//div[@id='bodyContents_ex']/p")));
            ApiKey = steam_api.Text.Replace("Key: ", null);
        }
        public static void GetAvailableAmount()
        {
            AvailableAmount = Balance * 10;
            if (MyOrders.Any())
            {
                decimal sum = MyOrders.Sum(s => s.OrderPrice);

                AvailableAmount = Math.Round(AvailableAmount - sum, 2);
            }
        }
    }
}