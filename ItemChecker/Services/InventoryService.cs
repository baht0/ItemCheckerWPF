using ItemChecker.MVVM.Model;
using ItemChecker.Net;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ItemChecker.Services
{
    public class InventoryService
    {
        static List<DataInventory> inventory = new();
        static decimal sumOfItems = 0;
        public static List<DataInventory> CheckInventory()
        {
            var json = SteamRequest.Get.Request("http://steamcommunity.com/my/inventory/json/730/2");
            JObject rgInventory = (JObject)JObject.Parse(json)["rgInventory"];
            JObject rgDescriptions = (JObject)JObject.Parse(json)["rgDescriptions"];

            List<DataInventory> inventory = new();
            foreach (var jObject in rgInventory)
            {
                string assetid = jObject.Value["id"].ToString();
                string classid = jObject.Value["classid"].ToString();
                string instanceid = jObject.Value["instanceid"].ToString();

                JObject jsonItem = (JObject)rgDescriptions[$"{classid}_{instanceid}"];

                string name = jsonItem["market_name"].ToString();
                bool marketable = (int)jsonItem["marketable"] != 0;
                if (!marketable) continue;
                bool tradable = (int)jsonItem["tradable"] != 0;
                bool nameTag = jsonItem.ContainsKey("fraudwarnings");
                bool stickers = jsonItem["descriptions"].ToString().Contains("sticker_info");
                DateTime tradeLock = jsonItem.ContainsKey("cache_expiration") ? (DateTime)jsonItem["cache_expiration"] : new();

                DataInventory item = inventory.FirstOrDefault(x => x.ItemName == name);
                item ??= new()
                {
                    ItemName = name
                };

                item.Data.Add(new()
                {
                    AssetId = assetid,
                    ClassId = classid,
                    InstanceId = instanceid,
                    Tradable = tradable,
                    TradeLock = tradeLock,
                    Marketable = marketable,
                    Stickers = stickers,
                    NameTag = nameTag,
                });
                item.Data.OrderBy(d => d.TradeLock);

                if (!inventory.Any(x => x.ItemName == name))
                    inventory.Add(item);
            }
            return inventory;
        }
        public static decimal GetSumOfItems(List<DataInventory> items = null)
        {
            items ??= CheckInventory();

            bool same = inventory.Count == items.Count
                && inventory.Select(x => x.ItemName).ToHashSet().SetEquals(items.Select(x => x.ItemName))
                && inventory.Select(x => x.Data.Count).ToHashSet().SetEquals(items.Select(x => x.Data.Count));

            if (!same)
            {
                sumOfItems = 0;
                inventory = items;
                foreach (var item in items)
                {
                    if (item.Data.Any(x => x.Marketable))
                    {
                        var baseItem = ItemsBase.List.FirstOrDefault(x => x.ItemName == item.ItemName);
                        var json = SteamRequest.Get.ItemOrdersHistogram(baseItem.ItemName, baseItem.Steam.Id, 1);
                        var price = Convert.ToDecimal(json["highest_buy_order"]) / 100;
                        sumOfItems += price * item.Data.Count;
                    }
                }
            }
            return sumOfItems;
        }

        public static List<DataTradeOffer> CheckOffer()
        {
            var offers = new List<DataTradeOffer>();
            var json = SteamRequest.Get.TradeOffers();
            var trades = (JArray)json["response"]["trade_offers_received"];
            foreach (var trade in trades)
            {
                if (trade["trade_offer_state"].ToString() == "2")
                {
                    offers.Add(new()
                    {
                        TradeOfferId = trade["tradeofferid"].ToString(),
                        PartnerId = trade["accountid_other"].ToString()
                    });
                }
                else
                    continue;
            }
            return offers;
        }
        public static void SellItem(DataInventory inventoryItem, HomeInventoryConfig config)
        {

            ItemBaseService.UpdateSteamItem(inventoryItem.ItemName, SteamAccount.Currency.Id);
            var baseItem = ItemsBase.List.FirstOrDefault(x => x.ItemName == inventoryItem.ItemName).Steam;

            foreach (var item in inventoryItem.Data)
            {
                if (!item.Marketable || item.Stickers || item.NameTag)
                    return;

                decimal price = 0;
                switch (config.SellingPriceId)
                {
                    case 0:
                        price = baseItem.LowestSellOrder;
                        break;
                    case 1:
                        price = baseItem.HighestBuyOrder;
                        break;
                    case 2:
                        price = config.Price;
                        break;
                }
                int sellPrice = (int)((price * 100 - 0.01m) * Calculator.CommissionSteam);
                SteamRequest.Post.SellItem(item.AssetId, sellPrice);
            }
        }
    }
}