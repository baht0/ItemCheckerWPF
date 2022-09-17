using ItemChecker.Core;
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

        private decimal _commission = 0;
        private decimal _purchase = 0;
        private decimal _price = 0;
        private decimal _get = 0;
        private decimal _precent = 0;
        private decimal _difference = 0;
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
        public decimal Purchase
        {
            get { return _purchase; }
            set
            {
                _purchase = value;
                OnPropertyChanged();
            }
        }
        public decimal Price
        {
            get { return _price; }
            set
            {
                _price = value;
                OnPropertyChanged();
            }
        }
        public decimal Get
        {
            get { return _get; }
            set
            {
                _get = value;
                OnPropertyChanged();
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

        //currency
        public List<string> CurrencyList { get; set; } = SteamBase.CurrencyList.Select(x => x.Name).ToList();
        public int Currency1 { get; set; } = 0;
        public int Currency2 { get; set; } = 1;

        private decimal _value = 1;
        private decimal _converted = Math.Round(SteamBase.CurrencyList.FirstOrDefault(x => x.Id == 5).Value, 2);
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
                OnPropertyChanged();
            }
        }
    }
}
