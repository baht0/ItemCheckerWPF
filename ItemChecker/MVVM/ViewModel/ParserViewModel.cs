using ItemChecker.Core;
using ItemChecker.MVVM.Model;
using ItemChecker.MVVM.View;
using ItemChecker.Support;
using System;
using System.IO;
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
        readonly Timer TimerView = new(500);
        public BaseModel Parser
        {
            get { return _parser; }
            set
            {
                _parser = value;
                OnPropertyChanged();
            }
        }
        BaseModel _parser = new();
        public ParserViewModel()
        {
            DataGridParse.GridView = CollectionViewSource.GetDefaultView(DataGridParse.Items);
            TimerView.Elapsed += UpdateView;
            TimerView.Enabled = true;
            ToolPlaceOrder.Update();
        }
        void UpdateView(Object sender, ElapsedEventArgs e)
        {
            try
            {
                if (DataGridParse.CanBeUpdated)
                {
                    DataGridParse.Items = new(ToolParser.Items);
                    DataGridParse.CanBeUpdated = false;
                }
            }
            catch (Exception ex)
            {
                BaseModel.ErrorLog(ex, false);
            }
        }

        //Table
        public DataGridParse DataGridParse
        {
            get
            {
                return _dataGridParse;
            }
            set
            {
                _dataGridParse = value;
                OnPropertyChanged();
            }
        }
        DataGridParse _dataGridParse = new();
        public ParserFilter ParserFilter
        {
            get
            {
                return _parserFilter;
            }
            set
            {
                _parserFilter = value;
                OnPropertyChanged();
            }
        }
        ParserFilter _parserFilter = new();
        public string SearchString
        {
            get
            {
                return _searchString;
            }
            set
            {
                _searchString = value;
                ParserFilter = new();
                if (DataGridParse.GridView != null)
                    DataGridParse.GridView.Filter = item =>
                    {
                        return ((DataParser)item).ItemName.Contains(value, StringComparison.OrdinalIgnoreCase);
                    };
                OnPropertyChanged();
            }
        }
        string _searchString;
        public ICommand OpenItemOutCommand =>
            new RelayCommand((obj) =>
            {
                DataGridParse.ShowItemInService((int)obj, ToolParser.ServiceOne, ToolParser.ServiceTwo);
            }, (obj) => DataGridParse.Items.Any());
        public ICommand SwitchCurrencyCommand =>
            new RelayCommand((obj) =>
            {
                var currency = Currencies.Allow.FirstOrDefault(x => x.Name == (string)obj);
                var items = DataGridParse.Items.ToList();
                if (Parser.CurrentCurrency.Id != 1)
                    foreach (var item in items)
                    {
                        item.Purchase = Currency.ConverterToUsd(item.Purchase, Parser.CurrentCurrency.Id);
                        item.Price = Currency.ConverterToUsd(item.Price, Parser.CurrentCurrency.Id);
                        item.Get = Currency.ConverterToUsd(item.Get, Parser.CurrentCurrency.Id);
                        item.Difference = Currency.ConverterToUsd(item.Difference, Parser.CurrentCurrency.Id);
                    }
                foreach (var item in items)
                {
                    item.Purchase = Currency.ConverterFromUsd(item.Purchase, currency.Id);
                    item.Price = Currency.ConverterFromUsd(item.Price, currency.Id);
                    item.Get = Currency.ConverterFromUsd(item.Get, currency.Id);
                    item.Difference = Currency.ConverterFromUsd(item.Difference, currency.Id);
                }
                Parser.CurrentCurrency = currency;
                ParserFilter = new();
                DataGridParse.GridView = CollectionViewSource.GetDefaultView(items);
            }, (obj) => DataGridParse.Items.Any());
        public ICommand AddReserveCommand =>
            new RelayCommand((obj) =>
            {
                var name = obj as string;

                DataItem item = new(name, ToolParser.ServiceTwo);
                string message = SavedItems.Reserve.Add(item) ? $"{item.ItemName}\nItem has been added." : "Opps something went wrong...";
                Main.Message.Enqueue(message);
            }, (obj) => ToolParser.ServiceOne < 2);
        public ICommand RemoveReserveCommand =>
            new RelayCommand((obj) =>
            {
                string itemName = (string)obj;
                var item = SavedItems.Reserve.FirstOrDefault(x => x.ItemName == itemName);
                if (item != null)
                {
                    SavedItems.Reserve.Remove(item);
                    Main.Message.Enqueue($"{itemName}\nRemoved from list.");
                }
            }, (obj) => SavedItems.Reserve.Any());
        public ICommand ClearSearchCommand =>
            new RelayCommand((obj) =>
            {
                SearchString = string.Empty;
            }, (obj) => !String.IsNullOrEmpty(SearchString));
        public ICommand ApplyCommand =>
            new RelayCommand((obj) =>
            {
                DataGridParse.GridView.Filter = null;
                DataGridParse.GridView.Filter = item => {
                    return ParserFilter.ApplyFilter((DataParser)item);
                };
            }, (obj) => DataGridParse.Items.Any());
        public ICommand ResetCommand =>
            new RelayCommand((obj) =>
            {
                ParserFilter = new();
                DataGridParse.GridView.Filter = null;
            }, (obj) => DataGridParse.Items.Any());

        //Check
        public ToolParser ToolParser
        {
            get
            {
                return _toolParser;
            }
            set
            {
                _toolParser = value;
                OnPropertyChanged();
            }
        }
        ToolParser _toolParser = new();
        public ICommand MaxPriceCommand =>
            new RelayCommand((obj) =>
            {
                ToolParser.SetMaxPrice((int)obj);
            });
        public ICommand CheckCommand =>
            new RelayCommand((obj) =>
            {
                DataGridParse = new();
                ToolParser.Start();
                Parser.CurrencyId = 0;
                DataGridParse.Count = DataGridParse.Items.Count;
                DataGridParse.GridView = CollectionViewSource.GetDefaultView(DataGridParse.Items.OrderByDescending(x => x.Precent));

            }, (obj) => ItemsBase.List.Any() && ToolParser.ServiceOne != ToolParser.ServiceTwo
                        && ToolParser.MaxPrice != 0 && ToolParser.MaxPrice > ToolParser.MinPrice);
        public ICommand ContinueCheckCommand =>
            new RelayCommand((obj) =>
            {
                Task.Run(() => ToolParser.Start(DataGridParse.Items));
            }, (obj) => !ToolParser.IsParser && ToolParser.IsStoped);
        public ICommand ImportCommand =>
            new RelayCommand((obj) =>
            {
                ImportParserWindow window = new();
                window.ShowDialog();
                string filePath = window.ReturnValue;
                Task.Run(() =>
                {
                    var items = ToolParser.Import(filePath);
                    if (items.Any())
                    {
                        DataGridParse = new();
                        Parser.CurrencyId = 0;
                        DataGridParse.Count = items.Count;
                        DataGridParse.Items = items;

                        Main.Message.Enqueue("The table was successfully imported.");
                    }
                });

            }, (obj) => !ToolParser.IsParser && Directory.Exists(ProjectInfo.DocumentPath + "extract"));

        //PlaceOrder
        public ToolPlaceOrder ToolPlaceOrder
        {
            get { return _toolPlaceOrder; }
            set
            {
                _toolPlaceOrder = value;
                OnPropertyChanged();
            }
        }
        ToolPlaceOrder _toolPlaceOrder = new();
        public ICommand AddQueueCommand =>
            new RelayCommand((obj) =>
            {
                var data = obj as DataParser;
                var item = new DataQueue(data, Parser.CurrentCurrency);
                ToolPlaceOrder.AddQueue(item);

            }, (obj) => ToolParser.ServiceOne == 0);
        public ICommand RemoveQueueCommand =>
            new RelayCommand((obj) =>
            {
                var item = obj as DataQueue;
                ToolPlaceOrder.Items.Remove(item);
                ToolPlaceOrder.Update();

            }, (obj) => ToolPlaceOrder.Items.Any() && ToolPlaceOrder.SelectedItem != null);
        public ICommand ClearQueueCommand =>
            new RelayCommand((obj) =>
            {
                MessageBoxResult result = MessageBox.Show(
                    "Are you sure you want to clear the list?",
                    "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    ToolPlaceOrder.Items = new();
                    ToolPlaceOrder.Update();
                }
            }, (obj) => ToolPlaceOrder.Items.Any());
        public ICommand PlaceOrderCommand =>
            new RelayCommand((obj) =>
            {
                ToolPlaceOrder.Start(ToolParser.ServiceTwo);
                
            }, (obj) => ToolPlaceOrder.Items.Any() && !ToolPlaceOrder.IsBusy);
    }
}