using ItemChecker.Core;
using ItemChecker.Services;
using ItemChecker.Support;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace ItemChecker.MVVM.Model
{
    public class Details : ObservableObject
    {
        public static DetailsItemList<DetailItem> Items { get; set; } = new();
        public static DetailItem Item { get; set; }

        public List<string> CurrencyList { get; set; } = Currencies.Allow.Select(x => x.Name).ToList();
        public static DataCurrency CurectCurrency { get; set; } = Currencies.Allow.FirstOrDefault(x => x.Id == 1);
        public string CurrencySymbolSteam { get; set; } = SteamAccount.Currency.Symbol;
        public List<string> Services { get; set; } = Main.Services;
    }
    public class DetailsItemList<T> : List<DetailItem>
    {
        public new void Add(string itemName)
        {
            if (IsAllow(itemName))
                base.Add(new DetailItem(itemName));
        }
        bool IsAllow(string itemName)
        {
            var currentList = this as DetailsItemList<DetailItem>;
            return !currentList.Any(x => x.ItemName == itemName) && ItemsBase.List.Any(x => x.ItemName == itemName);
        }
    }
    public class DetailItem : ObservableObject
    {
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
        string _itemName = "Unknown";
        public ObservableCollection<DetailService> Services
        {
            get { return _services; }
            set
            {
                _services = value;
                OnPropertyChanged();
            }
        }
        ObservableCollection<DetailService> _services = new();

        public DetailItemCompare Compare
        {
            get { return _compare; }
            set
            {
                _compare = value;
                OnPropertyChanged();
            }
        }
        DetailItemCompare _compare = new();
        public DetailItemInfo Info
        {
            get { return _info; }
            set
            {
                _info = value;
                OnPropertyChanged();
            }
        }
        DetailItemInfo _info = new();
        public DetailService Service
        {
            get { return _service; }
            set
            {
                _service = value;
                Info = new(value.ServiceId, ItemName);
                OnPropertyChanged();
            }
        }
        DetailService _service = new();
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
        int _currencyId = 0;

        public DetailItem()
        {

        }
        public DetailItem(string itemName)
        {
            ItemName = itemName;
            IsBusy = true;
            Task.Run(() =>
            {
                try
                {
        
                    ItemBaseService.UpdateSteamItem(itemName);
                    ItemBaseService.UpdateCsmItem(itemName, false);
                    ItemBaseService.UpdateLfm();
                    ItemBaseService.UpdateBuffItem(itemName);

                    var prices = new List<DetailService>();
                    for (int i = 0; i < Main.Services.Count; i++)
                        prices.Add(new(i, itemName));
                    Services = new(prices);
                }
                catch (Exception ex)
                {
                    BaseService.errorLog(ex, true);
                }
                finally
                {
                    IsBusy = false;
                }
            });
        }
    }

    public class DetailService : ObservableObject
    {
        public int ServiceId { get; set; }
        public string Service { get; set; }
        public decimal Price { get; set; }
        public decimal Get { get; set; }
        public bool Have { get; set; }
        public bool Available { get; set; } = true;

        public DetailService()
        {

        }
        public DetailService(int service, string itemName)
        {
            dynamic item = null;
            this.ServiceId = service;
            this.Service = Main.Services[service];
            switch (service)
            {
                case 0:
                    this.Price = ItemsBase.List.FirstOrDefault(x => x.ItemName == itemName).Steam.HighestBuyOrder;
                    this.Get = Math.Round(this.Price * Calculator.CommissionSteam, 2);
                    this.Have = Price > 0;
                    break;
                case 1:
                    this.Price = ItemsBase.List.FirstOrDefault(x => x.ItemName == itemName).Steam.LowestSellOrder;
                    this.Get = Math.Round(this.Price * Calculator.CommissionSteam, 2);
                    this.Have = Price > 0;
                    break;
                case 2:
                    item = ItemsBase.List.FirstOrDefault(x => x.ItemName == itemName).Csm;
                    this.Price = item.Price;
                    this.Get = Math.Round(this.Price * Calculator.CommissionCsm, 2);
                    this.Have = item.IsHave;
                    this.Available = !item.Unavailable && !item.Overstock;
                    break;
                case 3:
                    item = ItemsBase.List.FirstOrDefault(x => x.ItemName == itemName).Lfm;
                    var price = item.Price;
                    this.Price = Math.Round(price * 1.03m, 2);
                    this.Get = Math.Round(price * Calculator.CommissionLf, 2);
                    this.Have = item.IsHave;
                    this.Available = !item.Unavailable && !item.Overstock;
                    break;
                case 4:
                    this.Price = ItemsBase.List.FirstOrDefault(x => x.ItemName == itemName).Buff.BuyOrder;
                    this.Get = Math.Round(this.Price * Calculator.CommissionBuff, 2);
                    this.Have = Price > 0;
                    break;
                case 5:
                    this.Price = ItemsBase.List.FirstOrDefault(x => x.ItemName == itemName).Buff.Price;
                    this.Get = Math.Round(this.Price * Calculator.CommissionBuff, 2);
                    this.Have = ItemsBase.List.FirstOrDefault(x => x.ItemName == itemName).Buff.IsHave;
                    break;
            }
        }
    }
    public class DetailItemCompare : ObservableObject
    {
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
        int _service1;
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
        int _service2;
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
        decimal _get;
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
        decimal _precent;
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
        decimal _difference;
    }
    public class DetailItemInfo : ObservableObject
    {
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

        public DetailItemInfo()
        {

        }
        public DetailItemInfo(int serviceId, string itemName)
        {
            if (itemName == "Unknown")
                return;

            IsBusy = true;

            SteamInfo.IsShow = false;
            CsmInfo.IsShow = false;
            LfmInfo.IsShow = false;
            BuffInfo.IsShow = false;

            Task.Run(() =>
            {
    

                switch (serviceId)
                {
                    case 0 or 1:
                        {
                            var data = ItemsBase.List.FirstOrDefault(x => x.ItemName == itemName).Steam;

                            data.LowestSellOrder = Currency.ConverterFromUsd(data.LowestSellOrder, ParserTable.CurectCurrency.Id);
                            data.HighestBuyOrder = Currency.ConverterFromUsd(data.HighestBuyOrder, ParserTable.CurectCurrency.Id);

                            ItemBaseService.UpdateSteamItemHistory(itemName);
                            SteamInfo.LastSale = data.History.FirstOrDefault().Date;
                            List<decimal> last30 = data.History.Where(x => x.Date > DateTime.Today.AddDays(-30)).Select(x => x.Price).ToList();
                            List<decimal> last60 = data.History.Where(x => x.Date > DateTime.Today.AddDays(-60)).Select(x => x.Price).ToList();
                            SteamInfo.Count = Tuple.Create(last30.Count, last60.Count);
                            decimal avg30 = last30.Any() ? Math.Round(Queryable.Average(last30.AsQueryable()), 2) : 0;
                            decimal avg60 = last60.Any() ? Math.Round(Queryable.Average(last60.AsQueryable()), 2) : 0;
                            SteamInfo.Avg = Tuple.Create(avg30, avg60);
                            SteamInfo.Item = data;

                            SteamInfo.IsShow = true;
                            break;
                        }
                    case 2:
                        {
                            ItemBaseService.UpdateCsmItem(itemName, true);
                            CsmInfo.Item = ItemsBase.List.FirstOrDefault(x => x.ItemName == itemName).Csm;
                            CsmInfo.CurrentItem = CsmInfo.Item.Inventory.FirstOrDefault();
                            CsmInfo.MaxValueSlide = CsmInfo.Item.Inventory.Count;
                            CsmInfo.ValueSlide = CsmInfo.Item.Inventory.Any() ? 1 : 0;

                            CsmInfo.IsShow = true;
                            break;
                        }
                    case 3:
                        {
                            LfmInfo.Item = ItemsBase.List.FirstOrDefault(x => x.ItemName == itemName).Lfm;

                            LfmInfo.IsShow = true;
                            break;
                        }
                    case 4 or 5:
                        {
                            ItemBaseService.UpdateBuffItemHistory(itemName);
                            var data = ItemsBase.List.FirstOrDefault(x => x.ItemName == itemName).Buff;
                            BuffInfo.LastSale = data.History.FirstOrDefault().Date;
                            BuffInfo.Item = data;

                            BuffInfo.IsShow = true;
                            break;
                        }
                }
                IsBusy = false;
            });
        }
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
        bool _isShow = false;
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
        DateTime _lastSale = new();
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
}
