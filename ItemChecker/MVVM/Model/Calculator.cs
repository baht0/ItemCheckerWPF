using ItemChecker.Core;
using ItemChecker.Properties;
using ItemChecker.Support;
using System;
using System.Collections.ObjectModel;

namespace ItemChecker.MVVM.Model
{
    public class Calculator : ObservableObject
    {
        public static decimal CommissionSteam { get; set; } = 0.869565m;
        public static decimal CommissionCsm { get; set; } = 0.93m;
        public static decimal CommissionLf { get; set; } = 0.95m;
        public static decimal CommissionBuff { get; set; } = 0.975m;

        //config
        public ObservableCollection<string> Services { get; set; } = new()
        {
            "SteamMarket",
            "Cs.Money",
            "Loot.Farm"
        };
        public int Service { get; set; }

        private decimal _price1 = 0;
        private decimal _price2 = 0;
        private decimal _result = 0;
        private decimal _precent = 0;
        private decimal _difference = 0;
        public decimal Price1
        {
            get { return _price1; }
            set
            {
                _price1 = value;
                OnPropertyChanged();
                Compare(true, false);
            }
        }
        public decimal Price2
        {
            get { return _price2; }
            set
            {
                _price2 = value;
                OnPropertyChanged();
                Compare(true, false);
            }
        }
        public decimal Result
        {
            get { return _result; }
            set
            {
                _result = value;
                OnPropertyChanged();
                //Compare(false, true);
            }
        }
        public decimal Precent
        {
            get { return _precent; }
            set
            {
                _precent = value;
                OnPropertyChanged();
            }
        }
        public decimal Difference
        {
            get { return _difference; }
            set
            {
                _difference = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<string> CurrencyList { get; set; } = new()
        {
            "USD ($)",
            "RUB (₽)"
        };
        public int Currency1 { get; set; } = 0;
        public int Currency2 { get; set; } = 1;

        private decimal _value = 1;
        private decimal _converted = SettingsProperties.Default.RUB;
        public decimal Converted
        {
            get { return _converted; }
            set
            {
                _converted = value;
                OnPropertyChanged();
            }
        }
        public decimal Value
        {
            get { return _value; }
            set
            {
                _value = value;
                if (Currency1 == 1 && Currency2 == 0)
                    Converted = Edit.ConverterToUsd(value, SettingsProperties.Default.RUB);
                if (Currency1 == 0 && Currency2 == 1)
                    Converted = Edit.ConverterToRub(value, SettingsProperties.Default.RUB);
                OnPropertyChanged();
            }
        }

        void Compare(bool isPrice, bool isResult)
        {
            decimal commission = 0;
            switch (Service)
            {
                case 0:
                    commission = CommissionSteam;
                    break;
                case 1:
                    commission = CommissionCsm;
                    break;
                case 2:
                    commission = CommissionLf;
                    break;
            }

            if (isPrice)
                Result = Math.Round(Price2 * commission);
            if (isResult)
                Result = Math.Round(Price2 / commission);
            Precent = Edit.Precent(Price1, Result);
            Difference = Edit.Difference(Result, Price1);
        }
    }
}
