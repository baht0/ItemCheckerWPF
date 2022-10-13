using ItemChecker.MVVM.Model;
using ItemChecker.Net;
using ItemChecker.Properties;
using ItemChecker.Support;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Web;

namespace ItemChecker.Services
{
    public class ItemBaseService : BaseService
    {
        public void CreateItemsBase()
        {
            JObject json = new();
            string path = $"{ProjectInfo.DocumentPath}steamBase.json";
            if (SettingsProperties.Default.UseLocalDb && File.Exists(path))
            {
                string file = File.ReadAllText(path);
                json = JObject.Parse(file);
            }
            else
                json = JObject.Parse(Get.DropboxRead("steamBase.json"));

            JArray skinsBase = JArray.Parse(json["Items"].ToString());

            SteamBase.Updated = Convert.ToDateTime(json["Updated"]);
            JObject csgobackpack = (JObject)JObject.Parse(Get.Request("https://csgobackpack.net/api/GetItemsList/v2/?no_details=true"))["items_list"];
            foreach (JObject item in skinsBase)
            {
                string itemName = item["itemName"].ToString();

                SteamBase.ItemList.Add(new()
                {
                    ItemName = itemName,
                    Type = item["type"].ToString(),
                    Quality = item["quality"].ToString(),

                    Steam = new()
                    {
                        Id = Convert.ToInt32(item["steamId"]),
                        AvgPrice = Get.SteamAvgPrice(itemName, csgobackpack),
                    }
                });
            }
        }
        //stm
        public void UpdateSteamItem(string itemName, int currencyId = 1)
        {
            var itemBase = SteamBase.ItemList.FirstOrDefault(x => x.ItemName == itemName).Steam;
            if (itemBase.Updated.AddMinutes(30) > DateTime.Now)
                return;

            JObject json = Get.ItemOrdersHistogram(itemBase.Id, currencyId);

            decimal high = !String.IsNullOrEmpty(json["highest_buy_order"].ToString()) ? Convert.ToDecimal(json["highest_buy_order"]) / 100 : 0;
            decimal low = !String.IsNullOrEmpty(json["lowest_sell_order"].ToString()) ? Convert.ToDecimal(json["lowest_sell_order"]) / 100 : 0;

            itemBase.HighestBuyOrder = high;
            itemBase.LowestSellOrder = low;
            itemBase.IsHave = low > 0;
        }
        public void UpdateSteamItemHistory(string itemName)
        {
            var itemBase = SteamBase.ItemList.FirstOrDefault(x => x.ItemName == itemName).Steam;
            if (itemBase == null || itemBase.History.Any())
                return;

            string json = Get.Request(SteamAccount.Cookies, "https://steamcommunity.com/market/pricehistory/?appid=730&market_hash_name=" + HttpUtility.UrlEncode(itemName));
            JArray sales = JArray.Parse(JObject.Parse(json)["prices"].ToString());
            foreach (var sale in sales.Reverse())
            {
                DateTime date = DateTime.Parse(sale[0].ToString()[..11]);
                decimal price = Decimal.Parse(sale[1].ToString());
                int count = Convert.ToInt32(sale[2]);

                itemBase.History.Add(new(date, price, count));
            }
        }
        //lfm
        public void UpdateLfm()
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
                        IsHave = Convert.ToInt32(item["tr"]) > 0,
                    };
                }
            }
        }
        //csm
        public void UpdateCsm()
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
                    SteamBase.ItemList.FirstOrDefault(x => x.ItemName == itemName).Csm = new()
                    {
                        Updated = DateTime.Now,
                        Id = Convert.ToInt32(item.Key),
                        Price = Convert.ToDecimal(item.Value["a"]),
                        OverstockDifference = overItem != null ? (Int32)overItem["overstock_difference"] : 0,
                        Overstock = overItem != null,
                        Unavailable = unavailable.FirstOrDefault(x => x["market_hash_name"].ToString() == itemName) != null,
                    };
                }
            }
        }
        public void UpdateInventoryCsm(ParserCheckConfig parserConfig)
        {
            if (SteamBase.ItemList.Select(x => x.Csm.Inventory.Select(x => x.Updated).Max()).Max().AddMinutes(30) > DateTime.Now)
                return;

            int offset = 0;
            string price = $"maxPrice={parserConfig.MaxPrice}&minPrice={parserConfig.MinPrice}&";
            string user = parserConfig.UserItems ? string.Empty : "isMarket=false&";

            string tradeLock = parserConfig.WithoutLock ? "hasTradeLock=false&" : "hasTradeLock=false&hasTradeLock=true&tradeLockDays=1&tradeLockDays=2&tradeLockDays=3&tradeLockDays=4&tradeLockDays=5&tradeLockDays=6&tradeLockDays=7&tradeLockDays=0&";
            string rare = parserConfig.RareItems ? "hasRareFloat=true&hasRarePattern=true&hasRareStickers=true&" : "hasRareFloat=false&hasRarePattern=false&hasRareStickers=false&";
            string onlyDopp = parserConfig.SelectedOnly == 3 ? "phase=Phase%201&phase=Phase%202&phase=Phase%203&phase=Phase%204&phase=Emerald&phase=Sapphire&phase=Ruby&phase=Black%20Pearl&" : string.Empty;

            while (true)
            {
                JObject json = JObject.Parse(Get.Request($"https://inventories.cs.money/5.0/load_bots_inventory/730?limit=60&offset={offset}&" + price + user + tradeLock + onlyDopp + rare + "&order=desc&priceWithBonus=40&sort=price&withStack=true"));
                if (!json.ContainsKey("error"))
                {
                    JArray items = json["items"] as JArray;
                    string itemName = items[0]["fullName"].ToString();
                    var baseItem = SteamBase.ItemList.FirstOrDefault(x => x.ItemName == itemName).Csm;
                    baseItem.Inventory.Clear();
                    foreach (JObject item in items)
                    {
                        InventoryCsm newItem = new()
                        {
                            NameId = Convert.ToInt32(item["nameId"]),
                            StackSize = item.ContainsKey("stackSize") ? Convert.ToInt32(item["stackSize"]) : 1,
                            Price = Convert.ToDecimal(item["price"]),
                            Float = item["float"].Type != JTokenType.Null ? Convert.ToDecimal(item["float"]) : 0,
                            Sticker = item["stickers"].Type != JTokenType.Null,
                            RareItem = item["overpay"].Type != JTokenType.Null,
                            User = item["userId"].Type != JTokenType.Null,
                            TradeLock = item.ContainsKey("tradeLock") ? Edit.ConvertFromUnixTimestamp(Convert.ToDouble(item["tradeLock"])) : new(),
                        };
                        baseItem.Inventory.Add(newItem);
                    }
                    baseItem.IsHave = items.HasValues;
                    offset += 60;
                }
                else
                    break;
            }
        }
        public void UpdateInventoryCsmItem(string itemName)
        {
            var baseItem = SteamBase.ItemList.FirstOrDefault(x => x.ItemName == itemName).Csm;
            if (baseItem.Inventory.Any() && baseItem.Inventory.Select(x => x.Updated).Max().AddMinutes(30) > DateTime.Now)
                return;

            string market_hash_name = Edit.EncodeMarketHashName(itemName);
            string stattrak = market_hash_name.Contains("StatTrak") ? "true" : "false";
            string souvenir = market_hash_name.Contains("Souvenir") ? "true" : "false";

            JObject json = JObject.Parse(Get.Request($"https://inventories.cs.money/5.0/load_bots_inventory/730?isSouvenir=" + souvenir + "&isStatTrak=" + stattrak + "&limit=60&name=" + market_hash_name + "&offset=0&order=asc&priceWithBonus=30&sort=price&withStack=true"));
            if (!json.ContainsKey("error"))
            {
                JArray items = json["items"] as JArray;
                baseItem.Inventory.Clear();
                foreach (JObject item in items)
                {
                    if (item["fullName"].ToString() != itemName)
                        continue;
                    baseItem.Inventory.Add(new()
                    {
                        NameId = Convert.ToInt32(item["nameId"]),
                        StackSize = item.ContainsKey("stackSize") ? Convert.ToInt32(item["stackSize"]) : 1,
                        Price = Convert.ToDecimal(item["price"]),
                        Float = item["float"].Type != JTokenType.Null ? Convert.ToDecimal(item["float"]) : 0,
                        Sticker = item["stickers"].Type != JTokenType.Null,
                        RareItem = item["overpay"].Type != JTokenType.Null,
                        User = item["userId"].Type != JTokenType.Null,
                        TradeLock = item.ContainsKey("tradeLock") ? Edit.ConvertFromUnixTimestampJava(Convert.ToDouble(item["tradeLock"])) : new(),
                        Updated = DateTime.Now,
                    });
                }
                baseItem.IsHave = items.HasValues;
            }
        }
        //buff
        public void UpdateBuff(bool isBuyOrder, int min, int max)
        {
            if (SteamBase.ItemList.Select(x => x.Buff.Updated).Max().AddMinutes(30) > DateTime.Now)
                return;

            decimal CNY = SteamBase.AllowCurrencys.FirstOrDefault(x => x.Id == 23).Value;
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
        public void UpdateBuffItem(string itemName)
        {
            var itemBase = SteamBase.ItemList.FirstOrDefault(x => x.ItemName == itemName);
            if (itemBase.Buff.Updated.AddMinutes(30) > DateTime.Now)
                return;

            string market_hash_name = HttpUtility.UrlEncode(itemName);
            decimal CNY = SteamBase.AllowCurrencys.FirstOrDefault(x => x.Id == 23).Value;
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
                        string serviceItemName = item["market_hash_name"].ToString();
                        if (itemBase != null && serviceItemName == itemName && itemName != last_item)
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
                        last_item = serviceItemName;
                    }
                }
                catch
                {
                    continue;
                }
            }
        }
        public void UpdateBuffItemHistory(string itemName)
        {
            var itemBase = SteamBase.ItemList.FirstOrDefault(x => x.ItemName == itemName).Buff;
            if (itemBase == null || itemBase.History.Any())
                return;

            decimal CNY = SteamBase.AllowCurrencys.FirstOrDefault(x => x.Id == 23).Value;

            string url = "https://buff.163.com/api/market/goods/bill_order?game=csgo&goods_id=" + itemBase.Id;
            JObject json = JObject.Parse(Get.Request(BuffAccount.Cookies, url));
            JArray items = json["data"]["items"] as JArray;
            foreach (JObject item in items)
            {
                double time = Convert.ToDouble(item["buyer_pay_time"]);
                DateTime date = Edit.ConvertFromUnixTimestamp(time);
                decimal price = Edit.ConverterToUsd(Convert.ToDecimal(item["price"]), CNY);
                itemBase.History.Add(new(date, price, 1));
            }
        }
    }
}