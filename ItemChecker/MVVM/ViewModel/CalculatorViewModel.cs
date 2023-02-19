using ItemChecker.Core;
using ItemChecker.MVVM.Model;
using System.Windows.Input;

namespace ItemChecker.MVVM.ViewModel
{
    class CalculatorViewModel : ObservableObject
    {
        public Calculator Calculator
        {
            get { return _calculator; }
            set
            {
                _calculator = value;
                OnPropertyChanged();
            }
        }
        Calculator _calculator = new();

        public ICommand CompareCommand =>
            new RelayCommand((obj) =>
            {
                string[] values = (string[])obj;
                Calculator.Compare(values);
            });
        public ICommand CommissionCommand =>
            new RelayCommand((obj) =>
            {
                Calculator.GetCommission();
            });

        public ICommand ChangeCommand =>
            new RelayCommand((obj) =>
            {
                Calculator.Switch();
            });
        public ICommand CurrencyConvertCommand =>
            new RelayCommand((obj) =>
            {
                Calculator.CurrencyConvert((string)obj);
            });
    }
}