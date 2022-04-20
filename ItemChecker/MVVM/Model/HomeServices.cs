using ItemChecker.Properties;
using ItemChecker.Services;
using ItemChecker.Core;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;

namespace ItemChecker.MVVM.Model
{
    //services
    public class HomePush : ObservableObject
    {
        public static System.Timers.Timer Timer { get; set; } = new(1000);
        public static int TimerTick { get; set; }
        public static CancellationTokenSource cts { get; set; } = new();
        public static CancellationToken token { get; set; } = cts.Token;

        public int Time { get; set; } = HomeProperties.Default.TimePush;
        public int Reserve { get; set; } = HomeProperties.Default.Reserve;
        public bool Unwanted { get; set; } = HomeProperties.Default.Unwanted;

        private bool _isService;
        private int _check = 0;
        private int _push = 0;
        private int _progress = 0;
        private int _maxProgress = 0;
        private string _status = "Off";

        public bool IsService
        {
            get
            {
                return _isService;
            }
            set
            {
                _isService = value;
                OnPropertyChanged();
            }
        }
        public int Check
        {
            get
            {
                return _check;
            }
            set
            {
                _check = value;
                OnPropertyChanged();
            }
        }
        public int Push
        {
            get
            {
                return _push;
            }
            set
            {
                _push = value;
                OnPropertyChanged();
            }
        }
        public int Progress
        {
            get { return _progress; }
            set
            {
                _progress = value;
                OnPropertyChanged();
            }
        }
        public int MaxProgress
        {
            get { return _maxProgress; }
            set
            {
                _maxProgress = value;
                OnPropertyChanged();
            }
        }
        public string Status
        {
            get { return _status; }
            set
            {
                _status = value;
                OnPropertyChanged();
            }
        }
    }
    public class HomeCsmCheck : ObservableObject
    {
        public static System.Timers.Timer Timer { get; set; } = new(1000);
        public static int TimerTick { get; set; }
        public static CancellationTokenSource cts { get; set; } = new();
        public static CancellationToken token { get; set; } = cts.Token;

        public int Time { get; set; } = HomeProperties.Default.TimeCsm;
        public static List<string> List { get; set; } = BaseService.ReadList("CsmList");
        public int MinPrecent { get; set; } = HomeProperties.Default.MinPrecent;

        private bool _isService;
        private int _ListCount = List.Count;
        private int _check = 0;
        private int _successfulTrades = 0;
        private int _progress = 0;
        private int _maxProgress = 0;
        private string _status = "Off";

        public bool IsService
        {
            get
            {
                return _isService;
            }
            set
            {
                _isService = value;
                OnPropertyChanged();
            }
        }
        public int ListCount
        {
            get
            {
                return _ListCount;
            }
            set
            {
                _ListCount = value;
                OnPropertyChanged();
            }
        }
        public int Check
        {
            get
            {
                return _check;
            }
            set
            {
                _check = value;
                OnPropertyChanged();
            }
        }
        public int SuccessfulTrades
        {
            get
            {
                return _successfulTrades;
            }
            set
            {
                _successfulTrades = value;
                OnPropertyChanged();
            }
        }
        public int Progress
        {
            get { return _progress; }
            set
            {
                _progress = value;
                OnPropertyChanged();
            }
        }
        public int MaxProgress
        {
            get { return _maxProgress; }
            set
            {
                _maxProgress = value;
                OnPropertyChanged();
            }
        }
        public string Status
        {
            get { return _status; }
            set
            {
                _status = value;
                OnPropertyChanged();
            }
        }
    }
    public class HomeFloatCheck : ObservableObject
    {
        public static System.Timers.Timer Timer { get; set; } = new(1000);
        public static int TimerTick { get; set; }
        public static CancellationTokenSource cts { get; set; } = new();
        public static CancellationToken token { get; set; } = cts.Token;

        public int Time { get; set; } = HomeProperties.Default.TimeCsm;
        public static List<string> List { get; set; } = BaseService.ReadList("FloatList");
        public int MaxPrecent { get; set; } = HomeProperties.Default.MaxPrecent;
        public int Compare { get; set; } = HomeProperties.Default.Compare;
        public ObservableCollection<string> ComparePrices { get; set; } = new()
                {
                    "Lowest ST",
                    "Median ST",
                    "Buy CSM"
                };

