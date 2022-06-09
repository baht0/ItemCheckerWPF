using System;
using System.Collections.Generic;

namespace ItemChecker.MVVM.Model
{
    public class DataBuyItem
    {
        public string ListingId { get; set; } = "0";
        public decimal Fee { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Total { get; set; }
    }
    public class DataRare
    {
        public int ParameterId { get; set; }
        public int CompareId { get; set; }
        public string ItemName { get; set; } = "Unknown";
        public decimal Price { get; set; }
        public decimal PriceCompare { get; set; }
        public decimal Precent { get; set; }
        public decimal Difference { get; set; }
        public string Link { get; set; }

        public decimal FloatValue { get; set; }
        public List<string> Stickers { get; set; } = new();
        public string Phase { get; set; } = "-";
        public DateTime Checked { get; set; } = DateTime.Now;

        public DataBuyItem DataBuy { get; set; } = new();
    }
}
