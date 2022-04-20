using System.Collections.Generic;

namespace ItemChecker.MVVM.Model
{
    public class DataOrder
    {
        public string Type { get; set; }
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
    }
    public class DataFloat
    {
        public decimal LowestPrice { get; set; }
        public decimal MedianPrice { get; set; }
        public decimal CsmPrice { get; set; }
        public decimal Precent { get; set; }
        public decimal FloatValue { get; set; }
        public decimal MaxFloat { get; set; }
        public decimal PriceCompare { get; set; }
    }
    public class DataTradeOffer
    {
        public string TradeOfferId { get; set; }
        public string PartnerId { get; set; }

        public static List<DataTradeOffer> TradeOffers { get; set; } = new();
    }
    public class DataSell
    {
        public string ItemName { get; set; }
        public string AssetId { get; set; }
        public decimal Price { get; set; }

        public static List<DataSell> SellItems { get; set; } = new();
    }
}
