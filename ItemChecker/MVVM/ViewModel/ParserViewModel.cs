using ItemChecker.Core;
using ItemChecker.MVVM.Model;
using ItemChecker.Net;
using ItemChecker.Properties;
using ItemChecker.Services;
using ItemChecker.Support;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace ItemChecker.MVVM.ViewModel
{
    public class ParserViewModel : MainViewModel
    {
        private ObservableCollection<DataParser> _parserGrid = new();
        private ICollectionView _parserGridView;
        private DataParser _selectedParserItem;
        private ParserStatistics _parserStatistics = new();
        private ParserFilter _filterConfig = new();
        private string _searchString;

        //panel
        private ParserConfig _parserConfig = new();
        private ParserInfo _parserInfo = new();
        private int _manualCount;
        private ParserQueue _parserQueue = new();

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
                        if (!data.PriceHistory.Any())
                        {
                            var history = PriceHistory(value.ItemName);
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
                    ParserInfo.InventoryCsm = DataInventoryCsm.Inventory.Where(x => x.ItemName == value.ItemName).Reverse().ToList();
                    ParserInfo.InfoItemCount = ParserInfo.InventoryCsm.Count-1;
                    ParserInfo.InfoItemCurrent = 0;
                }
                else if (ParserInfo.LF)
                    ParserInfo.ItemLf = DataInventoryLf.Inventory.Where(x => x.ItemName == value.ItemName).FirstOrDefault();
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
        public ParserQueue ParserQueue
        {
            get { return _parserQueue; }
            set
            {
                _parserQueue = value;
                OnPropertyChanged();
            }
        }

        public ParserViewModel()
        {
            ManualCount = ParserProperties.Default.CheckList != null ? ParserProperties.Default.CheckList.Count : 0;
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
                Task.Run(() =>
                {
                    int i = Convert.ToInt32(obj);
                    List<string> response = new();
                    switch (i)
                    {
                        case -1:
                            if (ParserProperties.Default.CheckList != null)
                                ParserProperties.Default.CheckList.Clear();
                            break;
                        case 0:
                            ParserCheckService file = new();
                            response = file.SelectFile();
                            break;
                        case 1:
                            response = ItemBase.SkinsBase.Select(x => x.ItemName).ToList();
                            break;
                        case 2:
                            ItemBaseService.LoadBotsInventoryLf();
                            response = DataInventoryLf.Inventory.Select(x => x.ItemName).ToList();
                            break;
                        case 3:
                            if (HomeProperties.Default.FavoriteList != null)
                                response = HomeProperties.Default.FavoriteList.ToList();
                            break;
                        case 4:
                            response = DataOrder.Orders.Select(x => x.ItemName).ToList();
                            break;
                    }
                    if (response.Any())
                    {
                        ParserProperties.Default.CheckList = response;
                        ParserProperties.Default.Save();
                    }
                    ManualCount = ParserProperties.Default.CheckList.Count;
                });
            }, (obj) => !BaseModel.IsParsing);
        public ICommand StopCommand =>
            new RelayCommand((obj) =>
            {
                ParserConfig.cts.Cancel();
                ParserStatistics.CurrentProgress = ParserStatistics.MaxProgress;
            }, (obj) => BaseModel.IsParsing);
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
            }, (obj) => !BaseModel.IsParsing & !ParserConfig.Timer.Enabled & ParserConfig.ServiceOne != ParserConfig.ServiceTwo & ParserProperties.Default.CheckList != null);
        public ICommand UpdateInventoryCommand =>
            new RelayCommand((obj) =>
            {
                BaseModel.IsWorking = true;
                Task.Run(() => {
                    if (ParserStatistics.Service1 == "Cs.Money")
                        ItemBaseService.LoadBotsInventoryCsm();
                    else if (ParserStatistics.Service1 == "Loot.Farm")
                        ItemBaseService.LoadBotsInventoryLf();
                    BaseModel.IsWorking = false;
                });
            }, (obj) => ParserGrid.Any() & !BaseModel.IsWorking & !BaseModel.IsParsing);
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
                    MessageBox.Show("Nothing found. Adjust the parameters.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);

                ParserGridView = CollectionViewSource.GetDefaultView(ParserGrid);
                ParserStatistics.Currency = ParserStatistics.DataCurrency;
            }
            catch (Exception exp)
            {
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
            if (ParserGrid.Any())
            {
                FilterConfig = new();
                ParserFilter.FilterConfig = new();

                ParserGrid = new();
                ParserGridView = CollectionViewSource.GetDefaultView(ParserGrid);
            }
            ParserStatistics.Mode = mode;
            switch (serviceOne)
            {
                case 0:
                    ParserStatistics.Service1 = "SteamMarket";
                    ParserStatistics.Price1 = "Sale(ST)";
                    ParserStatistics.Price2 = "BuyOrder";
                    ParserInfo.ST = false;
                    ParserInfo.CSM = false;
                    ParserInfo.LF = false;
                    break;
                case 1:
                    ParserStatistics.Service1 = "SteamMarket(A)";
                    ParserStatistics.Price1 = "Sale(ST)";
                    ParserStatistics.Price2 = "BuyOrder";
                    ParserInfo.ST = true;
                    ParserInfo.CSM = false;
                    ParserInfo.LF = false;
                    break;
                case 2:
                    ParserStatistics.Service1 = "Cs.Money";
                    ParserStatistics.Price1 = "Trade(CSM)";
                    ParserStatistics.Price2 = "Give(CSM)";
                    ParserInfo.ST = false;
                    ParserInfo.CSM = true;
                    ParserInfo.LF = false;
                    ItemBaseService.LoadBotsInventoryCsm();
                    break;
                case 3:
                    ParserStatistics.Service1 = "Loot.Farm";
                    ParserStatistics.Price1 = "Trade(LF)";
                    ParserStatistics.Price2 = "Give(LF)";
                    ParserInfo.ST = false;
                    ParserInfo.CSM = false;
                    ParserInfo.LF = true;
                    ItemBaseService.LoadBotsInventoryLf();
                    break;
            }
            switch (serviceTwo)
            {
                case 0:
                    ParserStatistics.Service2 = "SteamMarket";
                    ParserStatistics.Price3 = "Sale(ST)";
                    ParserStatistics.Price4 = "BuyOrder";
                    break;
                case 1:
                    ParserStatistics.Service2 = "SteamMarket(A)";
                    ParserStatistics.Price3 = "Sale(ST)";
                    ParserStatistics.Price4 = "BuyOrder";
                    break;
                case 2:
                    ParserStatistics.Service2 = "Cs.Money";
                    ParserStatistics.Price3 = "Trade(CSM)";
                    ParserStatistics.Price4 = "Give(CSM)";
                    ItemBaseService.LoadBotsInventoryCsm();
                    break;
                case 3:
                    ParserStatistics.Service2 = "Loot.Farm";
                    ParserStatistics.Price3 = "Trade(LF)";
                    ParserStatistics.Price4 = "Give(LF)";
                    ItemBaseService.LoadBotsInventoryLf();
                    break;
            }
        }
        void SaveConfig(ParserConfig parser)
        {
            ParserProperties.Default.ServiceOne = parser.ServiceOne;
            ParserProperties.Default.ServiceTwo = parser.ServiceTwo;

            ParserProperties.Default.Souvenir = parser.Souvenir;
            ParserProperties.Default.Stattrak = parser.Stattrak;
            ParserProperties.Default.KnifeGlove = parser.KnifeGlove;
            ParserProperties.Default.KnifeGloveStattrak = parser.KnifeGloveStattrak;

            ParserProperties.Default.Overstock = parser.Overstock;
            ParserProperties.Default.Ordered = parser.Ordered;
            ParserProperties.Default.Dopplers = parser.Dopplers;
            ParserProperties.Default.OnlyDopplers = parser.OnlyDopplers;

            ParserProperties.Default.Save();
        }
        void StartCheck(List<string> checkList)
        {
            if (ParserConfig.token.IsCancellationRequested)
                return;
            TimeLeftAsync(checkList.Count, DateTime.Now);
            List<DataParser> checkedList = new();
            foreach (string itemName in checkList)
            {
                try
                {
                    ParserCheckService checkService = new();
                    DataParser data = checkService.Check(itemName);
                    checkedList.Add(data);
                }
                catch (WebException exp)
                {
                    var response = exp.Response as HttpWebResponse;
                    if (response.StatusCode == HttpStatusCode.TooManyRequests)
                        MessageBox.Show("Warning", "(429) Too Many Requests", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                finally
                {
                    ParserStatistics.CurrentProgress++;
                }
                if (ParserConfig.token.IsCancellationRequested)
                    break;
            }
            ParserGrid = new ObservableCollection<DataParser>(checkedList);
        }
        void TimeLeftAsync(int itemCount, DateTime timeStart)
        {
            Task.Run(() => {
                while (ParserStatistics.CurrentProgress <= itemCount)
                {
                    if (ParserConfig.token.IsCancellationRequested)
                        break;
                    ParserStatistics.Timer = Edit.calcTimeLeft(timeStart, itemCount, ParserStatistics.CurrentProgress);
                    Thread.Sleep(500);
                }
            });
        }

        //order
        public ICommand AddQueueCommand =>
            new RelayCommand((obj) =>
            {
                DataParser item = obj as DataParser;
                int count = ParserQueue.Items.Count;

                ParserQueue.Items = ParserQueue.AddQueue(ParserQueue.Items, item.ItemName, item.Price2);
                ParserQueue.OrderAmout = ParserQueue.Items.Select(x => x.OrderPrice).Sum();

                if (count < ParserQueue.Items.Count)
                    Message.Enqueue("Success. Item added to Queue");

            }, (obj) => ParserQueue.Items.Select(x => x.OrderPrice).Sum() < SteamAccount.GetAvailableAmount() & ParserConfig.ServiceOne == 1);
        public ICommand RemoveQueueCommand =>
            new RelayCommand((obj) =>
            {
                DataQueue item = obj as DataQueue;

                ParserQueue.Items.Remove(item);
                ParserQueue.OrderAmout = ParserQueue.Items.Select(x => x.OrderPrice).Sum();

            }, (obj) => ParserQueue.Items.Any() & ParserQueue.SelectedQueue != null);
        public ICommand ClearQueueCommand =>
            new RelayCommand((obj) =>
            {
                ParserQueue.Items = new();
                ParserQueue.OrderAmout = 0;
            }, (obj) => ParserQueue.Items.Any());
        public ICommand PlaceOrderCommand =>
            new RelayCommand((obj) =>
            {
                BaseModel.IsWorking = true;
                Task.Run(() => {
                    ParserQueue.MaxProgress = ParserQueue.Items.Count;
                    ParserQueue.CurrentProgress = 0;
                    foreach (var item in ParserQueue.Items)
                    {
                        try
                        {
                            ParserQueue.PlaceOrder(item.ItemName);
                        }
                        catch (Exception exp)
                        {
                            BaseService.errorLog(exp);
                            continue;
                        }
                        finally
                        {
                            ParserQueue.CurrentProgress++;
                        }
                    }
                    ParserQueue.Items = new();
                    ParserQueue.OrderAmout = 0;
                    MainInfo.AvailableAmount = SteamAccount.GetAvailableAmount();
                    BaseModel.IsWorking = false;
                });
            }, (obj) => ParserQueue.Items.Any() & !BaseModel.IsWorking);
        //file
        public ICommand ExportCsvCommand =>
            new RelayCommand((obj) =>
            {
                Task.Run(() =>
                {
                    ParserService list = new();
                    list.ExportCsv(ParserGrid, ParserGridView, ParserStatistics.Mode);
                });
            }, (obj) => ParserGrid.Any() & !BaseModel.IsParsing);
        public ICommand ImportCsvCommand =>
            new RelayCommand((obj) =>
            {
                Task.Run(() =>
                {
                    BaseModel.IsParsing = true;
                    ParserService list = new();
                    ObservableCollection<DataParser> response = list.ImportCsv();
                    if (response.Any())
                    {
                        DataGrid("Import", ParserProperties.Default.ServiceOne, ParserProperties.Default.ServiceTwo);

                        ParserGrid = response;
                        ParserGridView = CollectionViewSource.GetDefaultView(ParserGrid);

                        if (ParserProperties.Default.ServiceOne != 0)
                        {
                            ItemBaseService.LoadBotsInventoryCsm();
                            ItemBaseService.LoadBotsInventoryLf();
                        }
                        ParserStatistics.Currency = ParserStatistics.DataCurrency;
                    }
                    BaseModel.IsParsing = false;
                });
            }, (obj) => !BaseModel.IsParsing);
        public ICommand ExportTxtCommand =>
            new RelayCommand((obj) =>
            {
                Task.Run(() => {
                    ParserService list = new();
                    list.ExportTxt(ParserGridView, ParserStatistics.Price1, ParserStatistics.Mode);
                });
            }, (obj) => ParserGrid.Any() & !BaseModel.IsParsing);

        List<PriceHistory> PriceHistory(string itemName)
        {
            string json = Get.Request(SettingsProperties.Default.SteamCookies, "https://steamcommunity.com/market/pricehistory/?country=RU&currency=5&appid=730&market_hash_name=" + Edit.MarketHashName(itemName));
            JArray sales = JArray.Parse(JObject.Parse(json)["prices"].ToString());
            List<PriceHistory> history = new();
            foreach (var sale in sales.Reverse())
            {
                DateTime date = DateTime.Parse(sale[0].ToString()[..11]);
                decimal price = Decimal.Parse(sale[1].ToString());
                int count = Convert.ToInt32(sale[2]);

                history.Add(new PriceHistory()
                {
                    Date = date,
                    Price = price,
                    Count = count,
                });
            }
            return history;
        }
    }
}