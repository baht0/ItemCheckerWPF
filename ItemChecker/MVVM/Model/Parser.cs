using ItemChecker.Core;
using ItemChecker.Support;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace ItemChecker.MVVM.Model
{
    public class ParserTable : ObservableObject
    {
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
        int _currencyId = 0;
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
        string _currencySymbol = "$";

        public string CurrencySymbolSteam { get; set; } = SteamAccount.Currency.Symbol;
        public List<string> CurrencyList { get; set; } = Currencies.Allow.Select(x => x.Name).ToList();
        public static DataCurrency CurectCurrency { get; set; } = Currencies.Allow.FirstOrDefault(x => x.Id == 1);

        public List<DataParser> Items { get; set; } = new();

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
        ICollectionView _gridView;
        public DataParser SelectedItem
        {
            get
            {
                return _selectedItem;
            }
            set
            {
                _selectedItem = value;
                OnPropertyChanged();
            }
        }
        DataParser _selectedItem;

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
        int _count = 0;
    }
    public class ParserFilter
    {
        public static ParserFilter FilterConfig { get; set; }

        //category
        public bool Normal { get; set; }
        public bool Stattrak { get; set; }
        public bool Souvenir { get; set; }
        public bool KnifeGlove { get; set; }
        public bool KnifeGloveStattrak { get; set; }
        //other
        public List<string> Weapons { get; set; } = new()
        {
            "Any",
            "AK-47",
            "AUG",
            "AWP",
            "CZ75-Auto",
            "Desert Eagle",
            "Dual Berettas",
            "FAMAS",
            "Five-SeveN",
            "G3SG1",
            "Galil AR",
            "Glock-18",
            "M4A1-S",
            "M4A4",
            "M249",
            "MAC-10",
            "MAG-7",
            "MP5-SD",
            "MP7",
            "MP9",
            "Negev",
            "Nova",
            "P90",
            "P250",
            "P2000",
            "PP-Bizon",
            "R8 Revolver",
            "Sawed-Off",
            "SCAR-20",
            "SG 553",
            "SSG 08",
            "Tec-9",
            "UMP-45",
            "USP-S",
            "XM1014",
            "Bayonet",
            "Bowie Knife",
            "Butterfly Knife",
            "Classic Knife",
            "Falchion Knife",
            "Flip Knife",
            "Gut Knife",
            "Huntsman Knife",
            "Karambit",
            "M9 Bayonet",
            "Navaja Knife",
            "Nomad Knife",
            "Paracord Knife",
            "Shadow Daggers",
            "Skeleton Knife",
            "Stiletto Knife",
            "Survival Knife",
            "Talon Knife",
            "Ursus Knife",
        };
        public string SelectedWeapon { get; set; } = "Any";
        public bool HidePlaced { get; set; }
        //exterior
        public bool NotPainted { get; set; }
        public bool BattleScarred { get; set; }
        public bool WellWorn { get; set; }
        public bool FieldTested { get; set; }
        public bool MinimalWear { get; set; }
        public bool FactoryNew { get; set; }
        //type
        public bool Weapon { get; set; }
        public bool Knife { get; set; }
        public bool Gloves { get; set; }
        public bool Sticker { get; set; }
        public bool Agent { get; set; }
        public bool Capsule { get; set; }
        public bool Patch { get; set; }
        public bool Collectible { get; set; }
        public bool Key { get; set; }
        public bool Pass { get; set; }
        public bool MusicKit { get; set; }
        public bool Graffiti { get; set; }
        public bool Case { get; set; }
        public bool Package { get; set; }
        public bool PatchPack { get; set; }
        //Quality
        public bool Industrial { get; set; }
        public bool MilSpec { get; set; }
        public bool Restricted { get; set; }
        public bool Classified { get; set; }
        public bool Covert { get; set; }
        public bool Contraband { get; set; }
        //price
        public bool Price1 { get; set; }
        public bool Price2 { get; set; }
        public bool Price3 { get; set; }
        public decimal Price1From { get; set; }
        public decimal Price1To { get; set; }
        public decimal Price2From { get; set; }
        public decimal Price2To { get; set; }
        public decimal Price3From { get; set; }
        public decimal Price3To { get; set; }
        //profit
        public decimal PrecentFrom { get; set; }
        public decimal PrecentTo { get; set; }
        public decimal DifferenceFrom { get; set; }
        public decimal DifferenceTo { get; set; }
    }
    public class DataParser
    {
        public string ItemName { get; set; }
        public decimal Purchase { get; set; }
        public decimal Price { get; set; }
        public decimal Get { get; set; }
        public decimal Precent { get; set; }
        public decimal Difference { get; set; }
        public bool Have { get; set; }
    }

    public class ImportParser : ObservableObject
    {
        public ObservableCollection<ImportFile> List
        {
            get { return _list; }
            set
            {
                _list = value;
                OnPropertyChanged();
            }
        }
        ObservableCollection<ImportFile> _list = new();
        public ImportFile Selected
        {
            get
            {
                return _selected;
            }
            set
            {
                _selected = value;
                OnPropertyChanged();
            }
        }
        ImportFile _selected = new();
    }
    public class ImportFile : ParserCheckConfig
    {
        public string Path { get; set; }
        public int Size { get; set; }
    }
}
