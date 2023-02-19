using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;

namespace ItemChecker.Net
{
    public class ServicesRequest : HttpRequest
    {
        public static JObject InspectLinkDetails(string link)
        {
            string json = RequestGetAsync(@"https://api.csgofloat.com/?url=" + link).Result;

            return JObject.Parse(json)["iteminfo"] as JObject;
        }

        static Dictionary<string, string> OpenIdParams(string html)
        {
            HtmlDocument htmlDoc = new();
            htmlDoc.LoadHtml(html);
            var form = htmlDoc.DocumentNode.SelectSingleNode("//form[@id='openidForm']");
            var inputs = new List<string>()
            {
                "action",
                "openid.mode",
                "openidparams",
                "nonce",
            };
            var args = new Dictionary<string, string>();
            foreach (var i in inputs)
                args[i] = form.SelectSingleNode($"//input[@name='{i}']").GetAttributeValue("value", "");

            return args;
        }
        internal static CookieContainer GetCookie(string authUrl, string cookieHost)
        {
            string html = RequestGetAsync(authUrl, SteamRequest.Cookies, new("https://steamcommunity.com/")).Result;
            var args = OpenIdParams(html);
            var response = OpenIdRequestPostAsync(args, SteamRequest.Cookies).Result;

            return Helpers.GetCookieContainer(cookieHost, response);
        }

        public class CsMoney : Helpers
        {
            internal static CookieContainer Cookies { get; set; }
            public class Get
            {
                public static string Request(string url)
                {
                    return RequestGetAsync(url, Cookies).Result;
                }

                public static JObject ItemInfo(JObject item)
                {
                    string json = RequestGetAsync("https://cs.money/skin_info?appId=730&id=" + item["assetId"] + "&isBot=true&botInventory=true").Result;
                    return JObject.Parse(json);
                }
                public static JObject ItemStatus(string itemName)
                {
                    string market_hash_name = Uri.EscapeDataString(itemName);
                    string json = RequestGetAsync("https://cs.money/check_skin_status?appId=730&name=" + market_hash_name).Result;
                    return JObject.Parse(json);
                }
                public static JArray LoadBotsInventory(int offset, int minPrice, int maxPrice)
                {
                    string price = $"maxPrice={maxPrice}&minPrice={minPrice}&";

                    string json = RequestGetAsync($"https://inventories.cs.money/5.0/load_bots_inventory/730?limit=60&offset={offset}&" + price + "&order=desc&priceWithBonus=40&sort=price&withStack=true").Result;
                    return JObject.Parse(json)["items"] as JArray;
                }
                public static JArray LoadBotsInventoryItem(string itemName)
                {
                    string market_hash_name = Uri.EscapeDataString(itemName);
                    string stattrak = market_hash_name.Contains("StatTrak").ToString().ToLower();
                    string souvenir = market_hash_name.Contains("Souvenir").ToString().ToLower();

                    string json = RequestGetAsync($"https://inventories.cs.money/5.0/load_bots_inventory/730?isSouvenir=" + souvenir + "&isStatTrak=" + stattrak + "&limit=60&name=" + market_hash_name + "&offset=0&order=asc&priceWithBonus=30&sort=price&withStack=true").Result;
                    return JObject.Parse(json)["items"] as JArray;
                }

                public static JArray InventoryItems()
                {
                    string json = Request("https://cs.money/3.0/load_user_inventory/730?isPrime=false&limit=60&noCache=true&offset=0&order=desc&sort=price&withStack=true");
                    var obj = JObject.Parse(json);
                    return obj.ContainsKey("items") ? JArray.Parse(obj["items"].ToString()) : new();
                }

