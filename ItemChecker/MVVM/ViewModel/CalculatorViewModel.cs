using ItemChecker.Core;
using ItemChecker.MVVM.Model;
using ItemChecker.Support;
using System;
using System.Linq;
using System.Windows;
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

        public ICommand CompareCommand =>
            new RelayCommand((obj) =>
            {
                string[] values = (string[])obj;
                bool isDecimal = !decimal.TryParse(values[0], out decimal value);
                decimal purchase = value;
                isDecimal = !decimal.TryParse(values[1], out value);
                decimal price = value;
                isDecimal = !decimal.TryParse(values[2], out value);
                decimal commission = value;

                Calculator.Get = Math.Round(price * commission, 2);
                Calculator.Precent = Edit.Precent(purchase, Calculator.Get);
                Calculator.Difference = Edit.Difference(Calculator.Get, purchase);
            });
        public ICommand CommissionCommand =>
            new RelayCommand((obj) =>
            {
                switch (Calculator.Service)
                {
                    case 0:
                        Calculator.Commission = Calculator.CommissionSteam;
                        break;
                    case 1:
                        Calculator.Commission = Calculator.CommissionCsm;
                        break;
                    case 2:
                        Calculator.Commission = Calculator.CommissionLf;
                        break;
                    case 3:
                        Calculator.Commission = Calculator.CommissionBuff;
                        break;
                }
            });

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
        public ICommand CurrencyConvertCommand =>
            new RelayCommand((obj) =>
            {
                bool isDecimal = !decimal.TryParse((string)obj, out decimal value);
                decimal dol = value;
                decimal rub = SteamBase.CurrencyList.FirstOrDefault(x => x.Id == 5).Value;
                decimal cny = SteamBase.CurrencyList.FirstOrDefault(x => x.Id == 23).Value;
                switch (Calculator.Currency1) //any -> dol
                {
                    case 1:
                        dol = Edit.ConverterToUsd(value, rub);
                        break;
                    case 2:
                        dol = Edit.ConverterToUsd(value, cny);
                        break;
                }
                switch (Calculator.Currency2)//dol -> any
                {
                    case 0:
                        Calculator.Converted = dol;
                        break;
                    case 1:
                        Calculator.Converted = Edit.ConverterFromUsd(dol, rub);
                        break;
                    case 2:
                        Calculator.Converted = Edit.ConverterFromUsd(dol, cny);
                        break;
                }
            });
    }
}