using ItemChecker.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;

namespace ItemChecker.MVVM.Model
{
    public class ParserCheckConfig : ObservableObject
    {
        public DateTime CheckedTime { get; set; } = DateTime.Now;
        public int MinPrice { get; set; }
        public int MaxPrice
        {
            get
            {
                return _maxPrice;
            }
            set
            {
                _maxPrice = value;
                OnPropertyChanged();
            }
        }
        int _maxPrice;

        public bool NotWeapon { get; set; }
        public bool Normal { get; set; }
        public bool Souvenir { get; set; }
        public bool Stattrak { get; set; }
        public bool KnifeGlove { get; set; }
        public bool KnifeGloveStattrak { get; set; }

        public List<string> OnlyList
        {
            get
            {
                return new()
                        {
                            "None",
                            "Ordered",
                            "Favorite",
                        };
            }
        }
        public int SelectedOnly { get; set; }

        public bool WithoutLock { get; set; }
        public bool RareItems { get; set; }
        public bool UserItems { get; set; }

        public List<string> Services { get; set; } = Main.Services;
        public int ServiceOne { get; set; }
        public int ServiceTwo { get; set; }

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
        bool _isVisible;
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
        string _service1 = "Service1";
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
        string _service2 = "Service2";
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
        DateTime _dateTime = DateTime.MinValue;

        //progress
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
        bool _isParser;
        public int CountList
        {
            get { return _countList; }
            set
            {
                _countList = value;
                OnPropertyChanged();
            }
        }
        int _countList = 0;
        public int CurrentProgress
        {
            get { return _currentProgress; }
            set
            {
                _currentProgress = value;
                OnPropertyChanged();
            }
        }
        int _currentProgress;
        public int MaxProgress
        {
            get { return _maxProgress; }
            set
            {
                _maxProgress = value;
                OnPropertyChanged();
            }
        }
        int _maxProgress;
        public string Status
        {
            get { return _status; }
            set
            {
                _status = value;
                OnPropertyChanged();
            }
        }
        string _status;
        public bool TimerOn
        {
            get { return _timerOn; }
            set
            {
                _timerOn = value;
                OnPropertyChanged();
            }
        }
        bool _timerOn;
        public bool IsStoped { get; set; }

        //timer
        public static System.Timers.Timer Timer { get; set; } = new(1000);
        public static int TimerTick { get; set; }
        public static CancellationTokenSource cts { get; set; } = new();
        public static CancellationToken token { get; set; } = cts.Token;
    }
    public class ParserQueue : ObservableObject
    {
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
        ObservableCollection<DataQueue> _items = new();
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
        DataQueue _selectedQueue;

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
        decimal _totalAllowed = 0;
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
        decimal _availableAmount = 0;
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
        decimal _orderAmout = 0;
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
        decimal _availableAmountPrecent = 0;
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
        decimal _remaining = 0;

        public int CurrentProgress
        {
            get { return _currentProgress; }
            set
            {
                _currentProgress = value;
                OnPropertyChanged();
            }
        }
        int _currentProgress;
        public int MaxProgress
        {
            get { return _maxProgress; }
            set
            {
                _maxProgress = value;
                OnPropertyChanged();
            }
        }
        int _maxProgress;
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

        public static Queue<DataQueue> Queues { get; set; } = new();
    }
}
