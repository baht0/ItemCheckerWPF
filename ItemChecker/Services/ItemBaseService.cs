using ItemChecker.MVVM.Model;
using ItemChecker.Net;
using ItemChecker.Properties;
using ItemChecker.Support;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace ItemChecker.Services
{
    public class ItemBaseService : BaseService
    {
        #region private meth
        //steam
        List<Tuple<string, decimal>> GetSteamPrice()
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
        #endregion

        public void CreateItemsBase()
        {
            List<ItemBase> SkinsBase = new();
            JArray skinsBase = JArray.Parse(Post.DropboxRead("steamBase.json"));

            List<Tuple<string, decimal>> stPrices = GetSteamPrice();
            foreach (JObject item in skinsBase)
            {
                string itemName = item["itemName"].ToString();
                int steamId = Convert.ToInt32(item["steamId"]);

                decimal stPrice = stPrices.FirstOrDefault(x => x.Item1 == itemName) != null ? stPrices.FirstOrDefault(x => x.Item1 == itemName).Item2 : 0;
                SkinsBase.Add(new ItemBase()
                {
                    ItemName = itemName,
                    Type = item["type"].ToString(),
                    Quality = item["quality"].ToString(),

                    SteamInfo = new()
                    {
                        Id = steamId,
                        Price = stPrice,
                    }
                });
            }
            ItemBase.SkinsBase = SkinsBase;
        }
        public void UpdateLfmInfo()
        {
            if (ItemBase.SkinsBase.LastOrDefault().LfmInfo.Updated.AddMinutes(20) > DateTime.Now)
                return;
            JArray array = JArray.Parse(Get.Request("https://loot.farm/fullprice.json"));
            foreach (JToken item in array)
            {
                string itemName = item["name"].ToString().Replace("(Holo-Foil)", "(Holo/Foil)").Replace("  ", " ");
                decimal price = Convert.ToDecimal(item["price"]);
                int have = Convert.ToInt32(item["have"]);
                int max = Convert.ToInt32(item["max"]);

                if (ItemBase.SkinsBase.FirstOrDefault(x => x.ItemName == itemName) != null)
                {
                    ItemBase.SkinsBase.FirstOrDefault(x => x.ItemName == itemName).LfmInfo = new()
                    {
                        Updated = DateTime.Now,
                        Price = price / 100,
                        Have = have,
                        Limit = max,
                        Reservable = Convert.ToInt32(item["res"]),
                        Tradable = Convert.ToInt32(item["tr"]),
                        SteamPriceRate = Convert.ToInt32(item["rate"]),
                        Overstock = have >= max,
                        Unavailable = price <= 0,
                    };
                }
            }
        }
        public void UpdateCsmInfo()
        {
            if (ItemBase.SkinsBase.LastOrDefault().CsmInfo.Updated.AddMinutes(20) > DateTime.Now)
                return;
            JObject json = JObject.Parse(Get.Request("https://csm.auction/api/skins_base"));
            JArray unavailable = JArray.Parse(Get.Request("https://cs.money/list_unavailable?appId=730"));
            JArray overstock = JArray.Parse(Get.Request("https://cs.money/list_overstock?appId=730"));
            foreach (var item in json)
            {
                string itemName = Convert.ToString(item.Value["m"]);
                JToken overItem = overstock.FirstOrDefault(x => x["market_hash_name"].ToString() == itemName);
                if (ItemBase.SkinsBase.FirstOrDefault(x => x.ItemName == itemName) != null)
                {
                    ItemBase.SkinsBase.FirstOrDefault(x => x.ItemName == itemName).CsmInfo = new()
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
        public void UpdateBuffInfo(int min, int max)
        {
            if (ItemBase.SkinsBase.LastOrDefault().BuffInfo.Updated.AddMinutes(20) > DateTime.Now)
                return;
            CookieContainer container = new();
            container.Add(new Cookie("session", "1-8dBJa34rRl2kEpVaEdb3yJUBQ8sIimUszE1okDikHmWD2036357433", "/", "buff.163.com"));
            int pages = int.MaxValue;
            for (int i = 1; i <= pages; i++)
            {
                try
                {
                    string url = "https://buff.163.com/api/market/goods/buying?game=csgo&page_num=" + i + "&min_price=" + min + "&max_price=" + max + "&sort_by=price.asc&page_size=80";
                    JObject json = JObject.Parse(Get.Request(container, url));

                    pages = Convert.ToInt32(json["data"]["total_page"]);
                    JArray items = json["data"]["items"] as JArray;
                    foreach (JObject item in items)
                    {
                        string itemName = item["market_hash_name"].ToString();
                        if (ItemBase.SkinsBase.FirstOrDefault(x => x.ItemName == itemName) != null)
                        {
                            ItemBase.SkinsBase.FirstOrDefault(x => x.ItemName == itemName).BuffInfo = new()
                            {
                                Updated = DateTime.Now,
                                Id = Convert.ToInt32(item["id"]),
                                Price = Edit.ConverterToUsd(Convert.ToDecimal(item["sell_min_price"]), SettingsProperties.Default.CNY),
                                BuyOrder = Edit.ConverterToUsd(Convert.ToDecimal(item["buy_max_price"]), SettingsProperties.Default.CNY),
                                Count = Convert.ToInt32(item["sell_num"]),
                                OrderCount = Convert.ToInt32(item["buy_num"])
                            };
                        }
                    }
                }
                catch
                {
                    continue;
                }
            }
        }

        public void LoadInventoriesCsm(ParserConfig parserConfig)
        {
            if (DataInventoriesCsm.Items.LastOrDefault().Updated.AddMinutes(20) > DateTime.Now)
                return;
            DataInventoriesCsm.Items.Clear();
            int offset = 0;
            string price = $"maxPrice={parserConfig.MaxPrice}&minPrice={parserConfig.MinPrice}&";
            string user = parserConfig.UserItems ? string.Empty : "isMarket=false&";

            string tradeLock = parserConfig.WithoutLock ? "hasTradeLock=false&" : "hasTradeLock=false&hasTradeLock=true&tradeLockDays=1&tradeLockDays=2&tradeLockDays=3&tradeLockDays=4&tradeLockDays=5&tradeLockDays=6&tradeLockDays=7&tradeLockDays=0&";
            string rare = parserConfig.RareItems ? "hasRareFloat=true&hasRarePattern=true&hasRareStickers=true&" : "hasRareFloat=false&hasRarePattern=false&hasRareStickers=false&";
            string onlyDopp = parserConfig.OnlyDopplers ? "phase=Phase%201&phase=Phase%202&phase=Phase%203&phase=Phase%204&phase=Emerald&phase=Sapphire&phase=Ruby&phase=Black%20Pearl&" : string.Empty;

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
                            DefaultPrice = ItemBase.SkinsBase.FirstOrDefault(x => x.CsmInfo.Id == id) != null ? ItemBase.SkinsBase.FirstOrDefault(x => x.CsmInfo.Id == id).CsmInfo.Price : 0m,
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
    }
}