using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using ItemChecker.Support;

namespace ItemChecker.Net
{
    public class Get
    {
        public static String Request(string url)
        {
            WebRequest request = WebRequest.Create(url);
            WebResponse response = request.GetResponse();
            Stream stream = response.GetResponseStream();
            StreamReader sr = new(stream);
            var json = sr.ReadToEnd();

            return json;
        }
        public static String Request(string url, string address)
        {
            WebRequest request = WebRequest.Create(url);
            request.Proxy = new WebProxy(address.Trim(), true);
            request.Timeout = 5000;
            WebResponse response = request.GetResponse();
            Stream stream = response.GetResponseStream();
            StreamReader sr = new StreamReader(stream);
            var json = sr.ReadToEnd();

            return json;
        }
        //csm
        public static Tuple<String, Boolean> MrinkaRequest(string str)
        {
            try
            {
                string url = @"http://188.166.72.201:8080/singleitem?i=" + str;
                return Tuple.Create(Request(url), true);
            }
            catch
            {
                return Tuple.Create(string.Empty, false);
            }
        }
        public static String InventoriesCsMoney(string market_hash_name)
        {
            string stattrak = "false";
            string souvenir = "false";
            string rarity = null;
            if (market_hash_name.Contains("StatTrak"))
                stattrak = "true";
            if (market_hash_name.Contains("Souvenir"))
                souvenir = "true";
            if (market_hash_name.Contains("Sticker") & !market_hash_name.Contains("Holo") & !market_hash_name.Contains("Foil") & !market_hash_name.Contains("Gold"))
                rarity = "&rarity=High%20Grade";
            string url = @"https://inventories.cs.money/5.0/load_bots_inventory/730?hasRareFloat=false&hasRarePattern=false&hasRareStickers=false&hasTradeLock=false&hasTradeLock=true&isMarket=false&isSouvenir=" + souvenir + "&isStatTrak=" + stattrak + "&limit=60&name=" + market_hash_name + "&offset=0" + rarity + "&tradeLockDays=1&tradeLockDays=2&tradeLockDays=3&tradeLockDays=4&tradeLockDays=5&tradeLockDays=6&tradeLockDays=7&tradeLockDays=0";

            return Request(url);
        }
        //steam
        public static String GameServersStatus(string steam_api_key)
        {
            return Request("https://api.steampowered.com/ICSGOServers_730/GetGameServersStatus/v1/?key=" + steam_api_key);
        }
        public static Tuple<Decimal, Decimal> PriceOverview(string market_hash_name)
        {
            string json = Request("https://steamcommunity.com/market/priceoverview/?country=RU&currency=5&appid=730&market_hash_name=" + market_hash_name);
            JObject response = JObject.Parse(json);
            decimal lowest_price = 0m;
            decimal median_price = 0m;

            if (response.ContainsKey("lowest_price"))
                lowest_price = Edit.removeRub(JObject.Parse(json)["lowest_price"].ToString());
            if (response.ContainsKey("median_price"))
                median_price = Edit.removeRub(JObject.Parse(json)["median_price"].ToString());

            return Tuple.Create(lowest_price, median_price);
        }
        public static String TradeOffers(string steam_api_key)
        {
            return Request(@"http://api.steampowered.com/IEconService/GetTradeOffers/v1/?key=" + steam_api_key + "&get_received_offers=1&active_only=100");
        }
        public static Decimal ItemOrdersHistogram(int item_nameid)
        {
            var json = Request("https://steamcommunity.com/market/itemordershistogram?country=RU&language=english&currency=5&item_nameid=" + item_nameid + "&two_factor=0");

            var highest_buy_order = Convert.ToDecimal(JObject.Parse(json)["highest_buy_order"].ToString());
            return highest_buy_order / 100;
        }

        public static Decimal Course(string currency_api_key)
        {
            try
            {
                if (currency_api_key.Length == 20)
                {
                    string url = @"https://free.currconv.com/api/v7/convert?q=USD_RUB&compact=ultra&apiKey=" + currency_api_key;
                    var json = Get.Request(url);

                    return Math.Round(Convert.ToDecimal(JObject.Parse(json)["USD_RUB"].ToString()), 2);
                }
                else
                {
                    string url = @"https://openexchangerates.org/api/latest.json?app_id=" + currency_api_key;
                    var json = Get.Request(url);

                    return Math.Round(Convert.ToDecimal(JObject.Parse(json)["rates"]["RUB"].ToString()), 2);
                }
            }
            catch
            {
                return 0;
            }
        }

        public List<string> Overstock()
        {
            WebClient client = new();
            var str = client.DownloadString("https://cs.money/list_overstock?appId=730");
            var json = JsonConvert.DeserializeObject<RootObject[]>(str);
            List<string> list = new();

            foreach (var rootObject in json)
                list.Add(Edit.replaceSymbols(rootObject.market_hash_name));
            return list;
        }
        public List<string> Unavailable()
        {
            WebClient client = new();
            var str = client.DownloadString("https://cs.money/list_unavailable?appId=730");
            var json = JsonConvert.DeserializeObject<RootObject[]>(str);
            List<string> list = new();

            foreach (var rootObject in json)
                list.Add(Edit.replaceSymbols(rootObject.market_hash_name));
            return list;
        }
        private class RootObject
        {
            public string market_hash_name { get; set; }
        }
    }
}
