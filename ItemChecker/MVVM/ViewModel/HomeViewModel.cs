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
        private ObservableCollection<DataOrder> _orderedGrid = new(DataOrder.Orders);
        private DataOrder _selectedOrderItem;
        private HomeStatistics _homeStatistics = new();
        private HomeConfig _homeConfig = new();
        //favorite
        private string _selectedFavItem;

        public ObservableCollection<DataOrder> OrderedGrid
        {
            get
            {
                return _orderedGrid;
            }
            set
            {
                _orderedGrid = value;
                OnPropertyChanged();
            }
        } //DataGrid
        public DataOrder SelectedOrderItem
        {
            get
            {
                return _selectedOrderItem;
            }
            set
            {
                _selectedOrderItem = value;
                OnPropertyChanged();
            }
        }
        public HomeStatistics HomeStatistics
        {
            get
            {
                return _homeStatistics;
            }
            set
            {
                _homeStatistics = value;
                OnPropertyChanged();
            }
        }
        public HomeConfig HomeConfig
        {
            get
            {
                return _homeConfig;
            }
            set
            {
                _homeConfig = value;
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

        public HomeViewModel()
        {

        }
        //table
        public ICommand OrdersCommand =>
            new RelayCommand((obj) =>
            {
                if ((string)obj == "0")
                {
                    Task.Run(() => {
                        BaseModel.IsWorking = true;
                        SteamAccount.GetSteamBalance();
                        OrderCheckService orderCheck = new();
                        MainInfo.AvailableAmount = orderCheck.SteamOrders();
                        OrderedGrid = new ObservableCollection<DataOrder>(DataOrder.Orders);
                        BaseModel.IsWorking = false;
                    });
                }
                if ((string)obj == "1")
                {
                    MessageBoxResult result = MessageBox.Show(
                        "Do you really want to cancel all your orders?",
                        "Question",
                        MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                        Task.Run(() => {
                            BaseModel.IsWorking = true;
                            List<DataOrder> orders = new(DataOrder.Orders);
                            foreach (DataOrder order in orders)
                                OrderService.CancelOrder(order);
                            OrderedGrid = new ObservableCollection<DataOrder>(DataOrder.Orders);
                            MainInfo.AvailableAmount = SteamAccount.GetAvailableAmount();
                            BaseModel.IsWorking = false;
                        });
                }
            }, (obj) => !BaseModel.IsWorking);
        public ICommand CancelOrderCommand =>
            new RelayCommand((obj) =>
            {
                DataOrder item = obj as DataOrder;
                MessageBoxResult result = MessageBox.Show(
                    $"Are you sure you want to cancel order?\n{item.ItemName}",
                    "Question",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    OrderService.CancelOrder(item);
                    MainInfo.AvailableAmount = SteamAccount.GetAvailableAmount();
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
                        HomeStatistics.CsmListCount = response.Count;
                        HomeProperties.Default.CsmList = response;
                        HomeProperties.Default.Save();
                    }
                });
            }, (obj) => !HomeStatistics.CsmService);
        public ICommand AddFloatListCommand =>
            new RelayCommand((obj) =>
            {
                Task.Run(() =>
                {
                    FloatCheckService list = new();
                    List<string> response = list.SelectFile();
                    if (response.Any())
                    {
                        HomeStatistics.FloatListCount = response.Count;
                        HomeProperties.Default.FloatList = response;
                        HomeProperties.Default.Save();
                    }
                });
            }, (obj) => !HomeStatistics.FloatService);
        public ICommand PushCommand =>
            new RelayCommand((obj) =>
            {
                if (!HomeStatistics.PushService)
                {
                    HomeStatistics.PushService = true;
                    HomeStatistics.PushButton = "Stop";
                    HomeConfig config = obj as HomeConfig;
                    HomeProperties.Default.TimerPush = config.PushTimer;
                    HomeProperties.Default.Save();

                    HomeConfig.TimerPush.Elapsed += timerPushTick;
                    HomeConfig.TimerPushTick = config.PushTimer * 60;
                    HomeConfig.TimerPush.Enabled = true;
                }
                else
                {
                    HomeConfig.ctsPush.Cancel();
                    HomeStatistics.PushButton = "Play";
                    HomeStatistics.TimerPush = "Off";
                    HomeStatistics.PushService = false;
                    HomeConfig.TimerPush.Enabled = false;
                    HomeConfig.TimerPushTick = 0;
                    HomeConfig.TimerPush.Elapsed -= timerPushTick;
                }
            }, (obj) => DataOrder.Orders.Any());
        public ICommand CsmCommand =>
            new RelayCommand((obj) =>
            {
                if (!HomeStatistics.CsmService)
                {
                    HomeStatistics.CsmService = true;
                    HomeStatistics.CsmButton = "Stop";
                    HomeConfig config = obj as HomeConfig;
                    HomeProperties.Default.TimerCsm = config.CsmTimer;
                    HomeProperties.Default.MaxDeviation = config.MaxDeviation;
                    HomeProperties.Default.UserItems = config.UserItems;
                    HomeProperties.Default.Save();

                    HomeConfig.TimerCsm.Elapsed += timerCsmTick;
                    HomeConfig.TimerCsmTick = config.CsmTimer;
                    HomeConfig.TimerCsm.Enabled = true;
                }
                else
                {
                    HomeConfig.ctsCsm.Cancel();
                    HomeStatistics.CsmButton = "Play";
                    HomeStatistics.TimerCsm = "Off";
                    HomeStatistics.CsmService = false;
                    HomeConfig.TimerCsm.Enabled = false;
                    HomeConfig.TimerCsmTick = 0;
                    HomeConfig.TimerCsm.Elapsed -= timerCsmTick;
                }
            }, (obj) => HomeProperties.Default.CsmList != null);
        public ICommand FloatCommand => 
            new RelayCommand((obj) =>
            {
                if (!HomeStatistics.FloatService)
                {
                    HomeStatistics.FloatService = true;
                    HomeStatistics.FloatButton = "Stop";
                    HomeConfig config = obj as HomeConfig;
                    HomeProperties.Default.TimerFloat = config.FloatTimer;
                    HomeProperties.Default.MaxPrecent = config.MaxPrecent;
                    HomeProperties.Default.Compare = config.Compare;
                    HomeProperties.Default.Save();

                    HomeConfig.TimerFloat.Elapsed += timerFloatTick;
                    HomeConfig.TimerFloatTick = config.FloatTimer * 60;
                    HomeConfig.TimerFloat.Enabled = true;
                }
                else
                {
                    HomeConfig.ctsFloat.Cancel();
                    HomeStatistics.FloatButton = "Play";
                    HomeStatistics.TimerFloat = "Off";
                    HomeStatistics.FloatService = false;
                    HomeConfig.TimerFloat.Enabled = false;
                    HomeConfig.TimerFloatTick = 0;
                    HomeConfig.TimerFloat.Elapsed -= timerFloatTick;
                }
            }, (obj) => HomeProperties.Default.FloatList != null);
        void timerPushTick(Object sender, ElapsedEventArgs e)
        {
            HomeConfig.TimerPushTick--;
            TimeSpan timeSpan = TimeSpan.FromSeconds(HomeConfig.TimerPushTick);
            HomeStatistics.TimerPush = timeSpan.ToString("mm':'ss");
            if (HomeConfig.TimerPushTick <= 0)
            {
                HomeConfig.TimerPush.Enabled = false;
                HomeStatistics.TimerPush = "Preparation...";
                HomeStatistics.ProgressPush = 0;

                HomeConfig.ctsPush = new();
                HomeConfig.tokenPush = HomeConfig.ctsPush.Token;
                OrderPush();
            }
        }
        void timerCsmTick(Object sender, ElapsedEventArgs e)
        {
            HomeConfig.TimerCsmTick--;
            TimeSpan timeSpan = TimeSpan.FromSeconds(HomeConfig.TimerCsmTick);
            HomeStatistics.TimerCsm = timeSpan.ToString("mm':'ss");
            if (HomeConfig.TimerCsmTick <= 0)
            {
                HomeConfig.TimerCsm.Enabled = false;
                HomeStatistics.TimerCsm = "Preparation...";
                HomeStatistics.ProgressCsm = 0;

                HomeConfig.ctsCsm = new();
                HomeConfig.tokenCsm = HomeConfig.ctsCsm.Token;
                CsmCheck();
            }
        }
        void timerFloatTick(Object sender, ElapsedEventArgs e)
        {
            HomeConfig.TimerFloatTick--;
            TimeSpan timeSpan = TimeSpan.FromSeconds(HomeConfig.TimerFloatTick);
            HomeStatistics.TimerFloat = timeSpan.ToString("mm':'ss");
            if (HomeConfig.TimerFloatTick <= 0)
            {
                HomeConfig.TimerFloat.Enabled = false;
                HomeStatistics.TimerFloat = "Preparation...";
                HomeStatistics.ProgressFloat = 0;

                HomeConfig.ctsCsm = new();
                HomeConfig.tokenCsm = HomeConfig.ctsCsm.Token;
                FloatCheck();
            }
        }
        void OrderPush()
        {
            try
            {
                OrderPushService pushOrder = new();
                pushOrder.preparationPush();

                HomeStatistics.MaxProgressPush = DataOrder.Orders.Count;
                HomeStatistics.TimerPush = "Pushing...";
                List<DataOrder> cancelList = new();
                foreach (DataOrder order in DataOrder.Orders)
                {
                    try
                    {
                        Tuple<int, bool> response = pushOrder.PushItems(order);
                        HomeStatistics.Push += response.Item1;
                        if (response.Item2)
                            cancelList.Add(order);
                    }
                    catch (Exception exp)
                    {
                        BaseService.errorLog(exp);
                    }
                    finally
                    {
                        HomeStatistics.ProgressPush++;
                    }
                    if (HomeConfig.tokenPush.IsCancellationRequested)
                        break;
                }
                HomeStatistics.TimerPush = "Update...";
                foreach (DataOrder order in cancelList)
                {
                    OrderService.CancelOrder(order);
                    HomeStatistics.Cancel++;
                }
                OrderCheckService check = new();
                MainInfo.AvailableAmount = check.SteamOrders();
                OrderedGrid = new ObservableCollection<DataOrder>(DataOrder.Orders);

                HomeStatistics.CheckPush++;
            }
            catch (Exception exp)
            {
                BaseService.errorLog(exp);
                BaseService.errorMessage(exp);
            }
            finally
            {
                if (!HomeConfig.tokenPush.IsCancellationRequested)
                {
                    HomeConfig.TimerPushTick = HomeProperties.Default.TimerPush * 60;
                    HomeConfig.TimerPush.Enabled = true;
                }
            }
        }
        void CsmCheck()
        {
            try
            {
                BaseModel.IsBrowser = true;
                CsmCheckService csmCheck = new();
                BaseService.GetBase();
                //SteamAccount.GetCsmBalance();

                HomeStatistics.MaxProgressCsm = HomeProperties.Default.CsmList.Count;
                HomeStatistics.TimerCsm = "Checking...";
                foreach (string item in HomeProperties.Default.CsmList)
                {
                    try
                    {
                        HomeStatistics.SuccessfulTrades += csmCheck.checkCsm(item);
                    }
                    catch (Exception exp)
                    {
                        BaseService.errorLog(exp);
                    }
                    finally
                    {
                        csmCheck.getTransactions();
                        HomeStatistics.ProgressCsm++;
                    }
                    if (HomeConfig.tokenCsm.IsCancellationRequested)
                        break;
                }
                HomeStatistics.CheckCsm++;
                csmCheck.clearCart();
                BaseModel.IsBrowser = false;
            }
            catch (Exception exp)
            {
                BaseService.errorLog(exp);
                BaseService.errorMessage(exp);
            }
            finally
            {
                if (!HomeConfig.tokenCsm.IsCancellationRequested)
                {
                    HomeConfig.TimerCsmTick = HomeProperties.Default.TimerCsm;
                    HomeConfig.TimerCsm.Enabled = true;
                }
            }
        }
        void FloatCheck()
        {
            try
            {
                FloatCheckService floatCheck = new();
                BaseService.GetBase();
                SteamAccount.GetSteamBalance();

                HomeStatistics.MaxProgressFloat = HomeProperties.Default.FloatList.Count;
                HomeStatistics.TimerFloat = "Checking...";
                foreach (string item in HomeProperties.Default.FloatList)
                {
                    try
                    {
                        HomeStatistics.PurchasesMade += floatCheck.checkFloat(item);
                    }
                    catch (Exception exp)
                    {
                        BaseService.errorLog(exp);
                    }
                    finally
                    {
                        HomeStatistics.ProgressFloat++;
                    }
                    if (HomeConfig.tokenFloat.IsCancellationRequested)
                        break;
                }
                HomeStatistics.CheckFloat++;
            }
            catch (Exception exp)
            {
                BaseService.errorLog(exp);
                BaseService.errorMessage(exp);
            }
            finally
            {
                if (!HomeConfig.tokenFloat.IsCancellationRequested)
                {
                    HomeConfig.TimerFloatTick = HomeProperties.Default.TimerFloat * 60;
                    HomeConfig.TimerFloat.Enabled = true;
                }
            }
        }
        public ICommand TimerCommand =>
            new RelayCommand((obj) =>
            {
            switch ((int)obj)
                {
                    case 0:
                        HomeConfig.TimerPushTick = 1;
                        break;
                    case 1:
                        HomeConfig.TimerCsmTick = 1;
                        break;
                    case 2:
                        HomeConfig.TimerFloatTick = 1;
                        break;

                }
            }, (obj) => HomeStatistics.PushService | HomeStatistics.CsmService | HomeStatistics.FloatService);
        //tools
        public ICommand TradeOfferCommand =>
            new RelayCommand((obj) =>
            {
                Task.Run(() => {
                    HomeConfig.ctsTrade = new();
                    HomeConfig.tokenTrade = HomeConfig.ctsTrade.Token;

                    TradeOfferService tradeOffer = new();
                    do
                    {
                        HomeStatistics.ProgressTrade = 0;
                        HomeStatistics.MaxProgressTrade = DataTradeOffer.TradeOffers.Count;
                        HomeStatistics.Trades += DataTradeOffer.TradeOffers.Count;
                        foreach (DataTradeOffer offer in DataTradeOffer.TradeOffers)
                        {
                            try
                            {
                                tradeOffer.acceptTrade(offer.TradeOfferId, offer.PartnerId);
                            }
                            catch (Exception exp)
                            {
                                BaseService.errorLog(exp);
                            }
                            finally
                            {
                                HomeStatistics.ProgressTrade++;
                            }
                            if (HomeConfig.tokenTrade.IsCancellationRequested)
                                break;
                        }
                    }
                    while (tradeOffer.checkOffer());
                });
            }, (obj) => !HomeStatistics.TradeTool);
        public ICommand QuickSellCommand =>
            new RelayCommand((obj) =>
            {
                Task.Run(() => {
                    HomeConfig.ctsSale = new();
                    HomeConfig.tokenSale = HomeConfig.ctsSale.Token;
                    HomeProperties.Default.MaxPrice = (int)obj;
                    HomeProperties.Default.Save();


                    QuickSellService quickSell = new();
                    quickSell.checkInventory();

                    HomeStatistics.ProgressSell = 0;
                    HomeStatistics.MaxProgressSell = DataSell.SellItems.Count;
                    HomeStatistics.SellItems = DataSell.SellItems.Count;
                    HomeStatistics.Sum = DataSell.SellItems.Sum(s => s.Price);
                    foreach (DataSell item in DataSell.SellItems)
                    {
                        try
                        {
                            quickSell.sellItems(item);
                        }
                        catch (Exception exp)
                        {
                            BaseService.errorLog(exp);
                        }
                        finally
                        {
                            HomeStatistics.ProgressSell++;
                            Thread.Sleep(1500);
                        }
                        if (HomeConfig.tokenSale.IsCancellationRequested)
                            break;
                    }
                });
            }, (obj) => !HomeStatistics.SellTool);
        public ICommand WithdrawCommand =>
            new RelayCommand((obj) =>
            {
                Task.Run(() => {
                    BaseModel.IsBrowser = true;
                    HomeConfig.ctsWithdraw = new();
                    HomeConfig.tokenWithdraw = HomeConfig.ctsWithdraw.Token;

                    WithdrawService withdraw = new();
                    JArray inventory = withdraw.checkInventory();
                    JArray items = new();
                    if (inventory.Any() & !HomeConfig.tokenWithdraw.IsCancellationRequested)
                        items = withdraw.getItems(inventory);

                    if (!items.Any() | HomeConfig.tokenWithdraw.IsCancellationRequested)
                        return;

                    HomeStatistics.ProgressWithdraw = 0;
                    HomeStatistics.MaxProgressWithdraw = items.Count;
                    HomeStatistics.WithdrawItems = items.Count;
                    foreach (JObject item in items)
                    {
                        try
                        {
                            withdraw.withdrawItems(item);
                        }
                        catch (Exception exp)
                        {
                            BaseService.errorLog(exp);
                        }
                        finally
                        {
                            Thread.Sleep(1500);
                            HomeStatistics.ProgressWithdraw++;
                        }
                        if (HomeConfig.tokenWithdraw.IsCancellationRequested)
                            break;
                    }
                    BaseModel.IsBrowser = false;
                });
            }, (obj) => !BaseModel.IsBrowser & !HomeStatistics.WithdrawTool);
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
    }
}