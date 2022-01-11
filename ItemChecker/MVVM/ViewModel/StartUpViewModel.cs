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
using MaterialDesignThemes.Wpf;
using System.Threading;

namespace ItemChecker.MVVM.ViewModel
{
    public class StartUpViewModel : ObservableObject
    {
        IView _view;
        string _version = DataProjectInfo.CurrentVersion;
        bool _isLogin = false;
        string _status;
        SnackbarMessageQueue _mess = new();
        private SteamLogin _login = new();

        Task startTask { get; set; }
        public bool LoginSuccessful { get; set; }
        public string Version
        {
            get { return _version; }
            set
            {
                _version = value;
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
        public string Status
        {
            get { return _status; }
            set
            {
                _status = value;
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
        public SteamLogin Login
        {
            get { return _login; }
            set
            {
                _login = value;
                OnPropertyChanged();
            }
        }

        public StartUpViewModel(IView view)
        {
            _view = view;
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.CreateSpecificCulture("en-Us");
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.CreateSpecificCulture("en-Us");

            startTask = Task.Run(() => Starting());
        }
        public ICommand ExitCommand =>
            new RelayCommand((obj) =>
            {
                Task.Run(() => {
                    BaseModel.cts.Cancel();
                    startTask.Wait(4000);
                    Status = "Exit...";
                    if (BaseModel.Browser != null)
                        BaseService.BrowserExit();

                    Application.Current.Dispatcher.Invoke(() => { Application.Current.Shutdown(); });
                });                
            });
        void Hide()
        {
            _view.Close();
        }
        void Starting()
        {
            try
            {
                Status = "Check Update...";
                ProjectInfoService.AppUpdate();

                Status = "Signing In...";
                Task.Run(() => {
                    while (true)
                        if (!BaseModel.LoginSteam.IsLoggedIn)
                        {
                            IsLogin = true;
                            Message.Enqueue("Please, Sign In...");
                            Status = "Please, Sign In...";
                            Thread.Sleep(500);
                        }
                });
                bool isLogin = false;
                do
                {
                    if (BaseModel.token.IsCancellationRequested)
                        break;
                    Proccess();
                    isLogin = BaseModel.GetCookies();
                } while (!isLogin);

                Status = "Get Base...";
                BaseService.GetBase();
                Status = "Steam Account...";
                SteamAccount.GetSteamAccount();
                if (BaseModel.token.IsCancellationRequested)
                    return;
                Status = "My Orders...";
                OrderCheckService order = new();
                order.SteamOrders();

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
        void Proccess()
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
        public ICommand LoginCommand =>
            new RelayCommand((obj) =>
            {
                var propertyInfo = obj.GetType().GetProperty("Password");
                Login.Password = (string)propertyInfo.GetValue(obj, null);
                if (!String.IsNullOrEmpty(Login.Password))
                {
                    BaseModel.LoginSteam = Login;
                    BaseModel.LoginSteam.IsLoggedIn = true;
                    Status = "Continue Signing In...";
                    IsLogin = false;
                }
            }, (obj) => !BaseModel.LoginSteam.IsLoggedIn & !String.IsNullOrEmpty(Login.Login) & Login.Code2AF.Length == 5);
    }
}