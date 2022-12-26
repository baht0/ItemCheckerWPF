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
        private History _history = new();

        public ICommand SwitchCurrencyCommand =>
            new RelayCommand((obj) =>
            {
                Currency currency = SteamBase.AllowCurrencys.FirstOrDefault(x => x.Name == (string)obj);
                List<DataHistory> items = History.List.ToList();
                if (History.CurectCurrency.Id != 1)
                    foreach (var item in items)
                    {
                        item.Total = Edit.ConverterToUsd(item.Total, History.CurectCurrency.Value);
                        item.Steam = Edit.ConverterToUsd(item.Steam, History.CurectCurrency.Value);
                        item.CsMoney = Edit.ConverterToUsd(item.CsMoney, History.CurectCurrency.Value);
                        item.LootFarm = Edit.ConverterToUsd(item.LootFarm, History.CurectCurrency.Value);
                        item.Buff163 = Edit.ConverterToUsd(item.Buff163, History.CurectCurrency.Value);
                    }
                foreach (var item in items)
                {
                    item.Total = Edit.ConverterFromUsd(item.Total, currency.Value);
                    item.Steam = Edit.ConverterFromUsd(item.Steam, currency.Value);
                    item.CsMoney = Edit.ConverterFromUsd(item.CsMoney, currency.Value);
                    item.LootFarm = Edit.ConverterFromUsd(item.LootFarm, currency.Value);
                    item.Buff163 = Edit.ConverterFromUsd(item.Buff163, currency.Value);
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
                    1 => History.HistoryRecords.Where(x => x.Date >= DateTime.Today.AddDays(-1)).ToList(),
                    2 => History.HistoryRecords.Where(x => x.Date >= DateTime.Today.AddDays(-7)).ToList(),
                    3 => History.HistoryRecords.Where(x => x.Date >= DateTime.Today.AddDays(-30)).ToList(),
                    4 => History.HistoryRecords.Where(x => x.Date >= DateTime.Today.AddMonths(-3)).ToList(),
                    5 => History.HistoryRecords.Where(x => x.Date >= DateTime.Today.AddMonths(-6)).ToList(),
                    6 => History.HistoryRecords.Where(x => x.Date >= DateTime.Today.AddYears(-1)).ToList(),
                    _ => History.HistoryRecords,
                };
                History.List = new(list.OrderByDescending(d => d.Date));
                if (list.Any())
                {
                    History.Result = new DataResult()
                    {
                        AvgBalance = Math.Round(Queryable.Average(History.HistoryRecords.Select(x => x.Total).AsQueryable()), 2),
                        StartBalance = list.FirstOrDefault().Total,
                        EndBalance = list.LastOrDefault().Total,
                    };
                }
            }, (obj) => History.HistoryRecords.Any());
    }
}