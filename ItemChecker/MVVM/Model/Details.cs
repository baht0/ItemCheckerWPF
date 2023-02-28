using ItemChecker.Core;
using ItemChecker.Support;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ItemChecker.MVVM.Model
{
    public class Details : BaseModel
    {
        public static DetailsItemList<DetailItem> Items { get; set; } = new();
        public static DetailItem Item { get; set; }

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
    public class DataGridDetails : BaseTable<DetailItem>
    {
        public void ShowItemInService(int serviceId)
        {
            string itemName = SelectedItem.ItemName.Replace("(Holo/Foil)", "(Holo-Foil)");
            string market_hash_name = Uri.EscapeDataString(itemName);
            switch (serviceId)
            {
                case 0 or 1:
                    Edit.OpenUrl("https://steamcommunity.com/market/listings/730/" + market_hash_name);
                    break;
                case 2:
                    Edit.OpenCsm(itemName);
                    break;
                case 3:
                    Clipboard.SetText(SelectedItem.ItemName);
                    Edit.OpenUrl("https://loot.farm/");
                    break;
                case 4:
                    Edit.OpenUrl("https://buff.163.com/goods/" + ItemsBase.List.FirstOrDefault(x => x.ItemName == SelectedItem.ItemName).Buff.Id + "#tab=buying");
                    break;
                case 5:
                    Edit.OpenUrl("https://buff.163.com/goods/" + ItemsBase.List.FirstOrDefault(x => x.ItemName == SelectedItem.ItemName).Buff.Id);
                    break;
            }
        }
        public DataCurrency SwitchCurrency(DataCurrency currentCurrency, string currName)
        {
            var currency = Currencies.Allow.FirstOrDefault(x => x.Name == currName);
            var prices = SelectedItem.Services.ToList();
            if (currentCurrency.Id != 1)
            {
                foreach (var price in prices)
                {
                    price.Price = Currency.ConverterToUsd(price.Price, currentCurrency.Id);
                    price.Get = Currency.ConverterToUsd(price.Get, currentCurrency.Id);
                }
                SelectedItem.Compare.Get = Currency.ConverterToUsd(SelectedItem.Compare.Get, currentCurrency.Id);
                SelectedItem.Compare.Difference = Currency.ConverterToUsd(SelectedItem.Compare.Difference, currentCurrency.Id);
            }
            foreach (var price in prices)
            {
                price.Price = Currency.ConverterFromUsd(price.Price, currency.Id);
                price.Get = Currency.ConverterFromUsd(price.Get, currency.Id);
            }
            SelectedItem.Compare.Get = Currency.ConverterFromUsd(SelectedItem.Compare.Get, currency.Id);
            SelectedItem.Compare.Difference = Currency.ConverterFromUsd(SelectedItem.Compare.Difference, currency.Id);

            SelectedItem.Services = new(prices);
            return currency;
        }
        public void Compare()
        {
            SelectedItem.Compare.Get = SelectedItem.Services[SelectedItem.Compare.Service2].Get;
            SelectedItem.Compare.Precent = Edit.Precent(SelectedItem.Services[SelectedItem.Compare.Service1].Price, SelectedItem.Services[SelectedItem.Compare.Service2].Get);
            SelectedItem.Compare.Difference = Edit.Difference(SelectedItem.Services[SelectedItem.Compare.Service2].Get, SelectedItem.Services[SelectedItem.Compare.Service1].Price);
        }
    }
    public class DetailsItemList<T> : List<DetailItem>
    {
        public new void Add(string itemName)
        {
            if (IsAllow(itemName))
            {
                var item = new DetailItem(itemName);
                item.UpdateServices();
                base.Add(item);
            }
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
        bool _isBusy;
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

        public DetailServiceCompare Compare
        {
            get { return _compare; }
            set
            {
                _compare = value;
                OnPropertyChanged();
            }
        }
        DetailServiceCompare _compare = new();
        public DetailServiceInfo Info
        {
            get { return _info; }
            set
            {
                _info = value;
                OnPropertyChanged();
            }
        }
        DetailServiceInfo _info = new();
        public DetailService Service
        {
            get { return _service; }
            set
            {
                _service = value;
                Info.UpdateItems(value.ServiceId, ItemName);
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

        public DetailItem(string itemName)
        {
            if (!string.IsNullOrEmpty(itemName))
            {
                ItemName = itemName;
                IsBusy = true;
            }
        }
        public void UpdateServices()
        {
            Task.Run(() =>
            {
                try
                {
                    ItemBaseService.UpdateSteamItem(ItemName);
                    ItemBaseService.UpdateCsmItem(ItemName, false);
                    ItemBaseService.UpdateLfm();
                    ItemBaseService.UpdateBuffItem(ItemName);

                    var services = new List<DetailService>();
                    for (int id = 0; id < BaseModel.Services.Count; id++)
                    {
                        var service = new DetailService();
                        service.Update(id, ItemName);
                        services.Add(service);
                    }
                    Services = new(services);
                }
                catch (Exception ex)
                {
                    BaseModel.ErrorLog(ex, true);
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

        public DetailService Update(int service, string itemName)
        {
            dynamic item = null;
            ServiceId = service;
            Service = BaseModel.Services[service];
            switch (service)
            {
                case 0:
                    Price = ItemsBase.List.FirstOrDefault(x => x.ItemName == itemName).Steam.HighestBuyOrder;
                    Get = Math.Round(Price * Calculator.CommissionSteam, 2);
                    Have = Price > 0;
                    break;
                case 1:
                    Price = ItemsBase.List.FirstOrDefault(x => x.ItemName == itemName).Steam.LowestSellOrder;
                    Get = Math.Round(Price * Calculator.CommissionSteam, 2);
                    Have = Price > 0;
                    break;
                case 2:
                    item = ItemsBase.List.FirstOrDefault(x => x.ItemName == itemName).Csm;
                    Price = item.Price;
                    Get = Math.Round(Price * Calculator.CommissionCsm, 2);
                    Have = item.IsHave;
                    Available = item.Status == ItemStatus.Available;
                    break;
                case 3:
                    item = ItemsBase.List.FirstOrDefault(x => x.ItemName == itemName).Lfm;
                    var price = item.Price;
                    Price = Math.Round(price * 1.03m, 2);
                    Get = Math.Round(price * Calculator.CommissionLf, 2);
                    Have = item.IsHave;
                    Available = item.Status == ItemStatus.Available;
                    break;
                case 4:
                    Price = ItemsBase.List.FirstOrDefault(x => x.ItemName == itemName).Buff.BuyOrder;
                    Get = Math.Round(Price * Calculator.CommissionBuff, 2);
                    Have = Price > 0;
                    break;
                case 5:
                    Price = ItemsBase.List.FirstOrDefault(x => x.ItemName == itemName).Buff.Price;
                    Get = Math.Round(Price * Calculator.CommissionBuff, 2);
                    Have = ItemsBase.List.FirstOrDefault(x => x.ItemName == itemName).Buff.IsHave;
                    break;
            }
            return this;
        }
    }
    public class DetailServiceCompare : ObservableObject
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
    public class DetailServiceInfo : ObservableObject
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

        public void UpdateItems(int serviceId, string itemName)
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
                        SteamInfo.Update(itemName);
                        break;
                    case 2:
                        CsmInfo.Update(itemName);
                        break;
                    case 3:
                        LfmInfo.Update(itemName);
                        break;
                    case 4 or 5:
                        BuffInfo.Update(itemName);
                        break;
                }
                IsBusy = false;
            });
        }
    }
    public class ServiceInfo : ObservableObject
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
    public class SteamInfo : ServiceInfo
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
        SteamItem _item = new();
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
        Tuple<int, int> _count = new(0, 0);
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
        Tuple<decimal, decimal> _avg = new(0, 0);

        public void Update(string itemName)
        {
            var data = ItemsBase.List.FirstOrDefault(x => x.ItemName == itemName).Steam;

            ItemBaseService.UpdateSteamItemHistory(itemName);
            LastSale = data.History.FirstOrDefault().Date;
            List<decimal> last30 = data.History.Where(x => x.Date > DateTime.Today.AddDays(-30)).Select(x => x.Price).ToList();
            List<decimal> last60 = data.History.Where(x => x.Date > DateTime.Today.AddDays(-60)).Select(x => x.Price).ToList();
            Count = Tuple.Create(last30.Count, last60.Count);
            decimal avg30 = last30.Any() ? Math.Round(Queryable.Average(last30.AsQueryable()), 2) : 0;
            decimal avg60 = last60.Any() ? Math.Round(Queryable.Average(last60.AsQueryable()), 2) : 0;
            Avg = Tuple.Create(avg30, avg60);
            Item = data;

            IsShow = true;
        }
    }
    public class CsmInfo : ServiceInfo
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
        CsmItem _item = new();
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
        int _currentItemId;
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
        int _maxValueSlide;
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
        int _valueSlide;
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
        InventoryCsm _currentItem;

        public void Update(string itemName)
        {
            ItemBaseService.UpdateCsmItem(itemName, true);
            Item = ItemsBase.List.FirstOrDefault(x => x.ItemName == itemName).Csm;
            CurrentItem = Item.Inventory.FirstOrDefault();
            MaxValueSlide = Item.Inventory.Count;
            ValueSlide = Item.Inventory.Any() ? 1 : 0;

            IsShow = true;
        }
    }
    public class LfmInfo : ServiceInfo
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
        LfmItem _item = new();

        public void Update(string itemName)
        {
            Item = ItemsBase.List.FirstOrDefault(x => x.ItemName == itemName).Lfm;
            IsShow = true;
        }
    }
    public class BuffInfo : ServiceInfo
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
        BuffItem _item = new();
        public decimal Avg
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
        decimal _avg;
        public decimal AvgOnlySale
        {
            get
            {
                return _avgOnlySale;
            }
            set
            {
                _avgOnlySale = value;
                OnPropertyChanged();
            }
        }
        decimal _avgOnlySale;

        public void Update(string itemName)
        {
            ItemBaseService.UpdateBuffItemHistory(itemName);
            Item = ItemsBase.List.FirstOrDefault(x => x.ItemName == itemName).Buff;
            LastSale = Item.History.FirstOrDefault().Date;
            var avg = Queryable.Average(Item.History.Select(x => x.Price).AsQueryable());
            Avg = Math.Round(avg, 2);
            avg = Queryable.Average(Item.History.Where(x => x.IsBuyOrder != true).Select(x => x.Price).AsQueryable());
            AvgOnlySale = Math.Round(avg, 2);

            IsShow = true;
        }
    }
}
