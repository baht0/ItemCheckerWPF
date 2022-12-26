using System;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Threading;
using System.Threading.Tasks;
using System.Globalization;
using ItemChecker.Core;
using ItemChecker.Net;
using ItemChecker.MVVM.Model;
using ItemChecker.Services;
using ItemChecker.Properties;

namespace ItemChecker.MVVM.ViewModel
{
    public class StartUpViewModel : StartUp
    {
        public bool LaunchSuccessful { get; set; }
        IView _view;

        public StartUp StartUp
        {
            get { return _startUp; }
            set
            {
                _startUp = value;
                OnPropertyChanged();
            }
        }
        StartUp _startUp = new();
        public SignInData SignInData
        {
            get { return _signInData; }
            set
            {
                _signInData = value;
                OnPropertyChanged();
            }
        }
        SignInData _signInData = new();

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
            Task.Run(StartTask);
        }
        void Hide()
        {
            _view.Close();
        }
        void StartTask()
        {
            try
            {
                StartUp.Progress = Tuple.Create(1, "Checking Update...");
                bool isUpdate = ProjectInfoService.AppCheck();
                if (isUpdate)
                {
                    StartUp.Progress = Tuple.Create(1, "Updating...");
                    ProjectInfoService.Update();
                }
                StartUp.Progress = Tuple.Create(2, "Preparation...");
                BaseService.GetCurrencyList();

                StartUp.Progress = Tuple.Create(3, "Signing In...");
                if (!SteamRequest.Session.IsAuthorized())
                {
                    StartUp.IsSignInShow = true;
                    SignIn();
                }
                ServiceAccount.SignInToServices();

                StartUp.Progress = Tuple.Create(4, "Get Account...");
                SteamAccount.GetAccount();

                StartUp.Progress = Tuple.Create(5, "Creation ItemBase...");
                ItemBaseService itemBase = new();
                itemBase.CreateItemsBase();

                LaunchSuccessful = true;
                Application.Current.Dispatcher.Invoke(Hide);
            }
            catch (Exception exp)
            {
                StartUp.IsReset = true;
                BaseService.errorLog(exp, true);
                Application.Current.Dispatcher.Invoke(Application.Current.Shutdown);
            }
        }
        void SignIn()
        {
            StartUp.Progress = Tuple.Create(3, "Please, Signing In...");
            SignInData = new();
            StartUp.IsSubmitShow = true;

            while (!SignInData.IsCorrect) Thread.Sleep(100);

            bool isSetToken = false;
            Task.Run(() => {
                while (!isSetToken)
                {
                    isSetToken = SteamRequest.Session.CheckAuthStatus();
                    if (!StartUp.IsCodeEnabled && !isSetToken)
                    {
                        StartUp.IsCodeEnabled = true;
                        StartUp.IsErrorShow = true;
                    }
                    Thread.Sleep(5000);
                }
            });
            while (!isSetToken) Thread.Sleep(100);
            StartUp.IsSignInShow = false;

            StartUp.CurrencyList = new(SteamBase.CurrencyList);
            StartUp.SelectedCurrency = SteamBase.CurrencyList.FirstOrDefault();
            StartUp.IsCurrencyShow = true;
            while (StartUp.IsCurrencyShow) Thread.Sleep(100);
            StartUp.Progress = Tuple.Create(3, "Signing In...");
        }
        void SessionTimerTick(Object sender, ElapsedEventArgs e)
        {
            SignInData.TimerTick--;
            if (SignInData.TimerTick <= 0)
            {
                StartUp.Progress = Tuple.Create(5, "Failed to login...");
                StartUp.IsConfirmationShow = false;
                StartUp.IsExpiredShow = true;
                SignInData.Timer.Enabled = false;
                SignInData.Timer.Elapsed -= SessionTimerTick;
            }
        }
        public ICommand SignInCommand =>
            new RelayCommand((obj) =>
            {
                Task.Run(() =>
                {
                    StartUp.IsSubmitEnabled = false;
                    StartUp.IsErrorShow = false;
                    var propertyInfo = obj.GetType().GetProperty("Password");
                    SignInData.Password = (string)propertyInfo.GetValue(obj, null);
                    if (!String.IsNullOrEmpty(SignInData.Password))
                    {
                        if (SignInData.AllowUser(SignInData.AccountName))
                        {
                            SignInData.IsCorrect = SteamRequest.Session.SubmitSignIn(SignInData.AccountName, SignInData.Password);
                            if (SignInData.IsCorrect)
                            {
                                SignInData.Timer.Elapsed += SessionTimerTick;
                                SignInData.Timer.Enabled = true;
                                StartUp.IsErrorShow = false;
                                StartUp.IsSubmitShow = false;
                                StartUp.IsConfirmationShow = true;
                            }
                            else
                                StartUp.IsErrorShow = true;
                        }
                        else
                            StartUp.IsErrorShow = true;
                    }
                    else
                        StartUp.IsErrorShow = true;
                    StartUp.IsSubmitEnabled = true;
                });
            }, (obj) => !String.IsNullOrEmpty(SignInData.AccountName));
        public ICommand SubmitCodeCommand =>
            new RelayCommand((obj) =>
            {
                StartUp.IsErrorShow = false;
                Task.Run(() => SteamRequest.Session.SubmitCode(SignInData.Code2AF));
                StartUp.IsCodeEnabled = false;
            }, (obj) => !String.IsNullOrEmpty(SignInData.Code2AF) && SignInData.IsCorrect);
        public ICommand SelectCurrencyCommand =>
            new RelayCommand((obj) =>
            {
                var currency = obj as Currency;
                SteamAccount.Currency = currency;
                StartUp.IsCurrencyShow = false;

                MainProperties.Default.SteamCurrencyId = currency.Id;
                MainProperties.Default.Save();

            }, (obj) => StartUp.SelectedCurrency != null);
    }
}