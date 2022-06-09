using ItemChecker.Core;
using ItemChecker.Net;
using ItemChecker.Properties;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Web;

namespace ItemChecker.MVVM.Model
{
    public class ParserCheckConfig
    {
        public DateTime CheckedTime { get; set; } = DateTime.Now;
        public int MinPrice { get; set; } = ParserProperties.Default.MinPrice;
        public int MaxPrice { get; set; } = ParserProperties.Default.MaxPrice;

        public bool NotWeapon { get; set; } = ParserProperties.Default.NotWeapon;
        public bool Normal { get; set; } = ParserProperties.Default.Normal;
        public bool Souvenir { get; set; } = ParserProperties.Default.Souvenir;
        public bool Stattrak { get; set; } = ParserProperties.Default.Stattrak;
        public bool KnifeGlove { get; set; } = ParserProperties.Default.KnifeGlove;
        public bool KnifeGloveStattrak { get; set; } = ParserProperties.Default.KnifeGloveStattrak;

        public List<string> OnlyList { get; set; } = new()
        {
            "None",
            "Ordered",
            "Favorite",
        };
        public int SelectedOnly { get; set; } = ParserProperties.Default.SelectedOnly;

        public bool WithoutLock { get; set; } = ParserProperties.Default.WithoutLock;
        public bool RareItems { get; set; } = ParserProperties.Default.RareItems;
        public bool UserItems { get; set; } = ParserProperties.Default.UserItems;

        public List<string> Services { get; set; } = new()
        {
            "SteamMarket(A)",
            "SteamMarket",
            "Cs.Money",
            "Loot.Farm",
            "Buff163"
        };
        public int ServiceOne { get; set; } = ParserProperties.Default.ServiceOne;
        public int ServiceTwo { get; set; } = ParserProperties.Default.ServiceTwo;

        //last saved config
        public static ParserCheckConfig CheckedConfig { get; set; } = new();
        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
    public class ParserCheckInfo : ObservableObject
    {
        //info
        private bool _isVisible;
        private string _mode = "Unknown";
        private string _service1 = "Service1";
        private string _service2 = "Service2";
        private DateTime _dateTime = DateTime.MinValue;
        public bool IsVisible
        {
            get
            {
                return _isVisible;
            }
            set
            {
                _isVisible = value;
                OnPropertyChanged();
            }
        }
        public string Mode
        {
            get
            {
                return _mode;
            }
            set
            {
                _mode = value;
                OnPropertyChanged();
            }
        }
        public string Service1
        {
            get
            {
                return _service1;
            }
            set
            {
                _service1 = value;
                OnPropertyChanged();
            }
        }
        public string Service2
        {
            get
            {
                return _service2;
            }
            set
            {
                _service2 = value;
                OnPropertyChanged();
            }
        }
        public DateTime DateTime
        {
            get
            {
                return _dateTime;
            }
            set
            {
                _dateTime = value;
                OnPropertyChanged();
            }
        }

