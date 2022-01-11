using ItemChecker.Net;
using ItemChecker.Properties;
using ItemChecker.Support;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace ItemChecker.MVVM.Model
{
    public class ParserQueue : BaseModel
    {
        private ObservableCollection<DataQueue> _items = new();
        private DataQueue _selectedQueue;
        private decimal _orderAmout = 0;

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

        public static ObservableCollection<DataQueue> AddQueue(ObservableCollection<DataQueue> items, string itemName, decimal price)
        {
            itemName = Edit.removeDoppler(itemName);
            if (ParserStatistics.DataCurrency == "USD")
                price = Math.Round(price * SettingsProperties.Default.CurrencyValue, 2);

            if (items.Any(n => n.ItemName == itemName) | DataOrder.Orders.Any(n => n.ItemName == itemName) | price > SteamAccount.Balance | items.Select(x => x.OrderPrice).Sum() + price > SteamAccount.GetAvailableAmount())
                return items;

            items.Add(new DataQueue(itemName, price));
            return items;
        }
        public static void PlaceOrder(string itemName)
        {
            var market_hash_name = Edit.MarketHashName(itemName);
            var item_nameid = ItemBase.SkinsBase.Where(x => x.ItemName == itemName).Select(x => x.SteamId).FirstOrDefault();
            JObject json = Get.ItemOrdersHistogram(item_nameid);
            decimal highest_buy_order = Convert.ToDecimal(json["highest_buy_order"]) / 100;

            if (SteamAccount.Balance > highest_buy_order)
                Post.CreateBuyOrder(SettingsProperties.Default.SteamCookies, market_hash_name, highest_buy_order);
        }
    }
}
