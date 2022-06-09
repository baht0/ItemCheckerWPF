using ItemChecker.Core;
using ItemChecker.Support;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ItemChecker.MVVM.Model
{
    public class Main
    {
        public static SnackbarMessageQueue Message { get; set; } = new();
        public static ObservableCollection<DataNotification> Notifications { get; set; } = new();
    }
    public class MainInfo
    {
        public decimal Currency { get; set; } = SteamBase.CurrencyList.FirstOrDefault(x => x.Id == SteamAccount.CurrencyId).Value;
        public string CurrencySymbol { get; set; } = SteamBase.CurrencyList.FirstOrDefault(x => x.Id == SteamAccount.CurrencyId).Symbol;
        public decimal Balance { get; set; } = SteamAccount.Balance;
        public decimal BalanceCsm { get; set; } = CsmAccount.Balance;
        public decimal BalanceUsd { get; set; } = Edit.ConverterToUsd(SteamAccount.Balance, SteamBase.CurrencyList.FirstOrDefault(x => x.Id == SteamAccount.CurrencyId).Value);
        public int CountBase { get; set; } = SteamBase.ItemList.Count;
        public string StatusCommunity { get; set; } = SteamBase.StatusCommunity == "normal" ? "CheckCircle" : "CloseCircle";

        public SnackbarMessageQueue Message { get; set; } = Main.Message;
        public bool IsNotification { get; set; } = Main.Notifications.Any(x => !x.IsRead);
        public ObservableCollection<DataNotification> Notifications { get; set; } = new(Main.Notifications.OrderByDescending(x => x.Date));
    }
    public class Calculator : ObservableObject
    {
        public static decimal CommissionSteam { get; set; } = 0.869565m;
        public static decimal CommissionCsm { get; set; } = 0.93m;
        public static decimal CommissionLf { get; set; } = 0.95m;
        public static decimal CommissionBuff { get; set; } = 0.975m;

        //config
        public List<string> Services { get; set; } = new()
        {
            "SteamMarket(A)",
            "SteamMarket",
            "Cs.Money",
            "Loot.Farm",
            "Buff163"
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
                decimal dol = value;
                decimal rub = SteamBase.CurrencyList.FirstOrDefault(x => x.Id == 5).Value;
                decimal cny = SteamBase.CurrencyList.FirstOrDefault(x => x.Id == 23).Value;
                switch (Currency1) //any -> dol
                {
                    case 1:
                        dol = Edit.ConverterToUsd(value, rub);
                        break;
                    case 2:
                        dol = Edit.ConverterToUsd(value, cny);
                        break;
                }
                switch (Currency2)//dol -> any
                {
                    case 0:
                        Converted = dol;
                        break;
                    case 1:
                        Converted = Edit.ConverterFromUsd(dol, rub);
                        break;
                    case 2:
                        Converted = Edit.ConverterFromUsd(dol, cny);
                        break;
                }
                OnPropertyChanged();
            }
        }

        void Compare(bool isPrice, bool isResult)
        {
            decimal commission = 0;
            switch (Service)
            {
                case 0 or 1:
                    commission = CommissionSteam;
                    break;
                case 2:
                    commission = CommissionCsm;
                    break;
                case 3:
                    commission = CommissionLf;
                    break;
                case 4:
                    commission = CommissionBuff;
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