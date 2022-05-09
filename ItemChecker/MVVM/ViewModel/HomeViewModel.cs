using ItemChecker.Core;
using ItemChecker.MVVM.Model;
using ItemChecker.Properties;
using ItemChecker.Services;
using ItemChecker.Support;
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

        //DataGrid
        private ObservableCollection<DataOrder> _orderedGrid = new(DataOrder.Orders);
        private DataOrder _selectedOrderItem;
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
                OnPropertyChanged();
            }
        }

        //services
        private HomePush _homePush = new();
        private HomeFloatCheck _homeFloatCheck = new();
        public HomePush HomePush
        {
            get
            {
                return _homePush;
            }
            set
            {
                _homePush = value;
                OnPropertyChanged();
            }
        }
        public HomeFloatCheck HomeFloatCheck
        {
            get
            {
                return _homeFloatCheck;
            }
            set
            {
                _homeFloatCheck = value;
                OnPropertyChanged();
            }
        }
        //tools
        private HomeWithdraw _homeWithdraw = new();
        private HomeTrade _homeTrade = new();
        private HomeSell _homeSell = new();
        public HomeWithdraw HomeWithdraw
        {
            get
            {
                return _homeWithdraw;
            }
            set
            {
                _homeWithdraw = value;
                OnPropertyChanged();
            }
        }
        public HomeTrade HomeTrade
        {
            get
            {
                return _homeTrade;
            }
            set
            {
                _homeTrade = value;
                OnPropertyChanged();
            }
        }
        public HomeSell HomeSell
        {
            get
            {
                return _homeSell;
            }
            set
            {
                _homeSell = value;
                OnPropertyChanged();
            }
        }
        //favorite
        private ObservableCollection<string> _favoriteList = HomeFavorite.FavoriteList;
        private string _selectedFavItem;
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
                FavoriteList = HomeFavorite.FavoriteList;
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
                    case 0:
                        BaseModel.IsWorking = true;
                        Task.Run(() => {
                            OrderCheckService orderCheck = new();
                            orderCheck.SteamOrders(true);
                            OrderedGrid = new ObservableCollection<DataOrder>(DataOrder.Orders);
                            BaseModel.IsWorking = false;
                            Main.Message.Enqueue("MyOrders update is complete.");
                        });
                        break;
                    case 1:
                        MessageBoxResult result = MessageBox.Show(
                            "Do you really want to cancel all your orders?", "Question",
                            MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (result == MessageBoxResult.Yes)
                            Task.Run(() => {
                                BaseModel.IsWorking = true;
                                List<DataOrder> orders = new(DataOrder.Orders);
                                foreach (DataOrder order in orders)
                                    OrderService.CancelOrder(order);
                                OrderedGrid = new ObservableCollection<DataOrder>(DataOrder.Orders);
                                SteamAccount.GetAvailableAmount();
                                BaseModel.IsWorking = false;
                                Main.Message.Enqueue("All MyOrders have been cancelled.");
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

        #region services
        public ICommand AddFloatListCommand =>
            new RelayCommand((obj) =>
            {
                Task.Run(() =>
                {
                    List<string> response = BaseService.OpenFileDialog("txt");
                    if (response.Any())
                    {
                        HomeFloatCheck.ListCount = response.Count;
                        HomeFloatCheck.List = response;
                        BaseService.SaveList("FloatList", HomeFloatCheck.List);
                    }
                });
            }, (obj) => !HomeFloatCheck.IsService);
        public ICommand PushCommand =>
            new RelayCommand((obj) =>
            {
                if (!HomePush.IsService)
                {
                    HomePush.IsService = true;
                    HomePush config = obj as HomePush;
                    HomeProperties.Default.TimePush = config.Time;
                    HomeProperties.Default.Reserve = config.Reserve;
                    HomeProperties.Default.Unwanted = config.Unwanted;
                    HomeProperties.Default.Save();

                    HomePush.Timer.Elapsed += timerPushTick;
                    HomePush.TimerTick = config.Time * 60;
                    HomePush.Timer.Enabled = true;
                }
                else
                {
                    HomePush.cts.Cancel();
                    HomePush.Status = "Off";
                    HomePush.IsService = false;
                    HomePush.Timer.Enabled = false;
                    HomePush.TimerTick = 0;
                    HomePush.Timer.Elapsed -= timerPushTick;
                }
            }, (obj) => DataOrder.Orders.Any());
        public ICommand FloatCommand => 
            new RelayCommand((obj) =>
            {
                if (!HomeFloatCheck.IsService)
                {
                    HomeFloatCheck.IsService = true;
                    HomeFloatCheck config = obj as HomeFloatCheck;
                    HomeProperties.Default.TimeFloat = config.Time;
                    HomeProperties.Default.MaxPrecent = config.MaxPrecent;
                    HomeProperties.Default.Compare = config.Compare;
                    HomeProperties.Default.Save();

                    HomeFloatCheck.Timer.Elapsed += timerFloatTick;
                    HomeFloatCheck.TimerTick = config.Time * 60;
                    HomeFloatCheck.Timer.Enabled = true;
                }
                else
                {
                    HomeFloatCheck.cts.Cancel();
                    HomeFloatCheck.Status = "Off";
                    HomeFloatCheck.IsService = false;
                    HomeFloatCheck.Timer.Enabled = false;
                    HomeFloatCheck.TimerTick = 0;
                    HomeFloatCheck.Timer.Elapsed -= timerFloatTick;
                }
            }, (obj) => HomeFloatCheck.List.Any());
        void timerPushTick(Object sender, ElapsedEventArgs e)
        {
            HomePush.TimerTick--;
            TimeSpan timeSpan = TimeSpan.FromSeconds(HomePush.TimerTick);
            HomePush.Status = timeSpan.ToString("mm':'ss");
            if (HomePush.TimerTick <= 0)
            {
                HomePush.Status = "Preparation...";
                HomePush.Timer.Enabled = false;
                HomePush.Progress = 0;

                HomePush.cts = new();
                HomePush.token = HomePush.cts.Token;
                OrderPush();
            }
        }
        void timerFloatTick(Object sender, ElapsedEventArgs e)
        {
            HomeFloatCheck.TimerTick--;
            TimeSpan timeSpan = TimeSpan.FromSeconds(HomeFloatCheck.TimerTick);
            HomeFloatCheck.Status = timeSpan.ToString("mm':'ss");
            if (HomeFloatCheck.TimerTick <= 0)
            {
                HomeFloatCheck.Status = "Preparation...";
                HomeFloatCheck.Timer.Enabled = false;
                HomeFloatCheck.Progress = 0;

                HomeFloatCheck.cts = new();
                HomeFloatCheck.token = HomeFloatCheck.cts.Token;
                FloatCheck();
            }
        }
        void OrderPush()
        {
            try
            {
                SteamAccount.GetSteamBalance();

                OrderCheckService orderCheck = new();
                bool isUpdateService = HomePush.Check % 10 == 0;
                orderCheck.SteamOrders(isUpdateService);
                OrderPushService pushOrder = new();

                HomePush.Status = "Pushing...";
                HomePush.MaxProgress = DataOrder.Orders.Count;
                foreach (DataOrder order in DataOrder.Orders)
                {
                    try
                    {
                        HomePush.Push += pushOrder.PushItems(order) ? 1 : 0;
                    }
                    catch (Exception exp)
                    {
                        BaseService.errorLog(exp);
                    }
                    finally
                    {
                        HomePush.Progress++;
                    }
                    if (HomePush.token.IsCancellationRequested)
                        break;
                }
                HomePush.Status = "Update...";
                decimal availableAmount = SteamAccount.GetAvailableAmount();
                if (HomePush.Check % 5 == 4 && availableAmount >= SteamAccount.Balance * 10 * 0.15m)
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
                orderCheck.SteamOrders(false);
                OrderedGrid = new(DataOrder.Orders);
                HomePush.Check++;
            }
            catch (Exception exp)
            {
                HomePush.cts.Cancel();
                BaseService.errorLog(exp);
                BaseService.errorMessage(exp);
            }
            finally
            {
                if (!HomePush.token.IsCancellationRequested)
                {
                    HomePush.TimerTick = HomeProperties.Default.TimePush * 60;
                    HomePush.Timer.Enabled = true;
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

                HomeFloatCheck.MaxProgress = HomeFloatCheck.List.Count;
                HomeFloatCheck.Status = "Checking...";
                foreach (string item in HomeFloatCheck.List)
                {
                    try
                    {
                        HomeFloatCheck.PurchasesMade += floatCheck.checkFloat(item);
                    }
                    catch (Exception exp)
                    {
                        BaseService.errorLog(exp);
                    }
                    finally
                    {
                        HomeFloatCheck.Progress++;
                    }
                    if (HomeFloatCheck.token.IsCancellationRequested)
                        break;
                }
                HomeFloatCheck.Check++;
            }
            catch (Exception exp)
            {
                HomeFloatCheck.cts.Cancel();
                BaseService.errorLog(exp);
                BaseService.errorMessage(exp);
            }
            finally
            {
                if (!HomeFloatCheck.token.IsCancellationRequested)
                {
                    HomeFloatCheck.TimerTick = HomeProperties.Default.TimeFloat * 60;
                    HomeFloatCheck.Timer.Enabled = true;
                }
            }
        }
        public ICommand TimerCommand =>
            new RelayCommand((obj) =>
            {
            switch ((int)obj)
                {
                    case 0:
                        HomePush.TimerTick = 1;
                        break;
                    case 1:
                        HomeFloatCheck.TimerTick = 1;
                        break;

                }
            }, (obj) => HomePush.IsService | HomeFloatCheck.IsService);
        #endregion

        #region tools
        public ICommand TradeOfferCommand =>
            new RelayCommand((obj) =>
            {
                if (!HomeTrade.IsService)
                {
                    HomeTrade.IsService = true;
                    HomeTrade.cts = new();
                    HomeTrade.token = HomeTrade.cts.Token;
                    Task.Run(() => TradeOffer());
                }
                else
                {
                    HomeTrade.cts.Cancel();
                    HomeTrade.IsService = false;
                }
            }, (obj) => !String.IsNullOrEmpty(SteamAccount.ApiKey));
        public ICommand QuickSellCommand =>
            new RelayCommand((obj) =>
            {
                if (!HomeSell.IsService)
                {
                    HomeSell.IsService = true;
                    HomeSell.cts = new();
                    HomeSell.token = HomeSell.cts.Token;
                    HomeProperties.Default.MaxPrice = (int)obj;
                    HomeProperties.Default.Save();
                    Task.Run(() => QuickSell());
                }
                else
                {
                    HomeSell.cts.Cancel();
                    HomeSell.IsService = false;
                }
            });
        public ICommand WithdrawCommand =>
            new RelayCommand((obj) =>
            {
                if (!HomeWithdraw.IsService)
                {
                    HomeWithdraw.IsService = true;
                    BaseModel.IsBrowser = true;
                    HomeWithdraw.cts = new();
                    HomeWithdraw.token = HomeWithdraw.cts.Token;
                    Task.Run(() => Withdraw());
                }
                else
                {
                    HomeWithdraw.cts.Cancel();
                    HomeWithdraw.IsService = false;
                }
            }, (obj) => !BaseModel.IsBrowser);
        void TradeOffer()
        {
            try
            {
                TradeOfferService tradeOffer = new();
                do
                {
                    HomeTrade.Progress = 0;
                    HomeTrade.MaxProgress = DataTradeOffer.TradeOffers.Count;
                    HomeTrade.Count += DataTradeOffer.TradeOffers.Count;
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
                            HomeTrade.Progress++;
                        }
                        if (HomeTrade.token.IsCancellationRequested)
                            break;
                    }
                }
                while (tradeOffer.checkOffer());
            }
            catch (Exception exp)
            {
                HomeTrade.cts.Cancel();
                BaseService.errorLog(exp);
                BaseService.errorMessage(exp);
            }
            finally
            {
                HomeTrade.IsService = false;
                Main.Message.Enqueue("Accept trades has finished.");
            }
        }
        void QuickSell()
        {
            try
            {
                QuickSellService quickSell = new();
                quickSell.checkInventory();

                HomeSell.Progress = 0;
                HomeSell.MaxProgress = DataSell.SellItems.Count;
                HomeSell.Count = DataSell.SellItems.Count;
                HomeSell.Sum = DataSell.SellItems.Sum(s => s.Price);
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
                        HomeSell.Progress++;
                        Thread.Sleep(1500);
                    }
                    if (HomeSell.token.IsCancellationRequested)
                        break;
                }
            }
            catch (Exception exp)
            {
                HomeSell.cts.Cancel();
                BaseService.errorLog(exp);
                BaseService.errorMessage(exp);
            }
            finally
            {
                HomeSell.IsService = false;
                Main.Message.Enqueue("Quick sell items has finished.");
            }
        }
        void Withdraw()
        {
            try
            {
                if (BaseModel.Browser == null)
                    BaseService.OpenBrowser();
                bool isLogin = false;
                do isLogin = CsmAccount.Login();
                while (!isLogin);
                
                WithdrawService withdraw = new();
                JArray inventory = withdraw.CheckInventory();
                JArray items = new();
                if (inventory.Any() && !HomeWithdraw.token.IsCancellationRequested)
                    items = withdraw.GetItems(inventory);
                if (!items.Any() || HomeWithdraw.token.IsCancellationRequested)
                    return;

                HomeWithdraw.Progress = 0;
                HomeWithdraw.MaxProgress = items.Count;
                HomeWithdraw.Count = items.Count;
                foreach (JObject item in items)
                {
                    try
                    {
                        withdraw.WithdrawItems(item);
                    }
                    catch (Exception exp)
                    {
                        BaseService.errorLog(exp);
                    }
                    finally
                    {
                        Thread.Sleep(1500);
                        HomeWithdraw.Progress++;
                    }
                    if (HomeWithdraw.token.IsCancellationRequested)
                        break;
                }
            }
            catch (Exception exp)
            {
                HomeWithdraw.cts.Cancel();
                BaseService.errorLog(exp);
                BaseService.errorMessage(exp);
            }
            finally
            {
                if (SettingsProperties.Default.Quit)
                {
                    BaseModel.Browser.Quit();
                    BaseModel.Browser = null;
                }
                BaseModel.IsBrowser = false;
                HomeWithdraw.IsService = false;
                Main.Message.Enqueue("Withdraw has finished.");
            }
        }
        #endregion

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
                        HomeFavorite.FavoriteList.Clear();
                    });
                    BaseService.SaveList("FavoriteList", HomeFavorite.FavoriteList.ToList());
                }
            }, (obj) => HomeFavorite.FavoriteList.Any());
        public ICommand ExportFavCommand =>
            new RelayCommand((obj) =>
            {
                FavoriteService.ExportTxt(HomeFavorite.FavoriteList);
            }, (obj) => HomeFavorite.FavoriteList.Any());
        public ICommand ImportFavCommand =>
            new RelayCommand((obj) =>
            {
                ObservableCollection<string> list = new(BaseService.OpenFileDialog("txt"));
                if (list.Any() && list.Count <= 200)
                {
                    HomeFavorite.FavoriteList = list;
                    BaseService.SaveList("FavoriteList", HomeFavorite.FavoriteList.ToList());
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
                    HomeFavorite.FavoriteList.Remove(itemName);
                    Main.Message.Enqueue($"{itemName}\nRemoved from list.");
                    BaseService.SaveList("FavoriteList", HomeFavorite.FavoriteList.ToList());
                }
            }, (obj) => HomeFavorite.FavoriteList.Any() && !String.IsNullOrEmpty(SelectedFavItem));
        #endregion
    }
}