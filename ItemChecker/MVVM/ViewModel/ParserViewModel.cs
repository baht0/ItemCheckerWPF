using ItemChecker.Core;
using ItemChecker.MVVM.Model;
using ItemChecker.Properties;
using ItemChecker.Services;
using ItemChecker.Support;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
        #region field and property
        Timer TimerView = new(500);
        private ObservableCollection<DataParser> _parserGrid = new();
        private ICollectionView _parserGridView;
        private DataParser _selectedParserItem;
        private ParserStatistics _parserStatistics = new();
        private ParserFilter _filterConfig = new();
        private string _searchString;

        //panel
        private ParserConfig _parserConfig = new();
        private ParserInfo _parserInfo = new();
        private int _manualCount = 0;
        private QueueInfo _queueInfo = new();

        public ObservableCollection<DataParser> ParserGrid
        {
            get
            {
                return _parserGrid;
            }
            set
            {
                _parserGrid = value;
                OnPropertyChanged();
            }
        }
        public ICollectionView ParserGridView
        {
            get
            {
                return _parserGridView;
            }
            set
            {
                _parserGridView = value;
                OnPropertyChanged();
            }
        }
        public DataParser SelectedParserItem
        {
            get
            {
                return _selectedParserItem;
            }
            set
            {
                _selectedParserItem = value;
                OnPropertyChanged();
                if (value == null)
                    return;
                if (ParserInfo.ST)
                {
                    ParserInfo.ItemSt = new();
                    DataSteamMarket data = DataSteamMarket.MarketItems.Where(x => x.ItemName == value.ItemName).FirstOrDefault();
                    Task.Run(() => {
                        if (data != null && !data.PriceHistory.Any())
                        {
                            ParserCheckService checkService = new();
                            var history = checkService.PriceHistory(value.ItemName);
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
                    List<DataInventoriesCsm> datas = DataInventoriesCsm.Items.Where(x => x.ItemName == value.ItemName).Reverse().ToList();
                    ParserInfo.InventoryCsm = datas;
                    ParserInfo.ItemCsmComparePrice = value.Price2;
                    if (ParserStatistics.DataCurrency == "RUB")
                        ParserInfo.ItemCsmComparePrice = Edit.ConverterToUsd(value.Price1, SettingsProperties.Default.RUB);
                    ParserInfo.InfoItemCount = ParserInfo.InventoryCsm.Count - 1;
                    ParserInfo.InfoItemCurrent = 0;
                }
                else if (ParserInfo.LF)
                {
                    ParserInfo.ItemLf = ItemBase.SkinsBase.FirstOrDefault(x => x.ItemName == value.ItemName).LfmInfo;
                    ParserInfo.ItemNameLfm = value.ItemName;
                }
                else if (ParserInfo.BF)
                {
                    ParserInfo.ItemBf = ItemBase.SkinsBase.FirstOrDefault(x => x.ItemName == value.ItemName).BuffInfo;
                    ParserInfo.ItemNameBf = value.ItemName;
                }
            }
        }
        public ParserStatistics ParserStatistics
        {
            get
            {
                return _parserStatistics;
            }
            set
            {
                _parserStatistics = value;
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
                OnPropertyChanged();
                FilterConfig = new ParserFilter();
                if (ParserGridView != null)
                    ParserGridView.Filter = item =>
                    {
                        return ((DataParser)item).ItemName.Contains(value, StringComparison.OrdinalIgnoreCase);
                    };
            }
        }

        //panel
        public ParserConfig ParserConfig
        {
            get
            {
                return _parserConfig;
            }
            set
            {
                _parserConfig = value;
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
        public int ManualCount
        {
            get { return _manualCount; }
            set
            {
                _manualCount = value;
                OnPropertyChanged();
            }
        }
        public QueueInfo QueueInfo
        {
            get { return _queueInfo; }
            set
            {
                _queueInfo = value;
                OnPropertyChanged();
            }
        }
        #endregion 

        public ParserViewModel()
        {
            ManualCount = ParserConfig.CheckList.Count;

            TimerView.Elapsed += UpdateView;
            TimerView.Enabled = true;
        }
        void UpdateView(Object sender, ElapsedEventArgs e)
        {
            try
            {
                decimal availableAmount = SteamAccount.GetAvailableAmount();
                decimal total = SteamAccount.Balance * 10;
                QueueInfo.TotalAllowed = total;
                QueueInfo.AvailableAmount = availableAmount;
                QueueInfo.Remaining = availableAmount - QueueInfo.OrderAmout;
                QueueInfo.AvailableAmountPrecent = Math.Round(availableAmount / total * 100, 1);
            }
            catch (Exception ex)
            {
                BaseService.errorLog(ex);
            }
        }
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

                ParserGridView.Filter = null;
                ParserGridView.Filter = item =>
                {
                    return ParserService.ApplyFilter(filterConfig, (DataParser)item);
                };
                ParserFilter.FilterConfig = filterConfig;
            }, (obj) => ParserGridView != null);
        public ICommand ResetCommand =>
            new RelayCommand((obj) =>
            {
                FilterConfig = new();
                ParserGridView.Filter = null;
                ParserFilter.FilterConfig = new();
            }, (obj) => ParserGridView != null);
        //check
        public ICommand AddListCommand =>
            new RelayCommand((obj) =>
            {
                Task.Run((Action)(() =>
                {
                    List<string> response = new();
                    switch (Convert.ToInt32(obj))
                    {
                        case 0:
                            response = ItemBase.SkinsBase.Select(x => x.ItemName).ToList();
                            break;
                        case 1:
                            response = BaseService.OpenFileDialog("txt");
                            break;
                        case 2:
                            response = ItemBase.SkinsBase.Where(x => x.LfmInfo.Price != 0).Select(x => x.ItemName).ToList();
                            break;
                        case 3:
                            response = HomeFavorite.FavoriteList.ToList();
                            break;
                        case 4:
                            response = DataOrder.Orders.Select(x => x.ItemName).ToList();
                            break;
                    }
                    if (response.Any())
                    {
                        ParserConfig.CheckList = response;
                        ManualCount = ParserConfig.CheckList.Count;
                        ParserService.SaveList("CheckList", response);
                    }
                    else
                        Main.Message.Enqueue((object)"It is not possible to add an empty list.");
                }));
            }, (obj) => !BaseModel.IsParsing);
        public ICommand CheckCommand =>
            new RelayCommand((obj) =>
            {
                BaseModel.IsParsing = true;
                ParserConfig.cts = new();
                ParserConfig.token = ParserConfig.cts.Token;
                Task.Run(() => {
                    PreparationCheck((ParserConfig)obj);
                });
                SaveConfig((ParserConfig)obj);
            }, (obj) => !BaseModel.IsParsing & !ParserConfig.Timer.Enabled & ParserConfig.ServiceOne != ParserConfig.ServiceTwo & /*ParserProperties.Default.CheckList.Any()*/ ParserConfig.CheckList.Any());
        public ICommand StopCommand =>
            new RelayCommand((obj) =>
            {
                ParserConfig.cts.Cancel();
                ParserStatistics.CurrentProgress = ParserStatistics.MaxProgress;
            }, (obj) => BaseModel.IsParsing);
        void PreparationCheck(ParserConfig parserConfig)
        {
            try
            {
                ParserStatistics.TimerOn = true;
                ParserStatistics.Timer = "Preparation...";
                DataGrid("CheckList", parserConfig.ServiceOne, parserConfig.ServiceTwo);
                DataSteamMarket.MarketItems.Clear();

                ParserStatistics.Timer = "Create List...";
                List<string> checkList = ParserCheckService.ApplyConfig(parserConfig);
                ParserStatistics.CountList = checkList.Count;
                ParserStatistics.CurrentProgress = 0;
                ParserStatistics.MaxProgress = checkList.Count;
                if (checkList.Any())
                    StartCheck(checkList);
                else
                    Application.Current.Dispatcher.Invoke(() => { MessageBox.Show("Nothing found. Adjust the parameters.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning); });

                ParserGridView = CollectionViewSource.GetDefaultView(ParserGrid);
            }
            catch (Exception exp)
            {
                ParserConfig.cts.Cancel();
                BaseService.errorLog(exp);
                BaseService.errorMessage(exp);
            }
            finally
            {
                ParserStatistics.TimerOn = false;
                BaseModel.IsParsing = false;
            }
        }
        void DataGrid(string mode, int serviceOne, int serviceTwo)
        {
            switch (SettingsProperties.Default.CurrencyId)
            {
                case 0:
                    ParserStatistics.DataCurrency = "USD";
                    break;
                case 1:
                    ParserStatistics.DataCurrency = "RUB";
                    break;
            }
            ParserStatistics.Currency = ParserStatistics.DataCurrency;
            if (ParserGrid.Any())
            {
                FilterConfig = new();
                ParserFilter.FilterConfig = new();

                ParserGrid = new();
                ParserGridView = CollectionViewSource.GetDefaultView(ParserGrid);
            }
            ParserStatistics.Mode = mode;
            ItemBaseService baseService = new();
            switch (serviceOne)
            {
                case 0:
                    ParserStatistics.Service1 = "SteamMarket(A)";
                    ParserStatistics.Price1 = "Sale(ST)";
                    ParserStatistics.Price2 = "BuyOrder";
                    ParserInfo.ST = true;
                    ParserInfo.CSM = false;
                    ParserInfo.LF = false;
                    ParserInfo.BF = false;
                    break;
                case 1:
                    ParserStatistics.Service1 = "SteamMarket";
                    ParserStatistics.Price1 = "Sale(ST)";
                    ParserStatistics.Price2 = "BuyOrder";
                    ParserInfo.ST = false;
                    ParserInfo.CSM = false;
                    ParserInfo.LF = false;
                    ParserInfo.BF = false;
                    break;
                case 2:
                    ParserStatistics.Service1 = "Cs.Money";
                    ParserStatistics.Price1 = "Trade(CSM)";
                    ParserStatistics.Price2 = "Give(CSM)";
                    ParserInfo.ST = false;
                    ParserInfo.CSM = true;
                    ParserInfo.LF = false;
                    ParserInfo.BF = false;
                    baseService.LoadInventoriesCsm(ParserConfig);
                    break;
                case 3:
                    ParserStatistics.Service1 = "Loot.Farm";
                    ParserStatistics.Price1 = "Trade(LF)";
                    ParserStatistics.Price2 = "Give(LF)";
                    ParserInfo.ST = false;
                    ParserInfo.CSM = false;
                    ParserInfo.LF = true;
                    ParserInfo.BF = false;
                    baseService.UpdateLfmInfo();
                    break;
                case 4:
                    ParserStatistics.Service1 = "Buf163";
                    ParserStatistics.Price1 = "Sale(BF)";
                    ParserStatistics.Price2 = "BuyOrder";
                    ParserInfo.ST = false;
                    ParserInfo.CSM = false;
                    ParserInfo.LF = false;
                    ParserInfo.BF = true;
                    baseService.UpdateBuffInfo(ParserConfig.MinPrice, ParserConfig.MaxPrice);
                    break;
            }
            switch (serviceTwo)
            {
                case 0:
                    ParserStatistics.Service2 = "SteamMarket(A)";
                    ParserStatistics.Price3 = "Sale(ST)";
                    ParserStatistics.Price4 = "BuyOrder";
                    break;
                case 1:
                    ParserStatistics.Service2 = "SteamMarket";
                    ParserStatistics.Price3 = "Sale(ST)";
                    ParserStatistics.Price4 = "BuyOrder";
                    break;
                case 2:
                    ParserStatistics.Service2 = "Cs.Money";
                    ParserStatistics.Price3 = "Trade(CSM)";
                    ParserStatistics.Price4 = "Give(CSM)";
                    break;
                case 3:
                    ParserStatistics.Service2 = "Loot.Farm";
                    ParserStatistics.Price3 = "Trade(LF)";
                    ParserStatistics.Price4 = "Give(LF)";
                    baseService.UpdateLfmInfo();
                    break;
                case 4:
                    ParserStatistics.Service2 = "Buf163";
                    ParserStatistics.Price3 = "Sale(BF)";
                    ParserStatistics.Price4 = "BuyOrder";
                    baseService.UpdateBuffInfo(ParserConfig.MinPrice, ParserConfig.MaxPrice);
                    break;
            }
        }
        void SaveConfig(ParserConfig parser)
        {
            ParserProperties.Default.ServiceOne = parser.ServiceOne;
            ParserProperties.Default.ServiceTwo = parser.ServiceTwo;

            ParserProperties.Default.MinPrice = parser.MinPrice;
            ParserProperties.Default.MaxPrice = parser.MaxPrice;

            ParserProperties.Default.Normal = parser.Normal;
            ParserProperties.Default.Souvenir = parser.Souvenir;
            ParserProperties.Default.Stattrak = parser.Stattrak;
            ParserProperties.Default.KnifeGlove = parser.KnifeGlove;
            ParserProperties.Default.KnifeGloveStattrak = parser.KnifeGloveStattrak;

            ParserProperties.Default.Overstock = parser.Overstock;
            ParserProperties.Default.Ordered = parser.Ordered;
            ParserProperties.Default.UserItems = parser.UserItems;
            ParserProperties.Default.OnlyDopplers = parser.OnlyDopplers;

            ParserProperties.Default.Save();
        }
        void StartCheck(List<string> checkList)
        {
            if (ParserConfig.token.IsCancellationRequested)
                return;

            DateTime now = DateTime.Now;
            ParserConfig.Timer.Elapsed += (sender, e) => timerTick(checkList.Count, now);
            ParserConfig.Timer.Enabled = true;

            ParserCheckService checkService = new();
            List<DataParser> checkedList = new();
            foreach (string itemName in checkList)
            {
                try
                {
                    DataParser data = checkService.Check(itemName, ParserProperties.Default.ServiceOne, ParserProperties.Default.ServiceTwo);
                    checkedList.Add(data);
                }
                catch (Exception exp)
                {
                    ParserConfig.cts.Cancel();
                    if (!exp.Message.Contains("429"))
                    {
                        BaseService.errorLog(exp);
                        BaseService.errorMessage(exp);
                    }
                    else
                        Application.Current.Dispatcher.Invoke(() => { MessageBox.Show(exp.Message, "Parser stoped!", MessageBoxButton.OK, MessageBoxImage.Warning); });
                }
                finally
                {
                    ParserStatistics.CurrentProgress++;
                }
                if (ParserConfig.token.IsCancellationRequested)
                    break;
            }
            ParserGrid = new ObservableCollection<DataParser>(checkedList);
            ParserConfig.Timer.Enabled = false;
            ParserConfig.Timer.Elapsed -= (sender, e) => timerTick(checkList.Count, now);
        }
        void timerTick(int itemCount, DateTime timeStart)
        {
            if (ParserConfig.token.IsCancellationRequested)
                ParserConfig.Timer.Enabled = false;
            ParserStatistics.Timer = Edit.calcTimeLeft(timeStart, itemCount, ParserStatistics.CurrentProgress);
        }

        public ICommand UpdateInventoryCommand =>
            new RelayCommand((obj) =>
            {
                BaseModel.IsWorking = true;
                ItemBaseService baseService = new();
                Task.Run(() => {
                    if (ParserStatistics.Service1 == "Cs.Money")
                        baseService.LoadInventoriesCsm(ParserConfig);
                    else if (ParserStatistics.Service1 == "Loot.Farm")
                        baseService.UpdateLfmInfo();
                    BaseModel.IsWorking = false;
                    Main.Message.Enqueue((object)"Service inventory update completed.");
                });
            }, (obj) => ParserGrid.Any() & !BaseModel.IsWorking & !BaseModel.IsParsing);
        //order
        public ICommand AddQueueCommand =>
            new RelayCommand((obj) =>
            {
                DataParser item = obj as DataParser;
                int count = QueueInfo.Items.Count;

                QueueInfo.Items = Queue.AddQueue(QueueInfo.Items, item);
                QueueInfo.OrderAmout = QueueInfo.Items.Select(x => x.OrderPrice).Sum();

                if (count < QueueInfo.Items.Count)
                    Main.Message.Enqueue($"Success, added to Queue.\n{item.ItemName}");

            }, (obj) => QueueInfo.Items.Select(x => x.OrderPrice).Sum() < SteamAccount.GetAvailableAmount() && ParserConfig.ServiceOne == 0);
        public ICommand RemoveQueueCommand =>
            new RelayCommand((obj) =>
            {
                QueueData item = obj as QueueData;

                QueueInfo.Items.Remove(item);
                QueueInfo.OrderAmout = QueueInfo.Items.Select(x => x.OrderPrice).Sum();

            }, (obj) => QueueInfo.Items.Any() & QueueInfo.SelectedQueue != null);
        public ICommand ClearQueueCommand =>
            new RelayCommand((obj) =>
            {
                MessageBoxResult result = MessageBox.Show(
                    "Are you sure you want to clear the list?",
                    "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    QueueInfo.Items = new();
                    QueueInfo.OrderAmout = 0;
                }
            }, (obj) => QueueInfo.Items.Any());
        public ICommand PlaceOrderCommand =>
            new RelayCommand((obj) =>
            {
                BaseModel.IsWorking = true;
                Task.Run(() => {
                    QueueInfo.MaxProgress = QueueInfo.Items.Count;
                    QueueInfo.CurrentProgress = 0;
                    foreach (var item in QueueInfo.Items)
                    {
                        try
                        {
                            Queue.PlaceOrder(item.ItemName);
                            if (!HomeFavorite.FavoriteList.Contains(item.ItemName) && HomeFavorite.FavoriteList.Count < 200)
                                HomeFavorite.FavoriteList.Add(item.ItemName);
                        }
                        catch (Exception exp)
                        {
                            BaseService.errorLog(exp);
                            continue;
                        }
                        finally
                        {
                            QueueInfo.CurrentProgress++;
                        }
                    }
                    QueueInfo.Items = new();
                    QueueInfo.OrderAmout = 0;
                    BaseModel.IsWorking = false;
                });
            }, (obj) => QueueInfo.Items.Any() & !BaseModel.IsWorking);
        //file
        public ICommand ExportCsvCommand =>
            new RelayCommand((obj) =>
            {
                Task.Run((Action)(() => {
                    ParserService list = new();
                    list.ExportCsv(ParserGrid, ParserGridView, ParserStatistics.Mode);
                    Main.Message.Enqueue((object)"Export to CSV file done.");
                }));
            }, (obj) => ParserGrid.Any() & !BaseModel.IsParsing);
        public ICommand ImportCsvCommand =>
            new RelayCommand((obj) =>
            {
                Task.Run((Action)(() =>
                {
                    BaseModel.IsParsing = true;
                    ParserService list = new();
                    ItemBaseService baseService = new();
                    ObservableCollection<DataParser> response = list.ImportCsv();
                    if (response.Any())
                    {
                        DataGrid("Import", ParserProperties.Default.ServiceOne, ParserProperties.Default.ServiceTwo);

                        ParserGrid = response;
                        ParserGridView = CollectionViewSource.GetDefaultView(ParserGrid);

                        if (ParserProperties.Default.ServiceOne > 1)
                        {
                            baseService.LoadInventoriesCsm(ParserConfig);
                            baseService.UpdateLfmInfo();
                        }
                        ParserStatistics.Currency = ParserStatistics.DataCurrency;
                        Main.Message.Enqueue((object)"Import from CSV file done.");
                    }
                    BaseModel.IsParsing = false;
                }));
            }, (obj) => !BaseModel.IsParsing);
        //favorite
        public ICommand AddFavoriteCommand =>
            new RelayCommand((obj) =>
            {
                string itemName = (string)obj;
                if (!HomeFavorite.FavoriteList.Contains(itemName) && HomeFavorite.FavoriteList.Count < 200 && !String.IsNullOrEmpty(itemName))
                {
                    HomeFavorite.FavoriteList.Add(itemName);
                    Main.Message.Enqueue($"Success, added to Favorites\n{itemName}");
                    BaseService.SaveList("FavoriteList", HomeFavorite.FavoriteList.ToList());
                }
                else if (HomeFavorite.FavoriteList.Contains(itemName))
                    Main.Message.Enqueue("The item is already on the list.");
                else if (HomeFavorite.FavoriteList.Count >= 200)
                    Main.Message.Enqueue("Limit. The maximum is only 200!");
                else if (String.IsNullOrEmpty(itemName))
                    Main.Message.Enqueue("Oops, something went wrong.");
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
            }, (obj) => HomeFavorite.FavoriteList.Any());        
    }
}