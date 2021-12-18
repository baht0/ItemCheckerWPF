using ItemChecker.Core;
using ItemChecker.MVVM.Model;
using ItemChecker.Properties;
using ItemChecker.Services;
using ItemChecker.Support;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace ItemChecker.MVVM.ViewModel
{
    public class ParserViewModel : MainViewModel
    {
        //GridData
        private string _mode;
        private string _service1;
        private string _service2;
        private string _currency;
        private string _price1;
        private string _price2;
        private string _price3;
        private string _price4;
        private ObservableCollection<DataParser> _parserGrid;
        private DataParser _selectedParserItem;
        //filter
        private ICollectionView _parserGridView;
        private Filter _filterConfig;
        private string _searchString;
        //list
        private ParserList _parserListConfig;
        private int _manualCount;
        //info
        private int _infoItemCurrent;
        private int _infoItemCount;
        private List<DataInventoryCsm> _inventoryCsm;
        private DataInventoryCsm _itemCsm = new()
        {
            ItemName = "Unknown",
            StackSize = 0,
            DefaultPrice = 0,
            Price = 0,
            Sticker = false,
            NameTag = false,
            User = false,
            TradeLock = new(),
            RareItem = false
        };
        private bool _csm;
        private DataInventoryLf _itemLf = new()
        {
            ItemName = "Unknown",
            DefaultPrice = 0,
            Have = 0,
            Limit = 0,
            Reservable = 0,
            Tradable = 0,
            SteamPriceRate = 0
        };
        private bool _lf;
        private DataSteamMarket _itemSt = new()
        {
            ItemName = "Unknown",
            HighestBuyOrder = 0,
            LowestSellOrder = 0,
            BuyOrderGraph = new(),
            SellOrderGraph = new()
        };
        private bool _st;
        //queue
        private ObservableCollection<string> _placeOrderItems;
        private string _selectedQueue;
        private decimal _orderAmout;
        private decimal _orderAvailable;
        //progress
        private int _currentProgress;
        private int _maxProgress;
        private string _timer;
        private bool _timerOn;
        //GridData
        public string Mode
        {
            get
            {
                return _mode;
            }
            set
            {
                _mode = value;
                OnPropertyChanged();
            }
        }
        public string Service1
        {
            get
            {
                return _service1;
            }
            set
            {
                _service1 = value;
                OnPropertyChanged();
            }
        }
        public string Service2
        {
            get
            {
                return _service2;
            }
            set
            {
                _service2 = value;
                OnPropertyChanged();
            }
        }
        public string Currency
        {
            get
            {
                return _currency;
            }
            set
            {
                _currency = value;
                OnPropertyChanged();
            }
        }
        public string Price1
        {
            get
            {
                return _price1;
            }
            set
            {
                _price1 = value;
                OnPropertyChanged();
            }
        }
        public string Price2
        {
            get
            {
                return _price2;
            }
            set
            {
                _price2 = value;
                OnPropertyChanged();
            }
        }
        public string Price3
        {
            get
            {
                return _price3;
            }
            set
            {
                _price3 = value;
                OnPropertyChanged();
            }
        }
        public string Price4
        {
            get
            {
                return _price4;
            }
            set
            {
                _price4 = value;
                OnPropertyChanged();
            }
        }
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
                if (DataSteamMarket.MarketItems == null)
                    return;
                if (Service1 == "SteamMarket" & !BaseModel.IsLoading)
                {
                    ItemSt = DataSteamMarket.MarketItems.Where(x => x.ItemName == value.ItemName).First();
                }
                else if (Service1 == "Cs.Money" & !BaseModel.IsLoading)
                {
                    InventoryCsm = DataInventoryCsm.Inventory.Where(x => x.ItemName == value.ItemName).ToList();
                    InfoItemCount = InventoryCsm.Count - 1;
                    InfoItemCurrent = 0;
                }
                else if (Service1 == "Loot.Farm" & !BaseModel.IsLoading)
                {
                    ItemLf = DataInventoryLf.Inventory.Where(x => x.ItemName == value.ItemName).First();
                }
            }
        }
        //filter
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
        public Filter FilterConfig
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
                FilterConfig = new Filter();
                if (ParserGridView != null & !BaseModel.IsLoading)
                    ParserGridView.Filter = item =>
                    {
                        return ((DataParser)item).ItemName.Contains(value, StringComparison.OrdinalIgnoreCase);
                    };
            }
        }
        //list
        public ParserList ParserListConfig
        {
            get
            {
                return _parserListConfig;
            }
            set
            {
                _parserListConfig = value;
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
        //info
        public int InfoItemCurrent
        {
            get
            {
                return _infoItemCurrent;
            }
            set
            {
                _infoItemCurrent = value;
                OnPropertyChanged();
                ItemCsm = InventoryCsm[value];
            }
        }
        public int InfoItemCount
        {
            get
            {
                return _infoItemCount;
            }
            set
            {
                _infoItemCount = value;
                OnPropertyChanged();
            }
        }
        public List<DataInventoryCsm> InventoryCsm
        {
            get
            {
                return _inventoryCsm;
            }
            set
            {
                _inventoryCsm = value;
                OnPropertyChanged();
            }
        }
        public DataInventoryCsm ItemCsm
        {
            get
            {
                return _itemCsm;
            }
            set
            {
                _itemCsm = value;
                OnPropertyChanged();
            }
        }
        public bool CSM
        {
            get
            {
                return _csm;
            }
            set
            {
                _csm = value;
                OnPropertyChanged();
            }
        }
        public DataInventoryLf ItemLf
        {
            get
            {
                return _itemLf;
            }
            set
            {
                _itemLf = value;
                OnPropertyChanged();
            }
        }
        public bool LF
        {
            get
            {
                return _lf;
            }
            set
            {
                _lf = value;
                OnPropertyChanged();
            }
        }
        public DataSteamMarket ItemSt
        {
            get
            {
                return _itemSt;
            }
            set
            {
                _itemSt = value;
                OnPropertyChanged();
            }
        }
        public bool ST
        {
            get
            {
                return _st;
            }
            set
            {
                _st = value;
                OnPropertyChanged();
            }
        }
        //queue
        public ObservableCollection<string> PlaceOrderItems
        {
            get
            {
                return _placeOrderItems;
            }
            set
            {
                _placeOrderItems = value;
                OnPropertyChanged();
            }
        }
        public string SelectedQueue
        {
            get
            {
                return _selectedQueue;
            }
            set
            {
                _selectedQueue = value;
                OnPropertyChanged();
            }
        }
        public decimal OrderAmout
        {
            get
            {
                return _orderAmout;
            }
            set
            {
                _orderAmout = value;
                OnPropertyChanged();
            }
        }
        public decimal OrderAvailable
        {
            get
            {
                return _orderAvailable;
            }
            set
            {
                _orderAvailable = value;
                OnPropertyChanged();
            }
        }
        //progress
        public int CurrentProgress
        {
            get { return _currentProgress; }
            set
            {
                _currentProgress = value;
                OnPropertyChanged();
            }
        }
        public int MaxProgress
        {
            get { return _maxProgress; }
            set
            {
                _maxProgress = value;
                OnPropertyChanged();
            }
        }
        public string Timer
        {
            get { return _timer; }
            set
            {
                _timer = value;
                OnPropertyChanged();
            }
        }
        public bool TimerOn
        {
            get { return _timerOn; }
            set
            {
                _timerOn = value;
                OnPropertyChanged();
            }
        }

        public ParserViewModel()
        {
            Task.Run(() => { LoadView(); });
        }
        void LoadView()
        {
            Mode = ParserList.Mode;
            Service1 = ParserList.Service1;
            Service2 = ParserList.Service2;
            Currency = ParserList.DataCurrency;

            Price1 = ParserList.Price1;
            Price2 = ParserList.Price2;
            Price3 = ParserList.Price3;
            Price4 = ParserList.Price4;
            ST = Service1 == "SteamMarket";
            CSM = Service1 == "Cs.Money";
            LF = Service1 == "Loot.Farm";

            if (DataParser.ParserItems.Any())
                ParserGrid = DataParser.ParserItems;

            ParserGridView = CollectionViewSource.GetDefaultView(ParserGrid);
            FilterConfig = Filter.FilterConfig ?? new Filter();

            ParserListConfig = new ParserList()
            {
                ServiceOne = ParserProperties.Default.ServiceOne,
                ServiceTwo = ParserProperties.Default.ServiceTwo,

                MaxPrice = ParserProperties.Default.MaxPrice,
                MinPrice = ParserProperties.Default.MinPrice,
                MaxPrecent = ParserProperties.Default.MaxPrecent,
                MinPrecent = ParserProperties.Default.MinPrecent,
                SteamSales = ParserProperties.Default.SteamSales,
                NameContains = ParserProperties.Default.NameContains,
                KnifeTS = ParserProperties.Default.KnifeTS,
                StattrakTS = ParserProperties.Default.StattrakTS,
                SouvenirTS = ParserProperties.Default.SouvenirTS,
                StickerTS = ParserProperties.Default.StickerTS,

                SouvenirM = ParserProperties.Default.SouvenirM,
                StattrakM = ParserProperties.Default.StattrakTS,
                KnifeGloveM = ParserProperties.Default.KnifeGloveM,
                KnifeGloveStattrakM = ParserProperties.Default.KnifeGloveStattrakM,
                OverstockM = ParserProperties.Default.OverstockM,
            };
            ManualCount = ParserProperties.Default.CheckList != null ? ParserProperties.Default.CheckList.Count : 0;

            PlaceOrderItems = DataOrder.Queue;
            OrderAmout = DataOrder.AmountRub;
            OrderAvailable = DataOrder.AvailableAmount;
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
                Filter filterConfig = obj as Filter;

                ParserGridView.Filter = null;
                ParserGridView.Filter = item =>
                {
                    return ApplyFilter(filterConfig, (DataParser)item);
                };
                Filter.FilterConfig = filterConfig;
            }, (obj) => ParserGridView != null & !BaseModel.IsLoading);
        bool ApplyFilter(Filter filterConfig, DataParser item)
        {
            //category
            bool category = true;
            if (filterConfig.Normal | filterConfig.Stattrak | filterConfig.Souvenir | filterConfig.KnifeGlove | filterConfig.KnifeGloveStattrak)
            {
                category = false;
                if (filterConfig.Normal)
                    category = item.ItemType.Contains("Normal");
                if (filterConfig.Stattrak & !category)
                    category = item.ItemType.Contains("Souvenir");
                if (filterConfig.Souvenir & !category)
                    category = item.ItemType.Contains("Stattrak");
                if (filterConfig.KnifeGlove & !category)
                    category = item.ItemType.Contains("KnifeGlove");
                if (filterConfig.KnifeGloveStattrak & !category)
                    category = item.ItemType.Contains("KnifeGloveStattrak");
            }
            //status
            bool status = true;
            if (filterConfig.Tradable | filterConfig.Ordered | filterConfig.Overstock | filterConfig.Unavailable | filterConfig.Unknown)
            {
                status = false;
                if (filterConfig.Tradable)
                    status = item.Status.Contains("Tradable");
                if (filterConfig.Ordered & !status)
                    status = item.Status.Contains("Ordered");
                if (filterConfig.Overstock & !status)
                    status = item.Status.Contains("Overstock");
                if (filterConfig.Unavailable & !status)
                    status = item.Status.Contains("Unavailable");
                if (filterConfig.Unknown & !status)
                    status = item.Status.Contains("Unknown");
            }
            //exterior
            bool exterior = true;
            if (filterConfig.NotPainted | filterConfig.BattleScarred | filterConfig.WellWorn | filterConfig.FieldTested | filterConfig.MinimalWear | filterConfig.FactoryNew)
            {
                exterior = false;
                if (filterConfig.NotPainted)
                    exterior = !item.ItemName.Contains("Battle-Scarred") &
                        !item.ItemName.Contains("Well-Worn") &
                        !item.ItemName.Contains("Field-Tested") &
                        !item.ItemName.Contains("Minimal Wear") &
                        !item.ItemName.Contains("Factory New") &
                        item.ItemType.Contains("KnifeGlove");
                if (filterConfig.BattleScarred & !exterior)
                    exterior = item.ItemName.Contains("Battle-Scarred");
                if (filterConfig.WellWorn & !exterior)
                    exterior = item.ItemName.Contains("Well-Worn");
                if (filterConfig.FieldTested & !exterior)
                    exterior = item.ItemName.Contains("Field-Tested");
                if (filterConfig.MinimalWear & !exterior)
                    exterior = item.ItemName.Contains("Minimal Wear");
                if (filterConfig.FactoryNew & !exterior)
                    exterior = item.ItemName.Contains("Factory New");
            }
            //types
            bool types = true;
            if (filterConfig.Weapon | filterConfig.Knife | filterConfig.Gloves | filterConfig.Sticker | filterConfig.Patch | filterConfig.Pin | filterConfig.Key | filterConfig.Pass | filterConfig.MusicKit | filterConfig.Graffiti | filterConfig.Case | filterConfig.Package)
            {
                types = false;
                if (filterConfig.Weapon)
                    types = !item.ItemName.Contains("Sticker ") &
                        !item.ItemName.Contains("Patch ") &
                        !item.ItemName.Contains(" Pin") &
                        !item.ItemName.Contains(" Key") &
                        !item.ItemName.Contains(" Pass") &
                        !item.ItemName.Contains("Music Kit") &
                        !item.ItemName.Contains("Sealed Graffiti") &
                        !item.ItemName.Contains("Case") &
                        !item.ItemName.Contains(" Package");
                if (filterConfig.Knife & !types)
                    types = item.ItemName.Contains("Knife");
                if (filterConfig.Gloves & !types)
                    types = item.ItemName.Contains("Gloves |") | item.ItemName.Contains("★ Hand");
                if (filterConfig.Sticker & !types)
                    types = item.ItemName.Contains("Sticker ");
                if (filterConfig.Patch & !types)
                    types = item.ItemName.Contains("Patch ");
                if (filterConfig.Pin & !types)
                    types = item.ItemName.Contains(" Pin");
                if (filterConfig.Key & !types)
                    types = item.ItemName.Contains(" Key");
                if (filterConfig.Pass & !types)
                    types = item.ItemName.Contains(" Pass");
                if (filterConfig.MusicKit & !types)
                    types = item.ItemName.Contains("Music Kit");
                if (filterConfig.Graffiti & !types)
                    types = item.ItemName.Contains("Sealed Graffiti");
                if (filterConfig.Case & !types)
                    types = item.ItemName.Contains("Case");
                if (filterConfig.Package & !types)
                    types = item.ItemName.Contains(" Package");
            }
            //Prices
            bool prices = true;
            if (filterConfig.Price1 | filterConfig.Price2 | filterConfig.Price3 | filterConfig.Price4)
            {
                if (filterConfig.Price1)
                    prices = filterConfig.Price1From < item.Price1 & filterConfig.Price1To > item.Price1;
                if (filterConfig.Price2 & prices)
                    prices = filterConfig.Price2From < item.Price2 & filterConfig.Price2To > item.Price2;
                if (filterConfig.Price3 & prices)
                    prices = filterConfig.Price3From < item.Price3 & filterConfig.Price3To > item.Price3;
                if (filterConfig.Price4 & prices)
                    prices = filterConfig.Price4From < item.Price4 & filterConfig.Price4To > item.Price4;
            }
            //other
            bool other = true;
            if (filterConfig.PrecentFrom != 0 | filterConfig.PrecentTo != 0 | filterConfig.DifferenceFrom != 0 | filterConfig.DifferenceTo != 0 | filterConfig.Hide100 | filterConfig.Hide0 | filterConfig.Have)
            {
                if (filterConfig.PrecentFrom != 0)
                    other = filterConfig.PrecentFrom < item.Precent;
                if (filterConfig.PrecentTo != 0 & other)
                    other = filterConfig.PrecentTo > item.Precent;
                if (filterConfig.DifferenceFrom != 0 & other)
                    other = filterConfig.DifferenceFrom < item.Difference;
                if (filterConfig.DifferenceTo != 0 & other)
                    other = filterConfig.DifferenceTo > item.Difference;

                if (filterConfig.Hide100 & other)
                    other = item.Precent != -100;
                if (filterConfig.Hide0 & other)
                    other = item.Precent != 0;
                if (filterConfig.Have & other)
                    other = item.Have;
            }

            bool isShow = category & status & exterior & types & prices & other;
            return isShow;
        }
        public ICommand ResetCommand =>
            new RelayCommand((obj) =>
            {
                FilterConfig = new();
                ParserGridView.Filter = null;
                Filter.FilterConfig = new();
            }, (obj) => ParserGridView != null & !BaseModel.IsLoading);
        //manual
        public ICommand ManualListCommand =>
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
                            ParserManualService file = new();
                            response = file.SelectFile();
                            break;
                        case 1:
                            response = ItemBase.SkinsBase.Select(x => x.ItemName).ToList();
                            break;
                        case 2:
                            ItemBaseService.LoadBotsInventoryLf();
                            response = DataInventoryLf.Inventory.Select(x => x.ItemName).ToList();
                            break;
                    }
                    if (response.Any())
                    {
                        ParserProperties.Default.CheckList = response;
                        ParserProperties.Default.Save();
                    }
                    ManualCount = ParserProperties.Default.CheckList.Count;
                });
            }, (obj) => !BaseModel.IsLoading);
        //check
        public ICommand StopCommand =>
            new RelayCommand((obj) =>
            {
                BaseModel.cts.Cancel();
            }, (obj) => BaseModel.IsLoading);
        public ICommand TryskinsCommand =>
        new RelayCommand((obj) =>
            {
            Task.Run(() =>
            {
                ParserList parserConfig = (ParserList)obj;
                parserConfig.Manual = false;
                parserConfig.Tryskins = true;
                CheckStart(parserConfig);
            });
        }, (obj) => !BaseModel.IsLoading & !BaseModel.Timer.Enabled & ParserListConfig.ServiceOne != ParserListConfig.ServiceTwo);
        public ICommand ManualCommand =>
            new RelayCommand((obj) =>
            {
                Task.Run(() =>
                {
                    ParserList parserConfig = (ParserList)obj;
                    parserConfig.Manual = true;
                    parserConfig.Tryskins = false;
                    CheckStart(parserConfig);
                });
            }, (obj) => !BaseModel.IsLoading & !BaseModel.Timer.Enabled & ParserListConfig.ServiceOne != ParserListConfig.ServiceTwo & ParserProperties.Default.CheckList != null);
        public ICommand UpdateInventoryCommand =>
            new RelayCommand((obj) =>
            {
                Task.Run(() =>
                {
                    if (Service1 == "Cs.Money")
                        ItemBaseService.LoadBotsInventoryCsm();
                    else if (Service1 == "Loot.Farm")
                        ItemBaseService.LoadBotsInventoryLf();
                });
            }, (obj) => ParserGrid != null);
        void CheckStart(ParserList parserConfig)
        {
            try
            {
                BaseModel.IsLoading = true;
                BaseModel.cts = new();
                BaseModel.token = BaseModel.cts.Token;
                DataGrid(parserConfig.ServiceOne, parserConfig.ServiceTwo);
                SaveConfig(parserConfig);
                CurrentProgress = 0;
                TimerOn = true;

                if (parserConfig.Tryskins)
                    CheckTryskins();
                if (parserConfig.Manual)
                    CheckManual(parserConfig);

                if (DataParser.ParserItems.Any())
                    ParserGrid = DataParser.ParserItems;
                ParserGridView = CollectionViewSource.GetDefaultView(ParserGrid);
                Currency = ParserList.DataCurrency;
            }
            catch (Exception exp)
            {
                BaseModel.errorLog(exp);
                BaseModel.errorMessage(exp);
            }
            finally
            {
                TimerOn = false;
                BaseModel.IsLoading = false;
            }
        }
        void DataGrid(int serviceOne, int serviceTwo)
        {
            if (DataParser.ParserItems.Any())
            {
                FilterConfig = new();
                Filter.FilterConfig = new();

                DataParser.ParserItems = new();
                ParserGrid = new();
                ParserGridView = CollectionViewSource.GetDefaultView(ParserGrid);
            }
            switch (serviceOne)
            {
                case 0:
                    Service1 = "SteamMarket";
                    Price1 = "Sale(ST)";
                    Price2 = "BuyOrder";
                    ST = true;
                    CSM = false;
                    LF = false;
                    break;
                case 1:
                    Service1 = "Cs.Money";
                    Price1 = "Sale(CSM)";
                    Price2 = "Buy(CSM)";
                    ST = false;
                    CSM = true;
                    LF = false;
                    break;
                case 2:
                    Service1 = "Loot.Farm";
                    Price1 = "Sale(LF)";
                    Price2 = "Buy(LF)";
                    ST = false;
                    CSM = false;
                    LF = true;
                    break;
            }
            switch (serviceTwo)
            {
                case 0:
                    Service2 = "SteamMarket";
                    Price3 = "Sale(ST)";
                    Price4 = "BuyOrder";
                    break;
                case 1:
                    Service2 = "Cs.Money";
                    Price3 = "Sale(CSM)";
                    Price4 = "Buy(CSM)";
                    break;
                case 2:
                    Service2 = "Loot.Farm";
                    Price3 = "Sale(LF)";
                    Price4 = "Buy(LF)";
                    break;
            }

            ParserList.Mode = Mode;
            ParserList.Service1 = Service1;
            ParserList.Service2 = Service2;

            ParserList.Price1 = Price1;
            ParserList.Price2 = Price2;
            ParserList.Price3 = Price3;
            ParserList.Price4 = Price4;
        }
        void SaveConfig(ParserList parser)
        {
            ParserProperties.Default.ServiceOne = parser.ServiceOne;
            ParserProperties.Default.ServiceTwo = parser.ServiceTwo;

            ParserProperties.Default.MaxPrice = parser.MaxPrice;
            ParserProperties.Default.MinPrice = parser.MinPrice;
            ParserProperties.Default.MaxPrecent = parser.MaxPrecent;
            ParserProperties.Default.MinPrecent = parser.MinPrecent;
            ParserProperties.Default.SteamSales = parser.SteamSales;
            ParserProperties.Default.NameContains = parser.NameContains;
            ParserProperties.Default.KnifeTS = parser.KnifeTS;
            ParserProperties.Default.StattrakTS = parser.StattrakTS;
            ParserProperties.Default.SouvenirTS = parser.SouvenirTS;
            ParserProperties.Default.StickerTS = parser.StickerTS;
            ParserProperties.Default.SouvenirM = parser.SouvenirM;
            ParserProperties.Default.StattrakM = parser.StattrakM;
            ParserProperties.Default.KnifeGloveM = parser.KnifeGloveM;
            ParserProperties.Default.KnifeGloveStattrakM = parser.KnifeGloveStattrakM;
            ParserProperties.Default.OverstockM = parser.OverstockM;

            ParserProperties.Default.Save();
        }
        void CheckTryskins()
        {
            Mode = "TrySkins";
            Timer = "Get Items...";
            ParserTryskinsService tryskins = new();
            List<IWebElement> items = tryskins.GetItems();
            if (items.Any())
            {
                Timer = "Preparation...";
                ItemBaseService.LoadBotsInventoryCsm();
                ItemBaseService.LoadBotsInventoryLf();
                MaxProgress = items.Count;
                TimeLeftAsync(items.Count, DateTime.Now);
                foreach (IWebElement item in items)
                {
                    try
                    {
                        tryskins.Tryskins(item);
                    }
                    catch (Exception exp)
                    {
                        BaseModel.errorLog(exp);
                    }
                    finally
                    {
                        CurrentProgress++;
                    }
                    if (BaseModel.token.IsCancellationRequested)
                        break;
                }
            }
            else
                MessageBox.Show("Nothing found. Adjust the parameters.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        void CheckManual(ParserList config)
        {
            Mode = "Manual";
            Timer = "Create List...";
            ParserManualService manual = new();
            List<string> checkList = manual.ApplyConfig(config);
            if (checkList.Any())
            {
                Timer = "Preparation...";
                ItemBaseService.LoadBotsInventoryCsm();
                ItemBaseService.LoadBotsInventoryLf();
                MaxProgress = checkList.Count;
                TimeLeftAsync(checkList.Count, DateTime.Now);
                foreach (string itemName in checkList)
                {
                    try
                    {
                        manual.Manual(itemName);
                    }
                    catch (Exception exp)
                    {
                        BaseModel.errorLog(exp);
                    }
                    finally
                    {
                        CurrentProgress++;
                    }
                    if (BaseModel.token.IsCancellationRequested)
                        break;
                }
            }
            else
                MessageBox.Show("Nothing found. Adjust the parameters.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);            
        }
        void TimeLeftAsync(int itemCount, DateTime timeStart)
        {
            Task.Run(() => {
                while (CurrentProgress < itemCount)
                {
                    Timer = Edit.calcTimeLeft(timeStart, itemCount, CurrentProgress);
                    Thread.Sleep(500);
                }
            });
        }

        //order
        public ICommand AddQueueCommand =>
            new RelayCommand((obj) =>
            {
                DataParser item = obj as DataParser;
                PlaceOrderService.AddQueue(item.ItemName, item.Price2);
                OrderAmout = DataOrder.AmountRub;
            }, (obj) => DataOrder.AmountRub < DataOrder.AvailableAmount);
        public ICommand RemoveQueueCommand =>
            new RelayCommand((obj) =>
            {
                string item = obj as string;
                PlaceOrderService.RemoveQueue(item);
                OrderAmout = DataOrder.AmountRub;
            }, (obj) => DataOrder.Queue.Any() & !string.IsNullOrEmpty(SelectedQueue));
        public ICommand ClearQueueCommand =>
            new RelayCommand((obj) =>
            {
                ClearQueue();
            }, (obj) => DataOrder.Queue.Any());
        public ICommand PlaceOrderCommand =>
            new RelayCommand((obj) =>
            {
                BaseModel.IsLoading = true;
                Task.Run(() => {
                    PlaceOrder();
                });
            }, (obj) => !BaseModel.IsLoading & DataOrder.Queue.Any());
        void PlaceOrder()
        {
            PlaceOrderService placeOrder = new();
            MaxProgress = DataOrder.Queue.Count;
            CurrentProgress = 0;
            foreach (string itemName in DataOrder.Queue)
            {
                try
                {
                    placeOrder.PlaceOrder(itemName);
                }
                catch (Exception exp)
                {
                    BaseModel.errorLog(exp);
                    continue;
                }
                finally
                {
                    CurrentProgress++;
                }
            }
            ClearQueue();
            BaseModel.IsLoading = false;
        }
        void ClearQueue()
        {
            Application.Current.Dispatcher.Invoke(() => {
                DataOrder.Queue.Clear(); });
            DataOrder.AmountRub = 0;

            OrderAmout = DataOrder.AmountRub;
        }
        //file
        public ICommand ExportCsvCommand =>
            new RelayCommand((obj) =>
            {
                Task.Run(() =>
                {
                    BaseModel.IsLoading = true;
                    ParserService list = new();
                    list.ExportCsv(ParserGridView, Mode);
                    BaseModel.IsLoading = false;
                });
            }, (obj) => (ParserGrid != null | ParserGridView != null) & !BaseModel.IsLoading);
        public ICommand ImportCsvCommand =>
            new RelayCommand((obj) =>
            {
                Task.Run(() =>
                {
                    BaseModel.IsLoading = true;
                    ParserService list = new();
                    ObservableCollection<DataParser> response = list.ImportCsv();
                    if (response.Any())
                    {
                        Mode = "Import";
                        DataGrid(ParserProperties.Default.ServiceOne, ParserProperties.Default.ServiceTwo);

                        DataParser.ParserItems = response;
                        ParserGrid = response;
                        ParserGridView = CollectionViewSource.GetDefaultView(ParserGrid);
                        Currency = ParserList.DataCurrency;
                    }
                    BaseModel.IsLoading = false;
                });
            }, (obj) => !BaseModel.IsLoading);
        public ICommand ExportTxtCommand =>
            new RelayCommand((obj) =>
            {
                Task.Run(() =>
                {
                    BaseModel.IsLoading = true;
                    ParserService list = new();
                    list.ExportTxt(ParserGridView, Price1, Mode);
                    BaseModel.IsLoading = false;
                });
            }, (obj) => DataParser.ParserItems.Any() & !BaseModel.IsLoading);
    }
}