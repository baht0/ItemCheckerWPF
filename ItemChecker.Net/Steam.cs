using HtmlAgilityPack;
using ItemChecker.Net.Session;
using ItemChecker.Support;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;

namespace ItemChecker.Net
{
    public class SteamRequest : HttpRequest
    {
        public static string ApiKey
        {
            get
            {
                if (String.IsNullOrEmpty(_apiKey))
                {
                    string html = Get.Request("https://steamcommunity.com/dev/apikey");
                    var htmlDoc = new HtmlDocument();
                    htmlDoc.LoadHtml(html);
                    string apiKey = htmlDoc.DocumentNode.SelectSingleNode("//div[@id='bodyContents_ex']/p").InnerText;

                    if (apiKey.Contains("Key: "))
                        _apiKey = apiKey.Replace("Key: ", string.Empty);
                }
                return _apiKey;
            }
        }
        static string _apiKey = string.Empty;
        public static string ID64
        {
            get
            {
                if (String.IsNullOrEmpty(_id64))
                {
                    CookieCollection cookieCollection = Cookies.GetAllCookies();
                    var cookie = cookieCollection.FirstOrDefault(x => x.Name == "steamLoginSecure");
                    _id64 = cookie.Value[..17];
                }
                return _id64;
            }
        }
        static string _id64 = string.Empty;

        internal static CookieContainer Cookies { get; set; }
        internal static Cookie SessionId
        {
            get
            {
                if (Cookies != null)
                {
                    CookieCollection cookieCollection = Cookies.GetAllCookies();
                    _sessionId = cookieCollection.FirstOrDefault(x => x.Name == "sessionid");
                }
                return _sessionId == null ? Helpers.CreateSessionId() : _sessionId;
            }
        }
        static Cookie _sessionId;

        public class Get : Helpers
        {
            internal static DateTime ifModifiedSince { get; set; } = DateTime.Now.ToUniversalTime();

            public static string Request(string url)
            {
                return RequestGetAsync(url, Cookies).Result;
            }
            public static decimal Balance()
            {
                var html = Request("https://steamcommunity.com/market/");
                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(html);
                return Edit.GetPrice(htmlDoc.DocumentNode.SelectSingleNode("//a[@id='header_wallet_balance']").InnerText);
            }
            public static JObject GameServersStatus()
            {
                string json = RequestGetAsync("https://api.steampowered.com/ICSGOServers_730/GetGameServersStatus/v1/?key=" + ApiKey).Result;
                return JObject.Parse(json);
            }
            public static JObject TradeOffers()
            {
                string json = RequestGetAsync(@"http://api.steampowered.com/IEconService/GetTradeOffers/v1/?key=" + ApiKey + "&get_received_offers=1&active_only=100").Result;
                return JObject.Parse(json);
            }

            static string MarketRequest(string url, string market_hash_name)
            {
                var headers = new HttpClient().DefaultRequestHeaders;
                headers.Add("Accept", "*/*");
                headers.Add("User-Agent", UserAgent);
                headers.Add("Referer", "https://steamcommunity.com/market/listings/730/" + market_hash_name);
                headers.Add("Origin", "https://steamcommunity.com/");
                headers.IfModifiedSince = ifModifiedSince.AddSeconds(-5);
                headers.Add("sec-ch-ua", "Google Chrome\";v=\"107\", \"Chromium\";v=\"107\", \"Not=A?Brand\";v=\"24");
                headers.Add("sec-ch-ua-mobile", "ooooo");
                headers.Add("sec-ch-ua-platform", "Windows");
                headers.Add("Sec-Fetch-Dest", "empty");
                headers.Add("Sec-Fetch-Mode", "cors");
                headers.Add("Sec-Fetch-Site", "same-origin");

                return RequestGetAsync(url, headers).Result;
            }
            public static JObject ItemListings(string itemName)
            {
                string market_hash_name = Uri.EscapeDataString(itemName);
                string url = "https://steamcommunity.com/market/listings/730/" + market_hash_name + "/render?start=0&count=100&currency=1&language=english&format=json";
                var json = MarketRequest(url, market_hash_name);
                return JObject.Parse(json);
            }
            public static JObject PriceOverview(string itemName, int currencyId)
            {
                string market_hash_name = Uri.EscapeDataString(itemName);
                string url = "https://steamcommunity.com/market/priceoverview/?country=RU&currency=" + currencyId + "&appid=730&market_hash_name=" + market_hash_name;
                var json = MarketRequest(url, market_hash_name);
                return JObject.Parse(json);
            }
            public static JObject ItemOrdersHistogram(string itemName, int item_nameid, int currencyId)
            {
                string market_hash_name = Uri.EscapeDataString(itemName);
                string url = "https://steamcommunity.com/market/itemordershistogram?country=RU&language=english&currency=" + currencyId + "&item_nameid=" + item_nameid + "&two_factor=0";
                var json = MarketRequest(url, market_hash_name);
                return JObject.Parse(json);
            }
        }
        public class Post : Helpers
        {
            static HttpResponseMessage Request(string url, string referer, Dictionary<string, string> args)
            {
                Uri uriAddress = new(url);
                uriAddress = new($"https://{uriAddress.Host}");

                var headers = new HttpClient().DefaultRequestHeaders;
                headers.Add("Accept", "*/*");
                headers.Add("Referer", referer);
                headers.Add("User-Agent", UserAgent);
                headers.Add("Cookie", Cookies.GetCookieHeader(uriAddress));

                return RequestPostAsync(url, args, headers).Result;
            }

