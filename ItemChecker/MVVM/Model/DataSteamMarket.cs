using System.Collections.Generic;

namespace ItemChecker.MVVM.Model
{
    public class DataSteamMarket
    {
        public string ItemName { get; set; }
        public decimal HighestBuyOrder { get; set; }
        public decimal LowestSellOrder { get; set; }
        public List<decimal> BuyOrderGraph { get; set; } = new();
        public List<decimal> SellOrderGraph { get; set; } = new();

        public static List<DataSteamMarket> MarketItems { get; set; } = new();
    }
}