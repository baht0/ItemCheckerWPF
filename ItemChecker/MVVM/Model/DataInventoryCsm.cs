using System;
using System.Collections.Generic;

namespace ItemChecker.MVVM.Model
{
    public class DataInventoryCsm
    {
        public string ItemName { get; set; }
        public int StackSize { get; set; }
        public int Id { get; set; }
        public decimal DefaultPrice { get; set; }
        public decimal Price { get; set; }
        public bool Sticker { get; set; }
        public bool NameTag { get; set; }
        public bool User { get; set; }
        public DateTime TradeLock { get; set; }
        public bool RareItem { get; set; }

        public static List<DataInventoryCsm> Inventory { get; set; } = new();
    }
}
