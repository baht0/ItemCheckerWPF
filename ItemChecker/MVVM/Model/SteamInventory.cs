using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ItemChecker.MVVM.Model
{
    internal class SteamInventory
    {

    }
    public class DataTradeOffer
    {
        public string TradeOfferId { get; set; }
        public string PartnerId { get; set; }

        public static List<DataTradeOffer> Offers { get; set; } = new();
    }
    public class DataInventory
    {
        public string ItemName { get; set; } = "Unknown";
        public string AssetId { get; set; } = string.Empty;
        public string ClassId { get; set; } = string.Empty;
        public string InstanceId { get; set; } = string.Empty;
        public DateTime TradeLock { get; set; } = new();
        public bool Tradable { get; set; }
        public bool Marketable { get; set; }
        public bool Stickers { get; set; }
        public bool NameTag { get; set; }
        public decimal LowestSellOrder { get; set; }
        public decimal HighestBuyOrder { get; set; }
    }
}
