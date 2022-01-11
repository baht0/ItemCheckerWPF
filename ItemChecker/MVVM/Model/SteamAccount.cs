using HtmlAgilityPack;
using ItemChecker.Net;
using ItemChecker.Properties;
using ItemChecker.Services;
using ItemChecker.Support;
using System;
using System.Linq;
using System.Net;

namespace ItemChecker.MVVM.Model
{
    public class SteamAccount : BaseModel
    {
        public static string Id64 { get; set; } = string.Empty;
        public static string AccountName { get; set; } = string.Empty;
        public static string User { get; set; } = string.Empty;
        public static string SteamMarket { get; set; } = "Enabled";
        public static decimal Balance { get; set; } = 0.00m;
        public static decimal BalanceUsd { get; set; } = 0.00m;
        public static string ApiKey { get; set; } = string.Empty;

        public static void GetSteamAccount()
        {
            if (BaseModel.token.IsCancellationRequested)
                return;
            Cookie steamLoginSecure = SettingsProperties.Default.SteamCookies.GetAllCookies().FirstOrDefault(x=>x.Name == "steamLoginSecure");
            Id64 = steamLoginSecure.Value[..17];
            var html = Get.Request(SettingsProperties.Default.SteamCookies, "https://steamcommunity.com/market/");
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);
            Balance = Edit.GetPrice(htmlDoc.DocumentNode.SelectSingleNode("//a[@id='header_wallet_balance']").InnerText);
            BalanceUsd = Math.Round(Balance / SettingsProperties.Default.CurrencyValue, 2);
            User = htmlDoc.DocumentNode.SelectSingleNode("//span[@id='account_pulldown']").InnerText.Trim();
            AccountName = htmlDoc.DocumentNode.SelectSingleNode("//span[@class='persona online']").InnerText.Trim();

            var nodes = htmlDoc.DocumentNode.Descendants().Where(n => n.Attributes.Any(a => a.Value.Contains("market_headertip_container market_headertip_container_warning")));
            GetSteamApiKey();
        }
        public static void GetSteamBalance()
        {
            BaseService.StatusSteam();

            var html = Get.Request(SettingsProperties.Default.SteamCookies, "https://steamcommunity.com/market/");
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);
            Balance = Edit.GetPrice(htmlDoc.DocumentNode.SelectSingleNode("//a[@id='header_wallet_balance']").InnerText);
            BalanceUsd = Math.Round(Balance / SettingsProperties.Default.CurrencyValue, 2);
        }
        static void GetSteamApiKey()
        {
            try
            {
                var html = Get.Request(SettingsProperties.Default.SteamCookies, "https://steamcommunity.com/dev/apikey");
                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(html);
                string steam_api = htmlDoc.DocumentNode.SelectSingleNode("//div[@id='bodyContents_ex']/p").InnerText;

                ApiKey = steam_api.Replace("Key: ", null);
            }
            catch { }
        }

        public static Decimal GetAvailableAmount()
        {
            if (DataOrder.Orders.Any())
                return  Math.Round(Balance * 10 - DataOrder.Orders.Sum(s => s.OrderPrice), 2);
            return Balance * 10;
        }
    }
}