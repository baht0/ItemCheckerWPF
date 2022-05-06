using System;
using System.Collections.Generic;

namespace ItemChecker.MVVM.Model
{
    public class ItemBase
    {
        public string ItemName { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Quality { get; set; } = string.Empty;
        public Steam SteamInfo { get; set; } = new();
        public Csm CsmInfo { get; set; } = new();
        public Lfm LfmInfo { get; set; } = new();
        public Buff BuffInfo { get; set; } = new();

        public static List<ItemBase> SkinsBase { get; set; } = new();
    }
    public class Steam
    {
        public int Id { get; set; }
        public decimal Price { get; set; }
    }
    public class Csm
    {
        public DateTime Updated { get; set; } = DateTime.MinValue;
        public int Id { get; set; }
        public decimal Price { get; set; }
        public int OverstockDifference { get; set; }
        public bool Overstock { get; set; }
        public bool Unavailable { get; set; }
    }
    public class Lfm
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
    }
    public class Buff
    {
        public DateTime Updated { get; set; } = DateTime.MinValue;
        public int Id { get; set; }
        public decimal Price { get; set; }
        public decimal BuyOrder { get; set; }
        public int Count { get; set; }
        public int OrderCount { get; set; }
    }
}
