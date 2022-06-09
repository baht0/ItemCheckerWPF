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

        //homeView
        private Home _home = new();
        public Home Home
        {
            get
            {
                return _home;
            }
            set
            {
                _home = value;
                OnPropertyChanged();
            }
        }
        private HomeTable _homeTable = new();
        public HomeTable HomeTable
        {
            get
            {
                return _homeTable;
            }
            set
            {
                _homeTable = value;
                OnPropertyChanged();
            }
        }

        //tools
        private HomePush _homePush = new();
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
        private HomeWithdraw _homeWithdraw = new();
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
        //inventory
        private HomeInventory _homeInventory = new();
        public HomeInventory HomeInventory
        {
            get
            {
                return _homeInventory;
            }
            set
            {
                _homeInventory = value;
                OnPropertyChanged();
            }
        }
        private DataInventory _selectedInventory = new();
        public DataInventory SelectedInventory
        {
            get
            {
                return _selectedInventory;
            }
            set
            {
                DataInventory item = value;
                Task.Run(() =>
                {
                    item.CsmGive = Math.Round(SteamBase.ItemList.FirstOrDefault(x => x.ItemName == item.ItemName).Csm.Price * Calculator.CommissionCsm, 2);
                    item.LfmGive = Math.Round(SteamBase.ItemList.FirstOrDefault(x => x.ItemName == item.ItemName).Lfm.Price * Calculator.CommissionLf, 2);
                    ItemBaseService baseService = new();
                    baseService.UpdateSteamInfoItem(item.ItemName);
                    baseService.UpdateBuffInfoItem(item.ItemName);
                    item.LowestSellOrder = SteamBase.ItemList.FirstOrDefault(x => x.ItemName == item.ItemName).Steam.LowestSellOrder;
                    item.HighestBuyOrder = SteamBase.ItemList.FirstOrDefault(x => x.ItemName == item.ItemName).Steam.HighestBuyOrder;
                    item.PriceBuff = SteamBase.ItemList.FirstOrDefault(x => x.ItemName == item.ItemName).Buff.Price;
                    item.BuyOrderBuff = SteamBase.ItemList.FirstOrDefault(x => x.ItemName == item.ItemName).Buff.BuyOrder;
                    _selectedInventory = item;
                    OnPropertyChanged();
                });

            }
        }
        //favorite
        private HomeFavorite _homeFavorite = new();
        public HomeFavorite HomeFavorite
        {
            get
            {
                return _homeFavorite;
            }
            set
            {
                _homeFavorite = value;
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
                HomeFavorite.List = new(DataSavedList.Items.Where(x => x.ListName == "favorite"));
            }
            catch (Exception ex)
            {
                BaseService.errorLog(ex, false);
            }
        }
        //table
        public ICommand OpenItemOutCommand =>
            new RelayCommand((obj) =>
            {
                var item = HomeTable.SelectedOrderItem;
                string itemName = item.ItemName.Replace("(Holo/Foil)", "(Holo-Foil)");
                string market_hash_name = Edit.EncodeMarketHashName(itemName);
                switch ((Int32)obj)
                {
                    case 1 or 2:
                        Edit.OpenUrl("https://steamcommunity.com/market/listings/730/" + market_hash_name);
                        break;
                    case 3 or 4:
                        switch (SettingsProperties.Default.ServiceId)
                        {
                            case 0 or 1:
                                Edit.OpenUrl("https://steamcommunity.com/market/listings/730/" + market_hash_name);
                                break;
                            case 2:
                                Edit.OpenCsm(market_hash_name);
                                break;
                            case 4:
                                Edit.OpenUrl("https://buff.163.com/goods/" + SteamBase.ItemList.FirstOrDefault(x => x.ItemName == item.ItemName).Buff.Id);
                                break;
                        }
                        break;
                    default:
                        Clipboard.SetText(itemName);
                        break;
                }
            });
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
                            HomeTable.OrderedGrid = new ObservableCollection<DataOrder>(DataOrder.Orders);
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
                                HomeTable.OrderedGrid = new ObservableCollection<DataOrder>(DataOrder.Orders);
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
                    HomeTable.OrderedGrid = new ObservableCollection<DataOrder>(DataOrder.Orders);
                    Main.Message.Enqueue($"{item.ItemName}\nOrder has been canceled.");
                }
            });

        #region tools
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
        void OrderPush()
        {
            try
            {
                SteamAccount.GetSteamBalance();

                OrderCheckService orderCheck = new();
                orderCheck.SteamOrders(false);
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
                        BaseService.errorLog(exp, false);
                    }
                    finally
                    {
                        HomePush.Progress++;
                    }
                    if (HomePush.token.IsCancellationRequested)
                        break;
                }
                HomePush.Status = "Update...";
                if (HomePush.Check % 5 == 4)//every 5 check
                {
                    orderCheck.SteamOrders(true);//update service info
                    decimal availableAmount = SteamAccount.GetAvailableAmount();
                    if (availableAmount >= SteamAccount.Balance * 10 * 0.15m)//order fav items
                    {
                        FavoriteService favorite = new();
                        favorite.PlaceOrderFav(availableAmount);
                    }
                }
                HomeTable.OrderedGrid = new(DataOrder.Orders);
                HomePush.Check++;
            }
            catch (Exception exp)
            {
                HomePush.cts.Cancel();
                BaseService.errorLog(exp, true);
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
        public ICommand TimerCommand =>
            new RelayCommand((obj) =>
            {
            switch ((int)obj)
                {
                    case 0:
                        HomePush.TimerTick = 1;
                        break;

                }
            }, (obj) => HomePush.IsService);
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
                        BaseService.errorLog(exp, false);
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
                BaseService.errorLog(exp, true);
            }
            finally
            {
                BaseModel.Browser.Quit();
                BaseModel.Browser = null;
                BaseModel.IsBrowser = false;
                HomeWithdraw.IsService = false;
                Main.Message.Enqueue("Withdraw has finished.");
            }
        }
        #endregion

        //inventory
        public ICommand UpdateInformationsCommand =>
            new RelayCommand((obj) =>
            {
                BaseModel.IsWorking = true;
                Task.Run(() =>
                {
                    ItemBaseService baseService = new();
                    baseService.UpdateCsmInfo();
                    baseService.UpdateLfmInfo();
                    InventoryService inventoryService = new();
                    var items = inventoryService.CheckInventory();
                    HomeInventory.Items = new(items);
                    Main.Message.Enqueue("MyInventory updated.");
                    BaseModel.IsWorking = false;
                });
            }, (obj) => !BaseModel.IsWorking);
        public ICommand InventoryTaskCommand =>
            new RelayCommand((obj) =>
            {
                if (!HomeInventory.IsService)
                {
                    var config = (HomeInventory)obj;
                    HomeInventory.IsService = true;
                    HomeInventory.cts = new();
                    HomeInventory.token = HomeInventory.cts.Token;

                    HomeProperties.Default.AllAvailable = config.AllAvailable;
                    HomeProperties.Default.SelectedOnly = config.SelectedOnly;
                    HomeProperties.Default.MaxPrice = config.MaxPrice;
                    HomeProperties.Default.SellingPriceId = config.SellingPriceId;
                    HomeProperties.Default.TasksId = config.TasksId;
                    HomeProperties.Default.Save();
                    switch (config.TasksId)
                    {
                        case 0:
                            if (!String.IsNullOrEmpty(SteamAccount.ApiKey))
                                Task.Run(() => TradeOffer());
                            break;
                        case 1:
                            if (config.SelectedOnly && SelectedInventory == null)
                            {
                                HomeInventory.cts.Cancel();
                                HomeInventory.IsService = false;
                                break;
                            }
                            DataInventory selected = SelectedInventory;
                            Task.Run(() => QuickSell(selected));
                            break;
                    }
                }
                else
                {
                    HomeInventory.cts.Cancel();
                    HomeInventory.IsService = false;
                }
            });
        void TradeOffer()
        {
            try
            {
                InventoryService tradeOffer = new();
                do
                {
                    HomeInventory.Progress = 0;
                    HomeInventory.MaxProgress = DataTradeOffer.Offers.Count;
                    HomeInventory.Count += DataTradeOffer.Offers.Count;
                    foreach (DataTradeOffer offer in DataTradeOffer.Offers)
                    {
                        try
                        {
                            tradeOffer.AcceptTrade(offer.TradeOfferId, offer.PartnerId);
                        }
                        catch (Exception exp)
                        {
                            BaseService.errorLog(exp, false);
                        }
                        finally
                        {
                            HomeInventory.Progress++;
                        }
                        if (HomeInventory.token.IsCancellationRequested)
                            break;
                    }
                }
                while (tradeOffer.CheckOffer());
            }
            catch (Exception exp)
            {
                HomeInventory.cts.Cancel();
                BaseService.errorLog(exp, true);
            }
            finally
            {
                HomeInventory.IsService = false;
                Main.Message.Enqueue("Accept trades has finished.");
            }
        }
        void QuickSell(DataInventory selected)
        {
            try
            {
                InventoryService quickSell = new();
                List<DataInventory> items = quickSell.CheckInventory();

                HomeInventory.Items = new(items);
                HomeInventory.Progress = 0;
                HomeInventory.MaxProgress = items.Count;
                HomeInventory.Count = items.Count;
                decimal sum = HomeProperties.Default.SellingPriceId == 0 ? items.Sum(s => s.LowestSellOrder) : items.Sum(s => s.HighestBuyOrder);
                HomeInventory.Sum = Math.Round(sum * Calculator.CommissionSteam, 2);
                foreach (var item in items)
                {
                    try
                    {
                        if (HomeProperties.Default.SelectedOnly && selected.ItemName != item.ItemName)
                            continue;
                        quickSell.SellItem(item);
                    }
                    catch (Exception exp)
                    {
                        BaseService.errorLog(exp, false);
                    }
                    finally
                    {
                        HomeInventory.Progress++;
                        Thread.Sleep(1500);
                    }
                    if (HomeInventory.token.IsCancellationRequested)
                        break;
                }
            }
            catch (Exception exp)
            {
                HomeInventory.cts.Cancel();
                BaseService.errorLog(exp, true);
            }
            finally
            {
                HomeInventory.IsService = false;
                Main.Message.Enqueue("Quick sell items has finished.");
            }
        }

        #region Favorite
        public ICommand ClearFavListCommand =>
            new RelayCommand((obj) =>
            {
                MessageBoxResult result = MessageBox.Show( "Are you sure you want to clear the list?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                    DataSavedList.Clear("favorite");

            }, (obj) => DataSavedList.Items.Any(x => x.ListName == "favorite"));
        public ICommand RemoveFavoriteCommand =>
            new RelayCommand((obj) =>
            {
                var item = (DataSavedList)obj;

                DataSavedList.Items.Remove(item);
                Main.Message.Enqueue($"{item.ItemName}\nRemoved from list.");
                DataSavedList.Save();

            }, (obj) => DataSavedList.Items.Any() && HomeFavorite.SelectedItem != null);
        #endregion
    }
}