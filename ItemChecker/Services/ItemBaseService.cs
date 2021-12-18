using ItemChecker.MVVM.Model;
using ItemChecker.Net;
using ItemChecker.Support;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace ItemChecker.Services
{
    public class ItemBaseService : BaseService
    {
        public void Overstock()
        {
            ItemBase.Overstock.Clear();
            string json = Get.Request("https://cs.money/list_overstock?appId=730");
            JArray array = JArray.Parse(json);

            foreach (JObject item in array)
            {
                string name = Edit.replaceSymbols(item["market_hash_name"].ToString());
                int overstockDifference = Convert.ToInt32(item["overstock_difference"]);

                ItemBase.Overstock.Add(new ItemBase() { ItemName = name, OverstockDifference = overstockDifference });
            }
        }
        public void Unavailable()
        {
            ItemBase.Unavailable.Clear();
            string json = Get.Request("https://cs.money/list_unavailable?appId=730");
            JArray array = JArray.Parse(json);

            foreach (JObject item in array)
            {
                string name = Edit.replaceSymbols(item["market_hash_name"].ToString());

                ItemBase.Unavailable.Add(new ItemBase() { ItemName = name});
            }
        }

        public void ItemsBase()
        {
            ItemBase.SkinsBase.Clear();
            string json = Post.DropboxRead("SkinsBase.json");
            JArray items = JArray.Parse(json);

            json = Get.Request("https://csm.auction/api/skins_base");
            JObject csm_base = JObject.Parse(json);

            foreach (JObject item in items)
            {
                string name = item["Name"].ToString();
                int steamId = Convert.ToInt32(item["SteamId"]);
                int csmId = Convert.ToInt32(item["CsmId"]);
                decimal defPrice = 0;
                if (csmId != 0)
                    defPrice = Convert.ToDecimal(csm_base[csmId.ToString()]["a"]);

                ItemBase.SkinsBase.Add(new ItemBase()
                {
                    ItemName = name.Replace(";", ","),
                    SteamId = steamId,
                    CsmId = csmId,
                    PriceCsm = defPrice
                });
            }
        }

        public static void LoadBotsInventoryCsm()
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

                string name = ItemBase.SkinsBase.Where(x => x.CsmId == id).Select(x => x.ItemName).First();
                decimal defPrice = ItemBase.SkinsBase.Where(x => x.CsmId == id).Select(x => x.PriceCsm).First();

                DataInventoryCsm.Inventory.Add(new DataInventoryCsm()
                {
                    ItemName = name,
                    StackSize = count,
                    Id = id,
                    DefaultPrice = defPrice,
                    Price = price,
                    Sticker = sticker,
                    NameTag = nameTag,
                    TradeLock = tradeLock,
                    User = user,
                    RareItem = rareItem
                });
            }
        }
        public static void LoadBotsInventoryLf()
        {
            DataInventoryLf.Inventory.Clear();
            string json = Get.Request("https://loot.farm/fullprice.json");
            JArray array = JArray.Parse(json);

            foreach (JObject item in array)
            {
                string name = item["name"].ToString();
                decimal price = Convert.ToDecimal(item["price"]) / 100;
                int have = Convert.ToInt32(item["have"]);
                int max = Convert.ToInt32(item["max"]);
                int rate = Convert.ToInt32(item["rate"]);
                int tr = Convert.ToInt32(item["tr"]);
                int res = Convert.ToInt32(item["res"]);

                DataInventoryLf.Inventory.Add(new DataInventoryLf()
                {
                    ItemName = name,
                    DefaultPrice = price,
                    Have = have,
                    Limit = max,
                    Reservable = res,
                    SteamPriceRate = rate,
                    Tradable = tr,
                    IsOverstock = max - have <= 0
                });
            }
        }
    }
}
