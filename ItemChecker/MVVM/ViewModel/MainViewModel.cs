using ItemChecker.Core;
using ItemChecker.MVVM.Model;
using System;
using System.Windows.Input;

namespace ItemChecker.MVVM.ViewModel
{    
    public class MainViewModel : ObservableObject
    {
        decimal _course;
        decimal _balance;
        decimal _balanceCsm;
        decimal _balanceUsd;
        decimal _balanceCsmUsd;
        int _overstock;
        int _unavailable;
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


        public MainViewModel()
        {
            Course = Account.OrderSum;
            Balance = Account.Balance;
            BalanceCsm = Account.BalanceCsm;
            BalanceUsd = Account.BalanceUsd;
            BalanceCsmUsd = Account.BalanceCsmUsd;
            Overstock = Main.Overstock.Count;
            Unavailable = Main.Unavailable.Count;
        }
        public ICommand Converting
        {
            get
            {
                return new RelayCommand((obj) =>
                {
                    //
                });
            }
        }
    }
}
