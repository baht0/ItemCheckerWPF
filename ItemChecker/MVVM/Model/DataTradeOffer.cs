using System.Collections.Generic;

namespace ItemChecker.MVVM.Model
{
    public class DataTradeOffer
    {
        public string TradeOfferId { get; set; }
        public string PartnerId { get; set; }

        public static List<DataTradeOffer> TradeOffers = new();
    }
}