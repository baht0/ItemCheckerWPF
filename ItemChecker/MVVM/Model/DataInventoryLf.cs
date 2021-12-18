using System.Collections.Generic;

namespace ItemChecker.MVVM.Model
{
    public class DataInventoryLf
    {
        public string ItemName { get; set; }
        public decimal DefaultPrice { get; set; }
        public int Have { get; set; }
        public int Limit { get; set; }
        public int Reservable { get; set; }
        public int Tradable { get; set; }
        public int SteamPriceRate { get; set; }
        public bool IsOverstock { get; set; }
        public static List<DataInventoryLf> Inventory { get; set; } = new();
    }
}
