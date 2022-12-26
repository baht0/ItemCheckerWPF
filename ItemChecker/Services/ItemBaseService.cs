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
            JObject jobject = new();
            string path = $"{ProjectInfo.DocumentPath}steamBase.json";
            if (SettingsProperties.Default.UseLocalDb && File.Exists(path))
            {
                string file = File.ReadAllText(path);
                jobject = JObject.Parse(file);
            }
            else
                jobject = JObject.Parse(DropboxRequest.Get.Read("steamBase.json"));

            JArray skinsBase = JArray.Parse(jobject["Items"].ToString());

            SteamBase.Updated = Convert.ToDateTime(jobject["Updated"]);
            string json = HttpRequest.RequestGetAsync("https://csgobackpack.net/api/GetItemsList/v2/?no_details=true").Result;
            JObject csgobackpack = (JObject)JObject.Parse(json)["items_list"];
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
                        AvgPrice = Edit.SteamAvgPrice(itemName, csgobackpack),
                    }
                });
            }
        }
        //stm
        public void UpdateSteamItem(string itemName, int currencyId = 1)
        {
            var itemBase = SteamBase.ItemList.FirstOrDefault(x => x.ItemName == itemName).Steam;
            if (itemBase.Updated.AddMinutes(30) > DateTime.Now)
                if (itemBase.CurrencyId == currencyId)
                    return;

            JObject json = SteamRequest.Get.ItemOrdersHistogram(itemName, itemBase.Id, currencyId);

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

            string json = SteamRequest.Get.Request("https://steamcommunity.com/market/pricehistory/?appid=730&market_hash_name=" + Uri.EscapeDataString(itemName));
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

            string json = HttpRequest.RequestGetAsync("https://loot.farm/fullprice.json").Result;
            JArray array = JArray.Parse(json);
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
        public void UpdateCsm(ParserCheckConfig parserConfig)
        {
            if (SteamBase.ItemList.Select(x => x.Csm.Inventory.Select(x => x.Updated).Max()).Max().AddMinutes(30) > DateTime.Now)
                return;

            int offset = 0;

            while (true)
            {
                var items = ServicesRequest.CsMoney.Get.LoadBotsInventory(offset, parserConfig.MinPrice, parserConfig.MaxPrice, parserConfig.UserItems, parserConfig.WithoutLock, parserConfig.RareItems, parserConfig.SelectedOnly);
                if (items != null)
                {
                    string itemName = items[0]["fullName"].ToString();
                    var itemBase = SteamBase.ItemList.FirstOrDefault(x => x.ItemName == itemName).Csm;
                    itemBase.Inventory.Clear();
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
                        itemBase.Inventory.Add(newItem);
                    }
                    itemBase.IsHave = items.HasValues;
                    offset += 60;
                }
                else
                    break;
            }
        }
        public void UpdateCsmItem(string itemName, bool isInventory)
        {
            var itemBase = SteamBase.ItemList.FirstOrDefault(x => x.ItemName == itemName);
            if (itemBase.Csm.Updated.AddMinutes(30) > DateTime.Now && !isInventory)
                return;
            if (itemBase.Csm.Inventory.Any() && itemBase.Csm.Inventory.Select(x => x.Updated).Max().AddMinutes(30) > DateTime.Now && isInventory)
                return;

            var items = ServicesRequest.CsMoney.Get.LoadBotsInventoryItem(itemName);
            var status = ServicesRequest.CsMoney.Get.ItemStatus(itemName);
            if (items != null)
            {
                bool isAdded = false;
                foreach (JObject item in items)
                {
                    string serviceItemName = item["fullName"].ToString();
                    if (serviceItemName == itemName && !isAdded)
                    {
                        itemBase.Csm = new()
                        {
                            Updated = DateTime.Now,
                            Id = Convert.ToInt32(item["nameId"]),
                            Price = Convert.ToInt32(ServicesRequest.CsMoney.Get.ItemInfo(item)["defaultPrice"]),
                            OverstockDifference = Convert.ToInt32(status["overstockDiff"]),
                            Overstock = Convert.ToInt32(status["status"]) == 1,
                            Unavailable = Convert.ToInt32(status["status"]) == 0,
                        };
                        isAdded = true;
                    }
                    if (serviceItemName == itemName && isInventory)
                    {
                        itemBase.Csm.Inventory.Add(new()
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
                    else if (!isInventory && !isAdded)
                        continue;
                    else if (!isInventory && isAdded)
                        break;
                }
                itemBase.Csm.IsHave = items.HasValues;
            }
            else
            {
                itemBase.Csm = new()
                {
                    Updated = DateTime.Now,
                    OverstockDifference = Convert.ToInt32(status["overstockDiff"]),
                    Overstock = Convert.ToInt32(status["status"]) == 1,
                    Unavailable = Convert.ToInt32(status["status"]) == 0,
                };
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
                    JObject json = JObject.Parse(ServicesRequest.Buff163.Get.Request(url));

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
                    JObject json = JObject.Parse(ServicesRequest.Buff163.Get.Request(url));

                    pages = Convert.ToInt32(json["data"]["total_page"]);
                    JArray items = json["data"]["items"] as JArray;
                    foreach (JObject item in items)
                    {
                        string serviceItemName = item["market_hash_name"].ToString();
                        if (serviceItemName == itemName && itemName != last_item)
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
            JObject json = JObject.Parse(ServicesRequest.Buff163.Get.Request(url));
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