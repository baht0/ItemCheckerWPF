using System;
using System.Collections.Generic;

namespace ItemChecker.MVVM.Model
{
    public class DataInventoriesCsm
    {
        public string ItemName { get; set; } = "Unknown";
        public int NameId { get; set; } = 0;
        public int StackSize { get; set; } = 0;
        public decimal Price { get; set; } = 0;
        public decimal DefaultPrice { get; set; } = 0;
        public bool Sticker { get; set; } = false;
        public decimal Float { get; set; } = 0;
        public bool User { get; set; } = false;
        public DateTime TradeLock { get; set; } = new();
        public bool RareItem { get; set; } = false;

        public static List<DataInventoriesCsm> Items { get; set; } = new();
    }
}
