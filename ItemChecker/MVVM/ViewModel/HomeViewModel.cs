using ItemChecker.Core;
using ItemChecker.MVVM.Model;
using ItemChecker.Properties;
using ItemChecker.Services;
using ItemChecker.Support;
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
        private Home _home = new();
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
        private HomeTable _homeTable = new();

        //tools
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
        private HomePush _homePush = new();
        //inventory
        public HomeInventoryConfig HomeInventoryConfig
        {
            get
            {
                return _homeInventoryConfig;
            }
            set
            {
                _homeInventoryConfig = value;
                OnPropertyChanged();
            }
        }
        private HomeInventoryConfig _homeInventoryConfig = new();
        public HomeInventoryInfo HomeInventoryInfo
        {
            get
            {
                return _homeInventoryInfo;
            }
            set
            {
                _homeInventoryInfo = value;
                OnPropertyChanged();
            }
        }
        private HomeInventoryInfo _homeInventoryInfo = new();
        public DataInventory SelectedInventory
        {
            get
            {
                return _selectedInventory;
            }
            set
            {
                _selectedInventory = value;
                OnPropertyChanged();
            }
        }
        private DataInventory _selectedInventory = new();
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
                            case 3:
                                Edit.OpenUrl("https://loot.farm/");
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
                            HomeTable.OrderedGrid = new ObservableCollection<DataOrder>(SteamMarket.Orders);
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
                                List<DataOrder> orders = new(SteamMarket.Orders);
                                foreach (DataOrder order in orders)
                                    SteamMarket.Orders.Cancel(order);
                                HomeTable.OrderedGrid = new ObservableCollection<DataOrder>(SteamMarket.Orders);
                                SteamMarket.Orders.GetAvailableAmount();
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
                    SteamMarket.Orders.Cancel(item);
                    HomeTable.OrderedGrid = new ObservableCollection<DataOrder>(SteamMarket.Orders);
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
            }, (obj) => SteamMarket.Orders.Any());
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
                SteamAccount.GetBalance();
                OrderCheckService orderCheck = new();
                orderCheck.SteamOrders(false);

                HomePush.Status = "Pushing...";
                HomePush.MaxProgress = SteamMarket.Orders.Count;
                foreach (DataOrder order in SteamMarket.Orders)
                {
                    try
                    {
                        HomePush.Push += OrderService.PushItems(order) ? 1 : 0;
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

                    if (SteamMarket.Orders.GetAvailableAmount() >= SteamAccount.MaxAmount * 0.15m)//order fav items
                    {
                        FavoriteService favorite = new();
                        favorite.Check();
                    }
                }
                HomeTable.OrderedGrid = new(SteamMarket.Orders);
                HomePush.Check++;
            }
            catch (Exception exp)
            {
                HomePush.cts.Cancel();
                HomePush.Status = "Off";
                HomePush.IsService = false;
                HomePush.Timer.Enabled = false;
                HomePush.TimerTick = 0;
                HomePush.Timer.Elapsed -= timerPushTick;

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
        #endregion

        //inventory
        public ICommand UpdateInformationsCommand =>
            new RelayCommand((obj) =>
            {
                BaseModel.IsWorking = true;
                Task.Run(() =>
                {
                    InventoryService inventoryService = new();
                    var items = inventoryService.CheckInventory(null);
                    HomeInventoryInfo.Items = new(items);
                    SelectedInventory = HomeInventoryInfo.Items.Any() ? HomeInventoryInfo.Items.First() : new();
                    Main.Message.Enqueue("Steam Inventory updated.");
                    BaseModel.IsWorking = false;
                });
            }, (obj) => !BaseModel.IsWorking);
        public ICommand ShowInventoryItemCommand =>
            new RelayCommand((obj) =>
            {
                var item = (DataInventory)obj;

                Edit.OpenUrl("https://steamcommunity.com/my/inventory/#730_2_" + item.AssetId);

            }, (obj) => HomeInventoryInfo.Items.Any() && SelectedInventory != null);
        public ICommand InventoryTaskCommand =>
            new RelayCommand((obj) =>
            {
                if (!HomeInventoryInfo.IsService)
                {
                    var config = (HomeInventoryConfig)obj;
                    HomeInventoryInfo.IsService = true;
                    HomeInventoryInfo.cts = new();
                    HomeInventoryInfo.token = HomeInventoryInfo.cts.Token;

                    switch (config.TasksId)
                    {
                        case 0:
                            Task.Run(() => TradeOffer());
                            break;
                        case 1:
                            Task.Run(() => QuickSell(config));
                            break;
                    }
                }
                else
                {
                    HomeInventoryInfo.cts.Cancel();
                    HomeInventoryInfo.IsService = false;
                }
            }, (obj) => (HomeInventoryConfig.TasksId == 0 && !String.IsNullOrEmpty(SteamAccount.ApiKey)) || (HomeInventoryConfig.TasksId == 1 && ((HomeInventoryConfig.SelectedOnly && SelectedInventory.ItemName != "Unknown") || HomeInventoryConfig.AllAvailable)) || (HomeInventoryConfig.TasksId == 2 && !BaseModel.IsBrowser));
        void TradeOffer()
        {
            try
            {
                InventoryService tradeOffer = new();
                do
                {
                    HomeInventoryInfo.Progress = 0;
                    HomeInventoryInfo.MaxProgress = DataTradeOffer.Offers.Count;
                    HomeInventoryInfo.Count += DataTradeOffer.Offers.Count;
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
                            HomeInventoryInfo.Progress++;
                        }
                        if (HomeInventoryInfo.token.IsCancellationRequested)
                            break;
                    }
                }
                while (tradeOffer.CheckOffer());
            }
            catch (Exception exp)
            {
                HomeInventoryInfo.cts.Cancel();
                BaseService.errorLog(exp, true);
            }
            finally
            {
                HomeInventoryInfo.IsService = false;
                Main.Message.Enqueue("Accept trades has finished.");
            }
        }
        void QuickSell(HomeInventoryConfig config)
        {
            try
            {
                InventoryService service = new();

                string itemName = string.Empty;
                if (config.SelectedOnly)
                    itemName = SelectedInventory.ItemName;
                List<DataInventory> items = service.CheckInventory(itemName);

                HomeInventoryInfo.Items = !config.SelectedOnly ? new(items) : HomeInventoryInfo.Items;
                HomeInventoryInfo.Progress = 0;
                HomeInventoryInfo.MaxProgress = items.Count;
                HomeInventoryInfo.Count = items.Count;

                foreach (var item in items)
                {
                    try
                    {
                        service.SellItem(item, config);
                        decimal sum = config.SellingPriceId == 0 ? items.Sum(s => s.LowestSellOrder) : items.Sum(s => s.HighestBuyOrder);
                        HomeInventoryInfo.Sum = Math.Round(sum * Calculator.CommissionSteam, 2);
                    }
                    catch (Exception exp)
                    {
                        BaseService.errorLog(exp, false);
                    }
                    finally
                    {
                        HomeInventoryInfo.Progress++;
                        Thread.Sleep(1500);
                    }
                    if (HomeInventoryInfo.token.IsCancellationRequested)
                        break;
                }
            }
            catch (Exception exp)
            {
                HomeInventoryInfo.cts.Cancel();
                BaseService.errorLog(exp, true);
            }
            finally
            {
                SelectedInventory = new();
                HomeInventoryInfo.IsService = false;
                Main.Message.Enqueue("Quick sell items has finished.");
            }
        }
    }
}