using System;
using System.Windows;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Globalization;
using ItemChecker.Core;
using ItemChecker.Net;
using ItemChecker.MVVM.Model;
using ItemChecker.Properties;
using ItemChecker.Support;

namespace ItemChecker.MVVM.ViewModel
{
    public class StartUpViewModel : ObservableObject
    {
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

        public StartUpViewModel(IView view)
        {
            Task.Run(() =>
            {
                try
                {
                    CultureInfo.DefaultThreadCurrentCulture = CultureInfo.CreateSpecificCulture("en-Us");
                    CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.CreateSpecificCulture("en-Us");

                    StartUp.StartTask();
                    Application.Current.Dispatcher.Invoke(() => { view.Close(); });
                }
                catch (Exception exp)
                {
                    StartUp.IsReset = true;
                    BaseModel.ErrorLog(exp, true);
                    Application.Current.Dispatcher.Invoke(Application.Current.Shutdown);
                }
            });
        }
            
        public ICommand SignInCommand =>
            new RelayCommand((obj) =>
            {
                Task.Run(() =>
                {
                    var propertyInfo = obj.GetType().GetProperty("Password");
                    var pass = (string)propertyInfo.GetValue(obj, null);

                    StartUp.SignInSubmit(pass);
                });
            }, (obj) => !string.IsNullOrEmpty(StartUp.AccountName));
        public ICommand SubmitCodeCommand =>
            new RelayCommand((obj) =>
            {
                StartUp.IsErrorShow = false;
                Task.Run(() => SteamRequest.Session.SubmitCode(StartUp.Code2AF));
                StartUp.IsCodeEnabled = false;
            }, (obj) => !string.IsNullOrEmpty(StartUp.Code2AF) && StartUp.IsCorrect);
        public ICommand SelectCurrencyCommand =>
            new RelayCommand((obj) =>
            {
                var currency = obj as DataCurrency;
                SteamAccount.Currency = currency;
                StartUp.IsCurrencyShow = false;
                StartUp.IsSignInShow = false;

                MainProperties.Default.SteamCurrencyId = currency.Id;
                MainProperties.Default.Save();

            }, (obj) => StartUp.SelectedCurrency != null);
    }
}