            public static HttpResponseMessage CreateBuyOrder(string itemName, decimal highest_buy_order, int currencyId)
            {
                string market_hash_name = Uri.EscapeDataString(itemName);
                Dictionary<string, string> args = new()
                {
                    {"sessionid", SessionId.Value},
                    {"currency", currencyId.ToString()},
                    {"market_hash_name", market_hash_name},
                    {"price_total", ((int)(highest_buy_order * 100 + 1)).ToString()},
                    {"quantity", "1"},
                    {"billing_state", string.Empty},
                    {"save_my_address", "0"},
                };
                string url = "https://steamcommunity.com/market/createbuyorder/";
                string referer = "https://steamcommunity.com/market/listings/730/" + market_hash_name;

                return Request(url, referer, args);
            }
            public static HttpResponseMessage CancelBuyOrder(string itemName, string buyOrderId)
            {
                string market_hash_name = Uri.EscapeDataString(itemName);
                Dictionary<string, string> args = new()
                {
                    {"sessionid", SessionId.Value},
                    {"buy_orderid", buyOrderId},
                };

                string url = "https://steamcommunity.com/market/cancelbuyorder/";
                string referer = "https://steamcommunity.com/market/listings/730/" + market_hash_name;

                return Request(url, referer, args);
            }
            public static HttpResponseMessage BuyListing(string itemName, string listingId, decimal fee, decimal subtotal, decimal total, int currencyId)
            {
                string market_hash_name = Uri.EscapeDataString(itemName);
                Dictionary<string, string> args = new()
                {
                    {"sessionid", SessionId.Value},
                    {"currency", currencyId.ToString()},
                    {"fee", ((int)fee).ToString()},
                    {"subtotal", ((int)subtotal).ToString()},
                    {"total", ((int)total).ToString()},
                    {"quantity", "1"},
                    {"first_name", string.Empty},
                    {"last_name", string.Empty},
                    {"billing_address", string.Empty},
                    {"billing_address_two", string.Empty},
                    {"billing_country", string.Empty},
                    {"billing_city", string.Empty},
                    {"billing_state", string.Empty},
                    {"billing_postal_code", string.Empty},
                    {"save_my_address", "1"},
                };

                string url = "https://steamcommunity.com/market/buylisting/" + listingId;
                string referer = "https://steamcommunity.com/market/listings/730/" + market_hash_name;

                return Request(url, referer, args);
            }

            public static HttpResponseMessage SellItem(string assetId, int price)
            {
                Dictionary<string, string> args = new()
                {
                    {"sessionid", SessionId.Value},
                    {"appid", "730"},
                    {"contextid", "2"},
                    {"assetid", assetId},
                    {"amount", "1"},
                    {"price", price.ToString()},
                };
                string url = "https://steamcommunity.com/market/sellitem/";
                string referer = "https://steamcommunity.com/my/inventory/";

                return Request(url, referer, args);
            }
            public static HttpResponseMessage AcceptTrade(string tradeOfferId, string partnerId)
            {
                Dictionary<string, string> args = new()
                {
                    {"sessionid", SessionId.Value},
                    {"serverid", "1"},
                    {"tradeofferid", tradeOfferId},
                    {"partner", partnerId},
                    {"captcha", string.Empty},
                };

                string url = "https://steamcommunity.com/tradeoffer/" + tradeOfferId + "/accept";
                string referer = "https://steamcommunity.com/my/inventory/";

                return Request(url, referer, args);
            }
        }
        public class Session : Helpers
        {
            static StartResponse _startResponse { get; set; }

            public static bool IsAuthorized()
            {
                Cookies = DeserializeCookieAsync("stm").Result;
                if (Cookies == null)
                    return false;

                var html = Get.Request("https://steamcommunity.com/login/home/?goto=my/profile");
                HtmlDocument htmlDoc = new();
                htmlDoc.LoadHtml(html);
                string title = htmlDoc.DocumentNode.SelectSingleNode("html/head/title").InnerText;

                return !title.Contains("Sign In");
            }
            public static bool SubmitSignIn(string accountName, string password)
            {
                var rsaPass = RsaPassword.GetEncryptedPassword(accountName, password);
                _startResponse = BeginAuthSessionViaCredentials(accountName, rsaPass);

                return _startResponse != null ? true : false;
            }
            public static void SubmitCode(string code)
            {
                UpdateAuthSessionWithSteamGuardCode(_startResponse, code);
            }
            public static bool CheckAuthStatus()
            {
                PollResponse pollResponse = PollAuthSessionStatus(_startResponse);
                if (pollResponse == null)
                    return false;

                var finalizeResponse = FinalizeLogin(pollResponse);
                var httpResponse = SetToken(finalizeResponse);

                Cookies = GetCookieContainer("https://steamcommunity.com/", httpResponse);
                Cookies.Add(SessionId);
                SerializeCookieAsync(Cookies, "stm").Wait();

                return true;
            }

