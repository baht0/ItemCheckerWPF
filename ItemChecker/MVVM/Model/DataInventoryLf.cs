using System.Collections.Generic;

namespace ItemChecker.MVVM.Model
{
    public class DataInventoryLf
    {
        public string ItemName { get; set; } = "Unknown";
        public decimal DefaultPrice { get; set; } = 0;
        public int Have { get; set; } = 0;
        public int Limit { get; set; } = 0;
        public int Reservable { get; set; } = 0;
        public int Tradable { get; set; } = 0;
        public int SteamPriceRate { get; set; } = 0;
        public bool IsOverstock { get; set; } = false;

        public static List<DataInventoryLf> Inventory { get; set; } = new();
    }
}
