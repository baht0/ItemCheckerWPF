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
                return Currencies.Allow.Select(x => x.Name).ToList();
            }
        }
        public static DataCurrency CurectCurrency { get; set; } = Currencies.Allow.FirstOrDefault();
        public string CurrencySymbolSteam
        {
            get
            {
                return SteamAccount.Currency.Symbol;
            }
        }

        public static Records<DataHistory> Records { get; set; } = ReadFile().ToObject<Records<DataHistory>>();
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
            if (IsAllow(item))
            {
                base.Add(item);
                Save();
                return true;
            }
            return false;
        }
        Boolean IsAllow(T item)
        {
            var currentItem = item as DataHistory;
            var currentList = this as Records<DataHistory>;

            var haveItem = currentList.FirstOrDefault(x => x.Date == currentItem.Date);
            if (haveItem != null)
            {
                currentList.Remove(haveItem);
                return true;
            }
            else if ((int)currentList.LastOrDefault().Total != (int)currentItem.Total)
                return true;
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
