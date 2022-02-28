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
    public class HomeViewModel : ObservableObject
    {
        #region Properties
        System.Timers.Timer TimerView = new(500);
        private ObservableCollection<DataOrder> _orderedGrid = new(DataOrder.Orders);
        private DataOrder _selectedOrderItem;
        private HomeStatistics _homeStatistics = new();
        private HomeConfig _homeConfig = new();
        //favorite
        private ObservableCollection<string> _favoriteList = HomeProperties.Default.FavoriteList ?? (new());
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
        public ObservableCollection<string> FavoriteList
        {
            get { return _favoriteList; }
            set
            {
                _favoriteList = value;
                OnPropertyChanged();
            }
        }
        public string SelectedFavItem
        {
            get { return _selectedFavItem; }
            set
            {
                _selectedFavItem = value;
                OnPropertyChanged();
            }
        }
        #endregion

        public HomeViewModel()
        {
            TimerView.Elapsed += UpdateView;
            TimerView.Enabled = true;
        }
        void UpdateView(Object sender, ElapsedEventArgs e)
        {
            try
            {
                FavoriteList = HomeProperties.Default.FavoriteList;
            }
            catch (Exception ex)
            {
                BaseService.errorLog(ex);
            }
        }
        //table
        public ICommand OrdersCommand =>
            new RelayCommand((obj) =>
            {
                switch (Convert.ToInt32(obj))
                {
                    case 1:
                        BaseModel.IsWorking = true;
                        Task.Run(() => {
                            OrderCheckService orderCheck = new();
                            orderCheck.SteamOrders();
                            OrderedGrid = new ObservableCollection<DataOrder>(DataOrder.Orders);
                            BaseModel.IsWorking = false;
                        });
                        break;
                    case 2:
                        MessageBoxResult result = MessageBox.Show(
                            "Do you really want to cancel all your orders?", 
                            "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (result == MessageBoxResult.Yes)
                            Task.Run(() => {
                                BaseModel.IsWorking = true;
                                List<DataOrder> orders = new(DataOrder.Orders);
                                foreach (DataOrder order in orders)
                                    OrderService.CancelOrder(order);
                                OrderedGrid = new ObservableCollection<DataOrder>(DataOrder.Orders);
                                SteamAccount.GetAvailableAmount();
                                BaseModel.IsWorking = false;
                            });
                        break;
                }
            }, (obj) => !BaseModel.IsWorking);
        public ICommand CancelOrderCommand =>
            new RelayCommand((obj) =>
            {
                DataOrder item = obj as DataOrder;
                MessageBoxResult result = MessageBox.Show(
                    $"Are you sure you want to cancel order?\n{item.ItemName}",
                    "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);            

                if (result == MessageBoxResult.Yes)
                {
                    OrderService.CancelOrder(item);
                    OrderedGrid = new ObservableCollection<DataOrder>(DataOrder.Orders);
                    Main.Message.Enqueue($"{item.ItemName}\nOrder has been canceled.");
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
                    HomeConfig config = obj as HomeConfig;
                    HomeProperties.Default.TimerPush = config.PushTimer;
                    HomeProperties.Default.Reserve = config.Reserve;
                    HomeProperties.Default.Unwanted = config.Unwanted;
                    HomeProperties.Default.Save();

                    HomeConfig.TimerPush.Elapsed += timerPushTick;
                    HomeConfig.TimerPushTick = config.PushTimer * 60;
                    HomeConfig.TimerPush.Enabled = true;
                }
                else
                {
                    HomeConfig.ctsPush.Cancel();
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
                HomeStatistics.TimerPush = "Preparation...";
                HomeConfig.TimerPush.Enabled = false;
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
                HomeStatistics.TimerCsm = "Preparation...";
                HomeConfig.TimerCsm.Enabled = false;
                HomeStatistics.ProgressCsm = 0;

                BaseModel.IsBrowser = true;
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
                HomeStatistics.TimerFloat = "Preparation...";
                HomeConfig.TimerFloat.Enabled = false;
                HomeStatistics.ProgressFloat = 0;

                HomeConfig.ctsFloat = new();
                HomeConfig.tokenFloat = HomeConfig.ctsFloat.Token;
                FloatCheck();
            }
        }
        void OrderPush()
        {
            try
            {
                SteamAccount.GetSteamBalance();
                OrderCheckService orderCheck = new();
                orderCheck.SteamOrders();
                OrderPushService pushOrder = new();

                HomeStatistics.TimerPush = "Pushing...";
                HomeStatistics.MaxProgressPush = DataOrder.Orders.Count;
                foreach (DataOrder order in DataOrder.Orders)
                {
                    try
                    {
                        HomeStatistics.Push += pushOrder.PushItems(order) ? 1 : 0;
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
                decimal availableAmount = SteamAccount.GetAvailableAmount();
                if (HomeStatistics.CheckPush % 5 == 4 && availableAmount >= SteamAccount.Balance * 10 * 0.15m)
                {
                    FavoriteService favorite = new();
                    int count = favorite.PlaceOrderFav(availableAmount);
                    if (count > 0)
                        Main.Notifications.Add(new()
                        {
                            Title = "BuyOrderPush",
                            Message = $"{count} orders were placed in the last push.",
                        });
                }
                orderCheck.SteamOrders();
                OrderedGrid = new(DataOrder.Orders);
                HomeStatistics.CheckPush++;
            }
            catch (Exception exp)
            {
                HomeConfig.ctsPush.Cancel();
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
                if (BaseModel.Browser == null)
                    BaseModel.OpenBrowser();
                bool isLogin = false;
                do isLogin = CsmAccount.LoginCsm();
                while (!isLogin);

                CsmCheckService csmCheck = new();
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
                else
                {
                    BaseModel.IsBrowser = false;
                    if (SettingsProperties.Default.Quit)
                    {
                        BaseModel.Browser.Quit();
                        BaseModel.Browser = null;
                    }
                }
            }
        }
        void FloatCheck()
        {
            try
            {
                FloatCheckService floatCheck = new();
                BaseService.GetCurrency();
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
                HomeConfig.ctsFloat.Cancel();
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
            }, (obj) => HomeStatistics.PushService || HomeStatistics.CsmService || HomeStatistics.FloatService);
        //tools
        public ICommand TradeOfferCommand =>
            new RelayCommand((obj) =>
            {
                if (!HomeStatistics.TradeTool)
                {
                    HomeStatistics.TradeTool = true;
                    HomeConfig.ctsTrade = new();
                    HomeConfig.tokenTrade = HomeConfig.ctsTrade.Token;
                    Task.Run(() => TradeOffer());
                }
                else
                {
                    HomeConfig.ctsTrade.Cancel();
                    HomeStatistics.TradeTool = false;
                }
            }, (obj) => !String.IsNullOrEmpty(SteamAccount.ApiKey));
        public ICommand QuickSellCommand =>
            new RelayCommand((obj) =>
            {
                if (!HomeStatistics.SellTool)
                {
                    HomeStatistics.SellTool = true;
                    HomeConfig.ctsSale = new();
                    HomeConfig.tokenSale = HomeConfig.ctsSale.Token;
                    HomeProperties.Default.MaxPrice = (int)obj;
                    HomeProperties.Default.Save();
                    Task.Run(() => QuickSell());
                }
                else
                {
                    HomeConfig.ctsSale.Cancel();
                    HomeStatistics.SellTool = false;
                }
            });
        public ICommand WithdrawCommand =>
            new RelayCommand((obj) =>
            {
                if (!HomeStatistics.WithdrawTool)
                {
                    HomeStatistics.WithdrawTool = true;
                    BaseModel.IsBrowser = true;
                    HomeConfig.ctsWithdraw = new();
                    HomeConfig.tokenWithdraw = HomeConfig.ctsWithdraw.Token;
                    Task.Run(() => Withdraw());
                }
                else
                {
                    HomeConfig.ctsWithdraw.Cancel();
                    HomeStatistics.WithdrawTool = false;
                }
            }, (obj) => !BaseModel.IsBrowser);
        void TradeOffer()
        {
            try
            {
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
            }
            catch (Exception exp)
            {
                HomeConfig.ctsTrade.Cancel();
                BaseService.errorLog(exp);
                BaseService.errorMessage(exp);
            }
        }
        void QuickSell()
        {
            try
            {
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
            }
            catch (Exception exp)
            {
                HomeConfig.ctsSale.Cancel();
                BaseService.errorLog(exp);
                BaseService.errorMessage(exp);
            }
        }
        void Withdraw()
        {
            try
            {
                if (BaseModel.Browser == null)
                    BaseModel.OpenBrowser();
                bool isLogin = false;
                do isLogin = CsmAccount.LoginCsm();
                while (!isLogin);
                
                WithdrawService withdraw = new();
                JArray inventory = withdraw.checkInventory();
                JArray items = new();
                if (inventory.Any() && !HomeConfig.tokenWithdraw.IsCancellationRequested)
                    items = withdraw.getItems(inventory);
                if (!items.Any() || HomeConfig.tokenWithdraw.IsCancellationRequested)
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
            }
            catch (Exception exp)
            {
                HomeConfig.ctsWithdraw.Cancel();
                BaseService.errorLog(exp);
                BaseService.errorMessage(exp);
            }
            finally
            {
                BaseModel.IsBrowser = false;
                if (SettingsProperties.Default.Quit)
                {
                    BaseModel.Browser.Quit();
                    BaseModel.Browser = null;
                }
            }
        }
        #region Favorite
        public ICommand ClearFavListCommand =>
            new RelayCommand((obj) =>
            {
                MessageBoxResult result = MessageBox.Show(
                    "Are you sure you want to clear the list?",
                    "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    Application.Current.Dispatcher.Invoke(() => {
                        HomeProperties.Default.FavoriteList.Clear();
                    });
                    HomeProperties.Default.Save();
                }
            }, (obj) => HomeProperties.Default.FavoriteList.Any());
        public ICommand ExportFavCommand =>
            new RelayCommand((obj) =>
            {
                FavoriteService.ExportTxt(HomeProperties.Default.FavoriteList);
            }, (obj) => HomeProperties.Default.FavoriteList.Any());
        public ICommand ImportFavCommand =>
            new RelayCommand((obj) =>
            {
                FavoriteService favorite = new();
                var list = favorite.ImportTxt();
                if (list.Any() && list.Count <= 200)
                {
                    HomeProperties.Default.FavoriteList = list;
                    HomeProperties.Default.Save();
                }
                else if (list.Count > 200)
                    Main.Message.Enqueue("Limit. The maximum is only 200!");
            });
        public ICommand RemoveFavoriteCommand =>
            new RelayCommand((obj) =>
            {
                string itemName = (string)obj;
                if (!String.IsNullOrEmpty(itemName))
                {
                    HomeProperties.Default.FavoriteList.Remove(itemName);
                    HomeProperties.Default.Save();
                    Main.Message.Enqueue($"{itemName}\nRemoved from list.");
                }
            }, (obj) => HomeProperties.Default.FavoriteList.Any() && !String.IsNullOrEmpty(SelectedFavItem));
        #endregion
    }
}