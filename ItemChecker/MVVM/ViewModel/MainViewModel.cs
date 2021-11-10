using ItemChecker.Core;
using ItemChecker.MVVM.Model;
using ItemChecker.Properties;
using ItemChecker.Support;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ItemChecker.MVVM.ViewModel
{
    public class MainViewModel : ObservableObject
    {
        string _user;
        decimal _course;
        decimal _balance;
        decimal _balanceCsm;
        decimal _balanceUsd;
        decimal _balanceCsmUsd;
        int _overstock;
        int _unavailable;
        string _statusCommunity;
        string _statusSessions;
        //favorite
        private ObservableCollection<string> _favoriteList = new();
        public string User
        {
            get { return _user; }
            set
            {
                _user = value;
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
        //favorite
        public ObservableCollection<string> FavoriteList
        {
            get
            {
                return _favoriteList;
            }
            set
            {
                _favoriteList = value;
                OnPropertyChanged();
            }
        }

        public MainViewModel()
        {
            UpdateInformation();

            if (BuyOrderProperties.Default.FavoriteList != null)
                FavoriteList = BuyOrderProperties.Default.FavoriteList;
        }
        protected void UpdateInformation()
        {
            User = Account.User;
            Course = GeneralProperties.Default.CurrencyValue;
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
                Task.Run(() => {
                    BaseModel.cts.Cancel();
                    BaseModel.BrowserExit();
                }).Wait(5000);
                Application.Current.Shutdown();
            });
        //favorite
        public ICommand AddFavoriteCommand =>
            new RelayCommand((obj) =>
            {
                string itemName = string.Empty;
                if (obj is OrderData)
                {
                    var item = obj as OrderData;
                    itemName = item.ItemName;
                }
                else if (obj is ParserData)
                {
                    var item = obj as ParserData;
                    itemName = item.ItemName;
                }
                else if (obj is string)
                {
                    itemName = (string)obj;
                }
                if (!BuyOrderProperties.Default.FavoriteList.Contains(itemName))
                {
                    BuyOrderProperties.Default.FavoriteList.Add(itemName);
                    BuyOrderProperties.Default.Save();
                }
            });
    }
}