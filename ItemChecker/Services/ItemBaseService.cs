using ItemChecker.MVVM.Model;
using ItemChecker.Net;
using ItemChecker.Support;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ItemChecker.Services
{
    internal class CsmBase
    {
        public int Id { get; set; }
        public string ItemName { get; set; }
        public decimal Price { get; set; }
        public bool Overstock { get; set; }
        public int OverstockDifference { get; set; }
        public bool Unavailable { get; set; }
    }
    internal class LfmBase
    {
        public string ItemName { get; set; } = "Unknown";
        public decimal Price { get; set; }
        public int Have { get; set; }
        public int Limit { get; set; }
        public int Reservable { get; set; }
        public int Tradable { get; set; }
        public int SteamPriceRate { get; set; }
        public bool Overstock { get; set; }
        public bool Unavailable { get; set; }
    }
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
        //csm
        List<Tuple<string, int>> GetOverstock()
        {
            List<Tuple<string, int>> overstock = new();
            JArray array = JArray.Parse(Get.Request("https://cs.money/list_overstock?appId=730"));

            foreach (JObject item in array)
            {
                string name = Edit.replaceSymbols(item["market_hash_name"].ToString());
                int overstockDifference = Convert.ToInt32(item["overstock_difference"]);

                overstock.Add(Tuple.Create(name, overstockDifference));
            }
            return overstock;
        }
        List<string> GetUnavailable()
        {
            List<string> unavailable = new();
            JArray array = JArray.Parse(Get.Request("https://cs.money/list_unavailable?appId=730"));

            foreach (JObject item in array)
            {
                string name = Edit.replaceSymbols(item["market_hash_name"].ToString());

                unavailable.Add(name);
            }
            return unavailable;
        }
        List<CsmBase> GetCsmBases()
        {
            List<Tuple<string, int>> overstock = GetOverstock();
            List<string> unavailable = GetUnavailable();

            JObject csm_base = JObject.Parse(Get.Request("https://csm.auction/api/skins_base"));
            List<CsmBase> csm_items = new();
            foreach (var item in csm_base)
            {
                string name = Convert.ToString(item.Value["m"]);
                csm_items.Add(new CsmBase()
                {
                    Id = Convert.ToInt32(item.Key),
                    ItemName = name,
                    Price = Convert.ToDecimal(item.Value["a"]),
                    OverstockDifference = overstock.FirstOrDefault(x => x.Item1 == name) != null ? overstock.FirstOrDefault(x => x.Item1 == name).Item2 : 0,
                    Overstock = overstock.FirstOrDefault(x => x.Item1 == name) != null,
                    Unavailable = unavailable.Contains(name),
                });
            }
            return csm_items;
        }
        //lfm
        List<LfmBase> GetLfmBases()
        {
            JArray lfm_base = JArray.Parse(Get.Request("https://loot.farm/fullprice.json"));
            List<LfmBase> lfm_items = new();
            foreach (JToken item in lfm_base)
            {
                decimal price = Convert.ToDecimal(item["price"]);
                int have = Convert.ToInt32(item["have"]);
                int max = Convert.ToInt32(item["max"]);
                lfm_items.Add(new LfmBase()
                {
                    ItemName = item["name"].ToString(),
                    Price = price,
                    Have = have,
                    Limit = max,
                    Reservable = Convert.ToInt32(item["res"]),
                    Tradable = Convert.ToInt32(item["tr"]),
                    SteamPriceRate = Convert.ToInt32(item["rate"]),
                    Overstock = have >= max,
                    Unavailable = price <= 0,
                });
            }
            return lfm_items;
        }
        #endregion

        public void CreateItemsBase()
        {
            List<ItemBase> SkinsBase = new();
            JArray skinsBase = JArray.Parse(Post.DropboxRead("steamBase.json"));

            List<Tuple<string, decimal>> stPrices = GetSteamPrice();
            List<CsmBase> csmBases = GetCsmBases();
            List<LfmBase> lfmBases = GetLfmBases();
            foreach (JObject item in skinsBase)
            {
                string name = item["itemName"].ToString();
                int steamId = Convert.ToInt32(item["steamId"]);

                decimal stPrice = stPrices.FirstOrDefault(x => x.Item1 == name) != null ? stPrices.FirstOrDefault(x => x.Item1 == name).Item2 : 0;
                CsmBase csmItem = csmBases.FirstOrDefault(x => x.ItemName == name);
                LfmBase lfmItem = lfmBases.FirstOrDefault(x => x.ItemName == name);
                SkinsBase.Add(new ItemBase()
                {
                    ItemName = name,
                    Type = item["type"].ToString(),
                    Quality = item["quality"].ToString(),

                    SteamInfo = new()
                    {
                        Id = steamId,
                        Price = stPrice,
                    },
                    CsmInfo = new()
                    {
                        Id = csmItem != null ? csmItem.Id : 0,
                        Price = csmItem != null ? csmItem.Price : 0,
                        OverstockDifference = csmItem != null ? csmItem.OverstockDifference : 0,
                        Overstock = csmItem != null ? csmItem.Overstock : false,
                        Unavailable = csmItem != null ? csmItem.Unavailable : false,
                    },
                    LfmInfo = new()
                    {
                        Price = lfmItem != null ? lfmItem.Price : 0,
                        Have = lfmItem != null ? lfmItem.Have : 0,
                        Limit = lfmItem != null ? lfmItem.Limit : 0,
                        Reservable = lfmItem != null ? lfmItem.Reservable : 0,
                        Tradable = lfmItem != null ? lfmItem.Tradable : 0,
                        SteamPriceRate = lfmItem != null ? lfmItem.SteamPriceRate : 0,
                        Overstock = lfmItem != null ? lfmItem.Overstock : false,
                        Unavailable = lfmItem != null ? lfmItem.Unavailable : false,
                    }
                });
            }
            ItemBase.SkinsBase = SkinsBase;
        }
        public void UpdateLfmInfo()
        {
            List<LfmBase> lfmBases = GetLfmBases();
            foreach (LfmBase lfmItem in lfmBases)
            {
                Lfm item = ItemBase.SkinsBase.FirstOrDefault(x => x.ItemName == lfmItem.ItemName).LfmInfo;
                item = new Lfm()
                {
                    Price = lfmItem.Price,
                    Have = lfmItem.Have,
                    Limit = lfmItem.Limit,
                    Reservable = lfmItem.Reservable,
                    Tradable = lfmItem.Tradable,
                    SteamPriceRate = lfmItem.SteamPriceRate,
                    Overstock = lfmItem.Overstock,
                    Unavailable = lfmItem.Unavailable,
                };
            }
        }
        public void LoadBotsInventoryCsm()
        {
            DataInventoryCsm.Inventory.Clear();
            string json = Get.Request("https://old.cs.money/730/load_bots_inventory");
            JArray array = JArray.Parse(json);

            foreach (JObject item in array)
            {
                int count = item["id"].Count();
                int id = Convert.ToInt32(item["o"]);
                decimal price = Convert.ToDecimal(item["p"]);
                if (item.ContainsKey("cp"))
                    price = Convert.ToDecimal(item["cp"]);
                bool sticker = item.ContainsKey("s");
                bool nameTag = item.ContainsKey("n");
                bool user = item.ContainsKey("ui");
                DateTime tradeLock = new();
                if (item.ContainsKey("t"))
                    tradeLock = Edit.ConvertFromUnixTimestamp(Convert.ToDouble(item["t"][0]));
                bool rareItem = item.ContainsKey("ar");

                string name = ItemBase.SkinsBase.FirstOrDefault(x => x.CsmInfo.Id == id) != null ? ItemBase.SkinsBase.FirstOrDefault(x => x.CsmInfo.Id == id).ItemName : string.Empty;
                decimal defPrice = ItemBase.SkinsBase.FirstOrDefault(x => x.CsmInfo.Id == id) != null ? ItemBase.SkinsBase.FirstOrDefault(x => x.CsmInfo.Id == id).CsmInfo.Price : 0m;

                DataInventoryCsm.Inventory.Add(new DataInventoryCsm()
                {
                    ItemName = name,
                    StackSize = count,
                    Id = id,
                    Price = price,
                    Sticker = sticker,
                    NameTag = nameTag,
                    TradeLock = tradeLock,
                    User = user,
                    RareItem = rareItem
                });
            }
        }
    }
}