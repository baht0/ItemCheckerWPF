using ItemChecker.Core;
using ItemChecker.Support;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ItemChecker.MVVM.Model
{
    public class Calculator : ObservableObject
    {
        public static decimal CommissionSteam { get; set; } = 0.869565m;
        public static decimal CommissionCsm { get; set; } = 0.93m;
        public static decimal CommissionLf { get; set; } = 0.95m;
        public static decimal CommissionBuff { get; set; } = 0.975m;

        //config
        public List<string> Services
        {
            get
            {
                var services = BaseModel.ServicesShort;
                services.Add("Custom");
                return services;
            }
        }
        public int Service { get; set; }

        public decimal Commission
        {
            get { return _commission; }
            set
            {
                if (value < 1)
                    _commission = value;
                else
                    _commission = (100 - value) / 100;
                OnPropertyChanged();
            }
        }
        decimal _commission;
        public decimal Purchase
        {
            get { return _purchase; }
            set
            {
                _purchase = value;
                OnPropertyChanged();
            }
        }
        decimal _purchase;
        public decimal Price
        {
            get { return _price; }
            set
            {
                _price = value;
                OnPropertyChanged();
            }
        }
        decimal _price;
        public decimal Get
        {
            get { return _get; }
            set
            {
                _get = value;
                OnPropertyChanged();
            }
        }
        decimal _get;
        public decimal Precent
        {
            get { return _precent; }
            set
            {
                _precent = value;
                OnPropertyChanged();
            }
        }
        decimal _precent;
        public decimal Difference
        {
            get { return _difference; }
            set
            {
                _difference = value;
                OnPropertyChanged();
            }
        }
        decimal _difference;

        //currency
        public List<string> CurrencyList { get; set; } = Currencies.Allow.Select(x => x.Name).ToList();
        public int Currency1
        {
            get { return _currency1; }
            set
            {
                _currency1 = value;
                OnPropertyChanged();
            }
        }
        int _currency1;
        public int Currency2
        {
            get { return _currency2; }
            set
            {
                _currency2 = value;
                OnPropertyChanged();
            }
        }
        int _currency2 = 1;

        public decimal Converted
        {
            get { return _converted; }
            set
            {
                _converted = value;
                OnPropertyChanged();
            }
        }
        decimal _converted = Math.Round(Currencies.Allow.FirstOrDefault(x => x.Id == 5).Value, 2);
        public decimal Value
        {
            get { return _value; }
            set
            {
                _value = value;
                OnPropertyChanged();
            }
        }
        decimal _value = 1;

        public void Compare(string[] values)
        {
            bool isDecimal = !decimal.TryParse(values[0], out decimal value);
            decimal purchase = value;
            isDecimal = !decimal.TryParse(values[1], out value);
            decimal price = value;
            isDecimal = !decimal.TryParse(values[2], out value);
            decimal commission = value;

            Get = Math.Round(price * commission, 2);
            Precent = Edit.Precent(purchase, Get);
            Difference = Edit.Difference(Get, purchase);
        }
        public void GetCommission()
        {
            switch (Service)
            {
                case 0:
                    Commission = CommissionSteam;
                    break;
                case 1:
                    Commission = CommissionCsm;
                    break;
                case 2:
                    Commission = CommissionLf;
                    break;
                case 3:
                    Commission = CommissionBuff;
                    break;
            }
        }
        public void Switch()
        {
            var config = (Calculator)this.MemberwiseClone();

            Currency1 = config.Currency2;
            Currency2 = config.Currency1;
            Value = config.Converted;
            Converted = config.Value;
        }
        public void CurrencyConvert(string str)
        {
            bool isDecimal = !decimal.TryParse(str, out decimal value);
            decimal dol = value;
            switch (Currency1) //any -> dol
            {
                case 1:
                    dol = Currency.ConverterToUsd(value, 5);
                    break;
                case 2:
                    dol = Currency.ConverterToUsd(value, 23);
                    break;
            }
            switch (Currency2)//dol -> any
            {
                case 0:
                    Converted = dol;
                    break;
                case 1:
                    Converted = Currency.ConverterFromUsd(dol, 5);
                    break;
                case 2:
                    Converted = Currency.ConverterFromUsd(dol, 23);
                    break;
            }
        }
    }
}
