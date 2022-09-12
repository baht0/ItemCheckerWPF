using ItemChecker.Core;
using ItemChecker.MVVM.Model;
using ItemChecker.Properties;
using ItemChecker.Services;
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
        Timer TimerWindow = new(250);

        public MainInfo MainInfo
        {
            get { return _mainInfo; }
            set
            {
                _mainInfo = value;
                OnPropertyChanged();
            }
        }
        private MainInfo _mainInfo = new();
        #endregion

        public MainViewModel()
        {
            TimerInfo.Elapsed += UpdateInformation;
            TimerWindow.Elapsed += UpdateWindow;
            TimerInfo.Enabled = true;
            TimerWindow.Enabled = true;
        }
        public ICommand ExitCommand =>
            new RelayCommand((obj) =>
            {
                Task.Run(() => {
                    BaseService.BrowserExit();
                }).Wait(5000);
                Application.Current.Shutdown();
            });

        //Notification
        public ICommand ReadNotificationCommand =>
            new RelayCommand((obj) =>
            {
                foreach (var item in Main.Notifications)
                    item.IsRead = true;
            });

        void UpdateInformation(Object sender, ElapsedEventArgs e)
        {
            try
            {
                SteamAccount.GetBalance();
                if (SteamMarket.StatusCommunity != "normal")
                {
                    Main.Notifications.Add(new()
                    {
                        Title = "Steam Status",
                        Message = "There are problems with Steam servers. The program may not work correctly!"
                    });
                }
            }
            catch (Exception ex)
            {
                BaseService.errorLog(ex, false);
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
                BaseService.errorLog(ex, false);
            }
        }
    }
}