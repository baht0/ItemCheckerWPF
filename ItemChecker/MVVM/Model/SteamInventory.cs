using System;
using System.Collections.Generic;

namespace ItemChecker.MVVM.Model
{
    public class DataTradeOffer
    {
        public string TradeOfferId { get; set; }
        public string PartnerId { get; set; }

        public static List<DataTradeOffer> Offers { get; set; } = new();
    }
    public class DataInventory
    {
        public string ItemName { get; set; } = "Unknown";
        public List<DataInventoryItem> Data { get; set; } = new();
    }
    public class DataInventoryItem
    {
        public string AssetId { get; set; } = string.Empty;
        public string ClassId { get; set; } = string.Empty;
        public string InstanceId { get; set; } = string.Empty;
        public DateTime TradeLock { get; set; } = new();
        public bool Tradable { get; set; }
        public bool Marketable { get; set; }
        public bool Stickers { get; set; }
        public bool NameTag { get; set; }
    }
}
