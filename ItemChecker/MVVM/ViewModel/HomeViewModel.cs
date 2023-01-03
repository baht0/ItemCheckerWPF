using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using ItemChecker.Core;
using ItemChecker.MVVM.Model;
using ItemChecker.Net;
using ItemChecker.Properties;
using ItemChecker.Services;
using ItemChecker.Support;

namespace ItemChecker.MVVM.ViewModel
{
    public class HomeViewModel : ObservableObject
    {
        #region Properties

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
        Home _home = new();
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
        HomeTable _homeTable = new();

        //orders
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
        HomePush _homePush = new();
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
        HomeInventoryConfig _homeInventoryConfig = new();
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
        HomeInventoryInfo _homeInventoryInfo = new();
        public DataInventory SelectedInventory
        {
            get
            {
                if (_selectedInventory == null)
                    return HomeInventoryInfo.Items.FirstOrDefault();
                return _selectedInventory;
            }
            set
            {
                _selectedInventory = value;
                OnPropertyChanged();
            }
        }
        DataInventory _selectedInventory = new();
        #endregion

        public HomeViewModel()
        {
            Task.Run(() => { 
                OrderCheckService.SteamOrders(true);
                HomeTable.IsBusy = false;
                OnPropertyChanged(nameof(HomeTable));
            });
        }

