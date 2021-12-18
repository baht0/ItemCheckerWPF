using System.Collections.ObjectModel;

namespace ItemChecker.MVVM.Model
{
    public class ParserList
    {
        public static string Price1 { get; set; } = "Price1";
        public static string Price2 { get; set; } = "Price2";
        public static string Price3 { get; set; } = "Price3";
        public static string Price4 { get; set; } = "Price4";

        public static string Mode { get; set; } = "Unknown";
        public static string Service1 { get; set; } = "Service1";
        public static string Service2 { get; set; } = "Service2";
        public static string DataCurrency { get; set; } = "Unknown";

        //mode
        public bool Tryskins { get; set; }
        public bool Manual { get; set; }

        public ObservableCollection<string> Services { get; set; } = new()
        {
            "SteamMarket",
            "Cs.Money",
            "Loot.Farm"
        };
        public int ServiceOne { get; set; }
        public int ServiceTwo { get; set; }

        //Tryskins
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
        public decimal MinPrecent { get; set; }
        public decimal MaxPrecent { get; set; }
        public int SteamSales { get; set; }
        public string NameContains { get; set; }
        public bool KnifeTS { get; set; }
        public bool StattrakTS { get; set; }
        public bool SouvenirTS { get; set; }
        public bool StickerTS { get; set; }
        //manual
        public bool SouvenirM { get; set; }
        public bool StattrakM { get; set; }
        public bool KnifeGloveM { get; set; }
        public bool KnifeGloveStattrakM { get; set; }
        public bool OverstockM { get; set; }
    }
}        