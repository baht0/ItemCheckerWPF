using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using ItemChecker.Support;
using System.Text;

namespace ItemChecker.Net
{
    public class Get
    {
        public static Cookie SteamSessionId()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://steamcommunity.com/market/");
            request.CookieContainer = new();
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();            
            return response.Cookies["sessionid"];
        }

        public static String Request(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            using StreamReader stream = new(response.GetResponseStream(), Encoding.UTF8);
            return stream.ReadToEnd();
        }
        public static String Request(CookieContainer cookie, string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.CookieContainer = cookie;
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            using StreamReader stream = new(response.GetResponseStream(), Encoding.UTF8);
            return stream.ReadToEnd();
        }
        //steam
        public static JObject GameServersStatus(string steam_api_key)
        {
            string json = Request("https://api.steampowered.com/ICSGOServers_730/GetGameServersStatus/v1/?key=" + steam_api_key);
            return JObject.Parse(json);
        }
        public static Tuple<Decimal, Decimal> PriceOverview(string market_hash_name)
        {
            string json = Request("https://steamcommunity.com/market/priceoverview/?country=RU&currency=1&appid=730&market_hash_name=" + market_hash_name);
            JObject response = JObject.Parse(json);
            decimal lowest_price = response.ContainsKey("lowest_price") ? Edit.GetPrice(response["lowest_price"].ToString()) : 0m;
            decimal median_price = response.ContainsKey("median_price") ? Edit.GetPrice(response["median_price"].ToString()) : 0m;

            return Tuple.Create(lowest_price, median_price);
        }
        public static JObject TradeOffers(string steam_api_key)
        {
            string json = Request(@"http://api.steampowered.com/IEconService/GetTradeOffers/v1/?key=" + steam_api_key + "&get_received_offers=1&active_only=100");
            return JObject.Parse(json);
        }
        public static JObject ItemOrdersHistogram(int item_nameid)
        {
            string json = Request("https://steamcommunity.com/market/itemordershistogram?country=RU&language=english&currency=1&item_nameid=" + item_nameid + "&two_factor=0");
            return JObject.Parse(json);
        }
        public static JObject ItemOrdersHistogram(int item_nameid, int currencyId)
        {
            string json = Request("https://steamcommunity.com/market/itemordershistogram?country=RU&language=english&currency=" + currencyId + "&item_nameid=" + item_nameid + "&two_factor=0");
            return JObject.Parse(json);
        }
        public static Decimal CurrencySteam(int currencyId, int item_nameid, decimal price_usd)
        {
            try
            {
                string url = "https://steamcommunity.com/market/itemordershistogram?country=RU&language=english&currency=" + currencyId + "&item_nameid=" + item_nameid + "&two_factor=0";
                decimal price = Convert.ToDecimal(JObject.Parse(Request(url))["highest_buy_order"]);

                return price / price_usd;
            }
            catch
            {
                return 0;
            }
        }

        //dropbox
        public static String DropboxRead(string path)
        {
            HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create("https://content.dropboxapi.com/2/files/download");

            httpRequest.Method = "GET";
            httpRequest.Headers["Dropbox-API-Arg"] = "{\"path\": \"/" + path + "\"}";
            httpRequest.Headers["Authorization"] = "Bearer a94CSH6hwyUAAAAAAAAAAf3zRyhyZknI9J8KM3VZihWEILAuv6Vr3ht_-4RQcJxs";

            HttpWebResponse httpResponse = (HttpWebResponse)httpRequest.GetResponse();
            Stream stream = httpResponse.GetResponseStream();
            StreamReader streamReader = new(stream);
            string s = httpResponse.StatusCode.ToString();

            return streamReader.ReadToEnd();
        }
    }
}
