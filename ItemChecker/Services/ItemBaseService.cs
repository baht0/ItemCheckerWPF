using ItemChecker.MVVM.Model;
using ItemChecker.Net;
using ItemChecker.Support;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ItemChecker.Services
{
    public class ItemBaseService : BaseService
    {
        List<Tuple<string, decimal>> GetSteamPrice()
        {
            try
            {

                JObject csgobackpack = (JObject)JObject.Parse(Get.Request("https://csgobackpack.net/api/GetItemsList/v2/?no_details=true"))["items_list"];

                List<Tuple<string, decimal>> prices = new();
                foreach (var item in csgobackpack)
                {
                    string name = item.Key.Replace("&#39", "'");
                    decimal stPrice = 0;
                    if (item.Value["price"] != null)
                    {
                        if (item.Value["price"]["24_hours"] != null)
                            stPrice = Convert.ToDecimal(item.Value["price"]["24_hours"]["average"]);
                        else if (item.Value["price"]["7_days"] != null)
                            stPrice = Convert.ToDecimal(item.Value["price"]["7_days"]["average"]);
                        else if (item.Value["price"]["30_days"] != null)
                            stPrice = Convert.ToDecimal(item.Value["price"]["30_days"]["average"]);
                        else if (item.Value["price"]["all_time"] != null)
                            stPrice = Convert.ToDecimal(item.Value["price"]["all_time"]["average"]);
                    }
                    prices.Add(Tuple.Create(name, stPrice));
                }
                return prices;
            }
            catch
            {
                return new();
            }
        }

        public void CreateItemsBase()
        {
            JObject json = JObject.Parse(Get.DropboxRead("steamBase.json"));
            JArray skinsBase = JArray.Parse(json["Items"].ToString());

            List<Tuple<string, decimal>> stPrices = GetSteamPrice();
            foreach (JObject item in skinsBase)
            {
                string itemName = item["itemName"].ToString();
                int steamId = Convert.ToInt32(item["steamId"]);

                decimal stPrice = stPrices.FirstOrDefault(x => x.Item1 == itemName) != null ? stPrices.FirstOrDefault(x => x.Item1 == itemName).Item2 : 0;
                SteamBase.ItemList.Add(new()
                {
                    ItemName = itemName,
                    Type = item["type"].ToString(),
                    Quality = item["quality"].ToString(),

                    Steam = new()
                    {
                        Id = steamId,
                        AvgPrice = stPrice,
                    }
                });
            }
        }

        public void UpdateSteamInfoItem(string itemName)
        {
            if (SteamBase.ItemList.Select(x => x.Steam.Updated).Max().AddMinutes(30) > DateTime.Now)
                return;

            int id = SteamBase.ItemList.FirstOrDefault(x => x.ItemName == itemName).Steam.Id;
            JObject json = Get.ItemOrdersHistogram(id);

            decimal high = !String.IsNullOrEmpty(json["highest_buy_order"].ToString()) ? Convert.ToDecimal(json["highest_buy_order"]) / 100 : 0;
            decimal low = !String.IsNullOrEmpty(json["lowest_sell_order"].ToString()) ? Convert.ToDecimal(json["lowest_sell_order"]) / 100 : 0;

            var item = SteamBase.ItemList.FirstOrDefault(x => x.ItemName == itemName).Steam;
            item.HighestBuyOrder = high;
            item.LowestSellOrder = low;
        }
        public void UpdateLfmInfo()
        {
            if (SteamBase.ItemList.Select(x => x.Lfm.Updated).Max().AddMinutes(30) > DateTime.Now)
                return;
            JArray array = JArray.Parse(Get.Request("https://loot.farm/fullprice.json"));
            foreach (JToken item in array)
            {
                string itemName = item["name"].ToString().Replace("(Holo-Foil)", "(Holo/Foil)").Replace("  ", " ");
                decimal price = Convert.ToDecimal(item["price"]) / 100;
                int have = Convert.ToInt32(item["have"]);
                int max = Convert.ToInt32(item["max"]);

                if (SteamBase.ItemList.FirstOrDefault(x => x.ItemName == itemName) != null)
                {
                    SteamBase.ItemList.FirstOrDefault(x => x.ItemName == itemName).Lfm = new()
                    {
                        Updated = DateTime.Now,
                        Price = price,
                        Have = have,
                        Limit = max,
                        Reservable = Convert.ToInt32(item["res"]),
                        Tradable = Convert.ToInt32(item["tr"]),
                        SteamPriceRate = Convert.ToInt32(item["rate"]),
                        Overstock = have >= max,
                        Unavailable = price <= 0,
                        IsHave = price > 0,
                    };
                }
            }
        }
        public void UpdateCsmInfo()
        {
            if (SteamBase.ItemList.Select(x => x.Csm.Updated).Max().AddMinutes(30) > DateTime.Now)
                return;
            JObject json = JObject.Parse(Get.Request("https://csm.auction/api/skins_base"));
            JArray unavailable = JArray.Parse(Get.Request("https://cs.money/list_unavailable?appId=730"));
            JArray overstock = JArray.Parse(Get.Request("https://cs.money/list_overstock?appId=730"));
            foreach (var item in json)
            {
                string itemName = Convert.ToString(item.Value["m"]);
                JToken overItem = overstock.FirstOrDefault(x => x["market_hash_name"].ToString() == itemName);
                if (SteamBase.ItemList.FirstOrDefault(x => x.ItemName == itemName) != null)
                {
                    decimal price = Convert.ToDecimal(item.Value["a"]);
                    SteamBase.ItemList.FirstOrDefault(x => x.ItemName == itemName).Csm = new()
                    {
                        Updated = DateTime.Now,
                        Id = Convert.ToInt32(item.Key),
                        Price = price,
                        OverstockDifference = overItem != null ? (Int32)overItem["overstock_difference"] : 0,
                        Overstock = overItem != null,
                        Unavailable = unavailable.FirstOrDefault(x => x["market_hash_name"].ToString() == itemName) != null,
                        IsHave = price > 0,
                    };
                }
            }
        }
        public void UpdateBuffInfo(bool isBuyOrder, int min, int max)
        {
            if (SteamBase.ItemList.Select(x => x.Buff.Updated).Max().AddMinutes(30) > DateTime.Now)
                return;

            while (!BuffAccount.IsLogIn())
                System.Threading.Thread.Sleep(200);

            decimal CNY = SteamBase.CurrencyList.FirstOrDefault(x => x.Id == 23).Value;
            string tab = isBuyOrder ? "/buying" : string.Empty;
            min = (int)Edit.ConverterFromUsd(min, CNY);
            max = (int)Edit.ConverterFromUsd(max, CNY);
            int pages = int.MaxValue;
            string last_item = string.Empty;
            for (int i = 1; i <= pages; i++)
            {
                try
                {
                    string url = "https://buff.163.com/api/market/goods" + tab + "?game=csgo&page_num=" + i + "&min_price=" + min + "&max_price=" + max + "&sort_by=price.asc&page_size=80";
                    JObject json = JObject.Parse(Get.Request(BuffAccount.Cookies, url));

                    pages = Convert.ToInt32(json["data"]["total_page"]);
                    JArray items = json["data"]["items"] as JArray;
                    foreach (JObject item in items)
                    {
                        string itemName = item["market_hash_name"].ToString();
                        var itemBase = SteamBase.ItemList.FirstOrDefault(x => x.ItemName == itemName);
                        if (itemBase != null && itemName != last_item)
                        {
                            decimal price = Edit.ConverterToUsd(Convert.ToDecimal(item["sell_min_price"]), CNY);
                            itemBase.Buff = new()
                            {
                                Updated = DateTime.Now,
                                Id = Convert.ToInt32(item["id"]),
                                Price = price,
                                Count = Convert.ToInt32(item["sell_num"]),
                                BuyOrder = Edit.ConverterToUsd(Convert.ToDecimal(item["buy_max_price"]), CNY),
                                OrderCount = Convert.ToInt32(item["buy_num"]),
                                IsHave = price > 0,
                            };
                        }
                        last_item = item["market_hash_name"].ToString();
                    }
                }
                catch
                {
                    continue;
                }
            }
        }
        public void UpdateBuffInfoItem(string itemName)
        {
            var itemBase = SteamBase.ItemList.FirstOrDefault(x => x.ItemName == itemName);
            if (itemBase.Buff.Updated.AddMinutes(30) > DateTime.Now)
                return;

            while (!BuffAccount.IsLogIn())
                System.Threading.Thread.Sleep(200);

            string market_hash_name = HttpUtility.UrlEncode(itemName);
            decimal CNY = SteamBase.CurrencyList.FirstOrDefault(x => x.Id == 23).Value;
            int pages = int.MaxValue;
            string last_item = string.Empty;
            for (int i = 1; i <= pages; i++)
            {
                try
                {
                    string url = "https://buff.163.com/api/market/goods/buying?game=csgo&page_num=" + i + "&search=" + market_hash_name + "&sort_by=price.asc&page_size=80";
                    JObject json = JObject.Parse(Get.Request(BuffAccount.Cookies, url));

                    pages = Convert.ToInt32(json["data"]["total_page"]);
                    JArray items = json["data"]["items"] as JArray;
                    foreach (JObject item in items)
                    {
                        if (itemBase != null && itemName != last_item)
                        {
                            decimal price = Edit.ConverterToUsd(Convert.ToDecimal(item["sell_min_price"]), CNY);
                            itemBase.Buff = new()
                            {
                                Updated = DateTime.Now,
                                Id = Convert.ToInt32(item["id"]),
                                Price = price,
                                Count = Convert.ToInt32(item["sell_num"]),
                                BuyOrder = Edit.ConverterToUsd(Convert.ToDecimal(item["buy_max_price"]), CNY),
                                OrderCount = Convert.ToInt32(item["buy_num"]),
                                IsHave = price > 0,
                            };
                        }
                        last_item = item["market_hash_name"].ToString();
                    }
                }
                catch
                {
                    continue;
                }
            }
        }

        public void LoadInventoriesCsm(ParserCheckConfig parserConfig)
        {
            if (DataInventoriesCsm.Items.Any() && DataInventoriesCsm.Items.Select(x => x.Updated).Max().AddMinutes(30) > DateTime.Now)
                return;
            DataInventoriesCsm.Items.Clear();
            int offset = 0;
            string price = $"maxPrice={parserConfig.MaxPrice}&minPrice={parserConfig.MinPrice}&";
            string user = parserConfig.UserItems ? string.Empty : "isMarket=false&";

            string tradeLock = parserConfig.WithoutLock ? "hasTradeLock=false&" : "hasTradeLock=false&hasTradeLock=true&tradeLockDays=1&tradeLockDays=2&tradeLockDays=3&tradeLockDays=4&tradeLockDays=5&tradeLockDays=6&tradeLockDays=7&tradeLockDays=0&";
            string rare = parserConfig.RareItems ? "hasRareFloat=true&hasRarePattern=true&hasRareStickers=true&" : "hasRareFloat=false&hasRarePattern=false&hasRareStickers=false&";
            string onlyDopp = parserConfig.SelectedOnly == 3 ? "phase=Phase%201&phase=Phase%202&phase=Phase%203&phase=Phase%204&phase=Emerald&phase=Sapphire&phase=Ruby&phase=Black%20Pearl&" : string.Empty;

            while (true)
            {
                string url = $"https://inventories.cs.money/5.0/load_bots_inventory/730?limit=60&offset={offset}&" + price + user + tradeLock + onlyDopp + rare + "&order=desc&priceWithBonus=40&sort=price&withStack=true";
                JObject json = JObject.Parse(Get.Request(url));
                if (!json.ContainsKey("error"))
                {
                    JArray items = json["items"] as JArray;
                    foreach (JObject item in items)
                    {
                        int id = Convert.ToInt32(item["nameId"]);
                        int stack = item.ContainsKey("stackSize") ? Convert.ToInt32(item["stackSize"]) : 1;

                        DataInventoriesCsm.Items.Add(new()
                        {
                            Updated = DateTime.Now,
                            ItemName = item["fullName"].ToString(),
                            NameId = id,
                            StackSize = stack,
                            Price = Convert.ToDecimal(item["price"]),
                            DefaultPrice = SteamBase.ItemList.FirstOrDefault(x => x.Csm.Id == id) != null ? SteamBase.ItemList.FirstOrDefault(x => x.Csm.Id == id).Csm.Price : 0m,
                            Float = item["float"].Type != JTokenType.Null ? Convert.ToDecimal(item["float"]) : 0,
                            Sticker = item["stickers"].Type != JTokenType.Null,
                            RareItem = item["overpay"].Type != JTokenType.Null,
                            User = item["userId"].Type != JTokenType.Null,
                            TradeLock = item.ContainsKey("tradeLock") ? Edit.ConvertFromUnixTimestamp(Convert.ToDouble(item["tradeLock"])) : new(),
                        });
                    }
                    offset += 60;
                }
                else
                    break;
            }
        }
        public List<PriceHistory> GetPriceHistory(string itemName)
        {
            string json = Get.Request(SteamAccount.Cookies, "https://steamcommunity.com/market/pricehistory/?appid=730&market_hash_name=" + HttpUtility.UrlEncode(itemName));
            JArray sales = JArray.Parse(JObject.Parse(json)["prices"].ToString());
            List<PriceHistory> history = new();
            foreach (var sale in sales.Reverse())
            {
                DateTime date = DateTime.Parse(sale[0].ToString()[..11]);
                decimal price = Decimal.Parse(sale[1].ToString());
                int count = Convert.ToInt32(sale[2]);

                history.Add(new PriceHistory()
                {
                    Date = date,
                    Price = price,
                    Count = count,
                });
            }
            return history;
        }
    }
}