using System.Collections.Generic;

namespace ItemChecker.MVVM.Model
{
    public class ItemBase
    {
        public string ItemName { get; set; }
        public string Type { get; set; }
        public string Quality { get; set; }
        public Steam SteamInfo { get; set; }
        public Csm CsmInfo { get; set; }
        public Lfm LfmInfo { get; set; }

        public static List<ItemBase> SkinsBase { get; set; } = new();
    }
    public class Steam
    {
        public int Id { get; set; }
        public decimal Price { get; set; }
    }
    public class Csm
    {
        public int Id { get; set; }
        public decimal Price { get; set; }
        public int OverstockDifference { get; set; }
        public bool Overstock { get; set; }
        public bool Unavailable { get; set; }
    }
    public class Lfm
    {
        public decimal Price { get; set; }
        public int Have { get; set; }
        public int Limit { get; set; }
        public int Reservable { get; set; }
        public int Tradable { get; set; }
        public int SteamPriceRate { get; set; }
        public bool Overstock { get; set; }
        public bool Unavailable { get; set; }
    }
}
