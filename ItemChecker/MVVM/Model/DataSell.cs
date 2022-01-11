using System.Collections.Generic;

namespace ItemChecker.MVVM.Model
{
    public class DataSell
    {
        public string ItemName { get; set; }
        public string AssetId { get; set; }
        public decimal Price { get; set; }

        public static List<DataSell> SellItems { get; set; } = new();
    }
}
