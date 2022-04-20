using ItemChecker.MVVM.Model;
using ItemChecker.Net;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Threading;

namespace ItemChecker.Services
{
    public class TradeOfferService : BaseService
    {
        public Boolean checkOffer()
        {
            try
            {
                DataTradeOffer.TradeOffers = new();
                JObject json = Get.TradeOffers(SteamAccount.ApiKey);
                JArray trades = (JArray)json["response"]["trade_offers_received"];
                foreach (var trade in trades)
                {
                    var trade_status = trade["trade_offer_state"].ToString();
                    if (trade_status == "2")
                    {
                        DataTradeOffer.TradeOffers.Add(new() {
                            TradeOfferId = trade["tradeofferid"].ToString(),
                            PartnerId = trade["accountid_other"].ToString()
                        });
                    }
                    else
                        continue;
                }
                return DataTradeOffer.TradeOffers.Any();
            }
            catch
            {
                return false;
            }
        }
        public void acceptTrade(string tradeOfferId, string partnerId)
        {
            Thread.Sleep(1000);
            Post.AcceptTrade(SteamCookies, tradeOfferId, partnerId);
        }
    }
}
