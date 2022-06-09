using ItemChecker.Properties;
using ItemChecker.Core;
using System.Collections.ObjectModel;
using System.Threading;
using System.Linq;
using System.Collections.Generic;

namespace ItemChecker.MVVM.Model
{
    //tools
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
    //inventory
    public class HomeInventory : ObservableObject
    {
        private ObservableCollection<DataInventory> _items = new();
        public ObservableCollection<DataInventory> Items
        {
            get { return _items; }
            set
            {
                _items = value;
                OnPropertyChanged();
            }
        }

        public bool AllAvailable { get; set; } = HomeProperties.Default.AllAvailable;
        public bool SelectedOnly { get; set; } = HomeProperties.Default.SelectedOnly;
        public int MaxPrice { get; set; } = HomeProperties.Default.MaxPrice;
        public List<string> SellingPrice { get; set; } = new()
        {
            "LowestSellOrder",
            "HighestBuyOrder",
        };
        public int SellingPriceId { get; set; } = HomeProperties.Default.SellingPriceId;
        public List<string> Tasks { get; set; } = new()
        {
            "TradeOffers",
            "QuickSell",
        };
        public int TasksId { get; set; } = HomeProperties.Default.TasksId;

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
        public static CancellationTokenSource cts { get; set; } = new();
        public static CancellationToken token { get; set; } = cts.Token;
    }
    //favorite
    public class HomeFavorite : ObservableObject
    {
        private ObservableCollection<DataSavedList> _list = new(DataSavedList.Items.Where(x => x.ListName == "favorite"));
        public ObservableCollection<DataSavedList> List
        {
            get { return _list; }
            set
            {
                _list = value;
                OnPropertyChanged();
            }
        }
        private DataSavedList _selectedItem;
        public DataSavedList SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                _selectedItem = value;
                OnPropertyChanged();
            }
        }
    }
}