﻿using ItemChecker.Core;
using MaterialDesignThemes.Wpf;
using System;
using System.Threading;

namespace ItemChecker.MVVM.Model
{
    public class StartUp : ObservableObject
    {
        public static CancellationTokenSource cts { get; set; } = new();
        public static CancellationToken token { get; set; } = cts.Token;

        bool _isUpdate = DataProjectInfo.IsUpdate;
        string _version = DataProjectInfo.CurrentVersion;
        SnackbarMessageQueue _mess = new();
        Tuple<int, string> _progress = Tuple.Create(0, "Starting...");

        public bool IsUpdate
        {
            get { return _isUpdate; }
            set
            {
                _isUpdate = value;
                OnPropertyChanged();
            }
        }
        public string Version
        {
            get { return _version; }
            set
            {
                _version = value;
                OnPropertyChanged();
            }
        }
        public SnackbarMessageQueue Message
        {
            get { return _mess; }
            set
            {
                _mess = value;
                OnPropertyChanged();
            }
        }
        public Tuple<int, string> Progress
        {
            get { return _progress; }
            set
            {
                _progress = value;
                OnPropertyChanged();
            }
        }
    }
}
