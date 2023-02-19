using ItemChecker.Net;
using ItemChecker.Properties;
using ItemChecker.Support;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace ItemChecker.MVVM.Model
{
    public class ItemBaseService
    {
        public static void CreateItemsBase()
        {
            var jobject = new JObject();
            string path = $"{ProjectInfo.DocumentPath}SteamItemsBase.json";
            if (SettingsProperties.Default.UseLocalDb && File.Exists(path))
            {
                string file = File.ReadAllText(path);
                jobject = JObject.Parse(file);
            }
            else
                jobject = JObject.Parse(DropboxRequest.Get.Read("SteamItemsBase.json"));

            ItemsBase.Updated = Convert.ToDateTime(jobject["Updated"]);
            ItemsBase.List = JArray.Parse(jobject["Items"].ToString()).ToObject<List<Item>>();
        }

        //stm
        public static void UpdateSteam()
        {
            string json = HttpRequest.RequestGetAsync("https://csgobackpack.net/api/GetItemsList/v2/?no_details=true").Result;
            JObject csgobackpack = (JObject)JObject.Parse(json)["items_list"];
            foreach (var item in ItemsBase.List)
            {
                var baseItem = ItemsBase.List.FirstOrDefault(x => x.ItemName == item.ItemName).Steam;
                baseItem.AvgPrice = Edit.SteamAvgPrice(item.ItemName, csgobackpack);
            }
        }
        public static void UpdateSteamItem(string itemName, int currencyId = 1)
        {
            var itemBase = ItemsBase.List.FirstOrDefault(x => x.ItemName == itemName).Steam;
            if (itemBase.Updated.AddMinutes(30) > DateTime.Now && itemBase.CurrencyId == currencyId)
                return;

            JObject json = SteamRequest.Get.ItemOrdersHistogram(itemName, itemBase.Id, currencyId);
            decimal high = !String.IsNullOrEmpty(json["highest_buy_order"].ToString()) ? Convert.ToDecimal(json["highest_buy_order"]) / 100 : 0;
            decimal low = !String.IsNullOrEmpty(json["lowest_sell_order"].ToString()) ? Convert.ToDecimal(json["lowest_sell_order"]) / 100 : 0;

            itemBase.HighestBuyOrder = high;
            itemBase.LowestSellOrder = low;
            itemBase.IsHave = low > 0;
        }
        public static void UpdateSteamItemHistory(string itemName)
        {
            var itemBase = ItemsBase.List.FirstOrDefault(x => x.ItemName == itemName).Steam;
            if (itemBase == null || itemBase.History.Any())
                return;

            string json = SteamRequest.Get.Request("https://steamcommunity.com/market/pricehistory/?appid=730&market_hash_name=" + Uri.EscapeDataString(itemName));
            JArray sales = JArray.Parse(JObject.Parse(json)["prices"].ToString());
            foreach (var sale in sales.Reverse())
            {
                var date = DateTime.Parse(sale[0].ToString()[..11]);
                var price = decimal.Parse(sale[1].ToString());
                var count = Convert.ToInt32(sale[2]);

                itemBase.History.Add(new(date, price, count, false));
            }
        }
        //lfm
        public static void UpdateLfm()
        {
            if (ItemsBase.List.Select(x => x.Lfm.Updated).Max().AddMinutes(30) > DateTime.Now)
                return;

            string json = HttpRequest.RequestGetAsync("https://loot.farm/fullprice.json").Result;
            JArray array = JArray.Parse(json);
            foreach (JToken item in array)
            {
                string itemName = item["name"].ToString().Replace("(Holo-Foil)", "(Holo/Foil)").Replace("  ", " ");
                if (ItemsBase.List.FirstOrDefault(x => x.ItemName == itemName) != null)
                {
                    decimal price = Convert.ToDecimal(item["price"]) / 100;
                    int have = Convert.ToInt32(item["have"]);
                    int max = Convert.ToInt32(item["max"]);

                    ItemsBase.List.FirstOrDefault(x => x.ItemName == itemName).Lfm = new()
                    {
                        Updated = DateTime.Now,
                        Price = price,
                        Have = have,
                        Limit = max,
                        Reservable = Convert.ToInt32(item["res"]),
                        Tradable = Convert.ToInt32(item["tr"]),
                        SteamPriceRate = Convert.ToInt32(item["rate"]) / 100,
                        IsHave = Convert.ToInt32(item["tr"]) > 0,
                        Status = have >= max ? ItemStatus.Overstock : ItemStatus.Available
                    };
                }
            }
        }
        //csm
        public static void UpdateCsm(ParserConfig parserConfig)
        {
            if (ItemsBase.List.Select(x => x.Csm.Inventory.Select(x => x.Updated).Max()).Max().AddMinutes(30) > DateTime.Now)
                return;

            int offset = 0;

            while (true)
            {
                var items = ServicesRequest.CsMoney.Get.LoadBotsInventory(offset, parserConfig.MinPrice, parserConfig.MaxPrice);
                if (items != null)
                {
                    string itemName = items[0]["fullName"].ToString();
                    var itemBase = ItemsBase.List.FirstOrDefault(x => x.ItemName == itemName).Csm;
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
        public static void UpdateCsmItem(string itemName, bool isInventory)
        {
            var itemBase = ItemsBase.List.FirstOrDefault(x => x.ItemName == itemName);
            if (itemBase.Csm.Updated.AddMinutes(30) > DateTime.Now && !isInventory)
                return;
            if (itemBase.Csm.Inventory.Any() && itemBase.Csm.Inventory.Select(x => x.Updated).Max().AddMinutes(30) > DateTime.Now && isInventory)
                return;

            var items = ServicesRequest.CsMoney.Get.LoadBotsInventoryItem(itemName);
            var status = ServicesRequest.CsMoney.Get.ItemStatus(itemName);

            var itemStatus = ItemStatus.Available;
            int.TryParse((string)status["status"], out int result);
            if (result == 0)
                itemStatus = ItemStatus.Unavailable;
            else if (result == 1)
                itemStatus = ItemStatus.Overstock;

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
                            Status = itemStatus,
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
                    Status = itemStatus,
                };
            }
        }
        //buff
        public static void UpdateBuff(ParserConfig parserConfig, int serviceId)
        {
            if (ItemsBase.List.Select(x => x.Buff.Updated).Max().AddMinutes(30) > DateTime.Now)
                return;

            string quality = string.Empty;
            if (parserConfig.Normal)
                quality = "&quality=normal";
            else if (parserConfig.Souvenir)
                quality = "&quality=tournament";
            else if (parserConfig.Stattrak)
                quality = "&quality=strange";
            else if (parserConfig.KnifeGlove)
                quality = "&quality=unusual";
            else if (parserConfig.KnifeGloveStattrak)
                quality = "&quality=unusual_strange";

            int min = (int)Currency.ConverterFromUsd(parserConfig.MinPrice, 23);
            int max = (int)Currency.ConverterFromUsd(parserConfig.MaxPrice, 23);
            string tab = string.Empty;

            switch (serviceId)
            {
                case 1:
                    tab = parserConfig.ServiceOne == 4 ? "/buying" : string.Empty;
                    break;
                case 2:
                    min = (int)(parserConfig.MinPrice * 0.5m);
                    max = (int)(parserConfig.MaxPrice * 2.5m);
                    tab = parserConfig.ServiceTwo == 4 ? "/buying" : string.Empty;
                    break;
            }

            min = (int)Currency.ConverterFromUsd(min, 23);
            max = (int)Currency.ConverterFromUsd(max, 23);
            int pages = int.MaxValue;
            string last_item = string.Empty;
            for (int i = 1; i <= pages; i++)
            {
                try
                {
                    string url = "https://buff.163.com/api/market/goods" + tab + $"?game=csgo&page_num={i}&min_price={min}&max_price={max}{quality}&sort_by=price.asc&page_size=80";
                    JObject json = JObject.Parse(ServicesRequest.Buff163.Get.Request(url));

                    pages = Convert.ToInt32(json["data"]["total_page"]);
                    JArray items = json["data"]["items"] as JArray;
                    foreach (JObject item in items)
                    {
                        string itemName = item["market_hash_name"].ToString();
                        var itemBase = ItemsBase.List.FirstOrDefault(x => x.ItemName == itemName);
                        if (itemBase != null && itemName != last_item)
                        {
                            decimal price = Currency.ConverterToUsd(Convert.ToDecimal(item["sell_min_price"]), 23);
                            itemBase.Buff = new()
                            {
                                Updated = DateTime.Now,
                                Id = Convert.ToInt32(item["id"]),
                                Price = price,
                                Count = Convert.ToInt32(item["sell_num"]),
                                BuyOrder = Currency.ConverterToUsd(Convert.ToDecimal(item["buy_max_price"]), 23),
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
        public static void UpdateBuffItem(string itemName)
        {
            var itemBase = ItemsBase.List.FirstOrDefault(x => x.ItemName == itemName);
            if (itemBase.Buff.Updated.AddMinutes(30) > DateTime.Now)
                return;

            string market_hash_name = HttpUtility.UrlEncode(itemName);
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
                            decimal price = Currency.ConverterToUsd(Convert.ToDecimal(item["sell_min_price"]), 23);
                            itemBase.Buff = new()
                            {
                                Updated = DateTime.Now,
                                Id = Convert.ToInt32(item["id"]),
                                Price = price,
                                Count = Convert.ToInt32(item["sell_num"]),
                                BuyOrder = Currency.ConverterToUsd(Convert.ToDecimal(item["buy_max_price"]), 23),
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
        public static void UpdateBuffItemHistory(string itemName)
        {
            var itemBase = ItemsBase.List.FirstOrDefault(x => x.ItemName == itemName).Buff;
            if (itemBase == null || itemBase.History.Any())
                return;

            var url = "https://buff.163.com/api/market/goods/bill_order?game=csgo&goods_id=" + itemBase.Id;
            var json = JObject.Parse(ServicesRequest.Buff163.Get.Request(url));
            var items = (JArray)json["data"]["items"];
            foreach (JObject item in items)
            {
                double time = Convert.ToDouble(item["buyer_pay_time"]);
                DateTime date = Edit.ConvertFromUnixTimestamp(time).AddHours(3);
                decimal price = Currency.ConverterToUsd(Convert.ToDecimal(item["price"]), 23);
                bool isBuyOrder = Convert.ToInt32(item["type"]) == 2;
                itemBase.History.Add(new(date, price, 1, isBuyOrder));
            }
        }
    }
    public class ItemsBase
    {
        public static DateTime Updated { get; set; } = new();
        public static List<Item> List { get; set; } = new();
    }
    public class Item
    {
        public string ItemName { get; set; } = string.Empty;
        public Type Type { get; set; }
        public Quality? Quality { get; set; }
        public SteamItem Steam { get; set; }
        public CsmItem Csm { get; set; } = new();
        public LfmItem Lfm { get; set; } = new();
        public BuffItem Buff { get; set; } = new();
    }
    public enum Type
    {
        Weapon,
        Knife,
        Gloves,
        Agent,
        Sticker,
        Patch,
        Collectable,
        Key,
        Pass,
        MusicKit,
        Graffiti,
        Container,
        Gift,
        Tool
    }
    public enum Quality
    {
        ConsumerGrade,
        IndustrialGrade,
        MilSpec,
        Restricted,
        Classified,
        Covert,
        Contraband
    }
    public class SteamItem
    {
        public DateTime Updated { get; set; } = DateTime.Now.AddHours(-1);
        public int Id { get; set; }
        public decimal AvgPrice { get; set; }
        public int CurrencyId { get; set; }
        public decimal LowestSellOrder { get; set; }
        public decimal HighestBuyOrder { get; set; }
        public bool IsHave { get; set; }
        public List<SaleHistory> History { get; set; } = new();
    }
    public class CsmItem
    {
        public DateTime Updated { get; set; } = DateTime.Now.AddHours(-1);
        public int Id { get; set; }
        public decimal Price { get; set; }
        public int OverstockDifference { get; set; }
        public bool IsHave { get; set; }
        public ItemStatus Status { get; set; }
        public List<InventoryCsm> Inventory { get; set; } = new();
    }
    public class LfmItem
    {
        public DateTime Updated { get; set; } = DateTime.Now.AddHours(-1);
        public decimal Price { get; set; }
        public int Have { get; set; }
        public int Limit { get; set; }
        public int Reservable { get; set; }
        public int Tradable { get; set; }
        public decimal SteamPriceRate { get; set; }
        public bool IsHave { get; set; }
        public ItemStatus Status { get; set; } = ItemStatus.Unavailable;
    }
    public class BuffItem
    {
        public DateTime Updated { get; set; } = DateTime.Now.AddHours(-1);
        public int Id { get; set; }
        public decimal Price { get; set; }
        public int Count { get; set; }
        public decimal BuyOrder { get; set; }
        public int OrderCount { get; set; }
        public bool IsHave { get; set; }
        public List<SaleHistory> History { get; set; } = new();
    }
    public enum ItemStatus
    {
        Available,
        Overstock,
        Unavailable,
    }

    //History
    public class SaleHistory
    {
        public DateTime Date { get; set; } = new();
        public decimal Price { get; set; }
        public int Count { get; set; }
        public bool IsBuyOrder { get; set; }

        public SaleHistory(DateTime date, decimal price, int count, bool isBuyOrder)
        {
            Date = date;
            Price = price;
            Count = count;
            IsBuyOrder = isBuyOrder;
        }
    }
    //csm
    public class InventoryCsm
    {
        public DateTime Updated { get; set; } = DateTime.Now.AddHours(-1);
        public int NameId { get; set; }
        public int StackSize { get; set; }
        public decimal Price { get; set; }
        public bool Sticker { get; set; }
        public decimal Float { get; set; }
        public bool User { get; set; }
        public DateTime TradeLock { get; set; } = new();
        public bool RareItem { get; set; }
    }
}
