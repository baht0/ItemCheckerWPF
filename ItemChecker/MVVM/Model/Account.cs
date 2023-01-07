using HtmlAgilityPack;
using ItemChecker.Net;
using ItemChecker.Properties;
using ItemChecker.Support;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace ItemChecker.MVVM.Model
{
    public class Account
    {
        internal static void CheckBalance(decimal _balance, decimal value, string title)
        {
            if (_balance > value && _balance != -1)
            {
                Main.Notifications.Add(new()
                {
                    Title = title,
                    Message = $"Your balance has decreased.\n-{_balance - value}$\nThe balance is: {_balance}"
                });
            }
            else if (_balance < value && _balance != -1)
            {
                Main.Notifications.Add(new()
                {
                    Title = title,
                    Message = $"Your balance has increased.\n+{value - _balance}$\nThe balance is: {_balance}"
                });
            }
        }
    }
    public class SteamAccount
    {
        static decimal _balance = -1;
        static DataCurrency _currency;
        public static string AccountName { get; set; } = string.Empty;
        public static decimal Balance
        {
            get
            {
                return _balance;
            }
            set
            {
                Account.CheckBalance(_balance, value, "Steam");
                _balance = value;
            }
        }
        public static DataCurrency Currency
        {
            get
            {
                return _currency;
            }
            set
            {
                _currency = value;
                if (_currency != null)
                {
                    MainProperties.Default.SteamCurrencyId = _currency.Id;
                    MainProperties.Default.Save();
                }
            }
        }

        public static void GetAccount()
        {
            string html = SteamRequest.Get.Request("https://steamcommunity.com/market/");
            HtmlDocument htmlDoc = new();
            htmlDoc.LoadHtml(html);

            var nodes = htmlDoc.DocumentNode.Descendants().Where(n => n.Attributes.Any(a => a.Value.Contains("market_headertip_container market_headertip_container_warning")));
            if (nodes.Any())
            {
                Edit.OpenUrl("https://help.steampowered.com/en/faqs/view/71D3-35C2-AD96-AA3A");
                throw new Exception("Your user accounts are limited.");
            }
            if (String.IsNullOrEmpty(SteamRequest.ApiKey))
            {
                Edit.OpenUrl("https://steamcommunity.com/dev/apikey");
                throw new Exception("Make sure you have register Steam Web API Key.");
            }
            AccountName = htmlDoc.DocumentNode.SelectSingleNode("//span[@class='persona online']").InnerText.Trim();

            Currency ??= Currencies.Steam.FirstOrDefault(x => x.Id == MainProperties.Default.SteamCurrencyId);
            Currency.Value = !Currencies.Allow.Any(x => x.Id == Currency.Id) ? Support.Currency.GetCurrency(Currency.Id) : Currencies.Allow.FirstOrDefault(x => x.Id == Currency.Id).Value;

            Balance = Edit.GetDecimal(htmlDoc.DocumentNode.SelectSingleNode("//a[@id='header_wallet_balance']").InnerText);
        }
        public static void GetBalance() => Balance = SteamRequest.Get.Balance();
    }
    public class ServiceAccount : ServicesRequest
    {
        public static void SignInToServices()
        {
            ServicesRequest.CsMoney.Post.SignIn();
            ServicesRequest.LootFarm.Post.SignIn();
            ServicesRequest.Buff163.Post.SignIn();
        }
        public static void GetBalances()
        {
            Csm.GetBalance();
            Lfm.GetBalance();
            Buff.GetBalance();
        }

        public class Csm : CsMoney
        {
            static decimal _balance = -1;
            public static decimal Balance
            {
                get
                {
                    return _balance;
                }
                set
                {
                    Account.CheckBalance(_balance, value, "Cs.Money");
                    _balance = value;
                }
            }

            internal static void GetBalance() => Balance = Get.Balance();
            internal static decimal GetSumOfItems()
            {
                var array = Get.InventoryItems();

                decimal sum = 0;
                foreach (JObject item in array)
                {
                    if (item.ContainsKey("isVirtual"))
                        sum += Convert.ToDecimal(item["price"]);
                }
                return sum;
            }
        }
        public class Lfm : LootFarm
        {
            static decimal _balance = -1;
            public static decimal Balance
            {
                get
                {
                    return _balance;
                }
                set
                {
                    Account.CheckBalance(_balance, value, "Loot.Farm");
                    _balance = value;
                }
            }

            internal static void GetBalance() => Balance = Get.Balance();
            internal static decimal GetSumOfItems()
            {
                var obj = Get.InventoryItems();

                decimal sum = 0;
                if (obj != null)
                    foreach (var i in obj)
                        sum += Convert.ToDecimal(i.Value["p"]) / 100;
                return sum;
            }
        }
        public class Buff : Buff163
        {
            static decimal _balance = -1;
            public static decimal Balance
            {
                get
                {
                    return _balance;
                }
                set
                {
                    Account.CheckBalance(_balance, value, "Buff163");
                    _balance = value;
                }
            }

            internal static void GetBalance()
            {
                var balanceInCny = Get.Balance();
                Balance = Currency.ConverterToUsd(balanceInCny, 23);
            }
        }
    }
}
