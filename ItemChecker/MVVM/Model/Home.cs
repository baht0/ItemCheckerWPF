using System.Collections.ObjectModel;

namespace ItemChecker.MVVM.Model
{
    public class Home : BaseModel
    {
        //services
        public int PushTimer { get; set; }

        public int CsmTimer { get; set; }
        public decimal MaxDeviation { get; set; }
        public bool UserItems { get; set; }

        public int FloatTimer { get; set; }
        public int Compare { get; set; }
        public ObservableCollection<string> ComparePrices { get; set; } = new ObservableCollection<string>()
                {
                    "Lowest ST",
                    "Median ST",
                    "Buy CSM"
                };
        public decimal MaxPrecent { get; set; }
        public int MaxPrice { get; set; }

        //statistic
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