                public static decimal Balance()
                {
                    HtmlDocument htmlDoc = new();
                    var html = Request("https://cs.money/personal-info/");
                    htmlDoc.LoadHtml(html);
                    string trade = htmlDoc.DocumentNode.SelectSingleNode("//*[@id='layout-page-header']/div[1]/div/div[6]/div[1]/div[1]/div/div[1]/span[2]").InnerText;
                    html = Request("https://cs.money/market/buy/");
                    htmlDoc.LoadHtml(html);
                    string market = htmlDoc.DocumentNode.SelectSingleNode("//*[@id='layout-page-header']/div[1]/div/div[6]/div[1]/div[1]/div/div[1]/span[2]").InnerText;

                    return GetDecimal(market) + GetDecimal(trade);
                }
                internal static bool IsAuthorized()
                {
                    Cookies = DeserializeCookieAsync("csm").Result;
                    if (Cookies == null || Cookies.Count == 0)
                        return false;

                    var json = Request("https://cs.money/3.0/load_user_inventory/730");
                    return !JObject.Parse(json).ContainsKey("error");
                }
            }
            public class Post
            {
                public static void SignIn()
                {
                    if (!Get.IsAuthorized())
                    {
                        Cookies = GetCookie("https://auth.dota.trade/login?redirectUrl=https://cs.money/ru/&callbackUrl=https://cs.money/login", "https://cs.money");
                        SerializeCookieAsync(Cookies, "csm").Wait();
                    }
                }
            }
        }
        public class LootFarm : Helpers
        {
            internal static CookieContainer Cookies { get; set; }
            public class Get
            {
                public static string Request(string url)
                {
                    return RequestGetAsync(url, Cookies).Result;
                }
                public static JObject InventoryItems()
                {
                    string json = Request("https://loot.farm/getReserves.php");
                    var obj = JObject.Parse(json);
                    return obj["result"] as JObject;
                }

                public static decimal Balance()
                {
                    var json = Request("https://loot.farm/login_data.php");
                    var balance = Convert.ToInt32(JObject.Parse(json)["balance"]);
                    return Convert.ToDecimal(balance) / 100;
                }
                internal static bool IsAuthorized()
                {
                    Cookies = DeserializeCookieAsync("lfm").Result;
                    if (Cookies == null || Cookies.Count == 0)
                        return false;

                    var json = Request("https://loot.farm/historyJSON.php");

                    return !JObject.Parse(json).ContainsKey("error");
                }
            }
            public class Post
            {
                public static void SignIn()
                {
                    if (!Get.IsAuthorized())
                    {
                        Cookies = GetCookie("https://authsb.trade/lootlogin.php", "https://loot.farm");
                        SerializeCookieAsync(Cookies, "lfm").Wait();
                    }
                }
            }
        }
        public class Buff163 : Helpers
        {
            internal static CookieContainer Cookies { get; set; }
            public class Get
            {
                public static string Request(string url)
                {
                    return RequestGetAsync(url, Cookies).Result;
                }
                public static decimal Balance()
                {
                    var json = Request("https://buff.163.com/api/asset/get_brief_asset/");
                    var balanceInCny = Convert.ToDecimal(JObject.Parse(json)["data"]["alipay_amount"]);
                    return balanceInCny;
                }

                internal static bool IsAuthorized()
                {
                    Cookies = DeserializeCookieAsync("bff").Result;
                    if (Cookies == null || Cookies.Count == 0)
                        return false;

                    var json = Request("https://buff.163.com/api/market/goods?game=csgo&page_num=2");
                    var jobject = JObject.Parse(json);

                    return jobject["code"].ToString() != "Login Required";
                }
            }
            public class Post
            {
                public static void SignIn()
                {
                    if (!Get.IsAuthorized())
                    {
                        Cookies = GetCookie("https://buff.163.com/account/login/steam?back_url=/", "https://buff.163.com");
                        var lan = new Cookie("Locale-Supported", "en", "/", "buff.163.com")
                        {
                            Secure = true
                        };
                        Cookies.Add(lan);
                        SerializeCookieAsync(Cookies, "bff").Wait();
                    }
                }
            }
        }
    }
}
