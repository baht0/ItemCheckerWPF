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
        public DataGridRecords DataGridRecords
        {
            get
            {
                return _dataGridRecords;
            }
            set
            {
                _dataGridRecords = value;
                OnPropertyChanged();
            }
        }
        DataGridRecords _dataGridRecords = new();

        public ICommand SwitchCurrencyCommand =>
            new RelayCommand((obj) =>
            {
                string currName = (string)obj;
                var current = History.CurrentCurrency;
                History.CurrentCurrency = DataGridRecords.SwitchCurrency(current, currName);

            }, (obj) => DataGridRecords.Items != null);
        public ICommand SwitchIntervalCommand =>
            new RelayCommand((obj) =>
            {
                var index = (int)obj;
                List<DataHistory> list = index switch
                {
                    1 => DataGridRecords.Records.Where(x => x.Date >= DateTime.Today.AddDays(-1)).ToList(),
                    2 => DataGridRecords.Records.Where(x => x.Date >= DateTime.Today.AddDays(-7)).ToList(),
                    3 => DataGridRecords.Records.Where(x => x.Date >= DateTime.Today.AddDays(-30)).ToList(),
                    4 => DataGridRecords.Records.Where(x => x.Date >= DateTime.Today.AddMonths(-3)).ToList(),
                    5 => DataGridRecords.Records.Where(x => x.Date >= DateTime.Today.AddMonths(-6)).ToList(),
                    6 => DataGridRecords.Records.Where(x => x.Date >= DateTime.Today.AddYears(-1)).ToList(),
                    _ => DataGridRecords.Records,
                };
                DataGridRecords.Items = new(list.OrderByDescending(d => d.Date));
                if (list.Any())
                {
                    History.Result = new DataResult()
                    {
                        AvgBalance = Math.Round(Queryable.Average(DataGridRecords.Records.Select(x => x.Total).AsQueryable()), 2),
                        StartBalance = list.FirstOrDefault().Total,
                        EndBalance = list.LastOrDefault().Total,
                    };
                }
            }, (obj) => DataGridRecords.Records.Any());
    }
}