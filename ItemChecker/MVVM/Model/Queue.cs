using ItemChecker.Core;
using ItemChecker.Net;
using ItemChecker.Properties;
using ItemChecker.Support;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;

namespace ItemChecker.MVVM.Model
{
    public class Queue : BaseModel
    {
        public static ObservableCollection<QueueData> AddQueue(ObservableCollection<QueueData> items, DataParser item)
        {
            string itemName = Edit.removeDoppler(item.ItemName);
            decimal price = item.Price2;
            if (ParserStatistics.DataCurrency == "USD")
                price = Math.Round(price * SettingsProperties.Default.CurrencyValue, 2);

            if (items.Any(n => n.ItemName == itemName) ||
                items.Select(x => x.OrderPrice).Sum() + price > SteamAccount.GetAvailableAmount() ||
                DataOrder.Orders.Any(n => n.ItemName == itemName) ||
                price > SteamAccount.Balance ||
                item.Precent < SettingsProperties.Default.MinPrecent)
                return items;

            items.Add(new QueueData(itemName, price, item.Precent));
            return items;
        }
        public static void PlaceOrder(string itemName)
        {
            var market_hash_name = HttpUtility.UrlEncode(itemName);
            var item_nameid = ItemBase.SkinsBase.FirstOrDefault(x => x.ItemName == itemName).SteamInfo.Id;
            JObject json = Get.ItemOrdersHistogram(item_nameid);
            decimal highest_buy_order = Convert.ToDecimal(json["highest_buy_order"]) / 100;

            if (SteamAccount.Balance > highest_buy_order)
                Post.CreateBuyOrder(SteamCookies, market_hash_name, highest_buy_order);
        }
    }
    public class QueueInfo : ObservableObject
    {
        private ObservableCollection<QueueData> _items = new();
        private QueueData _selectedQueue;
        private decimal _totalAllowed = 0;
        private decimal _availableAmount = 0;
        private decimal _orderAmout = 0;
        private decimal _availableAmountPrecent = 0;
        private decimal _remaining = 0;

        public ObservableCollection<QueueData> Items
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
        public QueueData SelectedQueue
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
    }
    public class QueueData
    {
        public string ItemName { get; set; }
        public decimal OrderPrice { get; set; }
        public decimal Precent { get; set; }

        public QueueData(string itemName, decimal orderPrice, decimal precent)
        {
            ItemName = itemName;
            OrderPrice = orderPrice;
            Precent = precent;
        }
    }
}
