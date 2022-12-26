using ItemChecker.Core;
using ItemChecker.MVVM.Model;
using ItemChecker.Services;
using ItemChecker.Support;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;

namespace ItemChecker.MVVM.ViewModel
{
    public class DetailsViewModel : ObservableObject
    {
        readonly Timer Timer = new(500);
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

        public DetailItem SelectedItem
        {
            get
            {
                return _selectedItem;
            }
            set
            {
                _selectedItem = value;

                string itemName = _selectedItem.ItemName;
                if (_selectedItem.Price == null || _selectedItem.Prices.Any() || itemName == "New")
                    return;

                _selectedItem.Price.IsBusy = true;
                Task.Run(() =>
                {
                    try
                    {
                        ItemBaseService baseService = new();
                        baseService.UpdateSteamItem(itemName);
                        baseService.UpdateCsmItem(itemName, false);
                        baseService.UpdateLfm();
                        baseService.UpdateBuffItem(itemName);

                        List<DetailItemPrice> prices = new();
                        for (int i = 0; i < Main.Services.Count; i++)
                        {
                            DetailItemPrice price = new();
                            prices.Add(price.Add(i, itemName));
                        }
                        _selectedItem.Prices = new(prices);
                    }
                    catch (Exception ex)
                    {
                        BaseService.errorLog(ex, true);
                    }
                    finally
                    {
                        _selectedItem.Price.IsBusy = false;
                    }
                });
                OnPropertyChanged();
            }
        }
        DetailItem _selectedItem = new();

        public DetailsViewModel()
        {
            Timer.Elapsed += UpdateWindow;
            Timer.Enabled = true;

            Details.ItemsView = new(Details.Items);
            SelectedItem = Details.Items.LastOrDefault();
        }
        void UpdateWindow(Object sender, ElapsedEventArgs e)
        {
            if (Details.Item != null)
            {
                Details.ItemsView = new(Details.Items);
                SelectedItem = Details.Item;
                Details.Item = null;
            }
        }
        public ICommand UpdateItemsViewCommand =>
            new RelayCommand((obj) =>
            {
                OnPropertyChanged("SelectedItem");
            });

        public ICommand ShowSearchCommand =>
            new RelayCommand((obj) =>
            {
                if (!Details.IsSearch)
                    Details.IsSearch = true;
                else if (Details.ItemsView.Count > 0)
                    Details.IsSearch = false;
            });
        public ICommand SearchCommand =>
            new RelayCommand((obj) =>
            {
                var str = obj as string;
                Details.Items.Add(str);
                Details.ItemsView = new(Details.Items);
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
                    Details.Items.Clear();
                    Details.ItemsView = new();
                    Details.IsSearch = true;
                }
            }, (obj) => !SelectedItem.Price.IsBusy && !SelectedItem.Info.IsBusy);
        public ICommand SwitchCurrencyCommand =>
            new RelayCommand((obj) =>
            {
                Currency currency = SteamBase.AllowCurrencys.FirstOrDefault(x => x.Name == (string)obj);
                List<DetailItemPrice> prices = SelectedItem.Prices.ToList();
                if (Details.CurectCurrency.Id != 1)
                {
                    foreach (DetailItemPrice price in prices)
                    {
                        price.Price = Edit.ConverterToUsd(price.Price, Details.CurectCurrency.Value);
                        price.Get = Edit.ConverterToUsd(price.Get, Details.CurectCurrency.Value);
                    }
                    SelectedItem.Compare.Get = Edit.ConverterToUsd(SelectedItem.Compare.Get, Details.CurectCurrency.Value);
                    SelectedItem.Compare.Difference = Edit.ConverterToUsd(SelectedItem.Compare.Difference, Details.CurectCurrency.Value);
                }
                foreach (DetailItemPrice price in prices)
                {
                    price.Price = Edit.ConverterFromUsd(price.Price, currency.Value);
                    price.Get = Edit.ConverterFromUsd(price.Get, currency.Value);
                }
                SelectedItem.Compare.Get = Edit.ConverterFromUsd(SelectedItem.Compare.Get, currency.Value);
                SelectedItem.Compare.Difference = Edit.ConverterFromUsd(SelectedItem.Compare.Difference, currency.Value);

                Details.CurectCurrency = currency;
                SelectedItem.Prices = new(prices);
            }, (obj) => SelectedItem.Prices != null && SelectedItem.Prices.Any());
        public ICommand OpenItemOutCommand =>
            new RelayCommand((obj) =>
            {
                var item = (DetailItemPrice)obj;
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
                        Edit.OpenUrl("https://buff.163.com/goods/" + SteamBase.ItemList.FirstOrDefault(x => x.ItemName == SelectedItem.ItemName).Buff.Id + "#tab=buying");
                        break;
                    case 5:
                        Edit.OpenUrl("https://buff.163.com/goods/" + SteamBase.ItemList.FirstOrDefault(x => x.ItemName == SelectedItem.ItemName).Buff.Id);
                        break;
                }
            });
        public ICommand CompareCommand =>
            new RelayCommand((obj) =>
            {
                if (SelectedItem.Prices != null && SelectedItem.Prices.Any())
                {
                    SelectedItem.Compare.Get = SelectedItem.Prices[SelectedItem.Compare.Service2].Get;
                    SelectedItem.Compare.Precent = Edit.Precent(SelectedItem.Prices[SelectedItem.Compare.Service1].Price, SelectedItem.Prices[SelectedItem.Compare.Service2].Get);
                    SelectedItem.Compare.Difference = Edit.Difference(SelectedItem.Prices[SelectedItem.Compare.Service2].Get, SelectedItem.Prices[SelectedItem.Compare.Service1].Price);
                }
            }, (obj) => SelectedItem.Prices != null && SelectedItem.Prices.Any());
    }
}