        #region table
        public ICommand OpenItemOutCommand =>
            new RelayCommand((obj) =>
            {
                var item = HomeTable.SelectedOrderItem;
                string itemName = item.ItemName.Replace("(Holo/Foil)", "(Holo-Foil)");
                string market_hash_name = Uri.EscapeDataString(itemName);
                switch ((Int32)obj)
                {
                    case 1:
                        Edit.OpenUrl("https://steamcommunity.com/market/listings/730/" + market_hash_name);
                        break;
                    case 2 or 3:
                        switch (HomeProperties.Default.ServiceId)
                        {
                            case 0 or 1:
                                Edit.OpenUrl("https://steamcommunity.com/market/listings/730/" + market_hash_name);
                                break;
                            case 2:
                                Edit.OpenCsm(itemName);
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
                HomeTable.IsBusy = true;
                switch (Convert.ToInt32(obj))
                {
                    case 0:
                        {
                            Task.Run(() => {
                                try
                                {
                                    OrderCheckService.SteamOrders(true);
                                    HomeTable.OrderedGrid = new(SteamMarket.Orders);
                                    Main.Message.Enqueue("MyOrders update is complete.");
                                }
                                catch (Exception ex)
                                {
                                    BaseService.errorLog(ex, true);
                                }
                                finally
                                {
                                    HomeTable.IsBusy = false;
                                }
                            });
                            break;
                        }
                    case 1:
                        {
                            MessageBoxResult result = MessageBox.Show(
                                "Do you really want to cancel all your orders?", "Question",
                                MessageBoxButton.YesNo, MessageBoxImage.Question);
                            if (result == MessageBoxResult.Yes)
                                Task.Run(() => {
                                    try
                                    {
                                        List<DataOrder> orders = new(SteamMarket.Orders);
                                        foreach (DataOrder order in orders)
                                            SteamMarket.Orders.Cancel(order);
                                        HomeTable.OrderedGrid = new(SteamMarket.Orders);
                                        Main.Message.Enqueue("All MyOrders have been cancelled.");
                                    }
                                    catch (Exception ex)
                                    {
                                        BaseService.errorLog(ex, true);
                                    }
                                    finally
                                    {
                                        HomeTable.IsBusy = false;
                                    }
                                });
                            break;
                        }
                }
            }, (obj) => !HomeTable.IsBusy);
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
        #endregion

        #region order
        public ICommand PushCommand =>
            new RelayCommand((obj) =>
            {
                if (!HomePush.IsService)
                {
                    HomePush.IsService = true;
                    HomePush config = obj as HomePush;
                    HomeProperties.Default.ServiceId = config.ServiceId;
                    HomeProperties.Default.MinPrecent = config.MinPrecent;
                    HomeProperties.Default.TimePush = config.Time;
                    HomeProperties.Default.Reserve = config.Reserve;
                    HomeProperties.Default.Save();

                    HomePush.Timer.Elapsed += timerPushTick;
                    HomePush.TimerTick = config.Time * 60;
                    HomePush.Timer.Enabled = true;
                }
                else
                {
                    HomePush.cts.Cancel();
                    HomePush.Status = string.Empty;
                    HomePush.IsService = false;
                    HomePush.Timer.Enabled = false;
                    HomePush.TimerTick = 0;
                    HomePush.Timer.Elapsed -= timerPushTick;
                }
            }, (obj) => SteamMarket.Orders.Any() && HomePush.Time > 0 && HomePush.MinPrecent >= 0 && HomePush.Reserve >= 0);
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
                OrderCheckService.SteamOrders(false);

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
                    OrderCheckService.SteamOrders(true);//update service info

                    if (SteamMarket.Orders.GetAvailableAmount() >= SteamMarket.MaxAmount * 0.15m)//order fav items
                    {
                        FavoriteService favorite = new();
                        FavoriteService.Check();
                    }
                }
                HomeTable.OrderedGrid = new(SteamMarket.Orders);
                HomePush.Check++;
            }
            catch (Exception exp)
            {
                HomePush.cts.Cancel();
                HomePush.Status = string.Empty;
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
        public ICommand ResetTimerCommand =>
            new RelayCommand((obj) =>
            {
                HomePush.TimerTick = (int)obj == 0 ? 1 : HomePush.TimerTick;
            }, (obj) => HomePush.IsService);
        #endregion

        # region inventory
        public ICommand UpdateInformationsCommand =>
            new RelayCommand((obj) =>
            {
                Task.Run(() =>
                {
                    try
                    {
                        HomeInventoryInfo.IsBusy = true;
                        var items = InventoryService.CheckInventory();
                        HomeInventoryInfo.Items = new(items);
                        HomeInventoryInfo.SumOfItems = InventoryService.GetSumOfItems(items);
                        SelectedInventory = HomeInventoryInfo.Items.FirstOrDefault();
                    }
                    catch (Exception ex)
                    {
                        BaseService.errorLog(ex, true);
                    }
                    finally
                    {
                        HomeInventoryInfo.IsBusy = false;
                        HomeInventoryConfig.TaskId = 0;
                        Main.Message.Enqueue("Steam Inventory updated.");
                    }
                });
            }, (obj) => !HomeInventoryInfo.IsBusy);
        public ICommand ShowInventoryItemCommand =>
            new RelayCommand((obj) =>
            {
                var item = (DataInventory)obj;

                Edit.OpenUrl("https://steamcommunity.com/my/inventory/#730_2_" + item.Data.FirstOrDefault().AssetId);

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

                    switch (config.TaskId)
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
            }, (obj) => HomeInventoryConfig.TaskId == 0
                            || (HomeInventoryConfig.TaskId == 1 && HomeInventoryInfo.Items.Any()
                                && ((HomeInventoryConfig.AllAvailable && HomeInventoryConfig.SellingPriceId != 2)
                                    || (HomeInventoryConfig.SelectedOnly && SelectedInventory != null
                                        && (HomeInventoryConfig.SellingPriceId < 2 || (HomeInventoryConfig.SellingPriceId == 2 && HomeInventoryConfig.Price != 0))))));
        void TradeOffer()
        {
            try
            {
                var trades = InventoryService.CheckOffer();
                while (trades.Any())
                {
                    HomeInventoryInfo.Progress = 0;
                    HomeInventoryInfo.MaxProgress = trades.Count;
                    foreach (var offer in trades)
                    {
                        try
                        {
                            SteamRequest.Post.AcceptTrade(offer.TradeOfferId, offer.PartnerId);
                        }
                        catch (Exception exp)
                        {
                            BaseService.errorLog(exp, false);
                        }
                        finally
                        {
                            HomeInventoryInfo.Progress++;
                            Thread.Sleep(1000);
                        }
                        if (HomeInventoryInfo.token.IsCancellationRequested)
                            break;
                    }
                    trades = InventoryService.CheckOffer();
                }
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
                var items = InventoryService.CheckInventory();
                HomeInventoryInfo.Items = new(items);

                items = config.SelectedOnly ? items.Where(x => x.ItemName != SelectedInventory.ItemName).ToList() : items;
                HomeInventoryInfo.Progress = 0;
                HomeInventoryInfo.MaxProgress = items.Count;

                foreach (var item in items)
                {
                    try
                    {
                        InventoryService.SellItem(item, config);
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
                SelectedInventory = HomeInventoryInfo.Items.Any() ? HomeInventoryInfo.Items.FirstOrDefault() : new();
                HomeInventoryInfo.IsService = false;
                Main.Message.Enqueue("Quick sell items has finished.");
            }
        }
        #endregion
    }
}