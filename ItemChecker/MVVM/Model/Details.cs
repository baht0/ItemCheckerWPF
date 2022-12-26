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
        public static DetailItem Item { get; set; } = new();

        public List<string> CurrencyList { get; set; } = SteamBase.AllowCurrencys.Select(x => x.Name).ToList();
        public static Currency CurectCurrency { get; set; } = SteamBase.AllowCurrencys.FirstOrDefault(x => x.Id == 1);
        public string CurrencySymbolSteam { get; set; } = SteamAccount.Currency.Symbol;
        public List<string> Services { get; set; } = Main.Services;

        public ObservableCollection<DetailItem> ItemsView
        {
            get
            {
                return _itemsView;
            }
            set
            {
                _itemsView = value;
                OnPropertyChanged();
            }
        }
        ObservableCollection<DetailItem> _itemsView = new(Items);
        public bool IsSearch
        {
            get
            {
                return _isSearch;
            }
            set
            {
                _isSearch = value;
                OnPropertyChanged();
            }
        }
        bool _isSearch;
    }
    public class DetailsItemList<T> : List<DetailItem>
    {
        public new void Add(string itemName)
        {
            if (IsAllow(itemName))
            {
                base.Add(new DetailItem()
                {
                    ItemName = itemName,
                });
            }
        }
        bool IsAllow(string itemName)
        {
            var currentList = this as DetailsItemList<DetailItem>;
            return !currentList.Any(x => x.ItemName == itemName) && SteamBase.ItemList.Any(x => x.ItemName == itemName);
        }
    }
    public class DetailItem : ObservableObject
    {
        public string ItemName { get; set; }
        public ObservableCollection<DetailItemPrice> Prices
        {
            get { return _prices; }
            set
            {
                _prices = value;
                OnPropertyChanged();
            }
        }
        ObservableCollection<DetailItemPrice> _prices = new();

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
        public DetailItemPrice Price
        {
            get { return _price; }
            set
            {
                _price = value;
                if (value == null || ItemName == "New")
                    return;

                Info.IsBusy = true;
                Info.AllHide();

                Task.Run(() =>
                {
                    ItemBaseService baseService = new();

                    switch (value.ServiceId)
                    {
                        case 0 or 1:
                            {
                                var data = SteamBase.ItemList.FirstOrDefault(x => x.ItemName == ItemName).Steam;

                                data.LowestSellOrder = Edit.ConverterFromUsd(data.LowestSellOrder, ParserTable.CurectCurrency.Value);
                                data.HighestBuyOrder = Edit.ConverterFromUsd(data.HighestBuyOrder, ParserTable.CurectCurrency.Value);

                                baseService.UpdateSteamItemHistory(ItemName);
                                Info.SteamInfo.LastSale = data.History.FirstOrDefault().Date;
                                List<decimal> last30 = data.History.Where(x => x.Date > DateTime.Today.AddDays(-30)).Select(x => x.Price).ToList();
                                List<decimal> last60 = data.History.Where(x => x.Date > DateTime.Today.AddDays(-60)).Select(x => x.Price).ToList();
                                Info.SteamInfo.Count = Tuple.Create(last30.Count, last60.Count);
                                decimal avg30 = last30.Any() ? Math.Round(Queryable.Average(last30.AsQueryable()), 2) : 0;
                                decimal avg60 = last60.Any() ? Math.Round(Queryable.Average(last60.AsQueryable()), 2) : 0;
                                Info.SteamInfo.Avg = Tuple.Create(avg30, avg60);
                                Info.SteamInfo.Item = data;

                                Info.SteamInfo.IsShow = true;
                                break;
                            }
                        case 2:
                            {
                                baseService.UpdateCsmItem(ItemName, true);
                                Info.CsmInfo.Item = SteamBase.ItemList.FirstOrDefault(x => x.ItemName == ItemName).Csm;
                                Info.CsmInfo.CurrentItem = Info.CsmInfo.Item.Inventory.FirstOrDefault();
                                Info.CsmInfo.MaxValueSlide = Info.CsmInfo.Item.Inventory.Count;
                                Info.CsmInfo.ValueSlide = Info.CsmInfo.Item.Inventory.Any() ? 1 : 0;

                                Info.CsmInfo.IsShow = true;
                                break;
                            }
                        case 3:
                            {
                                Info.LfmInfo.Item = SteamBase.ItemList.FirstOrDefault(x => x.ItemName == ItemName).Lfm;

                                Info.LfmInfo.IsShow = true;
                                break;
                            }
                        case 4 or 5:
                            {
                                baseService.UpdateBuffItemHistory(ItemName);
                                var data = SteamBase.ItemList.FirstOrDefault(x => x.ItemName == ItemName).Buff;
                                Info.BuffInfo.LastSale = data.History.FirstOrDefault().Date;
                                Info.BuffInfo.Item = data;

                                Info.BuffInfo.IsShow = true;
                                break;
                            }
                    }
                    Info.IsBusy = false;
            });
            OnPropertyChanged();
            }
        }
        DetailItemPrice _price = new();
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
    }

    public class DetailItemPrice : ObservableObject
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
        bool _isBusy = new();
        public int ServiceId { get; set; }
        public string Service { get; set; }
        public decimal Price { get; set; }
        public decimal Get { get; set; }
        public bool Have { get; set; }
        public bool Available { get; set; } = true;

        public DetailItemPrice Add(int service, string itemName)
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
        bool _isBusy = new();
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

        public void AllHide()
        {
            SteamInfo.IsShow = false;
            CsmInfo.IsShow = false;
            LfmInfo.IsShow = false;
            BuffInfo.IsShow = false;
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
