using ItemChecker.Core;
using ItemChecker.Support;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace ItemChecker.MVVM.Model
{
    public class History : ObservableObject
    {
        public ObservableCollection<DataHistory> List
        {
            get { return _list; }
            set
            {
                _list = value;
                OnPropertyChanged();
            }
        }
        ObservableCollection<DataHistory> _list = new();
        public DataResult Result
        {
            get
            {
                return _result;
            }
            set
            {
                _result = value;
                OnPropertyChanged();
            }
        }
        private DataResult _result = new();

        public List<string> Interval
        {
            get
            {
                return new()
                {
                    "All Time",
                    "1 Day",
                    "7 Days",
                    "30 Days",
                    "3 Months",
                    "6 Months",
                    "1 Year",
                };
            }
        }

        public int CurrencyId
        {
            get
            {
                return _currencyId;
            }
            set
            {
                _currencyId = value;
                OnPropertyChanged();
            }
        }
        private int _currencyId = 0;
        public List<string> CurrencyList
        {
            get
            {
                return SteamBase.AllowCurrencys.Select(x => x.Name).ToList();
            }
        }
        public static Currency CurectCurrency { get; set; } = SteamBase.AllowCurrencys.FirstOrDefault(x => x.Id == 1);
        public string CurrencySymbolSteam
        {
            get
            {
                return SteamAccount.Currency.Symbol;
            }
        }

        public static Records<DataHistory> HistoryRecords { get; set; } = ReadFile().ToObject<Records<DataHistory>>();
        public static JArray ReadFile()
        {
            string path = ProjectInfo.DocumentPath + "History.json";

            if (!File.Exists(path))
            {
                File.Create(path);
                File.WriteAllText(path, "[]");
                return new();
            }
            return JArray.Parse(File.ReadAllText(path));
        }
    }
    public class DataHistory
    {
        public DateTime Date { get; set; } = DateTime.Today;
        public decimal Total { get; set; }
        public decimal Steam { get; set; }
        public decimal CsMoney { get; set; }
        public decimal LootFarm { get; set; }
        public decimal Buff163 { get; set; }
    }
    public class Records<T> : List<T>
    {
        public new Boolean Add(T item)
        {
            var currentItem = item as DataHistory;
            if (IsAllow(currentItem))
            {
                base.Add(item);
                Save();
                return true;
            }
            return false;
        }
        Boolean IsAllow(DataHistory item)
        {
            var currentList = this as Records<DataHistory>;

            bool isAllow = !currentList.Any(x => x.Date == item.Date);

            if (isAllow)
                return !currentList.Any(x
                    => (int)x.Total == (int)item.Total);

            return false;
        }
        void Save()
        {
            var currentList = this as Records<DataHistory>;
            var json = JArray.FromObject(currentList);

            string path = ProjectInfo.DocumentPath + "History.json";
            if (!File.Exists(path))
                File.Create(path);
            File.WriteAllText(path, json.ToString());
        }
    }
    public class DataResult
    {
        public decimal AvgBalance { get; set; }
        public decimal StartBalance { get; set; }
        public decimal EndBalance { get; set; }
        public decimal Precent
        {
            get
            {
                return Edit.Precent(StartBalance, EndBalance);
            }
        }
        public decimal Difference
        {
            get
            {
                return Edit.Difference(EndBalance, StartBalance);
            }
        }
    }
}
