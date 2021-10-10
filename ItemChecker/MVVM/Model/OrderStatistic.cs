namespace ItemChecker.MVVM.Model
{
    public class OrderStatistic
    {
        public static string CurrentService { get; set; } = "Unknown";
        public static bool PushService { get; set; }
        public static bool CsmService { get; set; }
        public static bool FloatService { get; set; }

        public static int Check { get; set; }
        public static int Push { get; set; }
        public static int Cancel { get; set; }
        public static int SuccessfulTrades { get; set; }
        public static int PurchasesMade { get; set; }
    }
}
