using ItemChecker.Net;
using ItemChecker.Properties;
using ItemChecker.Support;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ItemChecker.MVVM.Model
{
    public class ToolPlaceOrder : BaseTable<DataQueue>
    {
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

        public void Start(int serviceId)
        {
            IsBusy = true;
            Task.Run(() => { MainTask(serviceId); });
        }
        public static decimal PlaceOrder(string itemName, int serviceTwoId)
        {
            ItemBaseService.UpdateSteamItem(itemName, SteamAccount.Currency.Id);
            var item = ItemsBase.List.FirstOrDefault(x => x.ItemName == itemName).Steam;

            if (SteamAccount.Balance > item.HighestBuyOrder)
            {
                SteamRequest.Post.CreateBuyOrder(itemName, item.HighestBuyOrder, SteamAccount.Currency.Id);
                SavedItems.Reserve.Add(new(itemName, serviceTwoId));
                return item.HighestBuyOrder;
            }
            return 0;
        }
        public void AddQueue(DataQueue item)
        {
            decimal sum = Items.Select(x => x.OrderPrice).Sum();

            bool isAllow = !Items.Any(x => x.ItemName == item.ItemName);
            if (isAllow)
                isAllow = !SteamAccount.Orders.Any(n => n.ItemName == item.ItemName);
            if (isAllow)
                isAllow = (sum + item.OrderPrice) < SteamAccount.Orders.GetAvailableAmount();
            if (isAllow)
                isAllow = item.OrderPrice < SteamAccount.Balance;
            if (isAllow)
                isAllow = item.Precent > HomeProperties.Default.MinPrecent;

            if (isAllow)
            {
                Items.Add(item);
                Main.Message.Enqueue($"Success, added to Queue.\n{item.ItemName}");
                Update();
                return;
            }
            Main.Message.Enqueue("Not successful. Conditions not met.");
        }
        public void Update()
        {
            decimal availableAmount = SteamAccount.Orders.GetAvailableAmount();
            TotalAllowed = SteamAccount.MaxOrderAmount;
            AvailableAmount = availableAmount;
            OrderAmout = Items.Select(x => x.OrderPrice).Sum();
            Remaining = availableAmount - OrderAmout;
            AvailableAmountPrecent = Math.Round(availableAmount / SteamAccount.MaxOrderAmount * 100, 1);
        }

        void MainTask(int serviceId)
        {
            try
            {
                SteamAccount.GetBalance();

                int success = 0;
                MaxProgress = Items.Count;
                CurrentProgress = 0;
                foreach (var item in Items)
                {
                    try
                    {
                        PlaceOrder(item.ItemName, serviceId);
                        success++;
                    }
                    catch (Exception exp)
                    {
                        if (!exp.Message.Contains("429"))
                            BaseModel.ErrorLog(exp, true);
                        else
                            MessageBox.Show(exp.Message + "\n\nTo continue, you need to change the IP address.", "PlaceOrder stoped!", MessageBoxButton.OK, MessageBoxImage.Warning);
                        break;
                    }
                    finally
                    {
                        CurrentProgress++;
                    }
                }
                Main.Notifications.Add(new()
                {
                    Title = "Place Order",
                    Message = $"{success}/{Items.Count} orders has been placed."
                });
                Items = new();
            }
            catch (Exception ex)
            {
                BaseModel.ErrorLog(ex, true);
            }
            finally
            {
                IsBusy = false;
                Update();
            }
        }
    }
    public class DataQueue
    {
        public string ItemName { get; set; }
        public decimal OrderPrice { get; set; }
        public decimal Precent { get; set; }

        public DataQueue(DataParser data, DataCurrency parserCurrency)
        {
            var currency = SteamAccount.Currency.Id;
            var price = data.Purchase;
            if (parserCurrency.Id != 1)
                price = Currency.ConverterToUsd(price, parserCurrency.Id);
            price = Currency.ConverterFromUsd(price, currency);

            ItemName = data.ItemName;
            OrderPrice = price;
            Precent = data.Precent;
        }
    }
}
