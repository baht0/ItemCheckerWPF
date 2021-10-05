using ItemChecker.Core;
using ItemChecker.MVVM.Model;
using ItemChecker.Support;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ItemChecker.MVVM.ViewModel
{
    public class MainViewModel : ObservableObject
    {
        string _accountName;
        decimal _course;
        decimal _balance;
        decimal _balanceCsm;
        decimal _balanceUsd;
        decimal _balanceCsmUsd;
        int _overstock;
        int _unavailable;
        string _statusCommunity;
        string _statusSessions;
        public string AccountName
        {
            get { return _accountName; }
            set
            {
                _accountName = value;
                OnPropertyChanged();
            }
        }
        public decimal Course
        {
            get { return _course; }
            set
            {
                _course = value;
                OnPropertyChanged();
            }
        }
        public decimal Balance
        {
            get { return _balance; }
            set
            {
                _balance = value;
                OnPropertyChanged();
            }
        }
        public decimal BalanceCsm
        {
            get { return _balanceCsm; }
            set
            {
                _balanceCsm = value;
                OnPropertyChanged();
            }
        }
        public decimal BalanceUsd
        {
            get { return _balanceUsd; }
            set
            {
                _balanceUsd = value;
                OnPropertyChanged();
            }
        }
        public decimal BalanceCsmUsd
        {
            get { return _balanceCsmUsd; }
            set
            {
                _balanceCsmUsd = value;
                OnPropertyChanged();
            }
        }
        public int Overstock
        {
            get { return _overstock; }
            set
            {
                _overstock = value;
                OnPropertyChanged();
            }
        }
        public int Unavailable
        {
            get { return _unavailable; }
            set
            {
                _unavailable = value;
                OnPropertyChanged();
            }
        }
        public string StatusCommunity
        {
            get { return _statusCommunity; }
            set
            {
                _statusCommunity = value;
                OnPropertyChanged();
            }
        }
        public string StatusSessions
        {
            get { return _statusSessions; }
            set
            {
                _statusSessions = value;
                OnPropertyChanged();
            }
        }

        public MainViewModel()
        {
            UpdateInformation();
        }
        protected void UpdateInformation()
        {
            AccountName = Account.AccountName;
            Course = Account.OrderSum;
            Balance = Account.Balance;
            BalanceCsm = Account.BalanceCsm;
            BalanceUsd = Account.BalanceUsd;
            BalanceCsmUsd = Account.BalanceCsmUsd;
            Overstock = Main.Overstock.Count;
            Unavailable = Main.Unavailable.Count;
            StatusCommunity = "CheckCircle";
            if (Main.StatusCommunity != "normal")
                StatusCommunity = "CloseCircle";
        }
        public ICommand OpenFolderCommand =>
            new RelayCommand((obj) =>
            {
                Edit.openUrl(BaseModel.AppPath);
            });
        public ICommand ExitCommand =>
            new RelayCommand((obj) =>
            {
                MessageBoxResult result = MessageBox.Show("Are you sure you want to close?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    Task.Run(() =>
                    {
                        BaseModel.BrowserExit();
                    }).Wait(5000);
                    Application.Current.Shutdown();
                }
            });
    }
}