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
        private Calculator _calculator = new();

        public ICommand ChangeCommand =>
            new RelayCommand((obj) =>
            {
                Calculator config = (Calculator)obj;
                Calculator calculator = new();
                calculator.Service = config.Service;
                calculator.Commission = config.Commission;
                calculator.Purchase = config.Purchase;
                calculator.Price = config.Price;
                calculator.Precent = config.Precent;
                calculator.Difference = config.Difference;
                calculator.Get = config.Get;

                calculator.Currency1 = config.Currency2;
                calculator.Currency2 = config.Currency1;
                calculator.Value = config.Converted;
                calculator.Converted = config.Value;
                Calculator = calculator;
            });
        public ICommand CommissionCommand =>
            new RelayCommand((obj) =>
            {
                switch (Calculator.Service)
                {
                    case 0 or 1:
                        Calculator.Commission = Calculator.CommissionSteam;
                        break;
                    case 2:
                        Calculator.Commission = Calculator.CommissionCsm;
                        break;
                    case 3:
                        Calculator.Commission = Calculator.CommissionLf;
                        break;
                    case 4 or 5:
                        Calculator.Commission = Calculator.CommissionBuff;
                        break;
                }
            });
    }
}
