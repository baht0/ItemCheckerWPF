using ItemChecker.Core;
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
using ItemChecker.Support;

namespace ItemChecker.MVVM.ViewModel
{
    public class StartUpViewModel : StartUp
    {
        Task startTask { get; set; }
        public bool LoginSuccessful { get; set; }

        private IView _view;
        private StartUp _startUp = new();
        private bool _showLogin = false;
        private SteamSignUp _signUp = new();
        private string _currencyApi = string.Empty;

        public StartUp StartUp
        {
            get { return _startUp; }
            set
            {
                _startUp = value;
                OnPropertyChanged();
            }
        }
        public bool ShowLogin
        {
            get { return _showLogin; }
            set
            {
                _showLogin = value;
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
        public string CurrencyApi
        {
            get { return _currencyApi; }
            set
            {
                _currencyApi = value;
                OnPropertyChanged();
            }
        }

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
                    cts.Cancel();
                    StartUp.Progress = Tuple.Create(5, "Exit...");
                    ShowLogin = false;
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
                if (token.IsCancellationRequested)
                    return;
                StartUp.Progress = Tuple.Create(1, "Check Update...");
                ProjectInfoService.AppCheck();
                StartUp.IsUpdate = DataProjectInfo.IsUpdate;
                if (DataProjectInfo.IsUpdate)
                    StartUp.Message.Enqueue($"Update {DataProjectInfo.LatestVersion} is available.");
                BaseService.GetCurrency();

                if (token.IsCancellationRequested)
                    return;
                StartUp.Progress = Tuple.Create(2, "Signing In...");
                KillProccess();
                ShowLogin = SteamAccount.IsLogIn();
                StartUp.Progress = ShowLogin ? Tuple.Create(2, "Please, Signing In...") : Tuple.Create(3, "Get Account...");
                while (ShowLogin)
                {
                    if (token.IsCancellationRequested)
                        break;
                    ShowLogin = SteamAccount.Login();
                    StartUp.Progress = ShowLogin ? Tuple.Create(2, "Failed to login...") : Tuple.Create(3, "Get Account...");
                    SignUp.Code2AF = string.Empty;
                }
                if (token.IsCancellationRequested)
                    return;
                SteamAccount.GetSteamAccount();

                if (token.IsCancellationRequested)
                    return;
                StartUp.Progress = Tuple.Create(4, "Preparation...");
                ItemBaseService itemBase = new();
                itemBase.CreateItemsBase();

                if (token.IsCancellationRequested)
                    return;
                StartUp.Progress = Tuple.Create(5, "Check MyOrders...");
                OrderCheckService order = new();
                order.SteamOrders(true);

                if (token.IsCancellationRequested)
                    return;
                LoginSuccessful = true;
                Application.Current.Dispatcher.Invoke(() => { Hide(); });
            }
            catch (Exception exp)
            {
                BaseService.errorLog(exp, true);

                if (!DataProjectInfo.IsUpdate)
                {
                    Application.Current.Dispatcher.Invoke(() => {
                        if (Application.Current.MainWindow.DataContext is StartUpViewModel vw)
                            vw.ExitCommand.Execute(null);
                    });
                }
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
            foreach (Process proc in Process.GetProcessesByName("msedge")) proc.Kill();
            foreach (Process proc in Process.GetProcessesByName("msedgedriver")) proc.Kill();
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
                    ShowLogin = false;
                }
            }, (obj) => !SteamSignUp.SignUp.IsLoggedIn && !String.IsNullOrEmpty(SignUp.Login) && SignUp.Code2AF.Length == 5);
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