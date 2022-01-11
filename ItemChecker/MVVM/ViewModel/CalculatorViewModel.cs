using ItemChecker.Core;
using ItemChecker.MVVM.Model;
using System.Windows.Input;

namespace ItemChecker.MVVM.ViewModel
{
    public class CalculatorViewModel : MainViewModel
    {
        Calculator _calculator = new();
        public Calculator Calculator
        {
            get { return _calculator; }
            set
            {
                _calculator = value;
                OnPropertyChanged();
            }
        }

        public CalculatorViewModel()
        {
            Calculator = new();
        }
        public ICommand ChangeCommand =>
            new RelayCommand((obj) =>
            {
                Calculator config = (Calculator)obj; 
                Calculator calculator = new();
                calculator.Service = config.Service;
                calculator.Price1 = config.Price1;
                calculator.Price2 = config.Price2;
                calculator.Precent = config.Precent;
                calculator.Difference = config.Difference;
                calculator.Result = config.Result;

                calculator.Currency1 = config.Currency2;
                calculator.Currency2 = config.Currency1;
                calculator.Value = config.Converted;
                calculator.Converted = config.Value;
                Calculator = calculator;
            });
    }
}
