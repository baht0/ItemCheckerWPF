using ItemChecker.Properties;
using System.Collections.ObjectModel;
using System.Threading;

namespace ItemChecker.MVVM.Model
{
    public class HomeConfig
    {
        //pusher
        public int PushTimer { get; set; } = HomeProperties.Default.TimerPush;
        public static System.Timers.Timer TimerPush { get; set; } = new(1000);
        public static int TimerPushTick { get; set; }
        public static CancellationTokenSource ctsPush { get; set; } = new();
        public static CancellationToken tokenPush { get; set; } = ctsPush.Token;

        //cs money
        public int CsmTimer { get; set; } = HomeProperties.Default.TimerCsm;
        public decimal MaxDeviation { get; set; } = HomeProperties.Default.MaxDeviation;
        public bool UserItems { get; set; } = HomeProperties.Default.UserItems;
        public static System.Timers.Timer TimerCsm { get; set; } = new(1000);
        public static int TimerCsmTick { get; set; }
        public static CancellationTokenSource ctsCsm { get; set; } = new();
        public static CancellationToken tokenCsm { get; set; } = ctsCsm.Token;

        //float
        public int FloatTimer { get; set; } = HomeProperties.Default.TimerFloat;
        public int Compare { get; set; } = HomeProperties.Default.Compare;
        public ObservableCollection<string> ComparePrices { get; set; } = new ObservableCollection<string>()
                {
                    "Lowest ST",
                    "Median ST",
                    "Buy CSM"
                };
        public decimal MaxPrecent { get; set; } = HomeProperties.Default.MaxPrecent;
        public static System.Timers.Timer TimerFloat { get; set; } = new(1000);
        public static int TimerFloatTick { get; set; }
        public static CancellationTokenSource ctsFloat { get; set; } = new();
        public static CancellationToken tokenFloat { get; set; } = ctsFloat.Token;

        //trade
        public static CancellationTokenSource ctsTrade { get; set; } = new();
        public static CancellationToken tokenTrade { get; set; } = ctsTrade.Token;
        //sale
        public int MaxPrice { get; set; } = HomeProperties.Default.MaxPrice;
        public static CancellationTokenSource ctsSale { get; set; } = new();
        public static CancellationToken tokenSale { get; set; } = ctsSale.Token;
        //withdraw
        public static CancellationTokenSource ctsWithdraw { get; set; } = new();
        public static CancellationToken tokenWithdraw { get; set; } = ctsWithdraw.Token;
    }
}
