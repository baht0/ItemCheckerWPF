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
        //start
        public string Login { get; set; }
        public string Password { get; set; }
        public bool Remember { get; set; }
        public string Code2AF { get; set; }

        //informations
        public static string AccountName { get; set; }
        public static decimal Balance { get; set; } = 0.00m;
        public static decimal BalanceUsd { get; set; } = 0.00m;
        public static decimal BalanceCsm { get; set; } = 0.00m;
        public static decimal BalanceCsmUsd { get; set; } = 0.00m;

        //orders
        public static List<OrderData> MyOrders = new();
        public static int OrdersCount { get; set; } = 0;
        public static decimal AvailableAmount { get; set; } = 0.00m;

        public static void GetInformations()
        {
            decimal course = Get.Course(GeneralProperties.Default.CurrencyApiKey);
            if (course != 0)
            {
                GeneralProperties.Default.CurrencyValue = course;
                GeneralProperties.Default.Save();
            }
            GetSteamBalance();
            if (!GeneralProperties.Default.Guard)
                GetCsmBalance();

            Get get = new();
            Overstock = get.Overstock();
            Unavailable = get.Unavailable();
        }
        static void GetSteamBalance()
        {
            try
            {
                Browser.Navigate().GoToUrl("https://steamcommunity.com/market");
                IWebElement count = WebDriverWait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//span[@id='my_market_buylistings_number']")));
                IWebElement balance = WebDriverWait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//a[@id='header_wallet_balance']")));

                Balance = Edit.removeRub(balance.Text);
                OrdersCount = int.Parse(count.Text);
                BalanceUsd = Math.Round(Balance / GeneralProperties.Default.CurrencyValue, 2);
            }
            catch (Exception exp)
            {
                errorLog(exp);
            }
        }
        static void GetCsmBalance()
        {
            try
            {
                Browser.Navigate().GoToUrl("https://cs.money/personal-info/");
                IWebElement balance = WebDriverWait.Until(ExpectedConditions.ElementExists(By.XPath("//span[@class='styles_price__1m7op styles_balance__1OBZG']/span")));
                BalanceCsmUsd = Edit.removeDol(balance.GetAttribute("textContent"));
                BalanceCsm = Math.Round(BalanceCsmUsd * GeneralProperties.Default.CurrencyValue, 2);
            }
            catch (Exception exp)
            {
                errorLog(exp);
            }
        }
        public static String GetSteamApiKey()
        {
            Browser.Navigate().GoToUrl("https://steamcommunity.com/dev/apikey");
            IWebElement register = WebDriverWait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//div[@id='bodyContents_ex']/h2")));

            if (register.Text == "Register for a new Steam Web API Key")
            {
                register = WebDriverWait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//input[@id='domain']")));
                register.SendKeys("localhost");
                register = WebDriverWait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//input[@id='agreeToTerms']")));
                register.Click();
                register = WebDriverWait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//input[@class='btn_green_white_innerfade btn_border_2px btn_medium']")));
                register.Click();
                Thread.Sleep(500);
            }

            IWebElement steam_api = WebDriverWait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//div[@id='bodyContents_ex']/p")));
            return steam_api.Text.Replace("Key: ", null);
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