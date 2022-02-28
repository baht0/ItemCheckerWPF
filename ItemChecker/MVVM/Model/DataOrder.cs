using System.Collections.Generic;

namespace ItemChecker.MVVM.Model
{
    public class DataOrder
    {
        public string Type { get; set; }
        public string ItemName { get; set; }
        public string OrderId { get; set; }
        public decimal StmPrice { get; set; }
        public decimal OrderPrice { get; set; }
        public decimal ServicePrice { get; set; }
        public decimal ServiceGive { get; set; }
        public decimal Precent { get; set; }
        public decimal Difference { get; set; }
        //my orders
        public static List<DataOrder> Orders { get; set; } = new();

        public DataOrder(string Type, string ItemName, string OrderId, decimal StmPrice, decimal OrderPrice, decimal ServicePrice, decimal ServiceGive, decimal Precent, decimal Difference)
        {
            this.Type = Type;
            this.ItemName = ItemName;
            this.OrderId = OrderId;
            this.StmPrice = StmPrice;
            this.OrderPrice = OrderPrice;
            this.ServicePrice = ServicePrice;
            this.ServiceGive = ServiceGive;
            this.Precent = Precent;
            this.Difference = Difference;
        }
    }
}
