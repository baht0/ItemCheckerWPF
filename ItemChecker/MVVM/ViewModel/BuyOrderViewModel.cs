using ItemChecker.Core;
using ItemChecker.MVVM.Model;
using ItemChecker.Properties;
using ItemChecker.Services;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;

namespace ItemChecker.MVVM.ViewModel
{
    public class BuyOrderViewModel : MainViewModel
    {
        //DataGrid
        private ObservableCollection<OrderData> orderGrid;
        private OrderData _selectedOrderItem;
        //Order(services & tools)
        private Order _orderConfig;
        //list
        private int _favoriteListCount;
        private int _floatListCount;
        //statistic
        private string _currentService;
        private bool _pushService;
        private bool _favoriteService;
        private bool _floatService;
        private int _check;
        private int _push;
        private int _cancel;
        private int _successfulTrades;
        private int _purchasesMade;
        //tools stat
        private int _trades;
        private int _sellItems;
        private decimal _sum;
        private int _withdrawItems;
        //progress
        private int _currentProgress;
        private int _maxProgress;
        private string _timerText;
        private bool _timerVisible;

        //DataGrid
        public ObservableCollection<OrderData> OrderedGrid
        {
            get
            {
                return orderGrid;
            }
            set
            {
                orderGrid = value;
                OnPropertyChanged();
            }
        }
        public OrderData SelectedOrderItem
        {
            get
            {
                return _selectedOrderItem;
            }
            set
            {
                _selectedOrderItem = value;
                OnPropertyChanged("OrderConfig");
            }
        }
        //Order(services & tools)
        public Order OrderConfig
        {
            get
            {
                return _orderConfig;
            }
            set
            {
                _orderConfig = value;
                OnPropertyChanged("OrderConfig");
            }
        }
        //list
        public int FavoriteListCount
        {
            get
            {
                return _favoriteListCount;
            }
            set
            {
                _favoriteListCount = value;
                OnPropertyChanged();
            }
        }
        public int FloatListCount
        {
            get
            {
                return _floatListCount;
            }
            set
            {
                _floatListCount = value;
                OnPropertyChanged();
            }
        }
        //statistic
        public string CurrentService
        {
            get
            {
                return _currentService;
            }
            set
            {
                _currentService = value;
                OnPropertyChanged();
            }
        }
        public bool PushService
        {
            get
            {
                return _pushService;
            }
            set
            {
                _pushService = value;
                OnPropertyChanged();
            }
        }
        public bool FavoriteService
        {
            get
            {
                return _favoriteService;
            }
            set
            {
                _favoriteService = value;
                OnPropertyChanged();
            }
        }
        public bool FloatService
        {
            get
            {
                return _floatService;
            }
            set
            {
                _floatService = value;
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
        public int Cancel
        {
            get
            {
                return _cancel;
            }
            set
            {
                _cancel = value;
                OnPropertyChanged();
            }
        }
        public int SuccessfulTrades
        {
            get
            {
                return _successfulTrades;
            }
            set
            {
                _successfulTrades = value;
                OnPropertyChanged();
            }
        }
        public int PurchasesMade
        {
            get
            {
                return _purchasesMade;
            }
            set
            {
                _purchasesMade = value;
                OnPropertyChanged();
            }
        }
        //tools stat
        public int Trades
        {
            get
            {
                return _trades;
            }
            set
            {
                _trades = value;
                OnPropertyChanged();
            }
        }
        public int SellItems
        {
            get
            {
                return _sellItems;
            }
            set
            {
                _sellItems = value;
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
        public int WithdrawItems
        {
            get
            {
                return _withdrawItems;
            }
            set
            {
                _withdrawItems = value;
                OnPropertyChanged();
            }
        }
        //progress
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
        public string TimerText
        {
            get { return _timerText; }
            set
            {
                _timerText = value;
                OnPropertyChanged();
            }
        }
        public bool TimerVisible
        {
            get { return _timerVisible; }
            set
            {
                _timerVisible = value;
                OnPropertyChanged();
            }
        }

        public BuyOrderViewModel()
        {
            OrderedGrid = new ObservableCollection<OrderData>(Account.MyOrders);

            OrderConfig = new Order()
            {
                PushTimer = BuyOrderProperties.Default.TimerPush,

                FavoriteTimer = BuyOrderProperties.Default.TimerFavorite,
                MaxDeviation = BuyOrderProperties.Default.MaxDeviation,

                FloatTimer = BuyOrderProperties.Default.TimerFloat,
                MaxPrecent = BuyOrderProperties.Default.MaxPrecent,
                Compare = BuyOrderProperties.Default.Compare,
                ComparePrices = new List<string>()
                {
                    "Lowest ST",
                    "Median ST",
                    "Buy CSM"
                },
                MaxPrice = BuyOrderProperties.Default.MaxPrice
            };
            if (BuyOrderProperties.Default.FavoriteList != null)
                FavoriteListCount = BuyOrderProperties.Default.FavoriteList.Count;
            if (BuyOrderProperties.Default.FloatList != null)
                FloatListCount = BuyOrderProperties.Default.FloatList.Count;

            //statistic
            CurrentService = OrderStatistic.CurrentService;
            PushService = OrderStatistic.PushService;
            FavoriteService = OrderStatistic.FavoriteService;
            FloatService = OrderStatistic.FloatService;
            Check = OrderStatistic.Check;
            Push = OrderStatistic.Push;
            Cancel = OrderStatistic.Cancel;
            SuccessfulTrades = OrderStatistic.SuccessfulTrades;
            PurchasesMade = OrderStatistic.PurchasesMade;
        }

        public ICommand ReloadCommand =>
            new RelayCommand((obj) =>
            {
                Main.IsLoading = true;
                OrderCheckService orderCheck = new();
                orderCheck.SteamOrders();
                OrderedGrid = new ObservableCollection<OrderData>(Account.MyOrders);
                Main.IsLoading = false;
            }, (obj) => !Main.IsLoading);
        public ICommand AddFavoriteListCommand =>
            new RelayCommand((obj) =>
            {
                Task.Run(() =>
                {
                    FavoriteCheckService list = new();
                    List<string> response = list.SelectFile();
                    if (response.Any())
                    {
                        FavoriteListCount = response.Count;
                        BuyOrderProperties.Default.FavoriteList = response;
                        BuyOrderProperties.Default.Save();
                    }
                });
            }, (obj) => !FavoriteService);
        public ICommand AddFloatListCommand =>
            new RelayCommand((obj) =>
            {
                Task.Run(() =>
                {
                    FloatCheckService list = new();
                    List<string> response = list.SelectFile();
                    if (response.Any())
                    {
                        FloatListCount = response.Count;
                        BuyOrderProperties.Default.FloatList = response;
                        BuyOrderProperties.Default.Save();
                    }
                });
            }, (obj) => !FloatService);
        public ICommand PushCommand =>
            new RelayCommand((obj) => {
                Save("BuyOrder Pusher", true, false, false);
                Order config = obj as Order;
                BuyOrderProperties.Default.TimerPush = config.PushTimer;
                BuyOrderProperties.Default.Save();

                Main.Timer.Elapsed += timerTick;
                Main.TimerTick = config.PushTimer * 60;
                Main.Timer.Enabled = true;
            }, (obj) => Account.MyOrders.Any() & !Main.IsLoading & !Main.Timer.Enabled);
        public ICommand FavoriteCommand =>
            new RelayCommand((obj) =>
            {
                Save("Favorite Check", false, true, false);
                Order config = obj as Order;
                BuyOrderProperties.Default.TimerFavorite = config.FavoriteTimer;
                BuyOrderProperties.Default.MaxDeviation = config.MaxDeviation;
                BuyOrderProperties.Default.Save();

                Main.Timer.Elapsed += timerTick;
                Main.TimerTick = config.FavoriteTimer;
                Main.Timer.Enabled = true;
            }, (obj) => BuyOrderProperties.Default.FavoriteList != null & !Main.IsLoading & !Main.Timer.Enabled);
        public ICommand FloatCommand => 
            new RelayCommand((obj) =>
            {
                Save("Float Check", false, false, true);
                Order config = obj as Order;
                BuyOrderProperties.Default.TimerFloat = config.FloatTimer;
                BuyOrderProperties.Default.MaxPrecent = config.MaxPrecent;
                BuyOrderProperties.Default.Compare = config.Compare;
                BuyOrderProperties.Default.Save();

                Main.Timer.Elapsed += timerTick;
                Main.TimerTick = config.FloatTimer * 60;
                Main.Timer.Enabled = true;
            }, (obj) => BuyOrderProperties.Default.FloatList != null & !Main.IsLoading & !Main.Timer.Enabled);
        void Save(string service, bool isPush, bool isFavorite, bool isFloat)
        {
            OrderStatistic.CurrentService = service;
            OrderStatistic.PushService = isPush;
            OrderStatistic.FavoriteService = isFavorite;
            OrderStatistic.FloatService = isFloat;

            OrderStatistic.Check = 0;
            OrderStatistic.Push = 0;
            OrderStatistic.Cancel = 0;
            OrderStatistic.SuccessfulTrades = 0;
            OrderStatistic.PurchasesMade = 0;

            CurrentService = OrderStatistic.CurrentService;
            PushService = OrderStatistic.PushService;
            FavoriteService = OrderStatistic.FavoriteService;
            FloatService = OrderStatistic.FloatService;
            Check = OrderStatistic.Check;
            Push = OrderStatistic.Push;
            Cancel = OrderStatistic.Cancel;
            SuccessfulTrades = OrderStatistic.SuccessfulTrades;
            PurchasesMade = OrderStatistic.PurchasesMade;
        }
        void timerTick(Object sender, ElapsedEventArgs e)
        {
            TimerVisible = true;
            Main.TimerTick--;
            TimeSpan timeSpan = TimeSpan.FromSeconds(Main.TimerTick);
            TimerText = "Next check: " + timeSpan.ToString("mm':'ss");
            if (Main.TimerTick <= 0)
                TimerFineshed();
        }
        void TimerFineshed()
        {
            Main.Timer.Enabled = false;
            if (!Main.IsLoading)
            {
                Main.IsLoading = true;
                Main.cts = new();
                Main.token = Main.cts.Token;

                int TimeTick = 0;
                TimerText = "Preparation...";
                switch (OrderStatistic.CurrentService)
                {
                    case "BuyOrder Pusher":
                        TimeTick = BuyOrderProperties.Default.TimerPush * 60;
                        OrderPush();
                        break;
                    case "Favorite Check":
                        TimeTick = BuyOrderProperties.Default.TimerFavorite;
                        Favorite();
                        break;
                    case "Float Check":
                        TimeTick = BuyOrderProperties.Default.TimerFloat * 60;
                        Float();
                        break;
                }
                Main.IsLoading = false;
                Main.TimerTick = TimeTick;
                Main.Timer.Enabled = true;

                if (Main.token.IsCancellationRequested)
                    TimerStop();
            }
            else
                TimerStop();
        }

        void OrderPush()
        {
            try
            {
                OrderPushService pushOrder = new();
                pushOrder.preparationPush();

                UpdateInformation();

                MaxProgress = Account.MyOrders.Count;
                TimerText = "Pushing...";
                foreach (OrderData order in Account.MyOrders)
                {
                    try
                    {
                        pushOrder.PushItems(order);
                        Push = OrderStatistic.Push;
                        Cancel = OrderStatistic.Cancel;
                    }
                    catch (Exception exp)
                    {
                        BaseModel.errorLog(exp);
                        continue;
                    }
                    finally
                    {
                        CurrentProgress++;
                    }
                    if (Main.token.IsCancellationRequested)
                        break;
                }
                OrderCheckService check = new();
                check.SteamOrders();
                OrderedGrid = new ObservableCollection<OrderData>(Account.MyOrders);

                OrderStatistic.Check++;
                Check = OrderStatistic.Check;
            }
            catch (Exception exp)
            {
                BaseModel.errorLog(exp);
                BaseModel.errorMessage(exp);
            }
        }
        void Favorite()
        {
            try
            {
                FavoriteCheckService favoriteCheck = new();
                Account.GetInformations();
                UpdateInformation();

                MaxProgress = BuyOrderProperties.Default.FavoriteList.Count;
                TimerText = "Checking...";
                foreach (string item in BuyOrderProperties.Default.FavoriteList)
                {
                    try
                    {
                        favoriteCheck.checkFavorite(item);
                        SuccessfulTrades = OrderStatistic.SuccessfulTrades;
                    }
                    catch (Exception exp)
                    {
                        BaseModel.errorLog(exp);
                        continue;
                    }
                    finally
                    {
                        favoriteCheck.getTransactions();
                        CurrentProgress++;
                    }
                    if (Main.token.IsCancellationRequested)
                        break;
                }
                OrderStatistic.Check++;
                Check = OrderStatistic.Check;
                favoriteCheck.clearCart();
            }
            catch (Exception exp)
            {
                BaseModel.errorLog(exp);
                BaseModel.errorMessage(exp);
            }
        }
        void Float()
        {
            try
            {
                FloatCheckService floatCheck = new();
                Account.GetInformations();
                UpdateInformation();

                MaxProgress = BuyOrderProperties.Default.FloatList.Count;
                TimerText = "Checking...";
                foreach (string item in BuyOrderProperties.Default.FloatList)
                {
                    try
                    {
                        floatCheck.checkFloat(item);
                        PurchasesMade = OrderStatistic.PurchasesMade;
                    }
                    catch (Exception exp)
                    {
                        BaseModel.errorLog(exp);
                        continue;
                    }
                    finally
                    {
                        CurrentProgress++;
                    }
                    if (Main.token.IsCancellationRequested)
                        break;
                }
                OrderStatistic.Check++;
                Check = OrderStatistic.Check;
            }
            catch (Exception exp)
            {
                BaseModel.errorLog(exp);
                BaseModel.errorMessage(exp);
            }
        }
        void TimerStop()
        {
            TimerVisible = false;
            Save("Unknown", false, false, false);
            Main.Timer.Enabled = false;
            Main.TimerTick = 0;
            Main.Timer.Elapsed -= timerTick;
        }
        public ICommand StopCommand =>
            new RelayCommand((obj) =>
            {
                TimerStop();
                Main.cts.Cancel();
            }, (obj) => PushService | FavoriteService | FloatService);

        public ICommand TradeOfferCommand =>
            new RelayCommand((obj) =>
            {
                Task.Run(() => {
                    Main.IsLoading = true;
                    TradeOfferService tradeOffer = new();
                    do
                    {
                        MaxProgress = TradeOffer.TradeOffers.Count;
                        Trades += TradeOffer.TradeOffers.Count;
                        foreach (TradeOffer offer in TradeOffer.TradeOffers)
                        {
                            tradeOffer.acceptTrade(offer.TradeOfferId, offer.PartnerId);
                            CurrentProgress++;
                        }
                    }
                    while (tradeOffer.checkOffer());
                    Main.IsLoading = false;
                });
            }, (obj) => !Main.IsLoading & !Main.Timer.Enabled & (!PushService & !FavoriteService & !FloatService));
        public ICommand QuickSellCommand =>
            new RelayCommand((obj) =>
            {
                Task.Run(() => {
                    Main.IsLoading = true;
                    BuyOrderProperties.Default.MaxPrice = (int)obj;
                    BuyOrderProperties.Default.Save();

                    QuickSellService quickSell = new();
                    quickSell.checkInventory();
                    MaxProgress = QuickSell.SellItems.Count;
                    SellItems = QuickSell.SellItems.Count;
                    Sum = QuickSell.SellItems.Sum(s => s.Price);
                    foreach (QuickSell item in QuickSell.SellItems)
                    {
                        try
                        {
                            quickSell.sellItems(item);
                        }
                        finally
                        {
                            CurrentProgress++;
                            Thread.Sleep(1500);
                        }
                    }
                    Main.IsLoading = false;
                });
            }, (obj) => !Main.IsLoading & !Main.Timer.Enabled & (!PushService & !FavoriteService & !FloatService));
        public ICommand WithdrawCommand =>
            new RelayCommand((obj) =>
            {
                Task.Run(() => {
                    Main.IsLoading = true;

                    WithdrawService withdraw = new();
                    JArray inventory = withdraw.checkInventory();
                    JArray items = new();
                    if (inventory.Any())
                        items = withdraw.getItems(inventory);

                    if (!items.Any())
                        return;

                    MaxProgress = items.Count;
                    WithdrawItems = items.Count;
                    foreach (JObject item in items)
                    {
                        try
                        {
                            withdraw.withdrawItems(item);
                        }
                        finally
                        {
                            Thread.Sleep(1500);
                            CurrentProgress++;
                        }
                    }
                    Main.IsLoading = false;
                });
            }, (obj) => !Main.IsLoading & !Main.Timer.Enabled & (!PushService & !FavoriteService & !FloatService));

        public ICommand CancelOrderCommand =>
            new RelayCommand((obj) =>
            {
                OrderData item = obj as OrderData;
                MessageBoxResult result = MessageBox.Show($"Are you sure you want to cancel order?\n{item.ItemName}", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    OrderService order = new();
                    order.CancelOrder(item);

                    OrderedGrid = new ObservableCollection<OrderData>(Account.MyOrders);
                }
            });
    }
}