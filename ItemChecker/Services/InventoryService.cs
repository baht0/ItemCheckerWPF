using ItemChecker.MVVM.Model;
using ItemChecker.Net;
using ItemChecker.Properties;
using ItemChecker.Support;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ItemChecker.Services
{
    public class InventoryService : BaseService
    {
        public List<DataInventory> CheckInventory()
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
                bool marketable = (int)item["marketable"] != 0;
                bool tradable = (int)item["tradable"] != 0;
                bool nameTag = item.ContainsKey("fraudwarnings");
                string descriptions = item["descriptions"].ToString();
                bool stickers = descriptions.Contains("sticker_info");

                if (marketable)
                {
                    items.Add(new()
                    {
                        ItemName = name,
                        AssetId = assetid,
                        ClassId = classid,
                        InstanceId = instanceid,
                        Tradable = tradable,
                        Marketable = marketable,
                        Stickers = stickers,
                        NameTag = nameTag,
                    });
                }
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
        public void SellItem(DataInventory item)
        {
            if (!item.Marketable || item.Stickers || item.NameTag)
                return;

            if (item.SellPrice <= 0)
                item.SellPrice = SetSellPrice(item.ItemName);

            int sell_price = (int)(item.SellPrice * Calculator.CommissionSteam);

            Post.SellItem(SteamAccount.Cookies, SteamAccount.UserName, item.AssetId, sell_price);
        }

        public void AcceptTrade(string tradeOfferId, string partnerId)
        {
            Thread.Sleep(1000);
            Post.AcceptTrade(SteamAccount.Cookies, tradeOfferId, partnerId);
        }
        Decimal SetSellPrice(string itemName)
        {
            decimal lowest_price = 0;
            decimal median_price = 0;
            bool check = true;
            do
            {
                try
                {
                    Tuple<Decimal, Decimal> prices = Get.PriceOverview(Edit.EncodeMarketHashName(itemName));
                    lowest_price = prices.Item1;
                    median_price = prices.Item2;
                    check = false;
                }
                catch
                {
                    Thread.Sleep(10000);
                }
            } while (check);

            if (lowest_price > HomeProperties.Default.MaxPrice || lowest_price == 0)
                return -1;

            decimal sellPrice = HomeProperties.Default.SellingPriceId == 0 ? lowest_price : median_price;
            return sellPrice * 100 - 0.01m;
        }
    }
}