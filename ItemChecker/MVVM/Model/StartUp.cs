using ItemChecker.Core;
using ItemChecker.Net;
using ItemChecker.Support;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Timers;

namespace ItemChecker.MVVM.Model
{
    public class SignInData
    {
        public string AccountName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Code2AF { get; set; } = string.Empty;

        public bool IsCorrect { get; set; }

        public static Timer Timer { get; set; } = new(1000);
        public static int TimerTick { get; set; } = 300;

        public static Boolean AllowUser(string login)
        {
            JArray users = JArray.Parse(DropboxRequest.Get.Read("Users.json"));
            JObject user = (JObject)users.FirstOrDefault(x => x["Login"].ToString() == login);
            if (user != null)
            {
                int id = users.IndexOf(user);
                users[id]["LastLoggedIn"] = DateTime.Now;
                users[id]["Version"] = DataProjectInfo.CurrentVersion;

                DropboxRequest.Post.Delete("Users.json");
                System.Threading.Thread.Sleep(200);
                DropboxRequest.Post.Upload("Users.json", users.ToString());
                return Convert.ToBoolean(user["Allowed"]);
            }
            return false;
        }
    }
    public class StartUp : ObservableObject
    {
        public bool IsSignInShow
        {
            get { return _isSignInShow; }
            set
            {
                _isSignInShow = value;
                OnPropertyChanged();
            }
        }
        bool _isSignInShow;
        public bool IsSubmitShow
        {
            get { return _isSubmitShow; }
            set
            {
                _isSubmitShow = value;
                OnPropertyChanged();
            }
        }
        bool _isSubmitShow;
        public bool IsSubmitEnabled
        {
            get { return _isSubmitEnabled; }
            set
            {
                _isSubmitEnabled = value;
                OnPropertyChanged();
            }
        }
        bool _isSubmitEnabled = true;
        public bool IsErrorShow
        {
            get { return _isErrorShow; }
            set
            {
                _isErrorShow = value;
                OnPropertyChanged();
            }
        }
        bool _isErrorShow;
        public bool IsExpiredShow
        {
            get { return _isExpiredShow; }
            set
            {
                _isExpiredShow = value;
                OnPropertyChanged();
            }
        }
        bool _isExpiredShow;
        public bool IsConfirmationShow
        {
            get { return _isConfirmationShow; }
            set
            {
                _isConfirmationShow = value;
                OnPropertyChanged();
            }
        }
        bool _isConfirmationShow;
        public bool IsCodeEnabled
        {
            get { return _isCodeEnabled; }
            set
            {
                _isCodeEnabled = value;
                OnPropertyChanged();
            }
        }
        bool _isCodeEnabled = true;
        public bool IsCurrencyShow
        {
            get { return _isCurrencyShow; }
            set
            {
                _isCurrencyShow = value;
                OnPropertyChanged();
            }
        }
        bool _isCurrencyShow;
        public ObservableCollection<DataCurrency> CurrencyList
        {
            get { return _currencyList; }
            set
            {
                _currencyList = value;
                OnPropertyChanged();
            }
        }
        ObservableCollection<DataCurrency> _currencyList = new();
        public DataCurrency SelectedCurrency
        {
            get { return _selectedCurrency; }
            set
            {
                _selectedCurrency = value;
                OnPropertyChanged();
            }
        }
        DataCurrency _selectedCurrency = new();

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
