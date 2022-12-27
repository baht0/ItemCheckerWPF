using ItemChecker.Core;
using ItemChecker.Properties;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;

namespace ItemChecker.MVVM.Model
{
    public class Home
    {
        public string CurrencySymbol { get; set; } = SteamAccount.Currency.Symbol;
    }
    public class HomeTable : ObservableObject
    {
        public ObservableCollection<DataOrder> OrderedGrid
        {
            get
            {
                return _orderedGrid;
            }
            set
            {
                _orderedGrid = value;
                OnPropertyChanged();
            }
        }
        ObservableCollection<DataOrder> _orderedGrid = new(SteamMarket.Orders);
        public DataOrder SelectedOrderItem
        {
            get
            {
                return _selectedOrderItem;
            }
            set
            {
                _selectedOrderItem = value;
                OnPropertyChanged();
            }
        }
        DataOrder _selectedOrderItem;

        public bool IsBusy
        {
            get
            {
                return _isBusy;
            }
            set
            {
                _isBusy = value;
                OnPropertyChanged();
            }
        }
        bool _isBusy = true;
    }
    //tools
    public class HomePush : ObservableObject
    {
        public static System.Timers.Timer Timer { get; set; } = new(1000);
        public static int TimerTick { get; set; }
        public static CancellationTokenSource cts { get; set; } = new();
        public static CancellationToken token { get; set; } = cts.Token;

        public List<string> ServicesList
        {
            get
            {
                return Main.Services;
            }
        }
        public int ServiceId
        {
            get
            {
                return _serviceId;
            }
            set
            {
                _serviceId = value;
                HomeProperties.Default.ServiceId = value;
                HomeProperties.Default.Save();
                OnPropertyChanged();
            }
        }
        private int _serviceId = HomeProperties.Default.ServiceId;
        public int MinPrecent
        {
            get
            {
                return _minPrecent;
            }
            set
            {
                _minPrecent = value;
                HomeProperties.Default.MinPrecent = value;
                HomeProperties.Default.Save();
                OnPropertyChanged();
            }
        }
        private int _minPrecent = HomeProperties.Default.MinPrecent;
        public int Time { get; set; } = HomeProperties.Default.TimePush;
        public int Reserve { get; set; } = HomeProperties.Default.Reserve;

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
        private bool _isService;
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
        private int _check = 0;
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
        private int _push = 0;
        public int Progress
        {
            get { return _progress; }
            set
            {
                _progress = value;
                OnPropertyChanged();
            }
        }
        private int _progress = 0;
        public int MaxProgress
        {
            get { return _maxProgress; }
            set
            {
                _maxProgress = value;
                OnPropertyChanged();
            }
        }
        private int _maxProgress = 0;
        public string Status
        {
            get { return _status; }
            set
            {
                _status = value;
                OnPropertyChanged();
            }
        }
        private string _status = string.Empty;
    }
    //inventory
    public class HomeInventoryConfig : ObservableObject
    {
        public bool AllAvailable { get; set; }
        public bool SelectedOnly { get; set; } = true;
        public List<string> SellingPrice { get; set; } = new()
        {
            "LowestSellOrder",
            "HighestBuyOrder",
            "Custom",
        };
        public int SellingPriceId { get; set; }
        public decimal Price { get; set; }

        public List<string> Tasks
        {
            get
            {
                return new()
                {
                    "TradeOffers",
                    "QuickSell",
                };
            }
        }
        public int TaskId
        {
            get
            {
                return _taskId;
            }
            set
            {
                _taskId = value;
                OnPropertyChanged();
            }
        }
        int _taskId;
    }
    public class HomeInventoryInfo : ObservableObject
    {
        public ObservableCollection<DataInventory> Items
        {
            get { return _items; }
            set
            {
                _items = value;
                OnPropertyChanged();
            }
        }
        ObservableCollection<DataInventory> _items = new();

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
        bool _isService;
        public int Progress
        {
            get { return _progress; }
            set
            {
                _progress = value;
                OnPropertyChanged();
            }
        }
        int _progress = 0;
        public int MaxProgress
        {
            get { return _maxProgress; }
            set
            {
                _maxProgress = value;
                OnPropertyChanged();
            }
        }
        int _maxProgress = 0;
        public bool IsBusy
        {
            get
            {
                return _isBusy;
            }
            set
            {
                _isBusy = value;
                OnPropertyChanged();
            }
        }
        bool _isBusy;

        public static CancellationTokenSource cts { get; set; } = new();
        public static CancellationToken token { get; set; } = cts.Token;
    }
}
