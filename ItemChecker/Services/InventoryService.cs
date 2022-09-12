using ItemChecker.MVVM.Model;
using ItemChecker.Net;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ItemChecker.Services
{
    public class InventoryService
    {
        public List<DataInventory> CheckInventory(string itemName)
        {
            var json = Get.Request(SteamAccount.Cookies, "http://steamcommunity.com/my/inventory/json/730/2");
            JObject rgInventory = (JObject)JObject.Parse(json)["rgInventory"];
            JObject rgDescriptions = (JObject)JObject.Parse(json)["rgDescriptions"];

            List<DataInventory> items = new();
            foreach (var jObject in rgInventory)
            {
                string assetid = jObject.Value["id"].ToString();
                string classid = jObject.Value["classid"].ToString();
                string instanceid = jObject.Value["instanceid"].ToString();

                JObject item = (JObject)rgDescriptions[$"{classid}_{instanceid}"];

                string name = item["market_name"].ToString();
                if (!String.IsNullOrEmpty(itemName) && itemName != name)
                    continue;

                bool marketable = (int)item["marketable"] != 0;
                if (!marketable) continue;
                bool tradable = (int)item["tradable"] != 0;
                bool nameTag = item.ContainsKey("fraudwarnings");
                bool stickers = item["descriptions"].ToString().Contains("sticker_info");
                DateTime tradeLock = item.ContainsKey("cache_expiration") ? (DateTime)item["cache_expiration"] : new();

                items.Add(new()
                {
                    ItemName = name,
                    AssetId = assetid,
                    ClassId = classid,
                    InstanceId = instanceid,
                    Tradable = tradable,
                    TradeLock = tradeLock,
                    Marketable = marketable,
                    Stickers = stickers,
                    NameTag = nameTag,
                });
            }
            return items;
        }

        public Boolean CheckOffer()
        {
            try
            {
                DataTradeOffer.Offers = new();
                JObject json = Get.TradeOffers(SteamAccount.ApiKey);
                JArray trades = (JArray)json["response"]["trade_offers_received"];
                foreach (var trade in trades)
                {
                    var trade_status = trade["trade_offer_state"].ToString();
                    if (trade_status == "2")
                    {
                        DataTradeOffer.Offers.Add(new()
                        {
                            TradeOfferId = trade["tradeofferid"].ToString(),
                            PartnerId = trade["accountid_other"].ToString()
                        });
                    }
                    else
                        continue;
                }
                return DataTradeOffer.Offers.Any();
            }
            catch
            {
                return false;
            }
        }
        public void SellItem(DataInventory item, HomeInventoryConfig config)
        {
            if (!item.Marketable || item.Stickers || item.NameTag)
                return;

            if (item.LowestSellOrder <= 0 && item.HighestBuyOrder <= 0)
            {
                int item_nameid = SteamBase.ItemList.FirstOrDefault(x => x.ItemName == item.ItemName).Steam.Id;
                JObject json = Get.ItemOrdersHistogram(item_nameid, SteamAccount.CurrencyId);

                item.LowestSellOrder = !String.IsNullOrEmpty(json["lowest_sell_order"].ToString()) ? Convert.ToDecimal(json["lowest_sell_order"]) / 100 : 0;
                item.HighestBuyOrder = !String.IsNullOrEmpty(json["highest_buy_order"].ToString()) ? Convert.ToDecimal(json["highest_buy_order"]) / 100 : 0;
            }

            decimal sellPrice = config.SellingPriceId == 0 ? item.LowestSellOrder : item.HighestBuyOrder;
            int sell_price = (int)((sellPrice * 100 - 0.01m) * Calculator.CommissionSteam);
            Post.SellItem(SteamAccount.Cookies, SteamAccount.UserName, item.AssetId, sell_price);
        }

        public void AcceptTrade(string tradeOfferId, string partnerId)
        {
            Thread.Sleep(1000);
            Post.AcceptTrade(SteamAccount.Cookies, tradeOfferId, partnerId);
        }
    }
}