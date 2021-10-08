using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ItemChecker.MVVM.Model
{
    public class Parser
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
        public bool Queue { get; set; }

        public ObservableCollection<string> Services { get; set; }
        public int ServiceOne { get; set; }
        public int ServiceTwo { get; set; }

        //Tryskins
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
        public decimal MinPrecent { get; set; }
        public decimal MaxPrecent { get; set; }
        public int SteamSales { get; set; }
        public string NameContains { get; set; }
        public bool Knife { get; set; }
        public bool Stattrak { get; set; }
        public bool Souvenir { get; set; }
        public bool Sticker { get; set; }
    }
}        