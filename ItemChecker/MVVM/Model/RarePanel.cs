using ItemChecker.Core;
using ItemChecker.Properties;
using System.Collections.Generic;
using System.Threading;

namespace ItemChecker.MVVM.Model
{
    public class RareCheckConfig
    {
        public int Time { get; set; } = RareProperties.Default.Time;
        public int MinPrecent { get; set; } = RareProperties.Default.MinPrecent;
        public List<string> ComparePrices { get; set; } = new()
                {
                    "Lowest ST",
                    "Median ST",
                    "Buy CSM"
                };
        public int CompareId { get; set; } = RareProperties.Default.CompareId;
        public List<string> Parameters { get; set; } = new()
                {
                    "Float",
                    "Sticker",
                    "Doppler (Soon)"
                };
        public int ParameterId { get; set; } = RareProperties.Default.ParameterId;

        //stickers
        public bool Normal { get; set; } = RareProperties.Default.IsNormal;
        public bool Holo { get; set; } = RareProperties.Default.IsHolo;
        public bool Glitter { get; set; } = RareProperties.Default.IsGlitter;
        public bool Foil { get; set; } = RareProperties.Default.IsFoil;
        public bool Gold { get; set; } = RareProperties.Default.IsGold;
        public bool Contraband { get; set; } = RareProperties.Default.IsContraband;
        public int StickerCount { get; set; } = RareProperties.Default.StickerCount;
        public string NameContains { get; set; } = RareProperties.Default.NameContains;
        //stickers
        public bool Phase1 { get; set; } = RareProperties.Default.Phase1;
        public bool Phase2 { get; set; } = RareProperties.Default.Phase2;
        public bool Phase3 { get; set; } = RareProperties.Default.Phase3;
        public bool Phase4 { get; set; } = RareProperties.Default.Phase4;
        public bool Ruby { get; set; } = RareProperties.Default.Ruby;
        public bool Sapphire { get; set; } = RareProperties.Default.Sapphire;
        public bool BlackPearl { get; set; } = RareProperties.Default.BlackPearl;
        public bool Emerald { get; set; } = RareProperties.Default.Emerald;

        //float
        public decimal FactoryNew { get; set; } = RareProperties.Default.maxFloatValue_FN;
        public decimal MinimalWear { get; set; } = RareProperties.Default.maxFloatValue_MW;
        public decimal FieldTested { get; set; } = RareProperties.Default.maxFloatValue_FT;
        public decimal WellWorn { get; set; } = RareProperties.Default.maxFloatValue_WW;
        public decimal BattleScarred { get; set; } = RareProperties.Default.maxFloatValue_BS;

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

        //status
        private bool _isService;
        private int _cycles = 0;
        private int _purchasesMade = 0;
        private int _progress = 0;
        private int _maxProgress = 0;
        private string _status = "Off";

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
        public int Progress
        {
            get { return _progress; }
            set
            {
                _progress = value;
                OnPropertyChanged();
            }
        }
        public int MaxProgress
        {
            get { return _maxProgress; }
            set
            {
                _maxProgress = value;
                OnPropertyChanged();
            }
        }
        public string Status
        {
            get { return _status; }
            set
            {
                _status = value;
                OnPropertyChanged();
            }
        }
    }
    public class RareInfo : ObservableObject
    {
        private DataRare _data = new();
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
    }
}
