using System;
using System.Collections.Generic;

namespace ItemChecker.MVVM.Model
{
    public class SteamBase
    {
        public static List<Currency> CurrencyList { get; set; } = new();
        public static List<Item> ItemList { get; set; } = new();

        public static string StatusCommunity { get; set; } = string.Empty;
    }
    public class Currency
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string Symbol { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal Value { get; set; }
    }
    public class Item
    {
        public string ItemName { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Quality { get; set; } = string.Empty;
        public SteamInfo Steam { get; set; } = new();
        public CsmInfo Csm { get; set; } = new();
        public LfmInfo Lfm { get; set; } = new();
        public BuffInfo Buff { get; set; } = new();
    }
    public class SteamInfo
    {
        public DateTime Updated { get; set; } = DateTime.MinValue;
        public int Id { get; set; }
        public decimal AvgPrice { get; set; }
        public decimal LowestSellOrder { get; set; }
        public decimal HighestBuyOrder { get; set; }

        //history
        public List<PriceHistory> PriceHistory { get; set; } = new();
        public Tuple<DateTime, int, decimal> LastSale { get; set; } = new(new(), 0, 0m);
        public decimal Avg30 { get; set; }
        public int Count30 { get; set; }
        public decimal Avg60 { get; set; }
        public int Count60 { get; set; }
    }
    public class CsmInfo
    {
        public DateTime Updated { get; set; } = DateTime.MinValue;
        public int Id { get; set; }
        public decimal Price { get; set; }
        public int OverstockDifference { get; set; }
        public bool Overstock { get; set; }
        public bool Unavailable { get; set; }
        public bool IsHave { get; set; }
    }
    public class LfmInfo
    {
        public DateTime Updated { get; set; } = DateTime.MinValue;
        public decimal Price { get; set; }
        public int Have { get; set; }
        public int Limit { get; set; }
        public int Reservable { get; set; }
        public int Tradable { get; set; }
        public int SteamPriceRate { get; set; }
        public bool Overstock { get; set; }
        public bool Unavailable { get; set; }
        public bool IsHave { get; set; }
    }
    public class BuffInfo
    {
        public DateTime Updated { get; set; } = DateTime.MinValue;
        public int Id { get; set; }
        public decimal Price { get; set; }
        public int Count { get; set; }
        public decimal BuyOrder { get; set; }
        public int OrderCount { get; set; }
        public bool IsHave { get; set; }
    }

    //steamMarket
    public class PriceHistory
    {
        public DateTime Date { get; set; } = new();
        public decimal Price { get; set; }
        public int Count { get; set; }
    }
}
