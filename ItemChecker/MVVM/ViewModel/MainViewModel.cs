using ItemChecker.Core;
using ItemChecker.MVVM.Model;
using ItemChecker.Properties;
using ItemChecker.Services;
using ItemChecker.Support;
using System;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;

namespace ItemChecker.MVVM.ViewModel
{
    public class MainViewModel : ObservableObject
    {
        #region prop
        Timer TimerInfo = new(TimeSpan.FromMinutes(15).TotalMilliseconds);
        Timer TimerWindow = new(500);

        private MainInfo _mainInfo = new();
        private Calculator _calculator = new();
        private DataNotification _notification = new();

        public MainInfo MainInfo
        {
            get { return _mainInfo; }
            set
            {
                _mainInfo = value;
                OnPropertyChanged();
            }
        }
        public Calculator Calculator
        {
            get { return _calculator; }
            set
            {
                _calculator = value;
                OnPropertyChanged();
            }
        }
        public DataNotification Notification
        {
            get { return _notification; }
            set
            {
                value.IsRead = true;
                _notification = value;
                OnPropertyChanged();
            }
        }
        #endregion

        public MainViewModel()
        {
            TimerInfo.Elapsed += UpdateInformation;
            TimerWindow.Elapsed += UpdateWindow;
            TimerInfo.Enabled = true;
            TimerWindow.Enabled = true;
            
            HomeProperties.Default.FavoriteList = HomeProperties.Default.FavoriteList ?? (new());
        }
        public ICommand OpenFolderCommand =>
            new RelayCommand((obj) =>
            {
                Edit.openUrl(BaseModel.AppPath);
            });
        public ICommand ExitCommand =>
            new RelayCommand((obj) =>
            {
                Task.Run(() => {
                    BaseModel.cts.Cancel();
                    BaseService.BrowserExit();
                }).Wait(5000);
                Application.Current.Shutdown();
            });

        //calculator
        public ICommand ChangeCommand =>
            new RelayCommand((obj) =>
            {
                Calculator config = (Calculator)obj;
                Calculator calculator = new();
                calculator.Service = config.Service;
                calculator.Price1 = config.Price1;
                calculator.Price2 = config.Price2;
                calculator.Precent = config.Precent;
                calculator.Difference = config.Difference;
                calculator.Result = config.Result;

                calculator.Currency1 = config.Currency2;
                calculator.Currency2 = config.Currency1;
                calculator.Value = config.Converted;
                calculator.Converted = config.Value;
                Calculator = calculator;
            });
        void UpdateInformation(Object sender, ElapsedEventArgs e)
        {
            try
            {
                BaseService.StatusSteam();
                if (BaseModel.StatusCommunity != "normal")
                {
                    Main.Notifications.Add(new()
                    {
                        Title = "Steam Status",
                        Message = "There are problems with Steam servers. The program may not work correctly!"
                    });
                }
                BaseService.GetCurrency();
                if (!BaseModel.IsParsing && !BaseModel.IsWorking)
                {
                    ItemBaseService get = new();
                    get.CreateItemsBase();
                }
                SteamAccount.GetSteamBalance();
                if (SteamAccount.BalanceStartUp > SteamAccount.Balance)
                {
                    Main.Notifications.Add(new()
                    {
                        Title = "Balance",
                        Message = $"Your balance has decreased\n-{SteamAccount.BalanceStartUp - SteamAccount.Balance}."
                    });
                    SteamAccount.BalanceStartUp = SteamAccount.Balance;
                }
                else if (SteamAccount.BalanceStartUp < SteamAccount.Balance)
                {
                    Main.Notifications.Add(new()
                    {
                        Title = "Balance",
                        Message = $"Your balance has increased\n+{SteamAccount.Balance - SteamAccount.BalanceStartUp}."
                    });
                    SteamAccount.BalanceStartUp = SteamAccount.Balance;
                }
            }
            catch (Exception ex)
            {
                BaseService.errorLog(ex);
            }
        }
        void UpdateWindow(Object sender, ElapsedEventArgs e)
        {
            try
            {
                MainInfo = new();
                if (SettingsProperties.Default.SetHours)
                {
                    App app = (App)Application.Current;
                    app.AutoChangeTheme();
                }
            }
            catch (Exception ex)
            {
                BaseService.errorLog(ex);
            }
        }
    }
}