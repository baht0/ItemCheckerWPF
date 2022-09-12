using System;
using System.Collections.Generic;

namespace ItemChecker.MVVM.Model
{
    public class SteamBase
    {
        public static DateTime Updated { get; set; } = new();
        public static List<Currency> CurrencyList { get; set; } = new();
        public static List<Item> ItemList { get; set; } = new();
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
        public SteamItem Steam { get; set; }
        public CsmItem Csm { get; set; } = new();
        public LfmItem Lfm { get; set; } = new();
        public BuffItem Buff { get; set; } = new();
    }
    public class SteamItem
    {
        public DateTime Updated { get; set; } = DateTime.Now.AddHours(-1);
        public int Id { get; set; }
        public decimal AvgPrice { get; set; }
        public decimal LowestSellOrder { get; set; }
        public decimal HighestBuyOrder { get; set; }
        public bool IsHave { get; set; }
        public List<SaleHistory> History { get; set; } = new();
    }
    public class CsmItem
    {
        public DateTime Updated { get; set; } = DateTime.Now.AddHours(-1);
        public int Id { get; set; }
        public decimal Price { get; set; }
        public int OverstockDifference { get; set; }
        public bool Overstock { get; set; } = true;
        public bool Unavailable { get; set; } = true;
        public bool IsHave { get; set; }
        public List<InventoryCsm> Inventory { get; set; } = new();
    }
    public class LfmItem
    {
        public DateTime Updated { get; set; } = DateTime.Now.AddHours(-1);
        public decimal Price { get; set; }
        public int Have { get; set; }
        public int Limit { get; set; }
        public int Reservable { get; set; }
        public int Tradable { get; set; }
        public int SteamPriceRate { get; set; }
        public bool Overstock { get; set; } = true;
        public bool Unavailable { get; set; } = true;
        public bool IsHave { get; set; }
    }
    public class BuffItem
    {
        public DateTime Updated { get; set; } = DateTime.Now.AddHours(-1);
        public int Id { get; set; }
        public decimal Price { get; set; }
        public int Count { get; set; }
        public decimal BuyOrder { get; set; }
        public int OrderCount { get; set; }
        public bool IsHave { get; set; }
        public List<SaleHistory> History { get; set; } = new();
    }

    //History
    public class SaleHistory
    {
        public DateTime Date { get; set; } = new();
        public decimal Price { get; set; }
        public int Count { get; set; }

        public SaleHistory(DateTime date, decimal price, int count)
        {
            Date = date;
            Price = price;
            Count = count;
        }
    }
    //csm
    public class InventoryCsm
    {
        public DateTime Updated { get; set; } = DateTime.Now.AddHours(-1);
        public int NameId { get; set; }
        public int StackSize { get; set; }
        public decimal Price { get; set; }
        public bool Sticker { get; set; }
        public decimal Float { get; set; }
        public bool User { get; set; }
        public DateTime TradeLock { get; set; } = new();
        public bool RareItem { get; set; }
    }
}
