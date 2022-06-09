using System.Collections.Generic;

namespace ItemChecker.MVVM.Model
{
    public class DataOrder
    {
        public string ItemName { get; set; }
        public string OrderId { get; set; }
        public decimal StmPrice { get; set; }
        public decimal OrderPrice { get; set; }
        public decimal ServicePrice { get; set; }
        public decimal ServiceGive { get; set; }
        public decimal Precent { get; set; }
        public decimal Difference { get; set; }

        public static List<DataOrder> Orders { get; set; } = new();
    }
    public class DataCsmCheck
    {
        public string ItemName { get; set; }
        public decimal StmPrice { get; set; }
        public decimal CsmPrice { get; set; }
        public decimal Precent { get; set; }

        public static List<DataCsmCheck> Items { get; set; } = new();
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
        public string AssetId { get; set; }
        public string ClassId { get; set; }
        public string InstanceId { get; set; }
        public decimal SellPrice { get; set; }
        public bool Tradable { get; set; }
        public bool Marketable { get; set; }
        public bool Stickers { get; set; }
        public bool NameTag { get; set; }
        public decimal LowestSellOrder { get; set; }
        public decimal HighestBuyOrder { get; set; }
        public decimal CsmGive { get; set; }
        public decimal LfmGive { get; set; }
        public decimal PriceBuff { get; set; }
        public decimal BuyOrderBuff { get; set; }
    }
}