        private bool _isService;
        private int _ListCount = List.Count;
        private int _check = 0;
        private int _purchasesMade = 0;
        private int _progress = 0;
        private int _maxProgress = 0;
        private string _status = "Off";

        public bool IsService
        {
            get
            {
                return _isService;
            }
            set
            {
                _isService = value;
                OnPropertyChanged();
            }
        }
        public int ListCount
        {
            get
            {
                return _ListCount;
            }
            set
            {
                _ListCount = value;
                OnPropertyChanged();
            }
        }
        public int Check
        {
            get
            {
                return _check;
            }
            set
            {
                _check = value;
                OnPropertyChanged();
            }
        }
        public int PurchasesMade
        {
            get
            {
                return _purchasesMade;
            }
            set
            {
                _purchasesMade = value;
                OnPropertyChanged();
            }
        }
        public int Progress
        {
            get { return _progress; }
            set
            {
                _progress = value;
                OnPropertyChanged();
            }
        }
        public int MaxProgress
        {
            get { return _maxProgress; }
            set
            {
                _maxProgress = value;
                OnPropertyChanged();
            }
        }
        public string Status
        {
            get { return _status; }
            set
            {
                _status = value;
                OnPropertyChanged();
            }
        }
    }
    //tools
    public class HomeWithdraw : ObservableObject
    {
        public static CancellationTokenSource cts { get; set; } = new();
        public static CancellationToken token { get; set; } = cts.Token;

        private bool _isService;
        private int _count = 0;
        private int _progress = 0;
        private int _maxProgress = 0;

        public bool IsService
        {
            get
            {
                return _isService;
            }
            set
            {
                _isService = value;
                OnPropertyChanged();
            }
        }
        public int Count
        {
            get
            {
                return _count;
            }
            set
            {
                _count = value;
                OnPropertyChanged();
            }
        }
        public int Progress
        {
            get { return _progress; }
            set
            {
                _progress = value;
                OnPropertyChanged();
            }
        }
        public int MaxProgress
        {
            get { return _maxProgress; }
            set
            {
                _maxProgress = value;
                OnPropertyChanged();
            }
        }
    }
    public class HomeTrade : ObservableObject
    {
        public static CancellationTokenSource cts { get; set; } = new();
        public static CancellationToken token { get; set; } = cts.Token;

        private bool _isService;
        private int _count = 0;
        private int _progress = 0;
        private int _maxProgress = 0;

        public bool IsService
        {
            get
            {
                return _isService;
            }
            set
            {
                _isService = value;
                OnPropertyChanged();
            }
        }
        public int Count
        {
            get
            {
                return _count;
            }
            set
            {
                _count = value;
                OnPropertyChanged();
            }
        }
        public int Progress
        {
            get { return _progress; }
            set
            {
                _progress = value;
                OnPropertyChanged();
            }
        }
        public int MaxProgress
        {
            get { return _maxProgress; }
            set
            {
                _maxProgress = value;
                OnPropertyChanged();
            }
        }
    }
    public class HomeSell : ObservableObject
    {
        public int MaxPrice { get; set; } = HomeProperties.Default.MaxPrice;
        public static CancellationTokenSource cts { get; set; } = new();
        public static CancellationToken token { get; set; } = cts.Token;

        private bool _isService;
        private int _count = 0;
        private decimal _sum = 0;
        private int _progress = 0;
        private int _maxProgress = 0;

        public bool IsService
        {
            get
            {
                return _isService;
            }
            set
            {
                _isService = value;
                OnPropertyChanged();
            }
        }
        public int Count
        {
            get
            {
                return _count;
            }
            set
            {
                _count = value;
                OnPropertyChanged();
            }
        }
        public decimal Sum
        {
            get
            {
                return _sum;
            }
            set
            {
                _sum = value;
                OnPropertyChanged();
            }
        }
        public int Progress
        {
            get { return _progress; }
            set
            {
                _progress = value;
                OnPropertyChanged();
            }
        }
        public int MaxProgress
        {
            get { return _maxProgress; }
            set
            {
                _maxProgress = value;
                OnPropertyChanged();
            }
        }
    }
    //favorite
    public class HomeFavorite
    {
        public static ObservableCollection<string> FavoriteList { get; set; } = FavoriteService.ReadFavoriteList();
    }
}