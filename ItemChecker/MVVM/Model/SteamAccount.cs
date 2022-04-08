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
        public static decimal BalanceStartUp { get; set; } = 0.00m;
        public static string ApiKey { get; set; } = string.Empty;

        public static void GetSteamAccount()
        {
            Cookie steamLoginSecure = SteamCookies.GetAllCookies().FirstOrDefault(x => x.Name == "steamLoginSecure");
            Id64 = steamLoginSecure.Value[..17];
            string html = Get.Request(SteamCookies, "https://steamcommunity.com/market/");
            HtmlDocument htmlDoc = new();
            htmlDoc.LoadHtml(html);
            Balance = Edit.GetPrice(htmlDoc.DocumentNode.SelectSingleNode("//a[@id='header_wallet_balance']").InnerText);
            BalanceUsd = Math.Round(Balance / SettingsProperties.Default.CurrencyValue, 2);
            User = htmlDoc.DocumentNode.SelectSingleNode("//span[@id='account_pulldown']").InnerText.Trim();
            AccountName = htmlDoc.DocumentNode.SelectSingleNode("//span[@class='persona online']").InnerText.Trim();

            var nodes = htmlDoc.DocumentNode.Descendants().Where(n => n.Attributes.Any(a => a.Value.Contains("market_headertip_container market_headertip_container_warning")));
            GetSteamApiKey();
            BaseModel.StatusCommunity = BaseService.StatusSteam();
        }
        public static void GetSteamBalance()
        {
            var html = Get.Request(SteamCookies, "https://steamcommunity.com/market/");
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);
            Balance = Edit.GetPrice(htmlDoc.DocumentNode.SelectSingleNode("//a[@id='header_wallet_balance']").InnerText);
            BalanceUsd = Math.Round(Balance / SettingsProperties.Default.CurrencyValue, 2);
            BalanceStartUp = Balance;
        }
        static void GetSteamApiKey()
        {
            try
            {
                var html = Get.Request(SteamCookies, "https://steamcommunity.com/dev/apikey");
                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(html);
                string steam_api = htmlDoc.DocumentNode.SelectSingleNode("//div[@id='bodyContents_ex']/p").InnerText;

                if (steam_api.Contains("Key: "))
                    ApiKey = steam_api.Replace("Key: ", string.Empty);
                else
                    Main.Notifications.Add(new()
                    {
                        Title = "Steam Account",
                        Message = "Failed to get your API Key!\nSome features will not be available to you."
                    });
            }
            catch
            {
                ApiKey = string.Empty;
            }
        }

        public static Decimal GetAvailableAmount()
        {
            if (DataOrder.Orders.Any())
                return  Math.Round(Balance * 10 - DataOrder.Orders.Sum(s => s.OrderPrice), 2);
            return Balance * 10;
        }
    }
}