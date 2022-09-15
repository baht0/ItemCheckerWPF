using ItemChecker.Core;
using ItemChecker.MVVM.Model;
using ItemChecker.Services;
using ItemChecker.Support;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ItemChecker.MVVM.ViewModel
{
    public class DetailsViewModel : ObservableObject
    {
        SnackbarMessageQueue _message = new();
        public SnackbarMessageQueue Message
        {
            get { return _message; }
            set
            {
                _message = value;
                OnPropertyChanged();
            }
        }

        Details _details = new();
        DetailsCompare _detailsCompare = new();
        DetailsInfo _detailsInfo = new();
        DetailsPrice _selectedPrice = new();
        public Details Details
        {
            get { return _details; }
            set
            {
                _details = value;
                OnPropertyChanged();
            }
        }
        public DetailsCompare DetailsCompare
        {
            get { return _detailsCompare; }
            set
            {
                _detailsCompare = value;
                OnPropertyChanged();
            }
        }
        public DetailsInfo DetailsInfo
        {
            get { return _detailsInfo; }
            set
            {
                _detailsInfo = value;
                OnPropertyChanged();
            }
        }
        public DetailsPrice SelectedPrice
        {
            get { return _selectedPrice; }
            set
            {
                _selectedPrice = value;
                if (value == null)
                    return;

                ItemBaseService baseService = new();
                Details.Loading = true;
                string itemName = Details.ItemName;
                if (value.ServiceId <= 1)
                {
                    DetailsInfo.SteamInfo.IsShow = true;
                    DetailsInfo.CsmInfo.IsShow = false;
                    DetailsInfo.LfmInfo.IsShow = false;
                    DetailsInfo.BuffInfo.IsShow = false;

                    var data = SteamBase.ItemList.FirstOrDefault(x => x.ItemName == itemName).Steam;

                    data.LowestSellOrder = Edit.ConverterFromUsd(data.LowestSellOrder, ParserTable.CurectCurrency.Value);
                    data.HighestBuyOrder = Edit.ConverterFromUsd(data.HighestBuyOrder, ParserTable.CurectCurrency.Value);

                    baseService.UpdateSteamItemHistory(itemName);
                    DetailsInfo.SteamInfo.LastSale = data.History.FirstOrDefault().Date;
                    List<decimal> last30 = data.History.Where(x => x.Date > DateTime.Today.AddDays(-30)).Select(x => x.Price).ToList();
                    List<decimal> last60 = data.History.Where(x => x.Date > DateTime.Today.AddDays(-60)).Select(x => x.Price).ToList();
                    DetailsInfo.SteamInfo.Count = Tuple.Create(last30.Count, last60.Count);
                    decimal avg30 = last30.Any() ? Math.Round(Queryable.Average(last30.AsQueryable()), 2) : 0;
                    decimal avg60 = last60.Any() ? Math.Round(Queryable.Average(last60.AsQueryable()), 2) : 0;
                    DetailsInfo.SteamInfo.Avg = Tuple.Create(avg30, avg60);

                    DetailsInfo.SteamInfo.Item = data;
                    Details.Loading = false;
                }
                else if (value.ServiceId == 2)
                {
                    DetailsInfo.SteamInfo.IsShow = false;
                    DetailsInfo.CsmInfo.IsShow = true;
                    DetailsInfo.LfmInfo.IsShow = false;
                    DetailsInfo.BuffInfo.IsShow = false;

                    baseService.UpdateInventoryCsmItem(itemName);
                    DetailsInfo.CsmInfo.Item = SteamBase.ItemList.FirstOrDefault(x => x.ItemName == itemName).Csm;
                    DetailsInfo.CsmInfo.CurrentItem = DetailsInfo.CsmInfo.Item.Inventory.FirstOrDefault();
                    DetailsInfo.CsmInfo.MaxValueSlide = DetailsInfo.CsmInfo.Item.Inventory.Count;
                    DetailsInfo.CsmInfo.ValueSlide = DetailsInfo.CsmInfo.Item.Inventory.Any() ? 1 : 0;
                    Details.Loading = false;
                }
                else if (value.ServiceId == 3)
                {
                    DetailsInfo.SteamInfo.IsShow = false;
                    DetailsInfo.CsmInfo.IsShow = false;
                    DetailsInfo.LfmInfo.IsShow = true;
                    DetailsInfo.BuffInfo.IsShow = false;

                    DetailsInfo.LfmInfo.Item = SteamBase.ItemList.FirstOrDefault(x => x.ItemName == itemName).Lfm;
                    Details.Loading = false;
                }
                else if (value.ServiceId == 4 || value.ServiceId == 5)
                {
                    DetailsInfo.SteamInfo.IsShow = false;
                    DetailsInfo.CsmInfo.IsShow = false;
                    DetailsInfo.LfmInfo.IsShow = false;
                    DetailsInfo.BuffInfo.IsShow = true;

                    baseService.UpdateBuffItemHistory(itemName);
                    var data = SteamBase.ItemList.FirstOrDefault(x => x.ItemName == itemName).Buff;
                    DetailsInfo.BuffInfo.LastSale = data.History.FirstOrDefault().Date;
                    DetailsInfo.BuffInfo.Item = data;
                    Details.Loading = false;
                }
                OnPropertyChanged();
            }
        }

        public DetailsViewModel(string itemName)
        {
            Details.ItemName = itemName;
            Details.Loading = true;
            if (!String.IsNullOrEmpty(itemName) && itemName != "Unknown")
            {
                Task.Run(() =>
                {
                    try
                    {
                        ItemBaseService baseService = new();
                        baseService.UpdateSteamItem(itemName);
                        baseService.UpdateCsm();
                        baseService.UpdateLfm();
                        baseService.UpdateBuffItem(itemName);

                        Details.Prices = new();
                        List<DetailsPrice> prices = new();
                        for (int i = 0; i < Main.Services.Count; i++)
                        {
                            DetailsPrice price = new();
                            prices.Add(price.Add(i, itemName));
                        }
                        Details.Prices = new(prices);
                        Details.Loading = false;
                    }
                    catch (Exception ex)
                    {
                        BaseService.errorLog(ex, true);
                    }
                });
            }
        }
        public ICommand CopyCommand =>
            new RelayCommand((obj) =>
            {
                Clipboard.SetText(Details.ItemName);
            });
        public ICommand SwitchCurrencyCommand =>
            new RelayCommand((obj) =>
            {
                Currency currency = SteamBase.CurrencyList.FirstOrDefault(x => x.Name == (string)obj);
                List<DetailsPrice> prices = Details.Prices.ToList();
                if (Details.CurectCurrency.Id != 1)
                {
                    foreach (DetailsPrice price in prices)
                    {
                        price.Price = Edit.ConverterToUsd(price.Price, Details.CurectCurrency.Value);
                        price.Get = Edit.ConverterToUsd(price.Get, Details.CurectCurrency.Value);
                    }
                    DetailsCompare.Get = Edit.ConverterToUsd(DetailsCompare.Get, Details.CurectCurrency.Value);
                    DetailsCompare.Difference = Edit.ConverterToUsd(DetailsCompare.Difference, Details.CurectCurrency.Value);
                }
                foreach (DetailsPrice price in prices)
                {
                    price.Price = Edit.ConverterFromUsd(price.Price, currency.Value);
                    price.Get = Edit.ConverterFromUsd(price.Get, currency.Value);
                }
                DetailsCompare.Get = Edit.ConverterFromUsd(DetailsCompare.Get, currency.Value);
                DetailsCompare.Difference = Edit.ConverterFromUsd(DetailsCompare.Difference, currency.Value);

                Details.CurrencySymbol = currency.Symbol;
                Details.CurectCurrency = currency;
                Details.Prices = new(prices);
            }, (obj) => Details.Prices != null);
        public ICommand OpenItemOutCommand =>
            new RelayCommand((obj) =>
            {
                var item = (DetailsPrice)obj;
                string itemName = Details.ItemName.Replace("(Holo/Foil)", "(Holo-Foil)");
                string market_hash_name = Edit.EncodeMarketHashName(itemName);
                switch (item.ServiceId)
                {
                    case 0 or 1:
                        Edit.OpenUrl("https://steamcommunity.com/market/listings/730/" + market_hash_name);
                        break;
                    case 2:
                        Edit.OpenCsm(market_hash_name);
                        break;
                    case 3:
                        Clipboard.SetText(Details.ItemName);
                        Edit.OpenUrl("https://loot.farm/");
                        break;
                    case 4:
                        Edit.OpenUrl("https://buff.163.com/goods/" + SteamBase.ItemList.FirstOrDefault(x => x.ItemName == Details.ItemName).Buff.Id + "#tab=buying");
                        break;
                    case 5:
                        Edit.OpenUrl("https://buff.163.com/goods/" + SteamBase.ItemList.FirstOrDefault(x => x.ItemName == Details.ItemName).Buff.Id);
                        break;
                }
            });
        public ICommand CompareCommand =>
            new RelayCommand((obj) =>
            {
                if (Details.Prices != null && Details.Prices.Any())
                {
                    DetailsCompare.Get = Details.Prices[DetailsCompare.Service2].Get;
                    DetailsCompare.Precent = Edit.Precent(Details.Prices[DetailsCompare.Service1].Price, Details.Prices[DetailsCompare.Service2].Get);
                    DetailsCompare.Difference = Edit.Difference(Details.Prices[DetailsCompare.Service2].Get, Details.Prices[DetailsCompare.Service1].Price);
                }
            });
    }
}
