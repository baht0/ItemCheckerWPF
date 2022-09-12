using ItemChecker.Core;
using ItemChecker.MVVM.Model;
using ItemChecker.Net;
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
    public class RareViewModel : ObservableObject
    {
        #region prop
        System.Timers.Timer TimerView = new(500);

        private RareTable _rareTable = new();
        private DataRare _selectedItem = new();
        private RareFilter _filterConfig = new();
        private string _searchString;
        public RareTable RareTable
        {
            get
            {
                return _rareTable;
            }
            set
            {
                _rareTable = value;
                OnPropertyChanged();
            }
        }
        public DataRare SelectedItem
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
                RareInfo.Data = value;
            }
        }
        public RareFilter FilterConfig
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
                FilterConfig = new RareFilter();
                if (RareTable.GridView != null)
                    RareTable.GridView.Filter = item =>
                    {
                        return ((DataParser)item).ItemName.Contains(value, StringComparison.OrdinalIgnoreCase);
                    };
            }
        }

        private RareCheckConfig _rareCheckConfig = new();
        private RareCheckStatus _rareCheckStatus = new();
        private RareInfo _rareInfo = new();
        private string _addItem = string.Empty;
        public RareCheckConfig RareCheckConfig
        {
            get
            {
                return _rareCheckConfig;
            }
            set
            {
                _rareCheckConfig = value;
                OnPropertyChanged();
            }
        }
        public RareCheckStatus RareCheckStatus
        {
            get
            {
                return _rareCheckStatus;
            }
            set
            {
                _rareCheckStatus = value;
                OnPropertyChanged();
            }
        }
        public RareInfo RareInfo
        {
            get
            {
                return _rareInfo;
            }
            set
            {
                _rareInfo = value;
                OnPropertyChanged();
            }
        }
        public string AddItem
        {
            get
            {
                return _addItem;
            }
            set
            {
                _addItem = value;
                OnPropertyChanged();
            }
        }

        #endregion
        public RareViewModel()
        {
            TimerView.Elapsed += UpdateView;
            TimerView.Enabled = true;
            RareTable.GridView = CollectionViewSource.GetDefaultView(new List<DataRare>());
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
                var item = SelectedItem;
                string market_hash_name = Edit.EncodeMarketHashName(item.ItemName);
                switch ((Int32)obj)
                {
                    case 1 or 2 or 3:
                        MessageBoxResult result = MessageBox.Show("Inspect in Game?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (result == MessageBoxResult.Yes)
                            Edit.OpenUrl(item.Link);
                        break;
                    case 4:
                        Edit.OpenUrl("https://steamcommunity.com/market/listings/730/" + market_hash_name);
                        break;
                    default:
                        Clipboard.SetText(item.ItemName);
                        break;
                }
            });
        public ICommand OpenStickerOutCommand =>
            new RelayCommand((obj) =>
            {
                string market_hash_name = Edit.EncodeMarketHashName((string)obj);
                Edit.OpenUrl("https://steamcommunity.com/market/listings/730/" + market_hash_name);
            });
        public ICommand SwitchCurrencyCommand =>
            new RelayCommand((obj) =>
            {
                Currency currency = SteamBase.CurrencyList.FirstOrDefault(x => x.Name == (string)obj);
                var items = RareTable.GridView.Cast<DataRare>().ToList();
                if (RareTable.CurectCurrency.Id != 1)
                    foreach (var item in items)
                    {
                        item.Price = Edit.ConverterToUsd(item.Price, RareTable.CurectCurrency.Value);
                        item.PriceCompare = Edit.ConverterToUsd(item.PriceCompare, RareTable.CurectCurrency.Value);
                        item.Difference = Edit.ConverterToUsd(item.Difference, RareTable.CurectCurrency.Value);
                    }
                foreach (var item in items)
                {
                    item.Price = Edit.ConverterFromUsd(item.Price, currency.Value);
                    item.PriceCompare = Edit.ConverterFromUsd(item.PriceCompare, currency.Value);
                    item.Difference = Edit.ConverterFromUsd(item.Difference, currency.Value);
                }
                RareTable.CurrencySymbol = currency.Symbol;
                RareTable.CurectCurrency = currency;
                RareTable.GridView = CollectionViewSource.GetDefaultView(items);
            }, (obj) => RareTable.GridView != null);
        //filter
        public ICommand ClearSearchCommand =>
            new RelayCommand((obj) =>
            {
                SearchString = string.Empty;
            }, (obj) => !String.IsNullOrEmpty(SearchString));
        public ICommand ApplyCommand =>
            new RelayCommand((obj) =>
            {
                var filterConfig = obj as RareFilter;

                RareTable.GridView.Filter = null;
                RareTable.GridView.Filter = item =>
                {
                    return RareService.ApplyFilter(filterConfig, (DataRare)item);
                };
                RareFilter.FilterConfig = filterConfig;
            }, (obj) => RareTable.Items.Any());
        public ICommand ResetCommand =>
            new RelayCommand((obj) =>
            {
                FilterConfig = new();
                RareTable.GridView.Filter = null;
                RareFilter.FilterConfig = new();
            }, (obj) => RareTable.Items.Any());
        //Check
        public ICommand ClearCheckedCommand =>
            new RelayCommand((obj) =>
            {
                MessageBoxResult result = MessageBox.Show("Are you sure you want to clear the list?",
                    "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    RareCheckStatus = new();
                    RareTable.Items.Clear();
                    RareTable.GridView = CollectionViewSource.GetDefaultView(RareTable.Items);
                }
            }, (obj) => RareTable.Items.Any() && !RareCheckStatus.IsService);
        public ICommand CheckCommand =>
            new RelayCommand((obj) =>
            {
                if (!RareCheckStatus.IsService)
                {
                    RareCheckStatus.IsService = true;
                    var config = obj as RareCheckConfig;
                    SaveConfig(config);

                    RareCheckStatus.Timer.Elapsed += timerTick;
                    RareCheckStatus.TimerTick = config.Time * 60;
                    RareCheckStatus.Timer.Enabled = true;
                }
                else
                {
                    RareCheckStatus.cts.Cancel();
                    RareCheckStatus.Status = string.Empty;
                    RareCheckStatus.IsService = false;
                    RareCheckStatus.Timer.Enabled = false;
                    RareCheckStatus.TimerTick = 0;
                    RareCheckStatus.Timer.Elapsed -= timerTick;
                }
            }, (obj) => ItemsList.Rare.Any());
        void SaveConfig(RareCheckConfig config)
        {
            RareProperties.Default.Time = config.Time;
            RareProperties.Default.MinPrecent = config.MinPrecent;
            RareProperties.Default.CompareId = config.CompareId;
            RareProperties.Default.ParameterId = config.ParameterId;

            RareProperties.Default.maxFloatValue_FN = config.FactoryNew;
            RareProperties.Default.maxFloatValue_MW = config.MinimalWear;
            RareProperties.Default.maxFloatValue_FT = config.FieldTested;
            RareProperties.Default.maxFloatValue_WW = config.WellWorn;
            RareProperties.Default.maxFloatValue_BS = config.BattleScarred;

            RareProperties.Default.IsNormal = config.Normal;
            RareProperties.Default.IsHolo = config.Holo;
            RareProperties.Default.IsGlitter = config.Glitter;
            RareProperties.Default.IsFoil = config.Foil;
            RareProperties.Default.IsGold = config.Gold;
            RareProperties.Default.IsContraband = config.Contraband;
            RareProperties.Default.StickerCount = config.StickerCount;
            RareProperties.Default.NameContains = config.NameContains;

            RareProperties.Default.Phase1 = config.Phase1;
            RareProperties.Default.Phase2 = config.Phase2;
            RareProperties.Default.Phase3 = config.Phase3;
            RareProperties.Default.Phase4 = config.Phase4;
            RareProperties.Default.Ruby = config.Ruby;
            RareProperties.Default.Sapphire = config.Sapphire;
            RareProperties.Default.BlackPearl = config.BlackPearl;
            RareProperties.Default.Emerald = config.Emerald;

            RareCheckConfig.CheckedConfig = (RareCheckConfig)config.Clone();
            RareProperties.Default.Save();
        }
        void timerTick(Object sender, ElapsedEventArgs e)
        {
            RareCheckStatus.TimerTick--;
            TimeSpan timeSpan = TimeSpan.FromSeconds(RareCheckStatus.TimerTick);
            RareCheckStatus.Status = timeSpan.ToString("mm':'ss");
            if (RareCheckStatus.TimerTick <= 0)
            {
                RareCheckStatus.Status = "Preparation...";
                RareCheckStatus.Timer.Enabled = false;
                RareCheckStatus.Progress = 0;

                RareCheckStatus.cts = new();
                RareCheckStatus.token = RareCheckStatus.cts.Token;
                Check();
            }
        }
        void Check()
        {
            try
            {
                int start = RareTable.Items.Count;

                RareCheckService floatCheck = new();
                RareCheckStatus.MaxProgress = ItemsList.Rare.Count;
                RareCheckStatus.Status = "Checking...";
                foreach (var list in ItemsList.Rare)
                {
                    try
                    {
                        foreach (var item in floatCheck.Check(list.ItemName))
                            if (!RareTable.Items.Any(x => x.FloatValue == item.FloatValue))
                                RareTable.Items.Add(item);
                    }
                    catch (Exception exp)
                    {
                        BaseService.errorLog(exp, false);
                    }
                    finally
                    {
                        RareTable.Count = RareTable.Items.Count;
                        RareCheckStatus.Progress++;
                    }
                    if (RareCheckStatus.token.IsCancellationRequested)
                        break;
                }
                RareTable.CurrencyId = 0;
                System.Threading.Thread.Sleep(1000);
                RareTable.GridView = CollectionViewSource.GetDefaultView(RareTable.Items);
                RareCheckStatus.Cycles++;
                if (RareTable.Items.Count - start != 0)
                    System.Media.SystemSounds.Beep.Play();
            }
            catch (Exception exp)
            {
                RareCheckStatus.cts.Cancel();
                RareCheckStatus.Status = string.Empty;
                RareCheckStatus.IsService = false;
                RareCheckStatus.Timer.Enabled = false;
                RareCheckStatus.TimerTick = 0;
                RareCheckStatus.Timer.Elapsed -= timerTick;

                BaseService.errorLog(exp, true);
            }
            finally
            {
                if (!RareCheckStatus.token.IsCancellationRequested)
                {
                    RareCheckStatus.TimerTick = RareProperties.Default.Time * 60;
                    RareCheckStatus.Timer.Enabled = true;
                }
            }
        }
        public ICommand TimerCommand =>
            new RelayCommand((obj) =>
            {
                RareCheckStatus.TimerTick = 1;
            }, (obj) => RareCheckStatus.IsService);
        //info
        public ICommand BuyItemCommand =>
            new RelayCommand((obj) =>
            {
                MessageBoxResult result = MessageBox.Show(
                        "Are you sure you want to buy this item?", "Question",
                        MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.No)
                    return;
                Task.Run(() =>
                {
                    try
                    {
                        BaseModel.IsWorking = true;
                        var data = (DataRare)obj;
                        string marketHashName = Edit.EncodeMarketHashName(data.ItemName);
                        var response = Post.BuyListing(SteamAccount.Cookies, marketHashName, data.DataBuy.ListingId, data.DataBuy.Fee, data.DataBuy.Subtotal, data.DataBuy.Total, SteamAccount.CurrencyId);
                        string message = response.StatusCode == System.Net.HttpStatusCode.OK ? $"{data.ItemName}\nWas bought." : "Something went wrong...";
                        Main.Message.Enqueue(message);
                    }
                    catch (Exception exp)
                    {
                        BaseService.errorLog(exp, true);
                    }
                    finally
                    {
                        BaseModel.IsWorking = false;
                    }
                });
                
            }, (obj) => SelectedItem != null && !String.IsNullOrEmpty(SelectedItem.Link));
    }
}
