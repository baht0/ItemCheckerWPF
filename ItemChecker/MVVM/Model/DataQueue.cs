using System.Collections.ObjectModel;

namespace ItemChecker.MVVM.Model
{
    public class DataQueue
    {
        public string ItemName { get; set; }
        public decimal OrderPrice { get; set; }

        public DataQueue(string itemName, decimal orderPrice)
        {
            ItemName = itemName;
            OrderPrice = orderPrice;
        }
    }
}
