using ItemChecker.Core;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Threading;

namespace ItemChecker.MVVM.Model
{
    public class StartUp : ObservableObject
    {
        public static CancellationTokenSource cts { get; set; } = new();
        public static CancellationToken token { get; set; } = cts.Token;

        public List<Currency> CurrencyList { get; set; } = SteamBase.CurrencyList;
        public Currency SelectedCurrency
        {
            get { return _selectedCurrency; }
            set
            {
                _selectedCurrency = value;
                OnPropertyChanged();
            }
        }
        Currency _selectedCurrency;
        public bool IsCurrency
        {
            get { return _isCurrency; }
            set
            {
                _isCurrency = value;
                OnPropertyChanged();
            }
        }
        bool _isCurrency;

        public bool IsReset
        {
            get { return _isReset; }
            set
            {
                _isReset = value;
                OnPropertyChanged();
            }
        }
        bool _isReset;
        public bool IsUpdate
        {
            get { return _isUpdate; }
            set
            {
                _isUpdate = value;
                OnPropertyChanged();
            }
        }
        bool _isUpdate = DataProjectInfo.IsUpdate;
        public string Version
        {
            get { return _version; }
            set
            {
                _version = value;
                OnPropertyChanged();
            }
        }
        string _version = DataProjectInfo.CurrentVersion;
        public SnackbarMessageQueue Message
        {
            get { return _mess; }
            set
            {
                _mess = value;
                OnPropertyChanged();
            }
        }
        SnackbarMessageQueue _mess = new();
        public Tuple<int, string> Progress
        {
            get { return _progress; }
            set
            {
                _progress = value;
                OnPropertyChanged();
            }
        }
        Tuple<int, string> _progress = Tuple.Create(0, "Starting...");
    }
}
