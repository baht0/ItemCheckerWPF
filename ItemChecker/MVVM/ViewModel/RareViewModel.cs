using ItemChecker.Core;
using ItemChecker.MVVM.Model;
using ItemChecker.MVVM.View;
using ItemChecker.Support;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace ItemChecker.MVVM.ViewModel
{
    public class RareViewModel : ObservableObject
    {
        readonly Timer TimerView = new(500);
        public BaseModel Rare
        {
            get
            {
                return _rare;
            }
            set
            {
                _rare = value;
                OnPropertyChanged();
            }
        }
        BaseModel _rare = new();
        public RareViewModel()
        {
            DataGridRare.GridView = CollectionViewSource.GetDefaultView(new List<DataRare>());
            TimerView.Elapsed += UpdateView;
            TimerView.Enabled = true;
        }
        void UpdateView(Object sender, ElapsedEventArgs e)
        {
            try
            {
                if (DataGridRare.CanBeUpdated)
                {
                    DataGridRare.Items = new(ToolRare.Items);
                    Rare.CurrencyId = 0;
                    DataGridRare.Count = ToolRare.Items.Count;
                    DataGridRare.CanBeUpdated = false;
                }
            }
            catch (Exception ex)
            {
                BaseModel.ErrorLog(ex, false);
            }
        }

        //Table
        public DataGridRare DataGridRare
        {
            get
            {
                return _dataGridRare;
            }
            set
            {
                _dataGridRare = value;
                OnPropertyChanged();
            }
        }
        DataGridRare _dataGridRare = new();
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
                if (DataGridRare.GridView != null)
                    DataGridRare.GridView.Filter = item =>
                    {
                        return ((DataParser)item).ItemName.Contains(value, StringComparison.OrdinalIgnoreCase);
                    };
            }
        }
        string _searchString;
        public ICommand OpenItemOutCommand =>
            new RelayCommand((obj) =>
            {
                var columnId = (int)obj;
                DataGridRare.OpenItem(columnId);                
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
                var items = DataGridRare.GridView.Cast<DataRare>().ToList();
                if (Rare.CurrentCurrency.Id != 1)
                    foreach (var item in items)
                    {
                        item.Price = Currency.ConverterToUsd(item.Price, Rare.CurrentCurrency.Id);
                        item.PriceCompare = Currency.ConverterToUsd(item.PriceCompare, Rare.CurrentCurrency.Id);
                        item.Difference = Currency.ConverterToUsd(item.Difference, Rare.CurrentCurrency.Id);
                    }
                foreach (var item in items)
                {
                    item.Price = Currency.ConverterFromUsd(item.Price, currency.Id);
                    item.PriceCompare = Currency.ConverterFromUsd(item.PriceCompare, currency.Id);
                    item.Difference = Currency.ConverterFromUsd(item.Difference, currency.Id);
                }
                Rare.CurrentCurrency = currency;
                DataGridRare.GridView = CollectionViewSource.GetDefaultView(items);
            }, (obj) => DataGridRare.GridView != null);
        public ICommand ClearSearchCommand =>
            new RelayCommand((obj) =>
            {
                SearchString = string.Empty;
            }, (obj) => !String.IsNullOrEmpty(SearchString));
        public ICommand ApplyCommand =>
            new RelayCommand((obj) =>
            {
                DataGridRare.GridView.Filter = null;
                DataGridRare.GridView.Filter = item =>
                {
                    return FilterConfig.ApplyFilter((DataRare)item);
                };
            }, (obj) => DataGridRare.Items.Any());
        public ICommand ResetCommand =>
            new RelayCommand((obj) =>
            {
                FilterConfig = new();
                DataGridRare.GridView.Filter = null;
            }, (obj) => DataGridRare.Items.Any());

        //Check
        public ToolRare ToolRare
        {
            get
            {
                return _toolRare;
            }
            set
            {
                _toolRare = value;
                OnPropertyChanged();
            }
        }
        ToolRare _toolRare = new();
        public ICommand ClearCheckedCommand =>
            new RelayCommand((obj) =>
            {
                MessageBoxResult result = MessageBox.Show("Are you sure you want to clear the list?",
                    "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    DataGridRare.Items.Clear();
                    ToolRare.Items.Clear();
                    DataGridRare.Count = 0;
                    DataGridRare.GridView = CollectionViewSource.GetDefaultView(DataGridRare.Items);
                }
            }, (obj) => DataGridRare.Items.Any() && !ToolRare.IsService);
        public ICommand CheckCommand =>
            new RelayCommand((obj) =>
            {
                MainWindow.CloseShowListWin("Rare");
                ToolRare.Start();

            }, (obj) => ToolRare.MinPrecent > 0 && ToolRare.Time > 0
                        && (SavedItems.Rare.Any(x => x.ServiceId == ToolRare.ParameterId))
                            || ToolRare.ParameterId == 2 && ToolRare.AllDopplers);
        public ICommand TimerCommand =>
            new RelayCommand((obj) =>
            {
                ToolRare.ResetTime();
            }, (obj) => ToolRare.IsService);
        public ICommand BuyItemCommand =>
            new RelayCommand((obj) =>
            {
                DataGridRare.SelectedItem.BuyItem();

            }, (obj) => DataGridRare.SelectedItem != null && !string.IsNullOrEmpty(DataGridRare.SelectedItem.Link) && !DataGridRare.SelectedItem.IsBusy);
    }
}
