using HtmlAgilityPack;
using ItemChecker.Net;
using ItemChecker.Properties;
using ItemChecker.Services;
using ItemChecker.Support;
using System;
using System.Linq;

namespace ItemChecker.MVVM.Model
{
    public class SteamAccount
    {
        static decimal _balance = -1;
        static Currency _currency;
        public static string AccountName { get; set; } = string.Empty;
        public static decimal Balance
        {
            get
            {
                return _balance;
            }
            set
            {
                if (_balance > value && _balance != -1)
                {
                    Main.Notifications.Add(new()
                    {
                        Title = "Steam",
                        Message = $"Your balance has decreased\n-{_balance - value}."
                    });
                }
                else if (_balance < value && _balance != -1)
                {
                    Main.Notifications.Add(new()
                    {
                        Title = "Steam",
                        Message = $"Your balance has increased\n+{value - _balance}."
                    });
                }
                _balance = value;
            }
        }
        public static Currency Currency
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

            Currency ??= SteamBase.CurrencyList.FirstOrDefault(x => x.Id == MainProperties.Default.SteamCurrencyId);
            Currency.Value = !SteamBase.AllowCurrencys.Any(x => x.Id == Currency.Id) ? BaseService.GetCurrency(Currency.Id) : SteamBase.AllowCurrencys.FirstOrDefault(x => x.Id == Currency.Id).Value;

            Balance = Edit.GetPrice(htmlDoc.DocumentNode.SelectSingleNode("//a[@id='header_wallet_balance']").InnerText);
        }
        public static void GetBalance() => Balance = SteamRequest.Get.Balance();
    }
    public class ServiceAccount
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
        public class Csm
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
                    if (_balance > value && _balance != -1)
                    {
                        Main.Notifications.Add(new()
                        {
                            Title = "Cs.Money",
                            Message = $"Your balance has decreased\n-{_balance - value}."
                        });
                    }
                    else if (_balance < value && _balance != -1)
                    {
                        Main.Notifications.Add(new()
                        {
                            Title = "Cs.Money",
                            Message = $"Your balance has increased\n+{value - _balance}."
                        });
                    }
                    _balance = value;
                }
            }

            public static void GetBalance() => Balance = ServicesRequest.CsMoney.Get.Balance();
        }
        public class Lfm
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
                    if (_balance > value && _balance != -1)
                    {
                        Main.Notifications.Add(new()
                        {
                            Title = "Loot.Farm",
                            Message = $"Your balance has decreased\n-{_balance - value}."
                        });
                    }
                    else if (_balance < value && _balance != -1)
                    {
                        Main.Notifications.Add(new()
                        {
                            Title = "Loot.Farm",
                            Message = $"Your balance has increased\n+{value - _balance}."
                        });
                    }
                    _balance = value;
                }
            }

            public static void GetBalance() => Balance = ServicesRequest.LootFarm.Get.Balance();
        }
        public class Buff
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
                    if (_balance > value && _balance != -1)
                    {
                        Main.Notifications.Add(new()
                        {
                            Title = "Buff163",
                            Message = $"Your balance has decreased\n-{_balance - value}."
                        });
                    }
                    else if (_balance < value && _balance != -1)
                    {
                        Main.Notifications.Add(new()
                        {
                            Title = "Buff163",
                            Message = $"Your balance has increased\n+{value - _balance}."
                        });
                    }
                    _balance = value;
                }
            }

            public static void GetBalance()
            {
                var balanceInCny = ServicesRequest.Buff163.Get.Balance();
                Balance = Edit.ConverterToUsd(balanceInCny, SteamBase.AllowCurrencys.FirstOrDefault(x => x.Id == 23).Value);
            }
        }
    }
}
