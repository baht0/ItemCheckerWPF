using ItemChecker.Core;
using ItemChecker.MVVM.Model;
using ItemChecker.Support;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace ItemChecker.MVVM.ViewModel
{
    public class HistoryViewModel : ObservableObject
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
        public History History
        {
            get
            {
                return _history;
            }
            set
            {
                _history = value;
                OnPropertyChanged();
            }
        }
        History _history = new();

        public ICommand SwitchCurrencyCommand =>
            new RelayCommand((obj) =>
            {
                var currency = Currencies.Allow.FirstOrDefault(x => x.Name == (string)obj);
                List<DataHistory> items = History.List.ToList();
                if (History.CurectCurrency.Id != 1)
                    foreach (var item in items)
                    {
                        item.Total = Currency.ConverterToUsd(item.Total, History.CurectCurrency.Id);
                        item.Steam = Currency.ConverterToUsd(item.Steam, History.CurectCurrency.Id);
                        item.CsMoney = Currency.ConverterToUsd(item.CsMoney, History.CurectCurrency.Id);
                        item.LootFarm = Currency.ConverterToUsd(item.LootFarm, History.CurectCurrency.Id);
                        item.Buff163 = Currency.ConverterToUsd(item.Buff163, History.CurectCurrency.Id);
                    }
                foreach (var item in items)
                {
                    item.Total = Currency.ConverterFromUsd(item.Total, currency.Id);
                    item.Steam = Currency.ConverterFromUsd(item.Steam, currency.Id);
                    item.CsMoney = Currency.ConverterFromUsd(item.CsMoney, currency.Id);
                    item.LootFarm = Currency.ConverterFromUsd(item.LootFarm, currency.Id);
                    item.Buff163 = Currency.ConverterFromUsd(item.Buff163, currency.Id);
                }
                History.CurectCurrency = currency;
                History.List = new(items.OrderByDescending(d => d.Date));

            }, (obj) => History.List != null);
        public ICommand SwitchIntervalCommand =>
            new RelayCommand((obj) =>
            {
                var index = (int)obj;
                List<DataHistory> list = index switch
                {
                    1 => History.Records.Where(x => x.Date >= DateTime.Today.AddDays(-1)).ToList(),
                    2 => History.Records.Where(x => x.Date >= DateTime.Today.AddDays(-7)).ToList(),
                    3 => History.Records.Where(x => x.Date >= DateTime.Today.AddDays(-30)).ToList(),
                    4 => History.Records.Where(x => x.Date >= DateTime.Today.AddMonths(-3)).ToList(),
                    5 => History.Records.Where(x => x.Date >= DateTime.Today.AddMonths(-6)).ToList(),
                    6 => History.Records.Where(x => x.Date >= DateTime.Today.AddYears(-1)).ToList(),
                    _ => History.Records,
                };
                History.List = new(list.OrderByDescending(d => d.Date));
                if (list.Any())
                {
                    History.Result = new DataResult()
                    {
                        AvgBalance = Math.Round(Queryable.Average(History.Records.Select(x => x.Total).AsQueryable()), 2),
                        StartBalance = list.FirstOrDefault().Total,
                        EndBalance = list.LastOrDefault().Total,
                    };
                }
            }, (obj) => History.Records.Any());
    }
}