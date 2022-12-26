using ItemChecker.Core;
using ItemChecker.MVVM.Model;
using ItemChecker.Services;
using ItemChecker.Support;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace ItemChecker.MVVM.ViewModel
{
    public class ParserViewModel : ObservableObject
    {
        #region prop
        Timer TimerView = new(500);
        //table
        public ParserTable ParserTable
        {
            get
            {
                return _parserTable;
            }
            set
            {
                _parserTable = value;
                OnPropertyChanged();
            }
        }
        ParserTable _parserTable = new();
        public ParserFilter FilterConfig
        {
            get
            {
                return _filterConfig;
            }
            set
            {
                _filterConfig = value;
                OnPropertyChanged();
            }
        }
        ParserFilter _filterConfig = new();
        public string SearchString
        {
            get
            {
                return _searchString;
            }
            set
            {
                _searchString = value;
                FilterConfig = new ParserFilter();
                if (ParserTable.GridView != null)
                    ParserTable.GridView.Filter = item =>
                    {
                        return ((DataParser)item).ItemName.Contains(value, StringComparison.OrdinalIgnoreCase);
                    };
                OnPropertyChanged();
            }
        }
        string _searchString;

        //panel
        public ParserCheckConfig ParserCheckConfig
        {
            get
            {
                return _parserCheckConfig;
            }
            set
            {
                _parserCheckConfig = value;
                OnPropertyChanged();
            }
        }
        ParserCheckConfig _parserCheckConfig = new();
        public ParserCheckInfo ParserCheckInfo
        {
            get
            {
                return _parserCheckInfo;
            }
            set
            {
                _parserCheckInfo = value;
                OnPropertyChanged();
            }
        }
        ParserCheckInfo _parserCheckInfo = new();
        public ParserQueue ParserQueue
        {
            get { return _parserQueue; }
            set
            {
                _parserQueue = value;
                OnPropertyChanged();
            }
        }
        ParserQueue _parserQueue = new();
        #endregion 

        public ParserViewModel()
        {
            ParserTable.GridView = CollectionViewSource.GetDefaultView(ParserTable.Items);
            TimerView.Elapsed += UpdateView;
            TimerView.Enabled = true;
        }
        void UpdateView(Object sender, ElapsedEventArgs e)
        {
            try
            {
                decimal availableAmount = SteamMarket.Orders.GetAvailableAmount();
                ParserQueue.TotalAllowed = SteamMarket.MaxAmount;
                ParserQueue.AvailableAmount = availableAmount;
                ParserQueue.OrderAmout = ParserQueue.Queues.Select(x => x.OrderPrice).Sum();
                ParserQueue.Remaining = availableAmount - ParserQueue.OrderAmout;
                ParserQueue.AvailableAmountPrecent = Math.Round(availableAmount / SteamMarket.MaxAmount * 100, 1);
            }
            catch (Exception ex)
            {
                BaseService.errorLog(ex, false);
            }
        }

        #region table
        //grid
        public ICommand OpenItemOutCommand =>
            new RelayCommand((obj) =>
            {
                DataParser item = ParserTable.SelectedItem;
                string itemName = item.ItemName.Replace("(Holo/Foil)", "(Holo-Foil)");
                string market_hash_name = Uri.EscapeDataString(itemName);
                switch ((Int32)obj)
                {
                    case 1:
                        switch (ParserCheckConfig.CheckedConfig.ServiceOne)
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
                                Edit.OpenUrl("https://buff.163.com/goods/" + SteamBase.ItemList.FirstOrDefault(x => x.ItemName == item.ItemName).Buff.Id + "#tab=buying");
                                break;
                            case 5:
                                Edit.OpenUrl("https://buff.163.com/goods/" + SteamBase.ItemList.FirstOrDefault(x => x.ItemName == item.ItemName).Buff.Id);
                                break;
                        }
                        break;
                    case 2 or 3:
                        switch (ParserCheckConfig.CheckedConfig.ServiceTwo)
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
                                Edit.OpenUrl("https://buff.163.com/goods/" + SteamBase.ItemList.FirstOrDefault(x => x.ItemName == item.ItemName).Buff.Id + "#tab=buying");
                                break;
                            case 5:
                                Edit.OpenUrl("https://buff.163.com/goods/" + SteamBase.ItemList.FirstOrDefault(x => x.ItemName == item.ItemName).Buff.Id);
                                break;
                        }
                        break;
                    default:
                        Clipboard.SetText(itemName);
                        break;
                }
            });
        public ICommand SwitchCurrencyCommand =>
            new RelayCommand((obj) =>
            {
                Currency currency = SteamBase.AllowCurrencys.FirstOrDefault(x => x.Name == (string)obj);
                List<DataParser> items = ParserTable.Items.ToList();
                if (ParserTable.CurectCurrency.Id != 1)
                    foreach (DataParser item in items)
                    {
                        item.Purchase = Edit.ConverterToUsd(item.Purchase, ParserTable.CurectCurrency.Value);
                        item.Price = Edit.ConverterToUsd(item.Price, ParserTable.CurectCurrency.Value);
                        item.Get = Edit.ConverterToUsd(item.Get, ParserTable.CurectCurrency.Value);
                        item.Difference = Edit.ConverterToUsd(item.Difference, ParserTable.CurectCurrency.Value);
                    }
                foreach (DataParser item in items)
                {
                    item.Purchase = Edit.ConverterFromUsd(item.Purchase, currency.Value);
                    item.Price = Edit.ConverterFromUsd(item.Price, currency.Value);
                    item.Get = Edit.ConverterFromUsd(item.Get, currency.Value);
                    item.Difference = Edit.ConverterFromUsd(item.Difference, currency.Value);
                }
                ParserTable.CurrencySymbol = currency.Symbol;
                ParserTable.CurectCurrency = currency;
                FilterConfig = new();
                ParserTable.GridView = CollectionViewSource.GetDefaultView(items);
            }, (obj) => ParserTable.Items.Any());
        //filter
        public ICommand ClearSearchCommand =>
            new RelayCommand((obj) =>
            {
                SearchString = string.Empty;
            }, (obj) => !String.IsNullOrEmpty(SearchString));
        public ICommand ApplyCommand =>
            new RelayCommand((obj) =>
            {
                ParserFilter filterConfig = obj as ParserFilter;

                ParserTable.GridView.Filter = null;
                ParserTable.GridView.Filter = item =>
                {
                    return ParserService.ApplyFilter(filterConfig, (DataParser)item);
                };
                ParserFilter.FilterConfig = filterConfig;
            }, (obj) => ParserTable.Items.Any());
        public ICommand ResetCommand =>
            new RelayCommand((obj) =>
            {
                FilterConfig = new();
                ParserTable.GridView.Filter = null;
                ParserFilter.FilterConfig = new();
            }, (obj) => ParserTable.Items.Any());
        #endregion

        #region Check
        //check
        public ICommand MaxPriceCommand =>
            new RelayCommand((obj) =>
            {
                switch ((int)obj)
                {
                    case 0 or 1:
                        var steamUsd = Edit.ConverterToUsd(SteamAccount.Balance, SteamAccount.Currency.Value);
                        ParserCheckConfig.MaxPrice = (int)Math.Ceiling(steamUsd);
                        break;
                    case 2:
                        ParserCheckConfig.MaxPrice = (int)Math.Ceiling(ServiceAccount.Csm.Balance);
                        break;
                    case 3:
                        ParserCheckConfig.MaxPrice = (int)Math.Ceiling(ServiceAccount.Lfm.Balance);
                        break;
                    case 4 or 5:
                        ParserCheckConfig.MaxPrice = (int)Math.Ceiling(ServiceAccount.Buff.Balance);
                        break;
                }
            });
        public ICommand CheckCommand =>
            new RelayCommand((obj) =>
            {
                if (!ParserCheckInfo.IsParser)
                {
                    var config = (ParserCheckConfig)obj;
                    config.CheckedTime = DateTime.Now;
                    ParserCheckConfig.CheckedConfig = (ParserCheckConfig)config.Clone();

                    Task.Run(() => PreparationCheck(config, new()));
                }
                else
                {
                    ParserCheckInfo.cts.Cancel();
                    ParserCheckInfo.IsParser = false;
                    ParserCheckInfo.IsStoped = true;
                }
            }, (obj) => ParserCheckConfig.ServiceOne != ParserCheckConfig.ServiceTwo && SteamBase.ItemList.Any() && ParserCheckConfig.MaxPrice != 0);
        public ICommand ContinueCheckCommand =>
            new RelayCommand((obj) =>
            {
                Task.Run(() => PreparationCheck(ParserCheckConfig.CheckedConfig, ParserTable.Items.ToList()));
            }, (obj) => !ParserCheckInfo.IsParser && ParserCheckInfo.IsStoped);
        void PreparationCheck(ParserCheckConfig config, List<DataParser> checkedList)
        {
            try
            {
                ParserCheckInfo.cts = new();
                ParserCheckInfo.token = ParserCheckInfo.cts.Token;
                PreparationView(config);

                var checkList = CreateList(config, checkedList);
                ParserCheckInfo.CountList = checkList.Count;
                ParserCheckInfo.CurrentProgress = 0;
                ParserCheckInfo.MaxProgress = checkList.Count;
                if (checkList.Any())
                    StartCheck(checkList, checkedList);
                else
                    MessageBox.Show("Nothing found. Adjust the parameters.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);

                ParserTable.Count = ParserTable.Items.Count;
                ParserTable.GridView = CollectionViewSource.GetDefaultView(ParserTable.Items);
            }
            catch (Exception exp)
            {
                ParserCheckInfo.cts.Cancel();
                BaseService.errorLog(exp, true);
            }
            finally
            {
                if (ParserTable.Items.Any())
                    ParserService.Export(ParserTable.Items, ParserCheckConfig.CheckedConfig);
                ParserCheckInfo.TimerOn = false;
                ParserCheckInfo.IsParser = false;
            }
        }
        void PreparationView(ParserCheckConfig config)
        {
            ParserCheckInfo.Status = "Preparation...";
            ParserCheckInfo.TimerOn = true;
            ParserCheckInfo.IsParser = true;
            ParserCheckInfo.IsStoped = false;
            ParserTable.Count = 0;
            ParserCheckInfo.DateTime = config.CheckedTime;
            if (ParserTable.Items.Any())
            {
                FilterConfig = new();
                ParserFilter.FilterConfig = new();

                ParserTable.Items = new();
                ParserTable.GridView = CollectionViewSource.GetDefaultView(ParserTable.Items);
            }
            ParserCheckInfo.Service1 = Main.Services[config.ServiceOne];
            ParserCheckInfo.Service2 = Main.Services[config.ServiceTwo];
            ParserCheckInfo.IsVisible = true;
        }
        List<string> CreateList(ParserCheckConfig config, List<DataParser> checkedList)
        {
            ParserCheckInfo.Status = "Create List...";
            ItemBaseService baseService = new();
            switch (config.ServiceOne)
            {
                case 2:
                    baseService.UpdateCsm(config);
                    break;
                case 3:
                    baseService.UpdateLfm();
                    break;
                case 4 or 5:
                    baseService.UpdateBuff(config.ServiceOne == 4, config.MinPrice, config.MaxPrice);
                    break;
            }
            switch (config.ServiceTwo)
            {
                case 3:
                    baseService.UpdateLfm();
                    break;
            }
            var list = ParserCheckService.ApplyConfig(config, checkedList);

            if ((config.ServiceTwo == 4 || config.ServiceTwo == 5) && (SteamBase.ItemList.Count / 80 < list.Count))
            {
                int min = (int)(config.MinPrice * 0.5m);
                int max = (int)(config.MaxPrice * 2.5m);

                baseService.UpdateBuff(config.ServiceTwo == 4, min, max);
            }
            return list;
        }
        void StartCheck(List<string> checkList, List<DataParser> checkedList)
        {
            DateTime now = DateTime.Now;
            ParserCheckInfo.Timer.Elapsed += (sender, e) => timerTick(checkList.Count, now);
            ParserCheckInfo.Timer.Enabled = true;

            ParserCheckService checkService = new();
            foreach (string itemName in checkList)
            {
                try
                {
                    DataParser data = checkService.Check(itemName, ParserCheckConfig.CheckedConfig.ServiceOne, ParserCheckConfig.CheckedConfig.ServiceTwo);
                    checkedList.Add(data);
                }
                catch (Exception exp)
                {
                    if (!exp.Message.Contains("429"))
                        BaseService.errorLog(exp, true);
                    else
                        MessageBox.Show(exp.Message + "\n\nTo continue, you need to change the IP address.", "Parser stoped!", MessageBoxButton.OK, MessageBoxImage.Warning);
                    ParserCheckInfo.IsStoped = true;
                    ParserCheckInfo.cts.Cancel();
                }
                finally
                {
                    ParserCheckInfo.CurrentProgress++;
                    System.Threading.Thread.Sleep(100);
                }
                if (ParserCheckInfo.token.IsCancellationRequested)
                    break;
            }
            ParserTable.CurrencyId = 0;
            System.Threading.Thread.Sleep(1000);
            ParserTable.Items = new(checkedList);
            ParserCheckInfo.Timer.Enabled = false;
            ParserCheckInfo.Timer.Elapsed -= (sender, e) => timerTick(checkList.Count, now);
        }
        void timerTick(int itemCount, DateTime timeStart)
        {
            if (ParserCheckInfo.token.IsCancellationRequested)
                ParserCheckInfo.Timer.Enabled = false;
            ParserCheckInfo.Status = Edit.calcTimeLeft(timeStart, itemCount, ParserCheckInfo.CurrentProgress);
        }
        //file
        public ICommand ImportCommand =>
            new RelayCommand((obj) =>
            {
                string filePath = (string)obj;
                Task.Run(() =>
                {
                    var response = ParserService.Import(filePath);
                    if (response.Any())
                    {
                        PreparationView(ParserCheckConfig.CheckedConfig);
                        ParserTable.CurrencyId = 0;
                        ParserTable.Count = response.Count;
                        ParserTable.Items = response;
                        ParserTable.GridView = CollectionViewSource.GetDefaultView(response);

                        Main.Message.Enqueue("The table was successfully imported.");
                    }
                    ParserCheckInfo.IsParser = false;
                    ParserCheckInfo.TimerOn = false;
                });
            }, (obj) => !ParserCheckInfo.IsParser);
        #endregion

        //order
        public ICommand AddQueueCommand =>
            new RelayCommand((obj) =>
            {
                var parseredItem = obj as DataParser;
                if (ParserQueue.Queues.Add(new(parseredItem.ItemName, parseredItem.Purchase, parseredItem.Precent)))
                {
                    ParserQueue.Items = new(ParserQueue.Queues);
                    Main.Message.Enqueue($"Success, added to Queue.\n{parseredItem.ItemName}");
                    return;
                }
                Main.Message.Enqueue("Not successful. Conditions not met.");

            }, (obj) => ParserCheckConfig.CheckedConfig.ServiceOne == 0);
        public ICommand RemoveQueueCommand =>
            new RelayCommand((obj) =>
            {
                var item = obj as DataQueue;

                ParserQueue.Queues.Remove(item);
                ParserQueue.Items = new(ParserQueue.Queues);

            }, (obj) => ParserQueue.Items.Any() && ParserQueue.SelectedQueue != null);
        public ICommand ClearQueueCommand =>
            new RelayCommand((obj) =>
            {
                MessageBoxResult result = MessageBox.Show(
                    "Are you sure you want to clear the list?",
                    "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    ParserQueue.Queues.Clear();
                    ParserQueue.Items = new();
                }
            }, (obj) => ParserQueue.Items.Any());
        public ICommand PlaceOrderCommand =>
            new RelayCommand((obj) =>
            {
                ParserQueue.IsBusy = true;
                Task.Run(() => {
                    try
                    {
                        SteamAccount.GetBalance();

                        int success = 0;
                        ParserQueue.MaxProgress = ParserQueue.Items.Count;
                        ParserQueue.CurrentProgress = 0;
                        foreach (var item in ParserQueue.Queues)
                        {
                            try
                            {
                                OrderService.PlaceOrder(item.ItemName);
                                success++;
                            }
                            catch (Exception exp)
                            {
                                if (!exp.Message.Contains("429"))
                                    BaseService.errorLog(exp, true);
                                else
                                    MessageBox.Show(exp.Message + "\n\nTo continue, you need to change the IP address.", "PlaceOrder stoped!", MessageBoxButton.OK, MessageBoxImage.Warning);
                                break;
                            }
                            finally
                            {
                                ParserQueue.CurrentProgress++;
                            }
                        }
                        Main.Notifications.Add(new()
                        {
                            Title = "Place Order",
                            Message = $"{success}/{ParserQueue.Items.Count} orders has been placed."
                        });
                        ParserQueue.Items = new();
                        ParserQueue.Queues.Clear();
                    }
                    catch (Exception ex)
                    {
                        BaseService.errorLog(ex, true);
                    }
                    finally
                    {
                        ParserQueue.IsBusy = false;
                    }                    
                });
            }, (obj) => ParserQueue.Items.Any() && !ParserQueue.IsBusy);
        //favorite
        public ICommand AddFavoriteCommand =>
            new RelayCommand((obj) =>
            {
                var name = obj as string;

                DataItem item = new(name, ParserCheckConfig.CheckedConfig.ServiceTwo);
                string message = ItemsList.Favorite.Add(item) ? $"{item.ItemName}\nItem has been added." : "Opps something went wrong...";
                Main.Message.Enqueue(message);
            }, (obj) => ParserCheckConfig.ServiceOne < 2);
        public ICommand RemoveFavoriteCommand =>
            new RelayCommand((obj) =>
            {
                string itemName = (string)obj;
                var item = ItemsList.Favorite.FirstOrDefault(x => x.ItemName == itemName);
                if (item != null)
                {
                    ItemsList.Favorite.Remove(item);
                    Main.Message.Enqueue($"{itemName}\nRemoved from list.");
                }
            }, (obj) => ItemsList.Favorite.Any());
    }
}