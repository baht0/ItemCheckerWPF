namespace ItemChecker.MVVM.Model
{
    public class OrderData
    {
        public string Type { get; set; }
        public string ItemName { get; set; }
        public string OrderId { get; set; }
        public decimal StmPrice { get; set; }
        public decimal OrderPrice { get; set; }
        public decimal CsmPrice { get; set; }
        public decimal CsmBuy { get; set; }
        public decimal Precent { get; set; }
        public decimal Difference { get; set; }

        public OrderData(string Type, string ItemName, string OrderId, decimal StmPrice, decimal OrderPrice, decimal CsmPrice, decimal CsmBuy, decimal Precent, decimal Difference)
        {
            this.Type = Type;
            this.ItemName = ItemName;
            this.OrderId = OrderId;
            this.StmPrice = StmPrice;
            this.OrderPrice = OrderPrice;
            this.CsmPrice = CsmPrice;
            this.CsmBuy = CsmBuy;
            this.Precent = Precent;
            this.Difference = Difference;
        }
    }
}
