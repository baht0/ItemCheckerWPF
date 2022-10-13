using ItemChecker.Core;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace ItemChecker.MVVM.Model
{
    public class ParserTable : ObservableObject
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

        public List<DataParser> Items { get; set; } = new();

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
        private DataParser _selectedItem;
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
        public bool Have { get; set; }
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
}
