using System;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using ItemChecker.Core;
using ItemChecker.MVVM.Model;
using ItemChecker.MVVM.View;
using ItemChecker.Properties;
using ItemChecker.Services;
using ItemChecker.Support;

namespace ItemChecker.MVVM.ViewModel
{
    public class MainViewModel : ObservableObject
    {
        #region prop
        readonly Timer TimerInfo = new(TimeSpan.FromMinutes(15).TotalMilliseconds);
        readonly Timer TimerWindow = new(250);

        public MainInfo MainInfo
        {
            get { return _mainInfo; }
            set
            {
                _mainInfo = value;
                OnPropertyChanged();
            }
        }
        MainInfo _mainInfo = new();
        #endregion

        public MainViewModel()
        {
            TimerInfo.Elapsed += UpdateInformation;
            TimerWindow.Elapsed += UpdateWindow;
            TimerInfo.Enabled = true;
            TimerWindow.Enabled = true;

            if (MainProperties.Default.CompletionUpdate)
            {
                Window window = new WhatsNewWindow();
                window.ShowDialog();

                MainProperties.Default.CompletionUpdate = false;
                MainProperties.Default.Save();
            }
            Task.Run(Main.CreateHistoryRecords);
        }
        public ICommand MenuCommand =>
            new RelayCommand((obj) =>
            {
                switch (obj as string)
                {
                    case "History":
                        {
                            HistoryWindow window = new();
                            window.ShowDialog();
                            break;
                        }
                    case "Details":
                        {
                            MainWindow.OpenDetailsItem(null);
                            break;
                        }
                    case "Calculator":
                        {
                            if (!MainWindow.IsWindowOpen<Window>("calculatorWindow"))
                            {
                                CalculatorWindow window = new();
                                window.Show();
                            }
                            else
                            {
                                Window window = Application.Current.Windows.OfType<Window>().Where(w => w.Name.Equals("calculatorWindow")).FirstOrDefault();
                                window.WindowState = WindowState.Normal;
                                window.Activate();
                            }
                            break;
                        }
                    case "SteamMarket":
                        {
                            Edit.OpenUrl("https://steamcommunity.com/market/");
                            break;
                        }
                    case "MyInventory":
                        {
                            Edit.OpenUrl("https://steamcommunity.com/my/inventory/");
                            break;
                        }
                    case "Cs.Money":
                        {
                            Edit.OpenUrl("https://cs.money/");
                            break;
                        }
                    case "Loot.Farm":
                        {
                            Edit.OpenUrl("https://loot.farm/");
                            break;
                        }
                    case "Buff163":
                        {
                            Edit.OpenUrl("https://buff.163.com/");
                            break;
                        }
                    case "Settings":
                        {
                            SettingWindow window = new();
                            window.ShowDialog();
                            break;
                        }
                    case "Exit":
                        {
                            MessageBoxResult result = MessageBox.Show(
                                "Are you sure you want to close?", "Question",
                                MessageBoxButton.YesNo, MessageBoxImage.Question);
                            if (result == MessageBoxResult.Yes && ExitCommand.CanExecute(null))
                                ExitCommand.Execute(null);
                            break;
                        }
                }
            });
        public ICommand ExitCommand =>
            new RelayCommand((obj) =>
            {
                Application.Current.Shutdown();
            });
        public ICommand ReadNotificationCommand =>
            new RelayCommand((obj) =>
            {
                foreach (var item in Main.Notifications.Where(x => x.IsRead == false))
                    item.IsRead = true;
            });

        void UpdateInformation(Object sender, ElapsedEventArgs e)
        {
            try
            {
                Main.CreateHistoryRecords();

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
            MainInfo = new();
        }
    }
}