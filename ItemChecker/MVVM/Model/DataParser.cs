using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ItemChecker.MVVM.Model
{
    public class DataParser
    {
        public string ItemType { get; set; }
        public string ItemName { get; set; }
        public decimal Price1 { get; set; }
        public decimal Price2 { get; set; }
        public decimal Price3 { get; set; }
        public decimal Price4 { get; set; }
        public decimal Precent { get; set; }
        public decimal Difference { get; set; }
        public string Status { get; set; }
        public bool Have { get; set; }

        public DataParser(string ItemType, string ItemName, decimal Price1, decimal Price2, decimal Price3, decimal Price4, decimal Precent, decimal Difference, string Status, bool Have)
        {
            this.ItemType = ItemType;
            this.ItemName = ItemName;
            this.Price1 = Price1;
            this.Price2 = Price2;
            this.Price3 = Price3;
            this.Price4 = Price4;
            this.Precent = Precent;
            this.Difference = Difference;
            this.Status = Status;
            this.Have = Have;
        }
    }
}
