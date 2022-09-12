using ItemChecker.Core;
using ItemChecker.MVVM.Model;
using ItemChecker.Support;
using LiveCharts;
using LiveCharts.Wpf;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ItemChecker.MVVM.ViewModel
{
    public class TradesViewModel : ObservableObject
    {
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
        public Trades Trades
        {
            get
            {
                return _trades;
            }
            set
            {
                _trades = value;
                OnPropertyChanged();
            }
        }
        private Trades _trades = new();
        public Analysis Analysis
        {
            get
            {
                return _analysis;
            }
            set
            {
                _analysis = value;
                OnPropertyChanged();
            }
        }
        private Analysis _analysis = new();

        public TradesViewModel()
        {
            Trades.List = new(Trades.MyTrades.OrderByDescending(d => d.Date));
            for (int i = 0; i < Trades.Services.Count; i++)
            {
                decimal balance = i == 0 ? Analysis.SteamBalanceUsd : 0;
                Analysis.Services.Add(new()
                {
                    Service = Trades.Services[i],
                    ServiceId = i,
                    Balance = balance,
                });
            }
            Analysis.SelectedService = Analysis.Services.FirstOrDefault();
        }
        public ICommand OpenItemOutCommand =>
            new RelayCommand((obj) =>
            {
                var item = Trades.SelectedItem;
                string itemName = item.ItemName.Replace("(Holo/Foil)", "(Holo-Foil)");
                string market_hash_name = Edit.EncodeMarketHashName(itemName);
                switch ((Int32)obj)
                {
                    case 1:
                        {
                            switch (item.ServiceId)
                            {
                                case 0 or 1:
                                    Edit.OpenUrl("https://steamcommunity.com/market/listings/730/" + market_hash_name);
                                    break;
                                case 2:
                                    Edit.OpenCsm(market_hash_name);
                                    break;
                                case 3:
                                    Edit.OpenUrl("https://loot.farm/");
                                    break;
                                case 4:
                                    Edit.OpenUrl("https://buff.163.com/goods/" + SteamBase.ItemList.FirstOrDefault(x => x.ItemName == item.ItemName).Buff.Id + "#tab=buying");
                                    break;
                                case 5:
                                    Edit.OpenUrl("https://buff.163.com/goods/" + SteamBase.ItemList.FirstOrDefault(x => x.ItemName == item.ItemName).Buff.Id);
                                    break;
                            }
                            break;
                        }
                    case 3 or 4:
                        switch (item.ServiceId)
                        {
                            case 0 or 1:
                                Edit.OpenUrl("https://steamcommunity.com/market/");
                                break;
                            case 2:
                                Edit.OpenUrl("https://cs.money/transactions/");
                                break;
                            case 3:
                                Edit.OpenUrl("https://loot.farm/en/account.html");
                                break;
                            case 4 or 5:
                                switch (item.Action)
                                {
                                    case 0:
                                        Edit.OpenUrl("https://buff.163.com/market/buy_order/history?game=csgo");
                                        break;
                                    case 1:
                                        Edit.OpenUrl("https://buff.163.com/market/sell_order/history?game=csgo");
                                        break;
                                }
                                break;
                        }
                        break;
                    default:
                        Clipboard.SetText(itemName);
                        break;
                }
            });
        public ICommand SwitchCurrencyCommand =>
            new RelayCommand((obj) =>
            {
                Currency currency = SteamBase.CurrencyList.FirstOrDefault(x => x.Name == (string)obj);
                List<DataTrade> trades = Trades.List.ToList();
                if (Trades.CurectCurrency.Id != 1)
                    foreach (DataTrade trade in trades)
                        trade.Price = Edit.ConverterToUsd(trade.Price, Trades.CurectCurrency.Value);
                foreach (DataTrade price in trades)
                    price.Price = Edit.ConverterFromUsd(price.Price, currency.Value);

                Trades.CurectCurrency = currency;
                Trades.List = new(trades.OrderByDescending(d => d.Date));
            }, (obj) => Trades.List != null);
        //trades
        public ICommand AddCommand =>
            new RelayCommand((obj) =>
            {
                var item = obj as DataTrade;
                if (item != null)
                {
                    string message = "Not successful. Conditions not met.";
                    if (Trades.MyTrades.Add(item))
                    {
                        message = $"{item.ItemName}\nItem has been added.";
                        Trades.AddItem = new();
                        Trades.List = new(Trades.MyTrades.OrderByDescending(d => d.Date));
                    }
                    Message.Enqueue(message);
                }
            }, (obj) => (!String.IsNullOrEmpty(Trades.AddItem.ItemName)) && Trades.AddItem.Count > 0 && Trades.AddItem.Price != 0);
        public ICommand ClearCommand =>
            new RelayCommand((obj) =>
            {
                MessageBoxResult result = MessageBox.Show(
                    "Are you sure you want to clear the list?",
                    "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.No)
                    return;

                Trades.MyTrades.Clear();
                Trades.List = new(Trades.MyTrades);

                Message.Enqueue("The list has been cleared.");
            });
        public ICommand RemoveCommand =>
            new RelayCommand((obj) =>
            {
                var item = obj as DataTrade;
                MessageBoxResult result = MessageBox.Show(
                    $"Are you sure you want to remove item?\n{item.ItemName}",
                    "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    Trades.MyTrades.Remove(item);
                    Trades.List = new(Trades.MyTrades.OrderByDescending(d => d.Date));
                    Message.Enqueue($"{item.ItemName}\nItem has been removed.");
                }
            }, (obj) => Trades.SelectedItem != null);
        //analysis
        public ICommand StartBalanceCommand =>
            new RelayCommand((obj) =>
            {
                if (decimal.TryParse(obj.ToString(), out decimal balance))
                    StartBalance(balance);
            });
        void StartBalance(decimal balance)
        {
            var data = Analysis.SelectedService;
            var service = Analysis.Services.FirstOrDefault(x => x.ServiceId == data.ServiceId);
            balance = data.ServiceId != 0 ? balance : service.Balance;


            decimal startBalance = Trades.MyTrades.Where(x => x.Action == 0 && x.ServiceId == data.ServiceId && Analysis.DateFrom <= x.Date && x.Date <= Analysis.DateTo).Select(x => x.Price).Sum()
                                   + balance -
                                   Trades.MyTrades.Where(x => x.Action == 1 && x.ServiceId == data.ServiceId && Analysis.DateFrom <= x.Date && x.Date <= Analysis.DateTo).Select(x => x.Price).Sum();

            service.StartBalance = startBalance > 0 ? startBalance : 0;
            service.Balance = balance;
            Analysis.SelectedService = service;
        }
        public ICommand ApplyCommand =>
            new RelayCommand((obj) =>
            {
                if (!Trades.MyTrades.Any())
                    return;

                Analysis.SelectedService = Analysis.Services.FirstOrDefault(x => x.ServiceId == 0);

                Analysis.Balances.Clear();
                var minDate = Trades.MyTrades.Select(x => x.Date).Min() < Analysis.DateFrom ? Analysis.DateFrom : Trades.MyTrades.Select(x => x.Date).Min();
                Analysis.Balances.Add(new()
                {
                    Balance = Analysis.TotalStartBalance,
                    Date = minDate.AddDays(-1),
                });
                foreach (var date in Trades.MyTrades.OrderBy(d => d.Date).GroupBy(x => x.Date))
                {
                    if (Analysis.DateFrom <= date.Key && date.Key <= Analysis.DateTo)
                    {
                        decimal withdraw = date.Where(x => x.Action == 0).Select(x => x.Price).Sum();
                        decimal deposite = date.Where(x => x.Action == 1).Select(x => x.Price).Sum();
                        decimal profit = deposite - withdraw;

                        decimal lastBalance = Analysis.Balances.LastOrDefault().Balance;

                        Analysis.Balances.Add(new()
                        {
                            Balance = lastBalance + profit,
                            Profit = profit,
                            Date = date.Key
                        });
                    }
                }
                CreateResult();

            }, (obj) => Analysis.DateFrom < Analysis.DateTo && Trades.MyTrades.Any());
        void CreateResult()
        {
            Analysis.Series.Clear();
            Analysis.Series.Add(new LineSeries()
            {
                Title = "Balance",
                Values = new ChartValues<decimal>(Analysis.Balances.Select(x => x.Balance)),
                LineSmoothness = 0,
                PointGeometrySize = 12,
            });
            Analysis.Series.Add(new LineSeries()
            {
                Title = "Change",
                Values = new ChartValues<decimal>(Analysis.Balances.Select(x => x.Profit)),
                LineSmoothness = 0,
                IsHitTestVisible = false,
            });
            Analysis.Labels = Analysis.Balances.Select(x => x.Date.ToString("dd MMM yy")).ToArray();

            Analysis.Result = new()
            {
                Count = Analysis.Balances.Count,
                IsLiveChart = true,
                AvgBalance = Math.Round(Analysis.Balances.Select(x => x.Balance).Average(), 2),
                StartBalance = Analysis.Balances.FirstOrDefault().Balance,
                EndBalance = Analysis.Balances.LastOrDefault().Balance,
            };
        }
    }
}
