﻿using ItemChecker.Core;
using ItemChecker.Support;
using LiveCharts;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace ItemChecker.MVVM.Model
{
    public class Trades : ObservableObject
    {
        public ObservableCollection<DataTrade> List
        {
            get { return _list; }
            set
            {
                _list = value;
                OnPropertyChanged();
            }
        }
        ObservableCollection<DataTrade> _list = new();
        public DataTrade SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                _selectedItem = value;
                OnPropertyChanged();
            }
        }
        DataTrade _selectedItem = new();
        public DataTrade AddItem
        {
            get { return _addItem; }
            set
            {
                _addItem = value;
                OnPropertyChanged();
            }
        }
        DataTrade _addItem = new();

        private int _currencyId = 0;
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
        public List<string> CurrencyList
        {
            get
            {
                return SteamBase.CurrencyList.Select(x => x.Name).ToList();
            }
        }
        public static Currency CurectCurrency { get; set; } = SteamBase.CurrencyList.FirstOrDefault(x => x.Id == 1);
        public string CurrencySymbolSteam
        {
            get
            {
                return SteamBase.CurrencyList.FirstOrDefault(x => x.Id == SteamAccount.CurrencyId).Symbol;
            }
        }
        public List<string> Services
        {
            get
            {
                return Main.ServicesShort;
            }
        }
        public List<string> Actions
        {
            get
            {
                return new()
                {
                    "Withdraw",
                    "Deposit",
                };
            }
        }

        public static Trades<DataTrade> MyTrades { get; set; } = ReadFile().ToObject<Trades<DataTrade>>();
        public static JArray ReadFile()
        {
            string path = ProjectInfo.DocumentPath + "Trades.json";

            if (!File.Exists(path))
            {
                File.Create(path);
                File.WriteAllText(path, "[]");
                return new();
            }
            return JArray.Parse(File.ReadAllText(path));
        }
    }
    public class DataTrade
    {
        public string ItemName { get; set; } = string.Empty;
        public int ServiceId { get; set; }
        public int Action { get; set; }
        public int Count { get; set; } = 1;
        public decimal Price { get; set; }
        public DateTime Date { get; set; } = DateTime.Today;
    }
    public class Trades<T> : List<T>
    {
        public new Boolean Add(T item)
        {
            var currentItem = item as DataTrade;
            if (IsAllow(currentItem))
            {
                base.Add(item);
                Save();
                return true;
            }
            return false;
        }
        Boolean IsAllow(DataTrade item)
        {
            var currentList = this as Trades<DataTrade>;
            var steamBase = SteamBase.ItemList.FirstOrDefault(x => x.ItemName == item.ItemName);

            bool isAllow = item != null && steamBase != null;

            if (isAllow)
                isAllow = !currentList.Any(x => x.ItemName == item.ItemName
                && x.ServiceId == item.ServiceId
                && x.Action == item.Action
                && x.Count == item.Count
                && x.Date == item.Date);

            return isAllow;
        }
        public new void Remove(T item)
        {
            base.Remove(item);
            Save();
        }
        public new void Clear()
        {
            base.Clear();
            Save();
        }
        void Save()
        {
            var json = JArray.FromObject(Trades.MyTrades);

            string path = ProjectInfo.DocumentPath + "Trades.json";
            if (!File.Exists(path))
                File.Create(path);
            File.WriteAllText(path, json.ToString());
        }
    }

    public class Analysis : ObservableObject
    {
        public decimal SteamBalanceUsd
        {
            get
            {
                return Edit.ConverterToUsd(SteamAccount.Balance, SteamBase.CurrencyList.FirstOrDefault(x => x.Id == SteamAccount.CurrencyId).Value);
            }
        }

        public List<DataServiceAnalysis> Services
        {
            get { return _services; }
            set
            {
                _services = value;
                OnPropertyChanged();
            }
        }
        List<DataServiceAnalysis> _services = new();
        public DataServiceAnalysis SelectedService
        {
            get { return _selectedService; }
            set
            {
                _selectedService = value;
                OnPropertyChanged();
            }
        }
        DataServiceAnalysis _selectedService = new();

        public List<string> Dateinterval
        {
            get { return _dateinterval; }
            set
            {
                _dateinterval = value;
                OnPropertyChanged();
            }
        }
        List<string> _dateinterval = new()
        {
            "7 Days",
            "30 Days",
            "3 Month",
            "6 Month",
            "All Time",
            "Custom"
        };
        public DateTime DateFrom
        {
            get { return _dateFrom; }
            set
            {
                _dateFrom = value;
                OnPropertyChanged();
            }
        }
        DateTime _dateFrom = DateTime.Today.AddDays(-7);
        public DateTime DateTo
        {
            get { return _dateTo; }
            set
            {
                _dateTo = value;
                OnPropertyChanged();
            }
        }
        DateTime _dateTo = DateTime.Today;

        public DataAnalysisResult Result
        {
            get { return _result; }
            set
            {
                _result = value;
                OnPropertyChanged();
            }
        }
        DataAnalysisResult _result = new();

        public List<DataTradeAnalysis> Balances
        {
            get { return _balances; }
            set
            {
                _balances = value;
                OnPropertyChanged();
            }
        }
        List<DataTradeAnalysis> _balances = new();
        public SeriesCollection Series
        {
            get { return _series; }
            set
            {
                _series = value;
                OnPropertyChanged();
            }
        }
        SeriesCollection _series = new();
        public string[] Labels
        {
            get { return _labels; }
            set
            {
                _labels = value;
                OnPropertyChanged();
            }
        }
        string[] _labels;

        public decimal TotalStartBalance
        {
            get
            {
                return _services.Select(x => x.StartBalance).Sum();
            }
        }
    }
    public class DataServiceAnalysis
    {
        public string Service { get; set; }
        public int ServiceId { get; set; }
        public decimal Balance { get; set; }
        public decimal StartBalance { get; set; }
    }
    public class DataTradeAnalysis
    {
        string _date = string.Empty;
        public decimal Profit { get; set; }
        public decimal Balance { get; set; }
        public DateTime Date { get; set; }
    }
    public class DataAnalysisResult
    {
        public int Count { get; set; }
        public bool IsLiveChart { get; set; }
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
