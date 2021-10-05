using System.Collections.Generic;

namespace ItemChecker.MVVM.Model
{
    public class QuickSell
    {
        public string AssetId { get; set; }
        public string ItemName { get; set; }
        public decimal Price { get; set; }

        public static List<QuickSell> SellItems = new();
    }
}