            static StartResponse BeginAuthSessionViaCredentials(string accountName, RsaPassword rsaPassword)
            {
                string url = "https://api.steampowered.com/IAuthenticationService/BeginAuthSessionViaCredentials/v1/";

                Dictionary<string, string> args = new()
                {
                    {"access_token", string.Empty},
                    {"device_friendly_name", Uri.EscapeDataString(UserAgent)},
                    {"account_name", accountName},
                    {"encrypted_password", Uri.EscapeDataString(rsaPassword.EncryptedPassword)},
                    {"encryption_timestamp", rsaPassword.TimeStamp},
                    {"remember_login", "true"},
                    {"platform_type", "2"},
                    {"persistence", "1"},
                    {"website_id", "Community"},
                };
                var response = SessionRequestPostAsync(url, args).Result;
                string json = ToResponseStringAsync(response).Result;

                var jobject = JObject.Parse(json)["response"] as JObject;
                if (jobject.ContainsKey("steamid"))
                {
                    return new StartResponse()
                    {
                        ClientId = jobject["client_id"].ToString(),
                        RequestId = jobject["request_id"].ToString(),
                        Interval = jobject["interval"].ToString(),
                        AllowedConfirmations = StartResponse.GetAllowedConfirmations(JArray.Parse(jobject["allowed_confirmations"].ToString())),
                        SteamId = jobject["steamid"].ToString(),
                        WeakToken = jobject["weak_token"].ToString(),
                        ExtendedErrorMessage = jobject["extended_error_message"].ToString()
                    };
                }
                return null;
            }
            static void UpdateAuthSessionWithSteamGuardCode(StartResponse data, string code)
            {
                string url = "https://api.steampowered.com/IAuthenticationService/UpdateAuthSessionWithSteamGuardCode/v1/";
                Dictionary<string, string> args = new()
                {
                    {"access_token", string.Empty},
                    {"client_id", data.ClientId},
                    {"steamid", data.SteamId},
                    {"code", code},
                    {"code_type", "3"},
                };
                var response = SessionRequestPostAsync(url, args).Result;
                string json = ToResponseStringAsync(response).Result;
            }
            static PollResponse PollAuthSessionStatus(StartResponse data)
            {
                string url = "https://api.steampowered.com/IAuthenticationService/PollAuthSessionStatus/v1/";
                Dictionary<string, string> args = new()
                {
                    {"access_token", string.Empty},
                    {"client_id", data.ClientId},
                    {"request_id", Uri.EscapeDataString(data.RequestId)},
                };
                var response = SessionRequestPostAsync(url, args).Result;
                string json = ToResponseStringAsync(response).Result;

                var jobject = JObject.Parse(json)["response"] as JObject;
                if (jobject.ContainsKey("refresh_token"))
                {
                    return new PollResponse()
                    {
                        RefreshToken = jobject["refresh_token"].ToString(),
                        AccessToken = jobject["access_token"].ToString(),
                        HadRemoteInteraction = Convert.ToBoolean(jobject["had_remote_interaction"]),
                        AccountName = jobject["account_name"].ToString()
                    };
                }
                return null;
            }
            static FinalizeResponse FinalizeLogin(PollResponse data)
            {
                string url = "https://login.steampowered.com/jwt/finalizelogin/";

                Dictionary<string, string> args = new()
                {
                    {"sessionid", SessionId.Value},
                    {"redir", "https://steamcommunity.com/"},
                    {"nonce", Uri.EscapeDataString(data.RefreshToken)},
                };
                var response = SessionRequestPostAsync(url, args).Result;
                string json = ToResponseStringAsync(response).Result;

                var jobject = JObject.Parse(json);
                if (jobject.ContainsKey("steamID"))
                {
                    return new FinalizeResponse()
                    {
                        SteamID = jobject["steamID"].ToString(),
                        Redir = jobject["redir"].ToString(),
                        TransferInfo = JArray.Parse(jobject["transfer_info"].ToString()).Select(x => new Transfer
                        {
                            Url = x["url"].ToString(),
                            Params = JObject.Parse(x["params"].ToString()).ToObject<Param>(),
                        }).ToList(),
                        PrimaryDomain = jobject["primary_domain"].ToString()
                    };
                }
                return new();
            }
            static HttpResponseMessage SetToken(FinalizeResponse data)
            {
                Dictionary<string, string> args = new()
                {
                    {"nonce", data.TransferInfo[1].Params.Nonce},
                    {"auth", data.TransferInfo[1].Params.Auth},
                    {"steamID", data.SteamID},
                };

                return SessionRequestPostAsync(data.TransferInfo[1].Url, args).Result;
            }
        }
    }
}
