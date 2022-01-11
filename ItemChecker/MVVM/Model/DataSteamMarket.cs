using System;
using System.Collections.Generic;

namespace ItemChecker.MVVM.Model
{
    public class DataSteamMarket
    {
        public string ItemName { get; set; } = "Unknown";
        public decimal LowestSellOrder { get; set; } = 0;
        public decimal HighestBuyOrder { get; set; } = 0;
        public List<PriceHistory> PriceHistory { get; set; } = new();
        public Tuple<DateTime, int, decimal> LastSale { get; set; } = new(new(), 0, 0m);
        public decimal Avg30 { get; set; } = 0;
        public int Count30 { get; set; } = 0;
        public decimal Avg60 { get; set; } = 0;
        public int Count60 { get; set; } = 0;

        public static List<DataSteamMarket> MarketItems { get; set; } = new();
    }
    public class PriceHistory
    {
        public DateTime Date { get; set; } = new();
        public decimal Price { get; set; } = 0;
        public int Count { get; set; } = 0;
    }
}