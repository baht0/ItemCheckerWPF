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
    public class HomeViewModel : MainViewModel
    {
        //DataGrid
        private ObservableCollection<DataOrder> _orderGrid;
        private DataOrder _selectedOrderItem;
        //Order(services & tools)
        private Home _servicesConfig;
        //list
        private int _csmListCount;
        private int _floatListCount;
        //statistic
        private string _currentService;
        private bool _pushService;
        private bool _csmService;
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
        //favorite
        private string _selectedFavItem;
        //progress
        private int _currentProgress;
        private int _maxProgress;
        private string _timerStatus;
        private bool _timerVisible;

        //DataGrid
        public ObservableCollection<DataOrder> OrderedGrid
        {
            get
            {
                return _orderGrid;
            }
            set
            {
                _orderGrid = value;
                OnPropertyChanged();
            }
        }
        public DataOrder SelectedOrderItem
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
        public Home ServicesConfig
        {
            get
            {
                return _servicesConfig;
            }
            set
            {
                _servicesConfig = value;
                OnPropertyChanged("OrderConfig");
            }
        }
        //list
        public int CsmListCount
        {
            get
            {
                return _csmListCount;
            }
            set
            {
                _csmListCount = value;
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
        public bool CsmService
        {
            get
            {
                return _csmService;
            }
            set
            {
                _csmService = value;
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
        //favorite
        public string SelectedFavItem
        {
            get { return _selectedFavItem; }
            set
            {
                _selectedFavItem = value;
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
        public string TimerStatus
        {
            get { return _timerStatus; }
            set
            {
                _timerStatus = value;
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

        public HomeViewModel()
        {
            OrderedGrid = new ObservableCollection<DataOrder>(DataOrder.Orders);

            ServicesConfig = new Home()
            {
                PushTimer = HomeProperties.Default.TimerPush,

                CsmTimer = HomeProperties.Default.TimerCsm,
                MaxDeviation = HomeProperties.Default.MaxDeviation,
                UserItems = HomeProperties.Default.UserItems,

                FloatTimer = HomeProperties.Default.TimerFloat,
                MaxPrecent = HomeProperties.Default.MaxPrecent,
                Compare = HomeProperties.Default.Compare,
                MaxPrice = HomeProperties.Default.MaxPrice
            };
            if (HomeProperties.Default.CsmList != null)
                CsmListCount = HomeProperties.Default.CsmList.Count;
            if (HomeProperties.Default.FloatList != null)
                FloatListCount = HomeProperties.Default.FloatList.Count;

            //statistic
            CurrentService = Home.CurrentService;
            PushService = Home.PushService;
            CsmService = Home.CsmService;
            FloatService = Home.FloatService;
            Check = Home.Check;
            Push = Home.Push;
            Cancel = Home.Cancel;
            SuccessfulTrades = Home.SuccessfulTrades;
            PurchasesMade = Home.PurchasesMade;
        }
        //table
        public ICommand ReloadCommand =>
            new RelayCommand((obj) =>
            {
                Task.Run(() => {
                    BaseModel.IsLoading = true;
                    OrderCheckService orderCheck = new();
                    orderCheck.SteamOrders();
                    OrderedGrid = new ObservableCollection<DataOrder>(DataOrder.Orders);
                    BaseModel.IsLoading = false;
                });
            }, (obj) => !BaseModel.IsLoading);
        public ICommand CancelOrderCommand =>
            new RelayCommand((obj) =>
            {
                DataOrder item = obj as DataOrder;
                MessageBoxResult result = MessageBox.Show($"Are you sure you want to cancel order?\n{item.ItemName}", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    OrderService order = new();
                    order.CancelOrder(item);

                    OrderedGrid = new ObservableCollection<DataOrder>(DataOrder.Orders);
                }
            });
        //services
        public ICommand AddCsmListCommand =>
            new RelayCommand((obj) =>
            {
                Task.Run(() =>
                {
                    CsmCheckService list = new();
                    List<string> response = list.SelectFile();
                    if (response.Any())
                    {
                        CsmListCount = response.Count;
                        HomeProperties.Default.CsmList = response;
                        HomeProperties.Default.Save();
                    }
                });
            }, (obj) => !CsmService);
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
                        HomeProperties.Default.FloatList = response;
                        HomeProperties.Default.Save();
                    }
                });
            }, (obj) => !FloatService);
        public ICommand PushCommand =>
            new RelayCommand((obj) =>
            {
                Save("BuyOrder Pusher", true, false, false);
                Home config = obj as Home;
                HomeProperties.Default.TimerPush = config.PushTimer;
                HomeProperties.Default.Save();

                BaseModel.Timer.Elapsed += timerTick;
                BaseModel.TimerTick = config.PushTimer * 60;
                BaseModel.Timer.Enabled = true;
            }, (obj) => DataOrder.Orders.Any() & !BaseModel.IsLoading & !BaseModel.Timer.Enabled);
        public ICommand CsmCommand =>
            new RelayCommand((obj) =>
            {
                Save("Cs.Money Check", false, true, false);
                Home config = obj as Home;
                HomeProperties.Default.TimerCsm = config.CsmTimer;
                HomeProperties.Default.MaxDeviation = config.MaxDeviation;
                HomeProperties.Default.UserItems = config.UserItems;
                HomeProperties.Default.Save();

                BaseModel.Timer.Elapsed += timerTick;
                BaseModel.TimerTick = config.CsmTimer;
                BaseModel.Timer.Enabled = true;
            }, (obj) => HomeProperties.Default.CsmList != null & !BaseModel.IsLoading & !BaseModel.Timer.Enabled & !GeneralProperties.Default.Guard);
        public ICommand FloatCommand => 
            new RelayCommand((obj) =>
            {
                Save("Float Check", false, false, true);
                Home config = obj as Home;
                HomeProperties.Default.TimerFloat = config.FloatTimer;
                HomeProperties.Default.MaxPrecent = config.MaxPrecent;
                HomeProperties.Default.Compare = config.Compare;
                HomeProperties.Default.Save();

                BaseModel.Timer.Elapsed += timerTick;
                BaseModel.TimerTick = config.FloatTimer * 60;
                BaseModel.Timer.Enabled = true;
            }, (obj) => HomeProperties.Default.FloatList != null & !BaseModel.IsLoading & !BaseModel.Timer.Enabled);
        void Save(string service, bool isPush, bool isCsm, bool isFloat)
        {
            Home.CurrentService = service;
            Home.PushService = isPush;
            Home.CsmService = isCsm;
            Home.FloatService = isFloat;

            Home.Check = 0;
            Home.Push = 0;
            Home.Cancel = 0;
            Home.SuccessfulTrades = 0;
            Home.PurchasesMade = 0;

            CurrentService = Home.CurrentService;
            PushService = Home.PushService;
            CsmService = Home.CsmService;
            FloatService = Home.FloatService;
            Check = Home.Check;
            Push = Home.Push;
            Cancel = Home.Cancel;
            SuccessfulTrades = Home.SuccessfulTrades;
            PurchasesMade = Home.PurchasesMade;
        }
        void timerTick(Object sender, ElapsedEventArgs e)
        {
            TimerVisible = true;
            BaseModel.TimerTick--;
            TimeSpan timeSpan = TimeSpan.FromSeconds(BaseModel.TimerTick);
            TimerStatus = "Next check: " + timeSpan.ToString("mm':'ss");
            if (BaseModel.TimerTick <= 0)
                TimerFineshed();
        }
        void TimerFineshed()
        {
            BaseModel.Timer.Enabled = false;
            if (!BaseModel.IsLoading)
            {
                BaseModel.IsLoading = true;
                BaseModel.cts = new();
                BaseModel.token = BaseModel.cts.Token;

                int TimeTick = 0;
                TimerStatus = "Preparation...";
                CurrentProgress = 0;
                switch (Home.CurrentService)
                {
                    case "BuyOrder Pusher":
                        TimeTick = HomeProperties.Default.TimerPush * 60;
                        OrderPush();
                        break;
                    case "Cs.Money Check":
                        TimeTick = HomeProperties.Default.TimerCsm;
                        CsmCheck();
                        break;
                    case "Float Check":
                        TimeTick = HomeProperties.Default.TimerFloat * 60;
                        Float();
                        break;
                }
                BaseModel.IsLoading = false;
                BaseModel.TimerTick = TimeTick;
                BaseModel.Timer.Enabled = true;

                if (BaseModel.token.IsCancellationRequested)
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

                MaxProgress = DataOrder.Orders.Count;
                TimerStatus = "Pushing...";
                foreach (DataOrder order in DataOrder.Orders)
                {
                    try
                    {
                        pushOrder.PushItems(order);
                        Push = Home.Push;
                        Cancel = Home.Cancel;
                    }
                    catch (Exception exp)
                    {
                        BaseModel.errorLog(exp);
                    }
                    finally
                    {
                        CurrentProgress++;
                    }
                    if (BaseModel.token.IsCancellationRequested)
                        break;
                }
                OrderCheckService check = new();
                check.SteamOrders();
                OrderedGrid = new ObservableCollection<DataOrder>(DataOrder.Orders);

                Home.Check++;
                Check = Home.Check;
            }
            catch (Exception exp)
            {
                BaseModel.errorLog(exp);
                BaseModel.errorMessage(exp);
            }
        }
        void CsmCheck()
        {
            try
            {
                CsmCheckService csmCheck = new();
                Account.GetBase();
                Account.GetCsmBalance();
                UpdateInformation();

                MaxProgress = HomeProperties.Default.CsmList.Count;
                TimerStatus = "Checking...";
                foreach (string item in HomeProperties.Default.CsmList)
                {
                    try
                    {
                        csmCheck.checkCsm(item);
                        SuccessfulTrades = Home.SuccessfulTrades;
                    }
                    catch (Exception exp)
                    {
                        BaseModel.errorLog(exp);
                    }
                    finally
                    {
                        csmCheck.getTransactions();
                        CurrentProgress++;
                    }
                    if (BaseModel.token.IsCancellationRequested)
                        break;
                }
                Home.Check++;
                Check = Home.Check;
                csmCheck.clearCart();
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
                Account.GetBase();
                Account.GetSteamAccount();
                UpdateInformation();

                MaxProgress = HomeProperties.Default.FloatList.Count;
                TimerStatus = "Checking...";
                foreach (string item in HomeProperties.Default.FloatList)
                {
                    try
                    {
                        floatCheck.checkFloat(item);
                        PurchasesMade = Home.PurchasesMade;
                    }
                    catch (Exception exp)
                    {
                        BaseModel.errorLog(exp);
                    }
                    finally
                    {
                        CurrentProgress++;
                    }
                    if (BaseModel.token.IsCancellationRequested)
                        break;
                }
                Home.Check++;
                Check = Home.Check;
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
            BaseModel.Timer.Enabled = false;
            BaseModel.TimerTick = 0;
            BaseModel.Timer.Elapsed -= timerTick;
        }
        public ICommand TimerCommand =>
            new RelayCommand((obj) =>
            {
                BaseModel.TimerTick = 1;
            }, (obj) => (PushService | CsmService | FloatService) & BaseModel.Timer.Enabled);
        public ICommand StopCommand =>
            new RelayCommand((obj) =>
            {
                TimerStop();
                BaseModel.cts.Cancel();
            }, (obj) => PushService | CsmService | FloatService | TimerVisible);
        //tools
        public ICommand TradeOfferCommand =>
            new RelayCommand((obj) =>
            {
                Task.Run(() => {
                    BaseModel.IsLoading = true;
                    BaseModel.cts = new();
                    BaseModel.token = BaseModel.cts.Token;
                    TimerVisible = true;
                    TimerStatus = "Accept Trades...";

                    TradeOfferService tradeOffer = new();
                    do
                    {
                        CurrentProgress = 0;
                        MaxProgress = DataTradeOffer.TradeOffers.Count;
                        Trades += DataTradeOffer.TradeOffers.Count;
                        foreach (DataTradeOffer offer in DataTradeOffer.TradeOffers)
                        {
                            try
                            {
                                tradeOffer.acceptTrade(offer.TradeOfferId, offer.PartnerId);
                            }
                            catch (Exception exp)
                            {
                                BaseModel.errorLog(exp);
                            }
                            finally
                            {
                                CurrentProgress++;
                            }
                            if (BaseModel.token.IsCancellationRequested)
                                break;
                        }
                    }
                    while (tradeOffer.checkOffer());
                    TimerVisible = false;
                    BaseModel.IsLoading = false;
                });
            }, (obj) => !BaseModel.IsLoading & !BaseModel.Timer.Enabled & (!PushService & !CsmService & !FloatService));
        public ICommand QuickSellCommand =>
            new RelayCommand((obj) =>
            {
                Task.Run(() => {
                    BaseModel.IsLoading = true;
                    BaseModel.cts = new();
                    BaseModel.token = BaseModel.cts.Token;
                    HomeProperties.Default.MaxPrice = (int)obj;
                    HomeProperties.Default.Save();

                    TimerVisible = true;
                    TimerStatus = "Quick Sell...";
                    CurrentProgress = 0;

                    QuickSellService quickSell = new();
                    quickSell.checkInventory();

                    MaxProgress = DataSell.SellItems.Count;
                    SellItems = DataSell.SellItems.Count;
                    Sum = DataSell.SellItems.Sum(s => s.Price);
                    foreach (DataSell item in DataSell.SellItems)
                    {
                        try
                        {
                            quickSell.sellItems(item);
                        }
                        catch (Exception exp)
                        {
                            BaseModel.errorLog(exp);
                        }
                        finally
                        {
                            CurrentProgress++;
                            Thread.Sleep(1500);
                        }
                        if (BaseModel.token.IsCancellationRequested)
                            break;
                    }
                    TimerVisible = false;
                    BaseModel.IsLoading = false;
                });
            }, (obj) => !BaseModel.IsLoading & !BaseModel.Timer.Enabled & (!PushService & !CsmService & !FloatService));
        public ICommand WithdrawCommand =>
            new RelayCommand((obj) =>
            {
                Task.Run(() => {
                    BaseModel.IsLoading = true;
                    BaseModel.cts = new();
                    BaseModel.token = BaseModel.cts.Token;

                    TimerVisible = true;
                    TimerStatus = "Withdrawing...";
                    WithdrawService withdraw = new();
                    JArray inventory = withdraw.checkInventory();
                    JArray items = new();
                    if (inventory.Any() & !BaseModel.token.IsCancellationRequested)
                        items = withdraw.getItems(inventory);

                    if (!items.Any() | BaseModel.token.IsCancellationRequested)
                        return;

                    CurrentProgress = 0;
                    MaxProgress = items.Count;
                    WithdrawItems = items.Count;
                    foreach (JObject item in items)
                    {
                        try
                        {
                            withdraw.withdrawItems(item);
                        }
                        catch (Exception exp)
                        {
                            BaseModel.errorLog(exp);
                        }
                        finally
                        {
                            Thread.Sleep(1500);
                            CurrentProgress++;
                        }
                        if (BaseModel.token.IsCancellationRequested)
                            break;
                    }
                    TimerVisible = false;
                    BaseModel.IsLoading = false;
                });
            }, (obj) => !BaseModel.IsLoading & !BaseModel.Timer.Enabled & (!PushService & !CsmService & !FloatService) & !GeneralProperties.Default.Guard);
        //favorite
        public ICommand RemoveFavoriteCommand =>
            new RelayCommand((obj) =>
            {
                HomeProperties.Default.FavoriteList.Remove((string)obj);
                HomeProperties.Default.Save();
            }, (obj) => FavoriteList.Any() & !String.IsNullOrEmpty(SelectedFavItem));
        public ICommand ClearFavListCommand =>
            new RelayCommand((obj) =>
            {
                Application.Current.Dispatcher.Invoke(() => {
                    HomeProperties.Default.FavoriteList.Clear();
                });
                HomeProperties.Default.Save();
            }, (obj) => FavoriteList.Any());
        public ICommand ExportFavCommand =>
            new RelayCommand((obj) =>
            {
                FavoriteService.ExportTxt(FavoriteList);
            }, (obj) => FavoriteList.Any());
        public ICommand ImportFavCommand =>
            new RelayCommand((obj) =>
            {
                FavoriteService favorite = new();
                FavoriteList = favorite.ImportTxt();
                HomeProperties.Default.FavoriteList = FavoriteList;
                HomeProperties.Default.Save();
            });
        public ICommand AddOrdersFavCommand =>
            new RelayCommand((obj) =>
            {
                var items = obj as ObservableCollection<DataOrder>;

                foreach (var item in items)
                    if (!FavoriteList.Contains(item.ItemName))
                        FavoriteList.Add(item.ItemName);

                HomeProperties.Default.FavoriteList = FavoriteList;
                HomeProperties.Default.Save();
            }, (obj) => OrderedGrid.Any());
        public ICommand PlaceOrderFavCommand =>
            new RelayCommand((obj) =>
            {
                BaseModel.IsLoading = true;
                Task.Run(() => {
                    PlaceOrder();
                });
            }, (obj) => FavoriteList.Any() & (!PushService | !CsmService | !FloatService) & !BaseModel.IsLoading & !BaseModel.Timer.Enabled);
        void PlaceOrder()
        {
            PlaceOrderService placeOrder = new();
            MaxProgress = FavoriteList.Count;
            CurrentProgress = 0;
            foreach (string itemName in FavoriteList)
            {
                try
                {
                    placeOrder.PlaceOrder(itemName);
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
            }
            BaseModel.IsLoading = false;
        }
    }
}