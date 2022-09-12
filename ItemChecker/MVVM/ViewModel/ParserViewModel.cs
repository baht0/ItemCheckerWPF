using ItemChecker.Core;
using ItemChecker.MVVM.Model;
using ItemChecker.Properties;
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
        private ParserTable _parserTable = new();
        private ParserFilter _filterConfig = new();
        private string _searchString;
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

        //panel
        private ParserCheckConfig _parserCheckConfig = new();
        private ParserCheckInfo _parserCheckInfo = new();
        private ParserQueue _parserQueue = new();
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
        public ParserQueue ParserQueue
        {
            get { return _parserQueue; }
            set
            {
                _parserQueue = value;
                OnPropertyChanged();
            }
        }
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
                ParserQueue.TotalAllowed = SteamAccount.MaxAmount;
                ParserQueue.AvailableAmount = availableAmount;
                ParserQueue.OrderAmout = ParserQueue.Queues.Select(x => x.OrderPrice).Sum();
                ParserQueue.Remaining = availableAmount - ParserQueue.OrderAmout;
                ParserQueue.AvailableAmountPrecent = Math.Round(availableAmount / SteamAccount.MaxAmount * 100, 1);
            }
            catch (Exception ex)
            {
                BaseService.errorLog(ex, false);
            }
        }
        //grid
        public ICommand OpenItemOutCommand =>
            new RelayCommand((obj) =>
            {
                DataParser item = ParserTable.SelectedItem;
                string itemName = item.ItemName.Replace("(Holo/Foil)", "(Holo-Foil)");
                string market_hash_name = Edit.EncodeMarketHashName(itemName);
                switch ((Int32)obj)
                {
                    case 1:
                        switch (ParserCheckConfig.CheckedConfig.ServiceOne)
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
                                Edit.OpenCsm(market_hash_name);
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
                Currency currency = SteamBase.CurrencyList.FirstOrDefault(x => x.Name == (string)obj);
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
        //check
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
            }, (obj) => ParserCheckConfig.ServiceOne != ParserCheckConfig.ServiceTwo && SteamBase.ItemList.Any());
        public ICommand ContinueCheckCommand =>
            new RelayCommand((obj) =>
            {
                Task.Run(() => PreparationCheck(ParserCheckConfig.CheckedConfig, ParserTable.Items.ToList()));
            }, (obj) => !ParserCheckInfo.IsParser && ParserCheckInfo.IsStoped);
        void PreparationCheck(ParserCheckConfig config, List<DataParser> checkedList)
        {
            try
            {
                ParserCheckInfo.Status = "Preparation...";
                ParserCheckInfo.TimerOn = true;
                ParserCheckInfo.IsParser = true;
                ParserCheckInfo.IsStoped = false;
                ParserCheckInfo.cts = new();
                ParserCheckInfo.token = ParserCheckInfo.cts.Token;
                SaveConfig(config);
                DataGrid(config);

                ParserCheckInfo.Status = "Create List...";
                List<string> checkList = ParserCheckService.ApplyConfig(config);
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
                {
                    ParserService list = new();
                    list.Export(ParserTable.Items, ParserCheckConfig.CheckedConfig);
                }
                ParserCheckInfo.TimerOn = false;
                ParserCheckInfo.IsParser = false;
            }
        }
        void DataGrid(ParserCheckConfig config)
        {
            ParserTable.Count = 0;
            ParserCheckInfo.DateTime = config.CheckedTime;
            if (ParserTable.Items.Any())
            {
                FilterConfig = new();
                ParserFilter.FilterConfig = new();

                ParserTable.Items = new();
                ParserTable.GridView = CollectionViewSource.GetDefaultView(ParserTable.Items);
            }

            ItemBaseService baseService = new();
            ParserCheckInfo.Service1 = Main.Services[config.ServiceOne];
            switch (config.ServiceOne)
            {
                case 2:
                    baseService.UpdateInventoryCsm(config);
                    break;
                case 3:
                    baseService.UpdateLfm();
                    break;
                case 4 or 5:
                    baseService.UpdateBuff(false, config.MinPrice, config.MaxPrice);
                    break;
            }
            ParserCheckInfo.Service2 = Main.Services[config.ServiceTwo];
            switch (config.ServiceTwo)
            {
                case 2:
                    baseService.UpdateCsm();
                    break;
                case 3:
                    baseService.UpdateLfm();
                    break;
                case 4:
                    int min = (int)(config.MinPrice * 0.5m);
                    int max = (int)(config.MaxPrice * 2.5m);
                    baseService.UpdateBuff(true, min, max);
                    break;
                case 5:
                    min = (int)(config.MinPrice * 0.5m);
                    max = (int)(config.MaxPrice * 2.5m);
                    baseService.UpdateBuff(false, min, max);
                    break;
            }
            ParserCheckInfo.IsVisible = true;
        }
        void SaveConfig(ParserCheckConfig config)
        {
            ParserProperties.Default.ServiceOne = config.ServiceOne;
            ParserProperties.Default.ServiceTwo = config.ServiceTwo;

            ParserProperties.Default.MinPrice = config.MinPrice;
            ParserProperties.Default.MaxPrice = config.MaxPrice;

            ParserProperties.Default.NotWeapon = config.NotWeapon;
            ParserProperties.Default.Normal = config.Normal;
            ParserProperties.Default.Souvenir = config.Souvenir;
            ParserProperties.Default.Stattrak = config.Stattrak;
            ParserProperties.Default.KnifeGlove = config.KnifeGlove;
            ParserProperties.Default.KnifeGloveStattrak = config.KnifeGloveStattrak;

            ParserProperties.Default.SelectedOnly = config.SelectedOnly;

            ParserProperties.Default.WithoutLock = config.WithoutLock;
            ParserProperties.Default.UserItems = config.UserItems;
            ParserProperties.Default.RareItems = config.RareItems;

            ParserProperties.Default.Save();
        }
        void StartCheck(List<string> checkList, List<DataParser> checkedList)
        {
            if (ParserCheckInfo.token.IsCancellationRequested)
                return;

            DateTime now = DateTime.Now;
            int itemCount = checkList.Count - checkedList.Count;
            ParserCheckInfo.Timer.Elapsed += (sender, e) => timerTick(checkList.Count, now);
            ParserCheckInfo.Timer.Enabled = true;

            ParserCheckService checkService = new();
            foreach (string itemName in checkList)
            {
                try
                {
                    if (!checkedList.Any(x => x.ItemName == itemName))
                    {
                        DataParser data = checkService.Check(itemName, ParserProperties.Default.ServiceOne, ParserProperties.Default.ServiceTwo);
                        checkedList.Add(data);
                    }
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
                    ParserCheckInfo.Status = "Reading...";
                    ParserCheckInfo.TimerOn = true;
                    ParserCheckInfo.IsParser = true;
                    ParserService service = new();
                    var response = service.Import(filePath);
                    if (response.Any())
                    {
                        ParserCheckInfo.Status = "Preparation...";
                        DataGrid(ParserCheckConfig.CheckedConfig);

                        ParserTable.CurrencyId = 0;
                        ParserTable.Count = response.Count;
                        ParserTable.Items = response;
                        ParserTable.GridView = CollectionViewSource.GetDefaultView(response);

                        Main.Message.Enqueue("Import table done.");
                    }
                    ParserCheckInfo.IsParser = false;
                    ParserCheckInfo.TimerOn = false;
                });
            }, (obj) => !ParserCheckInfo.IsParser);
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
                BaseModel.IsWorking = true;
                Task.Run(() => {
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
                    BaseModel.IsWorking = false;
                });
            }, (obj) => ParserQueue.Items.Any() & !BaseModel.IsWorking);
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