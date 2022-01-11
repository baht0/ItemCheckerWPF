using System.Collections.Generic;

namespace ItemChecker.MVVM.Model
{
    public class ItemBase
    {
        public string ItemName { get; set; }
        public int SteamId { get; set; }
        public int CsmId { get; set; }
        public decimal PriceSteam { get; set; }
        public decimal PriceCsm { get; set; }
        public int OverstockDifference { get; set; }

        public static List<ItemBase> Overstock { get; set; } = new();
        public static List<ItemBase> Unavailable { get; set; } = new();
        public static List<ItemBase> SkinsBase { get; set; } = new();
    }
}
