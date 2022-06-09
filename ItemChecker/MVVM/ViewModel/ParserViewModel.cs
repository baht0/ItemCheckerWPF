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
        private DataParser _selectedItem;
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
        public DataParser SelectedItem
        {
            get
            {
                return _selectedItem;
            }
            set
            {
                _selectedItem = value;
                OnPropertyChanged();
                if (value == null)
                    return;

                ParserInfo.ItemName = value.ItemName;
                if (ParserInfo.ST)
                {
                    ParserInfo.ItemSt = new();
                    var data = SteamBase.ItemList.FirstOrDefault(x => x.ItemName == value.ItemName).Steam;
                    Task.Run(() => {
                        if (!data.PriceHistory.Any())
                        {
                            data.LowestSellOrder = Edit.ConverterFromUsd(data.LowestSellOrder, ParserTable.CurectCurrency.Value);
                            data.HighestBuyOrder = Edit.ConverterFromUsd(data.HighestBuyOrder, ParserTable.CurectCurrency.Value);

                            ItemBaseService baseService = new();
                            var history = baseService.GetPriceHistory(value.ItemName);
                            data.PriceHistory = history;
                            int days = (int)(DateTime.Now - history.FirstOrDefault().Date).TotalDays;
                            data.LastSale = Tuple.Create(history.FirstOrDefault().Date, days, history.FirstOrDefault().Price);
                            List<decimal> last30 = history.Where(x => x.Date > DateTime.Today.AddDays(-30)).Select(x => x.Price).ToList();
                            data.Count30 = last30.Count;
                            List<decimal> last60 = history.Where(x => x.Date > DateTime.Today.AddDays(-60)).Select(x => x.Price).ToList();
                            data.Count60 = last60.Count;
                            if (last30.Any())
                                data.Avg30 = Math.Round(Queryable.Average(last30.AsQueryable()), 2);
                            if (last60.Any())
                                data.Avg60 = Math.Round(Queryable.Average(last60.AsQueryable()), 2);
                        }
                        ParserInfo.ItemSt = data;
                    });
                }
                else if (ParserInfo.CSM)
                {
                    ParserInfo.InventoryCsm = DataInventoriesCsm.Items.Where(x => x.ItemName == value.ItemName).Reverse().ToList();
                    ParserInfo.ItemCsmComparePrice = value.Price1;
                    ParserInfo.InfoItemCount = ParserInfo.InventoryCsm.Count - 1;
                    ParserInfo.InfoItemCurrent = 0;
                }
                else if (ParserInfo.LF)
                    ParserInfo.ItemLf = SteamBase.ItemList.FirstOrDefault(x => x.ItemName == value.ItemName).Lfm;
                else if (ParserInfo.BF)
                    ParserInfo.ItemBf = SteamBase.ItemList.FirstOrDefault(x => x.ItemName == value.ItemName).Buff;
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
                OnPropertyChanged();
                FilterConfig = new ParserFilter();
                if (ParserTable.GridView != null)
                    ParserTable.GridView.Filter = item =>
                    {
                        return ((DataParser)item).ItemName.Contains(value, StringComparison.OrdinalIgnoreCase);
                    };
            }
        }

        //panel
        private ParserCheckConfig _parserCheckConfig = new();
        private ParserCheckInfo _parserCheckInfo = new();
        private ParserInfo _parserInfo = new();
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
        public ParserInfo ParserInfo
        {
            get
            {
                return _parserInfo;
            }
            set
            {
                _parserInfo = value;
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
                decimal availableAmount = SteamAccount.GetAvailableAmount();
                decimal total = SteamAccount.Balance * 10;
                ParserQueue.TotalAllowed = total;
                ParserQueue.AvailableAmount = availableAmount;
                ParserQueue.Remaining = availableAmount - ParserQueue.OrderAmout;
                ParserQueue.AvailableAmountPrecent = Math.Round(availableAmount / total * 100, 1);
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
                DataParser item = SelectedItem;
                string itemName = item.ItemName.Replace("(Holo/Foil)", "(Holo-Foil)");
                string market_hash_name = Edit.EncodeMarketHashName(itemName);
                switch ((Int32)obj)
                {
                    case 1 or 2:
                        if (ParserCheckInfo.Service1 == "SteamMarket" || ParserCheckInfo.Service1 == "SteamMarket(A)")
                            Edit.OpenUrl("https://steamcommunity.com/market/listings/730/" + market_hash_name);
                        else if (ParserCheckInfo.Service1 == "Cs.Money")
                            Edit.OpenCsm(market_hash_name);
                        else if (ParserCheckInfo.Service1 == "Buf163")
                            Edit.OpenUrl("https://buff.163.com/goods/" + SteamBase.ItemList.FirstOrDefault(x => x.ItemName == item.ItemName).Buff.Id);
                        break;
                    case 3 or 4:
                        if (ParserCheckInfo.Service2 == "SteamMarket" || ParserCheckInfo.Service2 == "SteamMarket(A)")
                            Edit.OpenUrl("https://steamcommunity.com/market/listings/730/" + market_hash_name);
                        else if (ParserCheckInfo.Service2 == "Cs.Money")
                            Edit.OpenCsm(market_hash_name);
                        else if (ParserCheckInfo.Service2 == "Buf163")
                            Edit.OpenUrl("https://buff.163.com/goods/" + SteamBase.ItemList.FirstOrDefault(x => x.ItemName == item.ItemName).Buff.Id);
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
                        item.Price1 = Edit.ConverterToUsd(item.Price1, ParserTable.CurectCurrency.Value);
                        item.Price2 = Edit.ConverterToUsd(item.Price2, ParserTable.CurectCurrency.Value);
                        item.Price3 = Edit.ConverterToUsd(item.Price3, ParserTable.CurectCurrency.Value);
                        item.Price4 = Edit.ConverterToUsd(item.Price4, ParserTable.CurectCurrency.Value);
                        item.Difference = Edit.ConverterToUsd(item.Difference, ParserTable.CurectCurrency.Value);
                    }
                foreach (DataParser item in items)
                {
                    item.Price1 = Edit.ConverterFromUsd(item.Price1, currency.Value);
                    item.Price2 = Edit.ConverterFromUsd(item.Price2, currency.Value);
                    item.Price3 = Edit.ConverterFromUsd(item.Price3, currency.Value);
                    item.Price4 = Edit.ConverterFromUsd(item.Price4, currency.Value);
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
                DataGrid(config, "CheckList");

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
        void DataGrid(ParserCheckConfig config, string mode)
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
            ParserCheckInfo.Mode = mode;
            ItemBaseService baseService = new();
            switch (config.ServiceOne)
            {
                case 0:
                    ParserCheckInfo.Service1 = "SteamMarket(A)";
                    ParserTable.Price1 = "Sale(ST)";
                    ParserTable.Price2 = "BuyOrder(ST)";
                    ParserInfo.ST = true;
                    ParserInfo.CSM = false;
                    ParserInfo.LF = false;
                    ParserInfo.BF = false;
                    break;
                case 1:
                    ParserCheckInfo.Service1 = "SteamMarket";
                    ParserTable.Price1 = "Sale(ST)";
                    ParserTable.Price2 = "BuyOrder(ST)";
                    ParserInfo.ST = true;
                    ParserInfo.CSM = false;
                    ParserInfo.LF = false;
                    ParserInfo.BF = false;
                    break;
                case 2:
                    ParserCheckInfo.Service1 = "Cs.Money";
                    ParserTable.Price1 = "Trade(CSM)";
                    ParserTable.Price2 = "Give(CSM)";
                    ParserInfo.ST = false;
                    ParserInfo.CSM = true;
                    ParserInfo.LF = false;
                    ParserInfo.BF = false;
                    baseService.LoadInventoriesCsm(config);
                    break;
                case 3:
                    ParserCheckInfo.Service1 = "Loot.Farm";
                    ParserTable.Price1 = "Trade(LF)";
                    ParserTable.Price2 = "Give(LF)";
                    ParserInfo.ST = false;
                    ParserInfo.CSM = false;
                    ParserInfo.LF = true;
                    ParserInfo.BF = false;
                    baseService.UpdateLfmInfo();
                    break;
                case 4:
                    ParserCheckInfo.Service1 = "Buf163";
                    ParserTable.Price1 = "Sale(BF)";
                    ParserTable.Price2 = "BuyOrder(BF)";
                    ParserInfo.ST = false;
                    ParserInfo.CSM = false;
                    ParserInfo.LF = false;
                    ParserInfo.BF = true;
                    baseService.UpdateBuffInfo(false, config.MinPrice, config.MaxPrice);
                    break;
            }
            switch (config.ServiceTwo)
            {
                case 0:
                    ParserCheckInfo.Service2 = "SteamMarket(A)";
                    ParserTable.Price3 = "Sale(ST)";
                    ParserTable.Price4 = "BuyOrder(ST)";
                    break;
                case 1:
                    ParserCheckInfo.Service2 = "SteamMarket";
                    ParserTable.Price3 = "Sale(ST)";
                    ParserTable.Price4 = "BuyOrder(ST)";
                    break;
                case 2:
                    ParserCheckInfo.Service2 = "Cs.Money";
                    ParserTable.Price3 = "Trade(CSM)";
                    ParserTable.Price4 = "Give(CSM)";
                    baseService.UpdateCsmInfo();
                    break;
                case 3:
                    ParserCheckInfo.Service2 = "Loot.Farm";
                    ParserTable.Price3 = "Trade(LF)";
                    ParserTable.Price4 = "Give(LF)";
                    baseService.UpdateLfmInfo();
                    break;
                case 4:
                    ParserCheckInfo.Service2 = "Buf163";
                    ParserTable.Price3 = "Sale(BF)";
                    ParserTable.Price4 = "BuyOrder(BF)";
                    int min = (int)(config.MinPrice * 0.5m);
                    int max = (int)(config.MaxPrice * 2.5m);
                    baseService.UpdateBuffInfo(true, min, max);
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
                Task.Run(() =>
                {
                    ParserCheckInfo.Status = "Reading...";
                    ParserCheckInfo.TimerOn = true;
                    ParserCheckInfo.IsParser = true;
                    ParserService service = new();
                    var response = service.Import();
                    if (response.Any())
                    {
                        ParserCheckInfo.Status = "Preparation...";
                        DataGrid(ParserCheckConfig.CheckedConfig, "Import");

                        ParserTable.Count = response.Count;
                        ParserTable.Items = response;
                        ParserTable.GridView = CollectionViewSource.GetDefaultView(response);

                        Main.Message.Enqueue("Import table done.");
                    }
                    ParserCheckInfo.IsParser = false;
                    ParserCheckInfo.TimerOn = false;
                });
            }, (obj) => !ParserCheckInfo.IsParser);
        //info
        public ICommand UpdateInventoryCommand =>
            new RelayCommand((obj) =>
            {
                BaseModel.IsWorking = true;
                ItemBaseService baseService = new();
                Task.Run(() => {
                    switch (ParserCheckConfig.CheckedConfig.ServiceOne)
                    {
                        case 2:
                            baseService.LoadInventoriesCsm(ParserCheckConfig.CheckedConfig);
                            break;
                        case 3:
                            baseService.UpdateLfmInfo();
                            break;
                        case 4:
                            baseService.UpdateBuffInfo(false, ParserCheckConfig.CheckedConfig.MinPrice, ParserCheckConfig.CheckedConfig.MaxPrice);
                            break;
                    }
                    BaseModel.IsWorking = false;
                    Main.Message.Enqueue("Service inventory update completed.");
                });
            }, (obj) => ParserTable.Items.Any() && !BaseModel.IsWorking && !ParserCheckInfo.IsParser && ParserCheckConfig.CheckedConfig.ServiceOne > 1);
        //order
        public ICommand AddQueueCommand =>
            new RelayCommand((obj) =>
            {
                var parseredItem = obj as DataParser;

                if (!ParserQueue.CheckConditions(ParserQueue.Items, parseredItem))
                {
                    decimal price = parseredItem.Price2;
                    var currency = SteamBase.CurrencyList.FirstOrDefault(x => x.Id == SteamAccount.CurrencyId);
                    if (ParserTable.CurectCurrency.Id != 1)
                        price = Edit.ConverterToUsd(price, ParserTable.CurectCurrency.Value);
                    price = Edit.ConverterFromUsd(price, currency.Value);
                    ParserQueue.Items.Add(new()
                    {
                        ItemName = parseredItem.ItemName,
                        OrderPrice = price,
                        Precent = parseredItem.Precent
                    });
                    ParserQueue.OrderAmout = ParserQueue.Items.Select(x => x.OrderPrice).Sum();
                    Main.Message.Enqueue($"Success, added to Queue.\n{parseredItem.ItemName}");
                }

            }, (obj) => ParserQueue.Items.Select(x => x.OrderPrice).Sum() < SteamAccount.GetAvailableAmount() && ParserCheckConfig.CheckedConfig.ServiceOne < 2);
        public ICommand RemoveQueueCommand =>
            new RelayCommand((obj) =>
            {
                var item = obj as DataQueue;

                ParserQueue.Items.Remove(item);
                ParserQueue.OrderAmout = ParserQueue.Items.Select(x => x.OrderPrice).Sum();

            }, (obj) => ParserQueue.Items.Any() && ParserQueue.SelectedQueue != null);
        public ICommand ClearQueueCommand =>
            new RelayCommand((obj) =>
            {
                MessageBoxResult result = MessageBox.Show(
                    "Are you sure you want to clear the list?",
                    "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    ParserQueue.Items = new();
                    ParserQueue.OrderAmout = 0;
                }
            }, (obj) => ParserQueue.Items.Any());
        public ICommand PlaceOrderCommand =>
            new RelayCommand((obj) =>
            {
                BaseModel.IsWorking = true;
                Task.Run(() => {
                    int success = 0;
                    ParserQueue.MaxProgress = ParserQueue.Items.Count;
                    ParserQueue.CurrentProgress = 0;
                    foreach (var item in ParserQueue.Items)
                    {
                        try
                        {
                            ParserQueue.PlaceOrder(item.ItemName);
                            DataSavedList.Add(item.ItemName, "favorite", ParserCheckConfig.CheckedConfig.ServiceTwo);
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
                    ParserQueue.OrderAmout = 0;
                    BaseModel.IsWorking = false;
                });
            }, (obj) => ParserQueue.Items.Any() & !BaseModel.IsWorking);
        //favorite
        public ICommand AddFavoriteCommand =>
            new RelayCommand((obj) =>
            {
                var itemName = (string)obj;
                if (DataSavedList.Add(itemName, "favorite", ParserCheckConfig.CheckedConfig.ServiceTwo))
                    Main.Message.Enqueue($"Success, added to Favorites\n{itemName}");
                else
                    Main.Message.Enqueue("Oops, something went wrong.");
            }, (obj) => ParserCheckConfig.ServiceOne < 2);
        public ICommand RemoveFavoriteCommand =>
            new RelayCommand((obj) =>
            {
                string itemName = (string)obj;
                var item = DataSavedList.Items.FirstOrDefault(x => x.ItemName == itemName);
                if (item != null)
                {
                    DataSavedList.Items.Remove(item);
                    Main.Message.Enqueue($"{itemName}\nRemoved from list.");
                    DataSavedList.Save();
                }
            }, (obj) => DataSavedList.Items.Any());        
    }
}