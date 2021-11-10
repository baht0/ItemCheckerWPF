using ItemChecker.MVVM.Model;
using ItemChecker.Net;
using ItemChecker.Properties;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium.Support.Extensions;
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
                TradeOffer.TradeOffers = new();
                string json = Get.TradeOffers(Account.ApiKey);
                JArray trades = (JArray)JObject.Parse(json)["response"]["trade_offers_received"];
                foreach (var trade in trades)
                {
                    var trade_status = trade["trade_offer_state"].ToString();
                    if (trade_status == "2")
                    {
                        TradeOffer.TradeOffers.Add(new TradeOffer()
                        {
                            TradeOfferId = trade["tradeofferid"].ToString(),
                            PartnerId = trade["accountid_other"].ToString()
                        });
                    }
                    else
                        continue;
                }

                return TradeOffer.TradeOffers.Any();
            }
            catch
            {
                return false;
            }
        }
        public void acceptTrade(string tradeOfferId, string partnerId)
        {
            Browser.Navigate().GoToUrl("https://steamcommunity.com/tradeoffer/" + tradeOfferId);
            Thread.Sleep(500);
            Browser.ExecuteJavaScript(Post.AcceptTrade(tradeOfferId, partnerId, Account.SessionId));
        }
    }
}
