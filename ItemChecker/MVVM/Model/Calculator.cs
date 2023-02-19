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
                var services = Main.ServicesShort;
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
        public int Currency1 { get; set; } = 0;
        public int Currency2 { get; set; } = 1;

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
    }
}
