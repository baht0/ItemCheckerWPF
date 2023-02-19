using ItemChecker.Core;
using ItemChecker.MVVM.Model;
using ItemChecker.MVVM.View;
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
        RareTable _rareTable = new();
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
        DataRare _selectedItem = new();
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
        RareFilter _filterConfig = new();
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
        string _searchString;

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
        RareCheckConfig _rareCheckConfig = new();
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
        RareCheckStatus _rareCheckStatus = new();
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
        RareInfo _rareInfo = new();
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
        string _addItem = string.Empty;

        #endregion

        public RareViewModel()
        {
            RareTable.GridView = CollectionViewSource.GetDefaultView(new List<DataRare>());
        }

        #region table
        public ICommand OpenItemOutCommand =>
            new RelayCommand((obj) =>
            {
                var item = SelectedItem;
                string market_hash_name = Uri.EscapeDataString(item.ItemName);
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
                string market_hash_name = Uri.EscapeDataString((string)obj);
                Edit.OpenUrl("https://steamcommunity.com/market/listings/730/" + market_hash_name);
            });
        public ICommand SwitchCurrencyCommand =>
            new RelayCommand((obj) =>
            {
                var currency = Currencies.Allow.FirstOrDefault(x => x.Name == (string)obj);
                var items = RareTable.GridView.Cast<DataRare>().ToList();
                if (RareTable.CurectCurrency.Id != 1)
                    foreach (var item in items)
                    {
                        item.Price = Currency.ConverterToUsd(item.Price, RareTable.CurectCurrency.Id);
                        item.PriceCompare = Currency.ConverterToUsd(item.PriceCompare, RareTable.CurectCurrency.Id);
                        item.Difference = Currency.ConverterToUsd(item.Difference, RareTable.CurectCurrency.Id);
                    }
                foreach (var item in items)
                {
                    item.Price = Currency.ConverterFromUsd(item.Price, currency.Id);
                    item.PriceCompare = Currency.ConverterFromUsd(item.PriceCompare, currency.Id);
                    item.Difference = Currency.ConverterFromUsd(item.Difference, currency.Id);
                }
                RareTable.CurrencySymbol = currency.Symbol;
                RareTable.CurectCurrency = currency;
                RareTable.GridView = CollectionViewSource.GetDefaultView(items);
            }, (obj) => RareTable.GridView != null);

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
        #endregion

        #region Check
        public ICommand ClearCheckedCommand =>
            new RelayCommand((obj) =>
            {
                MessageBoxResult result = MessageBox.Show("Are you sure you want to clear the list?",
                    "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    RareCheckStatus = new();
                    RareTable.Items.Clear();
                    RareTable.Count = 0;
                    RareTable.GridView = CollectionViewSource.GetDefaultView(RareTable.Items);
                }
            }, (obj) => RareTable.Items.Any() && !RareCheckStatus.IsService);
        public ICommand CheckCommand =>
            new RelayCommand((obj) =>
            {
                if (!RareCheckStatus.IsService)
                {
                    RareCheckStatus.Status = "Starting...";
                    RareCheckStatus.IsService = true;
                    var config = obj as RareCheckConfig;
                    SaveConfig(config);
                    MainWindow.CloseShowListWin("Rare");

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
            }, (obj) => RareCheckConfig.MaxPrecent > 0
                        && RareCheckConfig.Time > 0
                        && (SavedItems.Rare.Any(x => x.ServiceId == RareCheckConfig.ParameterId))
                            || RareCheckConfig.ParameterId == 2 && RareCheckConfig.AllDopplers);
        void SaveConfig(RareCheckConfig config)
        {
            RareProperties.Default.Time = config.Time;

            RareProperties.Default.maxFloatValue_FN = config.FactoryNew;
            RareProperties.Default.maxFloatValue_MW = config.MinimalWear;
            RareProperties.Default.maxFloatValue_FT = config.FieldTested;
            RareProperties.Default.maxFloatValue_WW = config.WellWorn;
            RareProperties.Default.maxFloatValue_BS = config.BattleScarred;
            RareProperties.Default.Save();

            RareCheckConfig.CheckedConfig = (RareCheckConfig)config.Clone();
            RareCheckConfig.CheckedConfig.MaxPrecent *= -1;
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
            if (!SavedItems.Rare.Any(x => x.ServiceId == RareCheckConfig.ParameterId) && RareCheckConfig.ParameterId != 2 && !RareCheckConfig.AllDopplers)
            {
                RareCheckStatus.cts.Cancel();
                RareCheckStatus.Status = string.Empty;
                RareCheckStatus.IsService = false;
                RareCheckStatus.Timer.Enabled = false;
                RareCheckStatus.TimerTick = 0;
                RareCheckStatus.Timer.Elapsed -= timerTick;
            }
        }
        void Check()
        {
            try
            {
                var serviceList = SavedItems.Rare.Where(x => x.ServiceId == RareCheckConfig.ParameterId).ToList();
                serviceList = RareCheckConfig.ParameterId == 2 && RareCheckConfig.AllDopplers ? RareService.GetAllDoppler() : serviceList;
                int start = RareTable.Items.Count;

                RareCheckStatus.MaxProgress = serviceList.Count;
                RareCheckStatus.Status = "Checking...";
                foreach (var list in serviceList)
                {
                    try
                    {
                        var checkedList = RareCheckService.Check(list.ItemName);
                        foreach (var item in checkedList)
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
                    Main.Notifications.Add(new()
                    {
                        Title = "Rare",
                        Message = $"Found {RareTable.Items.Count - start} items."
                    });
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

        public ICommand BuyItemCommand =>
            new RelayCommand((obj) =>
            {
                MessageBoxResult result = MessageBox.Show(
                        "Are you sure you want to buy this item?", "Question",
                        MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.No)
                    return;

                RareInfo.IsBusy = true;
                Task.Run(() =>
                {
                    try
                    {
                        var data = (DataRare)obj;
                        var response = SteamRequest.Post.BuyListing(data.ItemName, data.DataBuy.ListingId, data.DataBuy.Fee, data.DataBuy.Subtotal, data.DataBuy.Total, SteamAccount.Currency.Id);
                        string message = response.StatusCode == System.Net.HttpStatusCode.OK ? $"{data.ItemName}\nWas bought." : "Something went wrong...";
                        Main.Message.Enqueue(message);
                    }
                    catch (Exception exp)
                    {
                        BaseService.errorLog(exp, true);
                    }
                    finally
                    {
                        RareInfo.IsBusy = false;
                    }
                });
                
            }, (obj) => SelectedItem != null && !String.IsNullOrEmpty(SelectedItem.Link) && !RareInfo.IsBusy);
        #endregion
    }
}
