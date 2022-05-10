﻿using ItemChecker.Core;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using ItemChecker.MVVM.Model;
using System.Windows;
using System.Globalization;
using ItemChecker.Services;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using ItemChecker.Properties;
using ItemChecker.Support;

namespace ItemChecker.MVVM.ViewModel
{
    public class StartUpViewModel : ObservableObject
    {
        #region prop
        IView _view;
        StartUp _startUp = new();
        bool _isLogin = false;
        bool _isFirst = false;
        private SteamSignUp _signUp = new();
        private Settings _settings = new();

        Task startTask { get; set; }
        public bool LoginSuccessful { get; set; }
        public StartUp StartUp
        {
            get { return _startUp; }
            set
            {
                _startUp = value;
                OnPropertyChanged();
            }
        }
        public bool IsLogin
        {
            get { return _isLogin; }
            set
            {
                _isLogin = value;
                OnPropertyChanged();
            }
        }
        public bool IsFirst
        {
            get { return _isFirst; }
            set
            {
                _isFirst = value;
                OnPropertyChanged();
            }
        }
        public SteamSignUp SignUp
        {
            get { return _signUp; }
            set
            {
                _signUp = value;
                OnPropertyChanged();
            }
        }
        public Settings Settings
        {
            get { return _settings; }
            set
            {
                _settings = value;
                OnPropertyChanged();
            }
        }
        #endregion

        public StartUpViewModel(IView view)
        {
            _view = view;
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.CreateSpecificCulture("en-Us");
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.CreateSpecificCulture("en-Us");

            Main.Notifications.Add(new()
            {
                IsRead = true,
                Title = "Welcome!",
                Message = "The program has been launched!"
            });
            startTask = Task.Run(() => StartTask());
        }
        public ICommand ExitCommand =>
            new RelayCommand((obj) =>
            {
                Task.Run(() => {
                    BaseModel.cts.Cancel();
                    StartUp.Progress = Tuple.Create(6, "Exit...");
                    IsFirst = false;
                    IsLogin = false;
                    if (startTask != null)
                        startTask.Wait(4000);
                    if (BaseModel.Browser != null)
                        BaseService.BrowserExit();

                    Application.Current.Dispatcher.Invoke(() => { Application.Current.Shutdown(); });
                });
            });
        void Hide()
        {
            _view.Close();
        }
        void StartTask()
        {
            try
            {
                if (BaseModel.token.IsCancellationRequested)
                    return;
                StartUp.Progress = Tuple.Create(1, "Check Update...");
                ProjectInfoService.AppCheck();
                StartUp.IsUpdate = DataProjectInfo.IsUpdate;
                if (DataProjectInfo.IsUpdate)
                    StartUp.Message.Enqueue($"Update {DataProjectInfo.LatestVersion} is available.");

                if (String.IsNullOrEmpty(SettingsProperties.Default.CurrencyApiKey))
                {
                    StartUp.Progress = Tuple.Create(2, "Initial settings...");
                    IsFirst = true;
                }
                while (IsFirst)
                    Thread.Sleep(500);
                BaseService.GetCurrency();

                if (BaseModel.token.IsCancellationRequested)
                    return;
                StartUp.Progress = Tuple.Create(3, "Signing In...");
                KillProccess();
                IsLogin = SteamAccount.IsLogIn();
                StartUp.Progress = IsLogin ? Tuple.Create(3, "Please, Signing In...") : Tuple.Create(4, "Get Account...");
                while (IsLogin)
                {
                    if (BaseModel.token.IsCancellationRequested)
                        break;
                    IsLogin = SteamAccount.Login();
                    StartUp.Progress = IsLogin ? Tuple.Create(3, "Failed to login...") : Tuple.Create(4, "Get Account...");
                    SignUp.Code2AF = string.Empty;
                }
                if (BaseModel.token.IsCancellationRequested)
                    return;
                SteamAccount.GetSteamAccount();

                if (BaseModel.token.IsCancellationRequested)
                    return;
                StartUp.Progress = Tuple.Create(5, "Preparation...");
                ItemBaseService itemBase = new();
                itemBase.CreateItemsBase();

                if (BaseModel.token.IsCancellationRequested)
                    return;
                StartUp.Progress = Tuple.Create(6, "Check MyOrders...");
                OrderCheckService order = new();
                order.SteamOrders(true);

                if (BaseModel.token.IsCancellationRequested)
                    return;
                LoginSuccessful = true;
                Application.Current.Dispatcher.Invoke(() => { Hide(); });
            }
            catch (Exception exp)
            {
                BaseService.errorLog(exp);
                BaseService.errorMessage(exp);
            }
        }
        void KillProccess()
        {
            if (Process.GetProcessesByName(Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location)).Count() > 1)
            {
                MessageBox.Show(
                    "The program is already running!",
                    "Warning",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Process.GetCurrentProcess().Kill();
            }
            BaseModel.Browser = null;
            //foreach (Process proc in Process.GetProcessesByName("chrome")) proc.Kill();
            foreach (Process proc in Process.GetProcessesByName("chromedriver")) proc.Kill();
            foreach (Process proc in Process.GetProcessesByName("conhost"))
            {
                try
                {
                    proc.Kill();
                }
                catch
                {
                    continue;
                }
            }
        }
        public ICommand UpdateCommand =>
            new RelayCommand((obj) =>
            {
                MessageBoxResult result = MessageBox.Show(
                    $"Want to upgrade from {DataProjectInfo.CurrentVersion} to {DataProjectInfo.LatestVersion}?", "Question",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                    ProjectInfoService.Update();

            }, (obj) => DataProjectInfo.IsUpdate);
        public ICommand LoginCommand =>
            new RelayCommand((obj) =>
            {
                var propertyInfo = obj.GetType().GetProperty("Password");
                SignUp.Password = (string)propertyInfo.GetValue(obj, null);
                if (!String.IsNullOrEmpty(SignUp.Password))
                {
                    SteamSignUp.SignUp = SignUp;
                    SteamSignUp.SignUp.IsLoggedIn = true;
                    StartUp.Progress = Tuple.Create(3, "Signing In...");
                    IsLogin = false;
                }
            }, (obj) => !SteamSignUp.SignUp.IsLoggedIn && !String.IsNullOrEmpty(SignUp.Login) && SignUp.Code2AF.Length == 5);
        public ICommand ContinueCommand =>
            new RelayCommand((obj) =>
            {
                Settings settings = obj as Settings;

                if (Net.Get.Currency(settings.CurrencyApi, "RUB") != 0)
                {
                    SettingsProperties.Default.CurrencyApiKey = settings.CurrencyApi;
                    SettingsProperties.Default.CurrencyId = settings.CurrencyId;
                    SettingsProperties.Default.ServiceId = settings.ServiceId;
                    SettingsProperties.Default.Save();
                    IsFirst = false;
                }
                else
                {
                    MessageBox.Show(
                        "The \"CurrencyApi\" you provided is not working!", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Stop);
                }
            }, (obj) => !String.IsNullOrEmpty(Settings.CurrencyApi));
        public ICommand GetCurrencyApiCommand
        {
            get
            {
                return new RelayCommand((obj) =>
                {
                    Edit.OpenUrl("https://free.currencyconverterapi.com/free-api-key");
                    Edit.OpenUrl("https://openexchangerates.org/signup/free");
                });
            }
        }
    }
}