        //progress
        private bool _isParser;
        private int _countList = 0;
        private int _currentProgress;
        private int _maxProgress;
        private string _status;
        private bool _timerOn;
        public bool IsParser
        {
            get
            {
                return _isParser;
            }
            set
            {
                _isParser = value;
                OnPropertyChanged();
            }
        }
        public int CountList
        {
            get { return _countList; }
            set
            {
                _countList = value;
                OnPropertyChanged();
            }
        }
        public int CurrentProgress
        {
            get { return _currentProgress; }
            set
            {
                _currentProgress = value;
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
        public bool TimerOn
        {
            get { return _timerOn; }
            set
            {
                _timerOn = value;
                OnPropertyChanged();
            }
        }
        public bool IsStoped { get; set; }

        //timer
        public static System.Timers.Timer Timer { get; set; } = new(1000);
        public static int TimerTick { get; set; }
        public static CancellationTokenSource cts { get; set; } = new();
        public static CancellationToken token { get; set; } = cts.Token;
    }
    public class ParserInfo : ObservableObject
    {
        private string _itemName = "Unknown";
        public string ItemName
        {
            get
            {
                return _itemName;
            }
            set
            {
                _itemName = value;
                OnPropertyChanged();
            }
        }

        //steam
        private SteamInfo _itemSt = new();
        private bool _st = false;
        public SteamInfo ItemSt
        {
            get
            {
                return _itemSt;
            }
            set
            {
                _itemSt = value;
                OnPropertyChanged();
            }
        }
        public bool ST
        {
            get
            {
                return _st;
            }
            set
            {
                _st = value;
                OnPropertyChanged();
            }
        }

        //csm
        private int _infoItemCurrent;
        private int _infoItemCount;
        private List<DataInventoriesCsm> _inventoryCsm = new();
        private DataInventoriesCsm _itemCsm = new();
        private decimal _itemCsmComparePrice = 0;
        private bool _csm = false;
        public int InfoItemCurrent
        {
            get
            {
                return _infoItemCurrent;
            }
            set
            {
                _infoItemCurrent = value;
                OnPropertyChanged();
                ItemCsm = InventoryCsm.Any() ? InventoryCsm[value] : new();
            }
        }
        public int InfoItemCount
        {
            get
            {
                return _infoItemCount;
            }
            set
            {
                _infoItemCount = value;
                OnPropertyChanged();
            }
        }
        public List<DataInventoriesCsm> InventoryCsm
        {
            get
            {
                return _inventoryCsm;
            }
            set
            {
                _inventoryCsm = value;
                OnPropertyChanged();
            }
        }
        public DataInventoriesCsm ItemCsm
        {
            get
            {
                return _itemCsm;
            }
            set
            {
                _itemCsm = value;
                OnPropertyChanged();
            }
        }
        public decimal ItemCsmComparePrice
        {
            get
            {
                return _itemCsmComparePrice;
            }
            set
            {
                _itemCsmComparePrice = value;
                OnPropertyChanged();
            }
        }
        public bool CSM
        {
            get
            {
                return _csm;
            }
            set
            {
                _csm = value;
                OnPropertyChanged();
            }
        }

        //lfm
        private LfmInfo _itemLf = new();
        private bool _lf = false;
        public LfmInfo ItemLf
        {
            get
            {
                return _itemLf;
            }
            set
            {
                _itemLf = value;
                OnPropertyChanged();
            }
        }
        public bool LF
        {
            get
            {
                return _lf;
            }
            set
            {
                _lf = value;
                OnPropertyChanged();
            }
        }

        //buff
        private BuffInfo _itemBf = new();
        private bool _bf = false;
        public BuffInfo ItemBf
        {
            get
            {
                return _itemBf;
            }
            set
            {
                _itemBf = value;
                OnPropertyChanged();
            }
        }
        public bool BF
        {
            get
            {
                return _bf;
            }
            set
            {
                _bf = value;
                OnPropertyChanged();
            }
        }
    }
    public class ParserQueue : ObservableObject
    {
        private ObservableCollection<DataQueue> _items = new();
        private DataQueue _selectedQueue;
        public ObservableCollection<DataQueue> Items
        {
            get
            {
                return _items;
            }
            set
            {
                _items = value;
                OnPropertyChanged();
            }
        }
        public DataQueue SelectedQueue
        {
            get
            {
                return _selectedQueue;
            }
            set
            {
                _selectedQueue = value;
                OnPropertyChanged();
            }
        }

        private decimal _totalAllowed = 0;
        private decimal _availableAmount = 0;
        private decimal _orderAmout = 0;
        private decimal _availableAmountPrecent = 0;
        private decimal _remaining = 0;
        public decimal TotalAllowed
        {
            get
            {
                return _totalAllowed;
            }
            set
            {
                _totalAllowed = value;
                OnPropertyChanged();
            }
        }
        public decimal AvailableAmount
        {
            get
            {
                return _availableAmount;
            }
            set
            {
                _availableAmount = value;
                OnPropertyChanged();
            }
        }
        public decimal OrderAmout
        {
            get
            {
                return _orderAmout;
            }
            set
            {
                _orderAmout = value;
                OnPropertyChanged();
            }
        }
        public decimal AvailableAmountPrecent
        {
            get
            {
                return _availableAmountPrecent;
            }
            set
            {
                _availableAmountPrecent = value;
                OnPropertyChanged();
            }
        }
        public decimal Remaining
        {
            get
            {
                return _remaining;
            }
            set
            {
                _remaining = value;
                OnPropertyChanged();
            }
        }

        private int _currentProgress;
        private int _maxProgress;
        public int CurrentProgress
        {
            get { return _currentProgress; }
            set
            {
                _currentProgress = value;
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

        public static Boolean CheckConditions(ObservableCollection<DataQueue> queueItems, DataParser parseredItem)
        {
            string itemName = parseredItem.ItemName;
            decimal price = parseredItem.Price2;

            return queueItems.Any(n => n.ItemName == itemName) ||
                (queueItems.Select(x => x.OrderPrice).Sum() + price) > SteamAccount.GetAvailableAmount() ||
                DataOrder.Orders.Any(n => n.ItemName == itemName) ||
                price > SteamAccount.Balance ||
                parseredItem.Precent < SettingsProperties.Default.MinPrecent ||
                itemName.Contains("Doppler");
        }
        public static void PlaceOrder(string itemName)
        {
            var market_hash_name = HttpUtility.UrlEncode(itemName);
            var item_nameid = SteamBase.ItemList.FirstOrDefault(x => x.ItemName == itemName).Steam.Id;
            JObject json = Get.ItemOrdersHistogram(item_nameid, SteamAccount.CurrencyId);
            decimal highest_buy_order = Convert.ToDecimal(json["highest_buy_order"]) / 100;

            if (SteamAccount.Balance > highest_buy_order)
                Post.CreateBuyOrder(SteamAccount.Cookies, market_hash_name, highest_buy_order, SteamAccount.CurrencyId);
        }
    }
}
