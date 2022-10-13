using ItemChecker.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ItemChecker.MVVM.Model
{
    public class Details : ObservableObject
    {
        private string _currencySymbol = "$";
        private int _currencyId = 0;
        public int CurrencyId
        {
            get
            {
                return _currencyId;
            }
            set
            {
                _currencyId = value;
                OnPropertyChanged();
            }
        }
        public string CurrencySymbol
        {
            get
            {
                return _currencySymbol;
            }
            set
            {
                _currencySymbol = value;
                OnPropertyChanged();
            }
        }
        public List<string> CurrencyList { get; set; } = SteamBase.AllowCurrencys.Select(x => x.Name).ToList();
        public static Currency CurectCurrency { get; set; } = SteamBase.AllowCurrencys.FirstOrDefault(x => x.Id == 1);
        public string CurrencySymbolSteam { get; set; } = SteamBase.AllowCurrencys.FirstOrDefault(x => x.Id == SteamAccount.CurrencyId).Symbol;
        public List<string> Services { get; set; } = Main.Services;

        private string _itemName = "Unknown";
        public string ItemName
        {
            get { return _itemName; }
            set
            {
                _itemName = value;
                OnPropertyChanged();
            }
        }
        private bool _loading;
        public bool Loading
        {
            get { return _loading; }
            set
            {
                _loading = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<DetailsPrice> _prices = new();
        public ObservableCollection<DetailsPrice> Prices
        {
            get { return _prices; }
            set
            {
                _prices = value;
                OnPropertyChanged();
            }
        }
    }
    public class DetailsPrice
    {
        public int ServiceId { get; set; }
        public string Service { get; set; }
        public decimal Price { get; set; }
        public decimal Get { get; set; }
        public bool Have { get; set; }
        public bool Available { get; set; } = true;

        public DetailsPrice Add(int service, string itemName)
        {
            dynamic item = null;
            this.ServiceId = service;
            this.Service = Main.Services[service];
            switch (service)
            {
                case 0:
                    this.Price = SteamBase.ItemList.FirstOrDefault(x => x.ItemName == itemName).Steam.HighestBuyOrder;
                    this.Get = Math.Round(this.Price * Calculator.CommissionSteam, 2);
                    this.Have = Price > 0;
                    break;
                case 1:
                    this.Price = SteamBase.ItemList.FirstOrDefault(x => x.ItemName == itemName).Steam.LowestSellOrder;
                    this.Get = Math.Round(this.Price * Calculator.CommissionSteam, 2);
                    this.Have = Price > 0;
                    break;
                case 2:
                    item = SteamBase.ItemList.FirstOrDefault(x => x.ItemName == itemName).Csm;
                    this.Price = item.Price;
                    this.Get = Math.Round(this.Price * Calculator.CommissionCsm, 2);
                    this.Have = item.IsHave;
                    this.Available = !item.Unavailable && !item.Overstock;
                    break;
                case 3:
                    item = SteamBase.ItemList.FirstOrDefault(x => x.ItemName == itemName).Lfm;
                    var price = item.Price;
                    this.Price = Math.Round(price * 1.03m, 2);
                    this.Get = Math.Round(price * Calculator.CommissionLf, 2);
                    this.Have = item.IsHave;
                    this.Available = !item.Unavailable && !item.Overstock;
                    break;
                case 4:
                    this.Price = SteamBase.ItemList.FirstOrDefault(x => x.ItemName == itemName).Buff.BuyOrder;
                    this.Get = Math.Round(this.Price * Calculator.CommissionBuff, 2);
                    this.Have = Price > 0;
                    break;
                case 5:
                    this.Price = SteamBase.ItemList.FirstOrDefault(x => x.ItemName == itemName).Buff.Price;
                    this.Get = Math.Round(this.Price * Calculator.CommissionBuff, 2);
                    this.Have = SteamBase.ItemList.FirstOrDefault(x => x.ItemName == itemName).Buff.IsHave;
                    break;
            }
            return this;
        }
    }
    public class DetailsCompare : ObservableObject
    {
        private int _service1;
        private int _service2;
        private decimal _get;
        private decimal _precent;
        private decimal _difference;

        public int Service1
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
        public int Service2
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
        public decimal Get
        {
            get
            {
                return _get;
            }
            set
            {
                _get = value;
                OnPropertyChanged();
            }
        }
        public decimal Precent
        {
            get
            {
                return _precent;
            }
            set
            {
                _precent = value;
                OnPropertyChanged();
            }
        }
        public decimal Difference
        {
            get
            {
                return _difference;
            }
            set
            {
                _difference = value;
                OnPropertyChanged();
            }
        }
    }

    public class DetailsInfo : ObservableObject
    {
        public SteamInfo SteamInfo
        {
            get
            {
                return _steamInfo;
            }
            set
            {
                _steamInfo = value;
                OnPropertyChanged();
            }
        }
        SteamInfo _steamInfo = new();
        public CsmInfo CsmInfo
        {
            get
            {
                return _csmInfo;
            }
            set
            {
                _csmInfo = value;
                OnPropertyChanged();
            }
        }
        CsmInfo _csmInfo = new();
        public LfmInfo LfmInfo
        {
            get
            {
                return _lfmInfo;
            }
            set
            {
                _lfmInfo = value;
                OnPropertyChanged();
            }
        }
        LfmInfo _lfmInfo = new();
        public BuffInfo BuffInfo
        {
            get
            {
                return _buffInfo;
            }
            set
            {
                _buffInfo = value;
                OnPropertyChanged();
            }
        }
        BuffInfo _buffInfo = new();
    }
    public class SteamInfo : ItemInfo
    {
        public SteamItem Item
        {
            get
            {
                return _item;
            }
            set
            {
                _item = value;
                OnPropertyChanged();
            }
        }
        private SteamItem _item = new();
        public Tuple<int, int> Count
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
        private Tuple<int, int> _count = new(0, 0);
        public Tuple<decimal, decimal> Avg
        {
            get
            {
                return _avg;
            }
            set
            {
                _avg = value;
                OnPropertyChanged();
            }
        }
        private Tuple<decimal, decimal> _avg = new(0, 0);
    }
    public class CsmInfo : ItemInfo
    {
        public CsmItem Item
        {
            get
            {
                return _item;
            }
            set
            {
                _item = value;
                OnPropertyChanged();
            }
        }
        private CsmItem _item = new();
        public int CurrentItemId
        {
            get
            {
                return _currentItemId;
            }
            set
            {
                _currentItemId = value;
                if (_item.Inventory.Count != value)
                    CurrentItem = _item.Inventory[value];
                ValueSlide = value + 1;
                OnPropertyChanged();
            }
        }
        public int _currentItemId;
        public int MaxValueSlide
        {
            get
            {
                return _maxValueSlide;
            }
            set
            {
                _maxValueSlide = value - 1;
                OnPropertyChanged();
            }
        }
        public int _maxValueSlide;
        public int ValueSlide
        {
            get
            {
                return _valueSlide;
            }
            set
            {
                _valueSlide = value;
                OnPropertyChanged();
            }
        }
        public int _valueSlide;
        public InventoryCsm CurrentItem
        {
            get
            {
                return _currentItem;
            }
            set
            {
                _currentItem = value;
                OnPropertyChanged();
            }
        }
        public InventoryCsm _currentItem;
    }
    public class LfmInfo : ItemInfo
    {
        public LfmItem Item
        {
            get
            {
                return _item;
            }
            set
            {
                _item = value;
                OnPropertyChanged();
            }
        }
        private LfmItem _item = new();
    }
    public class BuffInfo : ItemInfo
    {
        public BuffItem Item
        {
            get
            {
                return _item;
            }
            set
            {
                _item = value;
                OnPropertyChanged();
            }
        }
        private BuffItem _item = new();
    }
    public class ItemInfo : ObservableObject
    {
        public bool IsShow
        {
            get
            {
                return _isShow;
            }
            set
            {
                _isShow = value;
                OnPropertyChanged();
            }
        }
        private bool _isShow = false;
        public DateTime LastSale
        {
            get
            {
                return _lastSale;
            }
            set
            {
                _lastSale = value;
                OnPropertyChanged();
            }
        }
        private DateTime _lastSale = new();
    }
}