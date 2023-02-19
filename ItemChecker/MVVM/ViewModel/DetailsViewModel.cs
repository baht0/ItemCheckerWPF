using ItemChecker.Core;
using ItemChecker.MVVM.Model;
using ItemChecker.Support;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Input;

namespace ItemChecker.MVVM.ViewModel
{
    public class DetailsViewModel : ObservableObject
    {
        readonly Timer Timer = new(100);
        public SnackbarMessageQueue Message
        {
            get { return _message; }
            set
            {
                _message = value;
                OnPropertyChanged();
            }
        }
        SnackbarMessageQueue _message = new();
        public Details Details
        {
            get { return _details; }
            set
            {
                _details = value;
                OnPropertyChanged();
            }
        }
        Details _details = new();

        public ObservableCollection<DetailItem> ItemsView
        {
            get
            {
                return _itemsView;
            }
            set
            {
                _itemsView = value;
                OnPropertyChanged();
            }
        }
        ObservableCollection<DetailItem> _itemsView = new(Details.Items);
        public DetailItem SelectedItem
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
        DetailItem _selectedItem = new();
        public bool IsSearch
        {
            get
            {
                return _isSearch;
            }
            set
            {
                _isSearch = value;
                OnPropertyChanged();
            }
        }
        bool _isSearch;

        public DetailsViewModel(bool isMenu)
        {
            IsSearch = isMenu && !Details.Items.Any();

            Timer.Elapsed += UpdateWindow;
            Timer.Enabled = true;

            ItemsView = new(Details.Items);
            if (Details.Items.Any())
                SelectedItem = Details.Item != null ? Details.Item : Details.Items.LastOrDefault();
        }
        void UpdateWindow(Object sender, ElapsedEventArgs e)
        {
            if (ItemsView.Count < Details.Items.Count)
            {
                ItemsView = new(Details.Items);
                SelectedItem = Details.Items.LastOrDefault();
            }
            if (Details.Item != null)
            {
                SelectedItem = Details.Item;
                Details.Item = null;
            }
        }
        public ICommand UpdateItemsViewCommand =>
            new RelayCommand((obj) =>
            {
                OnPropertyChanged(nameof(SelectedItem));
            });

        public ICommand ShowSearchCommand =>
            new RelayCommand((obj) =>
            {
                if (!IsSearch)
                    IsSearch = true;
                else if (ItemsView.Count > 0)
                    IsSearch = false;
            });
        public ICommand SearchCommand =>
            new RelayCommand((obj) =>
            {
                var str = obj as string;
                Details.Items.Add(str);
                ItemsView = new(Details.Items);
                SelectedItem = Details.Items.LastOrDefault();
            });
        public ICommand CopyCommand =>
            new RelayCommand((obj) =>
            {
                Clipboard.SetText(SelectedItem.ItemName);
                Message.Enqueue("Item name copied.");
            });
        public ICommand DeleteCommand =>
            new RelayCommand((obj) =>
            {
                var result = MessageBox.Show(
                    "Are you sure you want to delete all items?", "Question",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    IsSearch = true;
                    Details.Items.Clear();
                    ItemsView = new(Details.Items);
                    SelectedItem = new();
                }
            }, (obj) => !ItemsView.Any(x => x.IsBusy) && !ItemsView.Any(x => x.Info.IsBusy));
        public ICommand SwitchCurrencyCommand =>
            new RelayCommand((obj) =>
            {
                var currency = Currencies.Allow.FirstOrDefault(x => x.Name == (string)obj);
                var prices = SelectedItem.Services.ToList();
                if (Details.CurectCurrency.Id != 1)
                {
                    foreach (var price in prices)
                    {
                        price.Price = Currency.ConverterToUsd(price.Price, Details.CurectCurrency.Id);
                        price.Get = Currency.ConverterToUsd(price.Get, Details.CurectCurrency.Id);
                    }
                    SelectedItem.Compare.Get = Currency.ConverterToUsd(SelectedItem.Compare.Get, Details.CurectCurrency.Id);
                    SelectedItem.Compare.Difference = Currency.ConverterToUsd(SelectedItem.Compare.Difference, Details.CurectCurrency.Id);
                }
                foreach (var price in prices)
                {
                    price.Price = Currency.ConverterFromUsd(price.Price, currency.Id);
                    price.Get = Currency.ConverterFromUsd(price.Get, currency.Id);
                }
                SelectedItem.Compare.Get = Currency.ConverterFromUsd(SelectedItem.Compare.Get, currency.Id);
                SelectedItem.Compare.Difference = Currency.ConverterFromUsd(SelectedItem.Compare.Difference, currency.Id);

                Details.CurectCurrency = currency;
                SelectedItem.Services = new(prices);
            }, (obj) => SelectedItem != null && SelectedItem.Services != null && SelectedItem.Services.Any());
        public ICommand OpenItemOutCommand =>
            new RelayCommand((obj) =>
            {
                var item = (DetailService)obj;
                string itemName = SelectedItem.ItemName.Replace("(Holo/Foil)", "(Holo-Foil)");
                string market_hash_name = Uri.EscapeDataString(itemName);
                switch (item.ServiceId)
                {
                    case 0 or 1:
                        Edit.OpenUrl("https://steamcommunity.com/market/listings/730/" + market_hash_name);
                        break;
                    case 2:
                        Edit.OpenCsm(itemName);
                        break;
                    case 3:
                        Clipboard.SetText(SelectedItem.ItemName);
                        Edit.OpenUrl("https://loot.farm/");
                        break;
                    case 4:
                        Edit.OpenUrl("https://buff.163.com/goods/" + ItemsBase.List.FirstOrDefault(x => x.ItemName == SelectedItem.ItemName).Buff.Id + "#tab=buying");
                        break;
                    case 5:
                        Edit.OpenUrl("https://buff.163.com/goods/" + ItemsBase.List.FirstOrDefault(x => x.ItemName == SelectedItem.ItemName).Buff.Id);
                        break;
                }
            });
        public ICommand CompareCommand =>
            new RelayCommand((obj) =>
            {
                if (SelectedItem.Services != null && SelectedItem.Services.Any())
                {
                    SelectedItem.Compare.Get = SelectedItem.Services[SelectedItem.Compare.Service2].Get;
                    SelectedItem.Compare.Precent = Edit.Precent(SelectedItem.Services[SelectedItem.Compare.Service1].Price, SelectedItem.Services[SelectedItem.Compare.Service2].Get);
                    SelectedItem.Compare.Difference = Edit.Difference(SelectedItem.Services[SelectedItem.Compare.Service2].Get, SelectedItem.Services[SelectedItem.Compare.Service1].Price);
                }
            }, (obj) => SelectedItem != null && SelectedItem.ItemName != "Unknown" && SelectedItem.Services != null && SelectedItem.Services.Any());
    }
}