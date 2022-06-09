namespace ItemChecker.MVVM.Model
{
    public class DataParser
    {
        public string ItemName { get; set; }
        public decimal Price1 { get; set; }
        public decimal Price2 { get; set; }
        public decimal Price3 { get; set; }
        public decimal Price4 { get; set; }
        public decimal Precent { get; set; }
        public decimal Difference { get; set; }
        public bool Have { get; set; }
    }
    public class DataQueue
    {
        public string ItemName { get; set; }
        public decimal OrderPrice { get; set; }
        public decimal Precent { get; set; }
    }
}