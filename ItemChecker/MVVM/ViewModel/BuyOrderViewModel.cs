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
        private string _test;
        public string test
        {
            get
            {
                return _test;
            }
            set
            {
                _test = value;
                OnPropertyChanged();
            }
        }
        //DataGrid
        private ObservableCollection<OrderData> _orderGrid;
        private OrderData _selectedOrderItem;
        //Order(services & tools)
        private OrderSevices _servicesConfig;
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
        public ObservableCollection<OrderData> OrderedGrid
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
        public OrderSevices ServicesConfig
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

        public BuyOrderViewModel()
        {
            OrderedGrid = new ObservableCollection<OrderData>(Account.MyOrders);

            ServicesConfig = new OrderSevices()
            {
                PushTimer = BuyOrderProperties.Default.TimerPush,

                CsmTimer = BuyOrderProperties.Default.TimerCsm,
                MaxDeviation = BuyOrderProperties.Default.MaxDeviation,

                FloatTimer = BuyOrderProperties.Default.TimerFloat,
                MaxPrecent = BuyOrderProperties.Default.MaxPrecent,
                Compare = BuyOrderProperties.Default.Compare,
                ComparePrices = new ObservableCollection<string>()
                {
                    "Lowest ST",
                    "Median ST",
                    "Buy CSM"
                },
                MaxPrice = BuyOrderProperties.Default.MaxPrice
            };
            if (BuyOrderProperties.Default.CsmList != null)
                CsmListCount = BuyOrderProperties.Default.CsmList.Count;
            if (BuyOrderProperties.Default.FloatList != null)
                FloatListCount = BuyOrderProperties.Default.FloatList.Count;

            //statistic
            CurrentService = OrderStatistic.CurrentService;
            PushService = OrderStatistic.PushService;
            CsmService = OrderStatistic.CsmService;
            FloatService = OrderStatistic.FloatService;
            Check = OrderStatistic.Check;
            Push = OrderStatistic.Push;
            Cancel = OrderStatistic.Cancel;
            SuccessfulTrades = OrderStatistic.SuccessfulTrades;
            PurchasesMade = OrderStatistic.PurchasesMade;
        }
        //table
        public ICommand ReloadCommand =>
            new RelayCommand((obj) =>
            {
                Task.Run(() => {
                    Main.IsLoading = true;
                    OrderCheckService orderCheck = new();
                    orderCheck.SteamOrders();
                    OrderedGrid = new ObservableCollection<OrderData>(Account.MyOrders);
                    Main.IsLoading = false;
                });
            }, (obj) => !Main.IsLoading);
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
                        BuyOrderProperties.Default.CsmList = response;
                        BuyOrderProperties.Default.Save();
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
                        BuyOrderProperties.Default.FloatList = response;
                        BuyOrderProperties.Default.Save();
                    }
                });
            }, (obj) => !FloatService);
        public ICommand PushCommand =>
            new RelayCommand((obj) =>
            {
                Save("BuyOrder Pusher", true, false, false);
                OrderSevices config = obj as OrderSevices;
                BuyOrderProperties.Default.TimerPush = config.PushTimer;
                BuyOrderProperties.Default.Save();

                Main.Timer.Elapsed += timerTick;
                Main.TimerTick = config.PushTimer * 60;
                Main.Timer.Enabled = true;
            }, (obj) => Account.MyOrders.Any() & !Main.IsLoading & !Main.Timer.Enabled);
        public ICommand CsmCommand =>
            new RelayCommand((obj) =>
            {
                Save("Cs.Money Check", false, true, false);
                OrderSevices config = obj as OrderSevices;
                BuyOrderProperties.Default.TimerCsm = config.CsmTimer;
                BuyOrderProperties.Default.MaxDeviation = config.MaxDeviation;
                BuyOrderProperties.Default.Save();

                Main.Timer.Elapsed += timerTick;
                Main.TimerTick = config.CsmTimer;
                Main.Timer.Enabled = true;
            }, (obj) => BuyOrderProperties.Default.CsmList != null & !Main.IsLoading & !Main.Timer.Enabled & !GeneralProperties.Default.Guard);
        public ICommand FloatCommand => 
            new RelayCommand((obj) =>
            {
                Save("Float Check", false, false, true);
                OrderSevices config = obj as OrderSevices;
                BuyOrderProperties.Default.TimerFloat = config.FloatTimer;
                BuyOrderProperties.Default.MaxPrecent = config.MaxPrecent;
                BuyOrderProperties.Default.Compare = config.Compare;
                BuyOrderProperties.Default.Save();

                Main.Timer.Elapsed += timerTick;
                Main.TimerTick = config.FloatTimer * 60;
                Main.Timer.Enabled = true;
            }, (obj) => BuyOrderProperties.Default.FloatList != null & !Main.IsLoading & !Main.Timer.Enabled);
        void Save(string service, bool isPush, bool isCsm, bool isFloat)
        {
            OrderStatistic.CurrentService = service;
            OrderStatistic.PushService = isPush;
            OrderStatistic.CsmService = isCsm;
            OrderStatistic.FloatService = isFloat;

            OrderStatistic.Check = 0;
            OrderStatistic.Push = 0;
            OrderStatistic.Cancel = 0;
            OrderStatistic.SuccessfulTrades = 0;
            OrderStatistic.PurchasesMade = 0;

            CurrentService = OrderStatistic.CurrentService;
            PushService = OrderStatistic.PushService;
            CsmService = OrderStatistic.CsmService;
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
            TimerStatus = "Next check: " + timeSpan.ToString("mm':'ss");
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
                TimerStatus = "Preparation...";
                CurrentProgress = 0;
                switch (OrderStatistic.CurrentService)
                {
                    case "BuyOrder Pusher":
                        TimeTick = BuyOrderProperties.Default.TimerPush * 60;
                        OrderPush();
                        break;
                    case "Cs.Money Check":
                        TimeTick = BuyOrderProperties.Default.TimerCsm;
                        CsmCheck();
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
                TimerStatus = "Pushing...";
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
        void CsmCheck()
        {
            try
            {
                CsmCheckService csmCheck = new();
                Account.GetInformations();
                UpdateInformation();

                MaxProgress = BuyOrderProperties.Default.CsmList.Count;
                TimerStatus = "Checking...";
                foreach (string item in BuyOrderProperties.Default.CsmList)
                {
                    try
                    {
                        csmCheck.checkCsm(item);
                        SuccessfulTrades = OrderStatistic.SuccessfulTrades;
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
                    if (Main.token.IsCancellationRequested)
                        break;
                }
                OrderStatistic.Check++;
                Check = OrderStatistic.Check;
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
                Account.GetInformations();
                UpdateInformation();

                MaxProgress = BuyOrderProperties.Default.FloatList.Count;
                TimerStatus = "Checking...";
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
        public ICommand TimerCommand =>
            new RelayCommand((obj) =>
            {
                Main.TimerTick = 1;
            }, (obj) => (PushService | CsmService | FloatService) & Main.Timer.Enabled);
        public ICommand StopCommand =>
            new RelayCommand((obj) =>
            {
                TimerStop();
                Main.cts.Cancel();
            }, (obj) => PushService | CsmService | FloatService | TimerVisible);
        //tools
        public ICommand TradeOfferCommand =>
            new RelayCommand((obj) =>
            {
                Task.Run(() => {
                    Main.IsLoading = true;
                    Main.cts = new();
                    Main.token = Main.cts.Token;
                    TimerVisible = true;
                    TimerStatus = "Accept Trades...";

                    TradeOfferService tradeOffer = new();
                    do
                    {
                        CurrentProgress = 0;
                        MaxProgress = TradeOffer.TradeOffers.Count;
                        Trades += TradeOffer.TradeOffers.Count;
                        foreach (TradeOffer offer in TradeOffer.TradeOffers)
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
                            if (Main.token.IsCancellationRequested)
                                break;
                        }
                    }
                    while (tradeOffer.checkOffer());
                    TimerVisible = false;
                    Main.IsLoading = false;
                });
            }, (obj) => !Main.IsLoading & !Main.Timer.Enabled & (!PushService & !CsmService & !FloatService));
        public ICommand QuickSellCommand =>
            new RelayCommand((obj) =>
            {
                Task.Run(() => {
                    Main.IsLoading = true;
                    Main.cts = new();
                    Main.token = Main.cts.Token;
                    BuyOrderProperties.Default.MaxPrice = (int)obj;
                    BuyOrderProperties.Default.Save();

                    TimerVisible = true;
                    TimerStatus = "Quick Sell...";
                    CurrentProgress = 0;

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
                        catch (Exception exp)
                        {
                            BaseModel.errorLog(exp);
                        }
                        finally
                        {
                            CurrentProgress++;
                            Thread.Sleep(1500);
                        }
                        if (Main.token.IsCancellationRequested)
                            break;
                    }
                    TimerVisible = false;
                    Main.IsLoading = false;
                });
            }, (obj) => !Main.IsLoading & !Main.Timer.Enabled & (!PushService & !CsmService & !FloatService));
        public ICommand WithdrawCommand =>
            new RelayCommand((obj) =>
            {
                Task.Run(() => {
                    Main.IsLoading = true;
                    Main.cts = new();
                    Main.token = Main.cts.Token;

                    TimerVisible = true;
                    TimerStatus = "Withdrawing...";
                    WithdrawService withdraw = new();
                    JArray inventory = withdraw.checkInventory();
                    JArray items = new();
                    if (inventory.Any())
                        items = withdraw.getItems(inventory);

                    if (!items.Any())
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
                        if (Main.token.IsCancellationRequested)
                            break;
                    }
                    TimerVisible = false;
                    Main.IsLoading = false;
                });
            }, (obj) => !Main.IsLoading & !Main.Timer.Enabled & (!PushService & !CsmService & !FloatService) & !GeneralProperties.Default.Guard);
        //favorite
        public ICommand RemoveFavoriteCommand =>
            new RelayCommand((obj) =>
            {
                BuyOrderProperties.Default.FavoriteList.Remove((string)obj);
                BuyOrderProperties.Default.Save();
            }, (obj) => FavoriteList.Any());
        public ICommand ClearFavListCommand =>
            new RelayCommand((obj) =>
            {
                Application.Current.Dispatcher.Invoke(() => {
                    BuyOrderProperties.Default.FavoriteList.Clear();
                });
                BuyOrderProperties.Default.Save();
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
                BuyOrderProperties.Default.FavoriteList = FavoriteList;
                BuyOrderProperties.Default.Save();
            });
        public ICommand AddOrdersFavCommand =>
            new RelayCommand((obj) =>
            {
                var items = obj as ObservableCollection<OrderData>;
                foreach (var item in items)
                {
                    if (!FavoriteList.Contains(item.ItemName))
                    {
                        FavoriteList.Add(item.ItemName);
                    }
                }
                BuyOrderProperties.Default.FavoriteList = FavoriteList;
                BuyOrderProperties.Default.Save();
            }, (obj) => OrderedGrid.Any());
        public ICommand PlaceOrderFavCommand =>
            new RelayCommand((obj) =>
            {
                Main.IsLoading = true;
                Task.Run(() => {
                    PlaceOrder();
                });
            }, (obj) => FavoriteList.Any() & (!PushService | !CsmService | !FloatService) & !Main.IsLoading & !Main.Timer.Enabled);
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
            Main.IsLoading = false;
        }
    }
}