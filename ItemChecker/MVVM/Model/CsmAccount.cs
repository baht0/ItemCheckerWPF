using ItemChecker.Properties;
using ItemChecker.Services;
using ItemChecker.Support;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Threading;

namespace ItemChecker.MVVM.Model
{
    public class CsmAccount : BaseModel
    {
        public static decimal BalanceCsm { get; set; } = 0.00m;
        public static decimal BalanceCsmUsd { get; set; } = 0.00m;

        public static Boolean LoginCsm()
        {
            try
            {
                BaseModel.Browser.Navigate().GoToUrl("https://cs.money/pending-trades");
                IWebElement html = BaseModel.WebDriverWait.Until(e => e.FindElement(By.XPath("//pre")));
                string json = html.Text;

                if (json.Contains("error"))
                {
                    string code_error = JObject.Parse(json)["error"].ToString();
                    if (code_error == "6")
                    {
                        BaseModel.Browser.Navigate().GoToUrl("https://auth.dota.trade/login?redirectUrl=https://cs.money/&callbackUrl=https://cs.money/login");

                        IWebElement signins = BaseModel.WebDriverWait.Until(e => e.FindElement(By.XPath("//input[@class='btn_green_white_innerfade']")));
                        signins.Click();
                        Thread.Sleep(500);
                    }
                }
                GetCsmBalance();
                return true;
            }
            catch
            {
                return false;
            }            
        }
        static void GetCsmBalance()
        {
            try
            {
                Browser.Navigate().GoToUrl("https://cs.money/personal-info/");
                IWebElement balance = WebDriverWait.Until(e => e.FindElement(By.XPath("//span[@class='styles_price__1m7op TradeBalance_balance__2Hxq3']/span")));
                BalanceCsmUsd = Edit.GetPrice(balance.GetAttribute("textContent"));
                BalanceCsm = Math.Round(BalanceCsmUsd * SettingsProperties.Default.CurrencyValue, 2);
            }
            catch (Exception exp)
            {
                BaseService.errorLog(exp);
            }
        }
    }
}
