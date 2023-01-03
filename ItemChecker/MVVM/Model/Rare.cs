using ItemChecker.Core;
using ItemChecker.Properties;
using ItemChecker.Support;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;

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
        public string CurrencySymbolSteam { get; set; } = SteamAccount.Currency.Symbol;
        public List<string> CurrencyList { get; set; } = Currencies.Allow.Select(x => x.Name).ToList();
        public static DataCurrency CurectCurrency { get; set; } = Currencies.Allow.FirstOrDefault(x => x.Id == 1);

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
        public bool Lenticular { get; set; }
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
        public List<int> CountStickers
        {
            get
            {
                return new()
                {
                    0, 1, 2, 3, 4, 5
                };
            }
        }
        public int MinStickerCount { get; set; } = 0;
        public bool Quality { get; set; }
    }
    public class RareCheckConfig
    {
        public int Time { get; set; } = RareProperties.Default.Time;
        public int MinPrecent { get; set; } = RareProperties.Default.MinPrecent;
        public List<string> ComparePrices
        {
            get
            {
                return new()
                {
                    "Lowest ST",
                    "Median ST",
                };
            }
        }
        public int CompareId { get; set; } = RareProperties.Default.CompareId;
        public List<string> Parameters
        {
            get
            {
                return new()
                {
                    "Float",
                    "Sticker",
                    "Doppler (Soon)"
                };
            }
        }
        public int ParameterId { get; set; }

        //float
        public decimal FactoryNew { get; set; } = RareProperties.Default.maxFloatValue_FN;
        public decimal MinimalWear { get; set; } = RareProperties.Default.maxFloatValue_MW;
        public decimal FieldTested { get; set; } = RareProperties.Default.maxFloatValue_FT;
        public decimal WellWorn { get; set; } = RareProperties.Default.maxFloatValue_WW;
        public decimal BattleScarred { get; set; } = RareProperties.Default.maxFloatValue_BS;
        //stickers
        public List<int> StickerCount
        {
            get
            {
                return new()
                {
                    1, 2, 3, 4, 5
                };
            }
        }
        public int MinSticker { get; set; } = 1;
        public string NameContains { get; set; } = string.Empty;
        public bool Normal { get; set; }
        public bool Holo { get; set; }
        public bool Glitter { get; set; }
        public bool Foil { get; set; }
        public bool Gold { get; set; }
        public bool Lenticular { get; set; }
        public bool Contraband { get; set; }
        //dopplers
        public bool Phase1 { get; set; }
        public bool Phase2 { get; set; }
        public bool Phase3 { get; set; }
        public bool Phase4 { get; set; }
        public bool Ruby { get; set; }
        public bool Sapphire { get; set; }
        public bool BlackPearl { get; set; }
        public bool Emerald { get; set; }

        //last saved config
        public static RareCheckConfig CheckedConfig { get; set; } = new();
        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
    public class RareCheckStatus : ObservableObject
    {
        public static System.Timers.Timer Timer { get; set; } = new(1000);
        public static int TimerTick { get; set; }
        public static CancellationTokenSource cts { get; set; } = new();
        public static CancellationToken token { get; set; } = cts.Token;

        public bool IsService
        {
            get
            {
                return _isService;
            }
            set
            {
                _isService = value;
                OnPropertyChanged();
            }
        }
        bool _isService;
        public int Cycles
        {
            get
            {
                return _cycles;
            }
            set
            {
                _cycles = value;
                OnPropertyChanged();
            }
        }
        int _cycles = 0;
        public int PurchasesMade
        {
            get
            {
                return _purchasesMade;
            }
            set
            {
                _purchasesMade = value;
                OnPropertyChanged();
            }
        }
        int _purchasesMade = 0;
        public int Progress
        {
            get { return _progress; }
            set
            {
                _progress = value;
                OnPropertyChanged();
            }
        }
        int _progress = 0;
        public int MaxProgress
        {
            get { return _maxProgress; }
            set
            {
                _maxProgress = value;
                OnPropertyChanged();
            }
        }
        int _maxProgress = 0;
        public string Status
        {
            get { return _status; }
            set
            {
                _status = value;
                OnPropertyChanged();
            }
        }
        string _status = string.Empty;
    }
    public class RareInfo : ObservableObject
    {
        public DataRare Data
        {
            get
            {
                return _data;
            }
            set
            {
                _data = value;
                OnPropertyChanged();
            }
        }
        DataRare _data = new();

        public bool IsBusy
        {
            get
            {
                return _isBusy;
            }
            set
            {
                _isBusy = value;
                OnPropertyChanged();
            }
        }
        bool _isBusy;
    }
}
