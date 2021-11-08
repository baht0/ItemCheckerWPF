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
        private ObservableCollection<ParserData> _parserGrid;
        private ParserData _selectedParserItem;
        //filter
        private ICollectionView _parserGridView;
        private Filter _filterConfig;
        private string _searchString;
        //config
        private Parser _parserConfig;
        private ObservableCollection<string> _checkList;
        private string _selectedItem;
        private string _selectedFile;
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
        public ObservableCollection<ParserData> ParserGrid
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
        public ParserData SelectedParserItem
        {
            get
            {
                return _selectedParserItem;
            }
            set
            {
                _selectedParserItem = value;
                OnPropertyChanged();
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
                ParserGridView.Filter = item =>
                {
                    return ((ParserData)item).ItemName.Contains(value);
                };
            }
        }
        //config
        public Parser ParserConfig
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
        public ObservableCollection<string> CheckList
        {
            get
            {
                return _checkList;
            }
            set
            {
                _checkList = value;
                OnPropertyChanged("CheckList");
            }
        }
        public string SelectedItem
        {
            get
            {
                return _selectedItem;
            }
            set
            {
                _selectedItem = value;
                OnPropertyChanged();
            }
        }
        public string SelectedFile
        {
            get
            {
                return _selectedFile;
            }
            set
            {
                _selectedFile = value;
                OnPropertyChanged();
            }
        }
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
            Mode = Parser.Mode;
            Service1 = Parser.Service1;
            Service2 = Parser.Service2;
            Currency = Parser.DataCurrency;

            Price1 = Parser.Price1;
            Price2 = Parser.Price2;
            Price3 = Parser.Price3;
            Price4 = Parser.Price4;

            ParserConfig = new Parser()
            {
                Tryskins = ParserProperties.Default.tryskins,
                Manual = ParserProperties.Default.manual,
                Queue = ParserProperties.Default.queue,

                Services = new ObservableCollection<string>()
                {
                    "SteamMarket",
                    "Cs.Money",
                    "Loot.Farm"
                },
                ServiceOne = ParserProperties.Default.serviceOne,
                ServiceTwo = ParserProperties.Default.serviceTwo,

                MaxPrice = ParserProperties.Default.maxPrice,
                MinPrice = ParserProperties.Default.minPrice,
                MaxPrecent = ParserProperties.Default.maxPrecent,
                MinPrecent = ParserProperties.Default.minPrecent,
                SteamSales = ParserProperties.Default.steamSales,
                NameContains = ParserProperties.Default.nameContains,
                Knife = ParserProperties.Default.knife,
                Stattrak = ParserProperties.Default.stattrak,
                Souvenir = ParserProperties.Default.souvenir,
                Sticker = ParserProperties.Default.sticker
            };
            CheckList = ParserProperties.Default.checkList;

            if (ParserData.ParserItems.Any())
                ParserGrid = ParserData.ParserItems;

            ParserGridView = CollectionViewSource.GetDefaultView(ParserGrid);
            FilterConfig = new Filter();

            PlaceOrderItems = OrderPlace.Queue;
            OrderAmout = OrderPlace.AmountRub;
            OrderAvailable = Account.AvailableAmount;
        }
        //filter
        public ICommand ApplyCommand =>
            new RelayCommand((obj) =>
            {
                Filter filterConfig = obj as Filter;

                ParserGridView.Filter = null;
                ParserGridView.Filter = item =>
                {
                    return ApplyFilter(filterConfig, (ParserData)item);
                };
            }, (obj) => ParserGridView != null & !Main.IsLoading);
        bool ApplyFilter(Filter filterConfig, ParserData item)
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
            if (filterConfig.PrecentFrom != 0 | filterConfig.PrecentTo != 0 | filterConfig.DifferenceFrom != 0 | filterConfig.DifferenceTo != 0 | filterConfig.Hide100 | filterConfig.Hide0)
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
                FilterConfig = new Filter();
                ParserGridView.Filter = null;

            }, (obj) => ParserGridView != null & !Main.IsLoading);
        //manual
        public ICommand SelectFileCommand =>
            new RelayCommand((obj) =>
            {
                Task.Run(() =>
                {
                    ParserService list = new();
                    ObservableCollection<string> response = list.SelectFile();
                    if (response.Any())
                    {
                        CheckList = response;
                    }
                });
            });
        public ICommand GetCsmCommand =>
            new RelayCommand((obj) =>
            {
                Task.Run(() =>
                {
                    ParserService list = new();
                    CheckList = list.GetCheckList("https://broskins.com/csmoneyfeed.php?_=1625556262031");
                });
            });
        public ICommand GetLFCommand =>
            new RelayCommand((obj) =>
            {
                Task.Run(() =>
                {
                    ParserService list = new();
                    CheckList = list.GetCheckList("https://loot.farm/fullprice.json");
                });
            });
        public ICommand AddItemCommand =>
            new RelayCommand((obj) =>
            {
                if (CheckList == null)
                    CheckList = new ObservableCollection<string>();
                if (!CheckList.Contains((string)obj))
                    CheckList.Add((string)obj);
            });
        public ICommand RemoveItemCommand =>
            new RelayCommand((obj) =>
            {
                CheckList.Remove((string)obj);
            }, (obj) => CheckList != null);
        public ICommand ClearListCommand =>
            new RelayCommand((obj) =>
            {
                CheckList.Clear();
            }, (obj) => CheckList != null);

        //check
        public ICommand CheckCommand =>
            new RelayCommand((obj) =>
            {
                Task.Run(() =>
                {
                    if (!CheckList.Any() & ParserConfig.Manual)
                        return;

                    Parser parserConfig = (Parser)obj;
                    Main.IsLoading = true;
                    Check(parserConfig);
                });
            }, (obj) => !Main.IsLoading & !Main.Timer.Enabled & ParserConfig.ServiceOne != ParserConfig.ServiceTwo & ((ParserConfig.Manual & CheckList != null) | ParserConfig.Tryskins));
        void Check(Parser parserConfig)
        {
            try
            {
                Main.IsLoading = true;
                Main.cts = new();
                Main.token = Main.cts.Token;
                CleanDataGrid();
                DataGrid(parserConfig.ServiceOne, parserConfig.ServiceTwo);
                SaveConfig(parserConfig);
                TimerOn = true;
                CurrentProgress = 0;
                if (parserConfig.Tryskins)
                    CheckTryskins();
                else if (parserConfig.Manual)
                    CheckManual();
                if (ParserData.ParserItems.Any())
                    ParserGrid = ParserData.ParserItems;
                ParserGridView = CollectionViewSource.GetDefaultView(ParserGrid);
                Currency = Parser.DataCurrency;
                TimerOn = false;
            }
            catch (Exception exp)
            {
                BaseModel.errorLog(exp);
                BaseModel.errorMessage(exp);
            }
            finally
            {
                Main.IsLoading = false;
            }
        }
        void DataGrid(int serviceOne, int serviceTwo)
        {
            switch (serviceOne)
            {
                case 0:
                    Service1 = "SteamMarket";
                    Price1 = "Sale(ST)";
                    Price2 = "BuyOrder";
                    break;
                case 1:
                    Service1 = "Cs.Money";
                    Price1 = "Sale(CSM)";
                    Price2 = "Buy(CSM)";
                    break;
                case 2:
                    Service1 = "Loot.Farm";
                    Price1 = "Sale(LF)";
                    Price2 = "Buy(LF)";
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

            Parser.Mode = Mode;
            Parser.Service1 = Service1;
            Parser.Service2 = Service2;

            Parser.Price1 = Price1;
            Parser.Price2 = Price2;
            Parser.Price3 = Price3;
            Parser.Price4 = Price4;
        }
        void SaveConfig(Parser parser)
        {
            ParserProperties.Default.serviceOne = parser.ServiceOne;
            ParserProperties.Default.serviceTwo = parser.ServiceTwo;

            ParserProperties.Default.maxPrice = parser.MaxPrice;
            ParserProperties.Default.minPrice = parser.MinPrice;
            ParserProperties.Default.maxPrecent = parser.MaxPrecent;
            ParserProperties.Default.minPrecent = parser.MinPrecent;
            ParserProperties.Default.steamSales = parser.SteamSales;
            ParserProperties.Default.nameContains = parser.NameContains;
            ParserProperties.Default.knife = parser.Knife;
            ParserProperties.Default.stattrak = parser.Stattrak;
            ParserProperties.Default.souvenir = parser.Souvenir;
            ParserProperties.Default.sticker = parser.Sticker;

            ParserProperties.Default.checkList = CheckList;

            ParserProperties.Default.Save();
        }
        void CheckTryskins()
        {
            Mode = "TrySkins";
            ParserTryskinsService tryskins = new();
            List<IWebElement> items = tryskins.GetItems();
            if (items.Any())
            {
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
                    if (Main.token.IsCancellationRequested)
                        break;
                }
            }
        }
        void CheckManual()
        {
            Mode = "Manual";
            ParserManualService manual = new();
            manual.GetLF();
            MaxProgress = ParserProperties.Default.checkList.Count;
            TimeLeftAsync(ParserProperties.Default.checkList.Count, DateTime.Now);
            foreach (string itemName in ParserProperties.Default.checkList)
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
                if (Main.token.IsCancellationRequested)
                    break;
            }
        }
        void TimeLeftAsync(int itemCount, DateTime timeStart)
        {
            Task.Run(() => {
                while (CurrentProgress < itemCount)
                {
                    Timer = Edit.calcTimeLeft(timeStart, itemCount, CurrentProgress);
                    Thread.Sleep(300);
                }
            });
        }
        void CleanDataGrid()
        {
            if (ParserData.ParserItems.Any())
            {
                ParserData.ParserItems = new();
                ParserGrid = new ObservableCollection<ParserData>();
                ParserGridView = CollectionViewSource.GetDefaultView(ParserGrid);
            }
        }
        public ICommand StopCommand =>
            new RelayCommand((obj) =>
            {
                Main.cts.Cancel();
            }, (obj) => Main.IsLoading);

        //order
        public ICommand AddQueueCommand =>
            new RelayCommand((obj) =>
            {
                ParserData item = obj as ParserData;
                PlaceOrderService.AddQueue(item.ItemName, item.Price2);
                OrderAmout = OrderPlace.AmountRub;
            }, (obj) => OrderPlace.AmountRub < Account.AvailableAmount);
        public ICommand RemoveQueueCommand =>
            new RelayCommand((obj) =>
            {
                string item = obj as string;
                PlaceOrderService.RemoveQueue(item);
                OrderAmout = OrderPlace.AmountRub;
            }, (obj) => OrderPlace.Queue.Any());
        public ICommand ClearQueueCommand =>
            new RelayCommand((obj) =>
            {
                ClearQueue();
            }, (obj) => OrderPlace.Queue.Any());
        public ICommand PlaceOrderCommand =>
            new RelayCommand((obj) =>
            {
                Main.IsLoading = true;
                Task.Run(() => {
                    PlaceOrder();
                });
            }, (obj) => !Main.IsLoading & OrderPlace.Queue.Any());
        void PlaceOrder()
        {
            PlaceOrderService placeOrder = new();
            MaxProgress = OrderPlace.Queue.Count;
            CurrentProgress = 0;
            foreach (string itemName in OrderPlace.Queue)
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
            Main.IsLoading = false;
        }
        void ClearQueue()
        {
            Application.Current.Dispatcher.Invoke(() => {
                OrderPlace.Queue.Clear(); });
            OrderPlace.AmountRub = 0;

            OrderAmout = OrderPlace.AmountRub;
        }
        //file
        public ICommand ExportCsvCommand =>
            new RelayCommand((obj) =>
            {
                Task.Run(() =>
                {
                    Main.IsLoading = true;
                    ParserService list = new();
                    list.ExportCsv(ParserGridView, Mode);
                    Main.IsLoading = false;
                });
            }, (obj) => (ParserGrid != null | ParserGridView != null) & !Main.IsLoading);
        public ICommand ImportCsvCommand =>
            new RelayCommand((obj) =>
            {
                Task.Run(() =>
                {
                    Main.IsLoading = true;
                    ParserService list = new();
                    ObservableCollection<ParserData> response = list.ImportCsv();
                    if (response.Any())
                    {
                        Mode = "Import";
                        CleanDataGrid();
                        DataGrid(ParserProperties.Default.serviceOne, ParserProperties.Default.serviceTwo);

                        ParserData.ParserItems = response;
                        ParserGrid = response;
                        ParserGridView = CollectionViewSource.GetDefaultView(ParserGrid);
                        Currency = Parser.DataCurrency;
                    }
                    Main.IsLoading = false;
                });
            }, (obj) => !Main.IsLoading);
        public ICommand ExportTxtCommand =>
            new RelayCommand((obj) =>
            {
                Task.Run(() =>
                {
                    Main.IsLoading = true;
                    ParserService list = new();
                    list.ExportTxt(ParserGridView, Price1, Mode);
                    Main.IsLoading = false;
                });
            }, (obj) => ParserData.ParserItems.Any() & !Main.IsLoading);
    }
}