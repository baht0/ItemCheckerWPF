using System.Collections.Generic;

namespace ItemChecker.MVVM.Model
{
    public class TradeOffer
    {
        public string TradeOfferId { get; set; }
        public string PartnerId { get; set; }

        public static List<TradeOffer> TradeOffers = new();
    }
}