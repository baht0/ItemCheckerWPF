using ItemChecker.Core;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace ItemChecker.MVVM.Model
{
    public class RareTable : ObservableObject
    {
        private string _currencySymbol = "$";
        private int _currencyId = 0;
        public int CurrencyId
        {
            get
            {
                return _currencyId;
            }
            set
            {
                _currencyId = value;
                OnPropertyChanged();
            }
        }
        public string CurrencySymbol
        {
            get
            {
                return _currencySymbol;
            }
            set
            {
                _currencySymbol = value;
                OnPropertyChanged();
            }
        }
        public string CurrencySymbolSteam { get; set; } = SteamBase.AllowCurrencys.FirstOrDefault(x => x.Id == SteamAccount.CurrencyId).Symbol;
        public List<string> CurrencyList { get; set; } = SteamBase.AllowCurrencys.Select(x => x.Name).ToList();
        public static Currency CurectCurrency { get; set; } = SteamBase.AllowCurrencys.FirstOrDefault(x => x.Id == 1);

        public List<DataRare> Items { get; set; } = new();

        private ICollectionView _gridView;
        public ICollectionView GridView
        {
            get
            {
                return _gridView;
            }
            set
            {
                _gridView = value;
                OnPropertyChanged();
            }
        }

        private int _count = 0;
        public int Count
        {
            get
            {
                return _count;
            }
            set
            {
                _count = value;
                OnPropertyChanged();
            }
        }
    }
    public class RareFilter
    {
        public static RareFilter FilterConfig { get; set; }

        //category
        public bool Normal { get; set; }
        public bool Stattrak { get; set; }
        public bool Souvenir { get; set; }
        public bool KnifeGlove { get; set; }
        public bool KnifeGloveStattrak { get; set; }
        //exterior
        public bool NotPainted { get; set; }
        public bool BattleScarred { get; set; }
        public bool WellWorn { get; set; }
        public bool FieldTested { get; set; }
        public bool MinimalWear { get; set; }
        public bool FactoryNew { get; set; }
        //Quality stickers
        public bool NormalS { get; set; }
        public bool Holo { get; set; }
        public bool Glitter { get; set; }
        public bool Foil { get; set; }
        public bool Gold { get; set; }
        public bool ContrabandS { get; set; }
        //Quality
        public bool Industrial { get; set; }
        public bool MilSpec { get; set; }
        public bool Restricted { get; set; }
        public bool Classified { get; set; }
        public bool Covert { get; set; }
        public bool Contraband { get; set; }
        //pattern
        public bool Phase1 { get; set; }
        public bool Phase2 { get; set; }
        public bool Phase3 { get; set; }
        public bool Phase4 { get; set; }
        public bool Ruby { get; set; }
        public bool Sapphire { get; set; }
        public bool BlackPearl { get; set; }
        public bool Emerald { get; set; }
        //price
        public bool Price { get; set; }
        public bool Compare { get; set; }
        public decimal PriceFrom { get; set; }
        public decimal PriceTo { get; set; }
        public decimal CompareFrom { get; set; }
        public decimal CompareTo { get; set; }
        //profit
        public decimal PrecentFrom { get; set; }
        public decimal PrecentTo { get; set; }
        public decimal DifferenceFrom { get; set; }
        public decimal DifferenceTo { get; set; }
        //other
        public bool AllSticker { get; set; }
        public int CountStickers { get; set; }
    }
}
