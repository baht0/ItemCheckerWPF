using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using ItemChecker.Core;
using ItemChecker.Support;

namespace ItemChecker.MVVM.Model
{
    public class DataGridParse : BaseMainTable<DataParser>
    {
        public static bool CanBeUpdated { get; set; }

        public void ShowItemInService(int columnId, int serviceOne, int serviceTwo)
        {
            DataParser item = SelectedItem;
            string itemName = item.ItemName.Replace("(Holo/Foil)", "(Holo-Foil)");
            string market_hash_name = Uri.EscapeDataString(itemName);
            switch (columnId)
            {
                case 1:
                    switch (serviceOne)
                    {
                        case 0 or 1:
                            Edit.OpenUrl("https://steamcommunity.com/market/listings/730/" + market_hash_name);
                            break;
                        case 2:
                            Edit.OpenCsm(itemName);
                            break;
                        case 3:
                            Clipboard.SetText(itemName);
                            Edit.OpenUrl("https://loot.farm/");
                            break;
                        case 4:
                            var id = ItemsBase.List.FirstOrDefault(x => x.ItemName == item.ItemName).Buff.Id;
                            if (id != 0)
                                Edit.OpenUrl("https://buff.163.com/goods/" + id + "#tab=buying");
                            else
                                Edit.OpenUrl("https://buff.163.com/market/csgo#tab=buying&page_num=1&search=" + market_hash_name);
                            break;
                        case 5:
                            id = ItemsBase.List.FirstOrDefault(x => x.ItemName == item.ItemName).Buff.Id;
                            if (id != 0)
                                Edit.OpenUrl("https://buff.163.com/goods/" + id);
                            else
                                Edit.OpenUrl("https://buff.163.com/market/csgo#tab=selling&page_num=1&search=" + market_hash_name);
                            break;
                    }
                    break;
                case 2 or 3:
                    switch (serviceTwo)
                    {
                        case 0 or 1:
                            Edit.OpenUrl("https://steamcommunity.com/market/listings/730/" + market_hash_name);
                            break;
                        case 2:
                            Edit.OpenCsm(itemName);
                            break;
                        case 3:
                            Clipboard.SetText(itemName);
                            Edit.OpenUrl("https://loot.farm/");
                            break;
                        case 4:
                            var id = ItemsBase.List.FirstOrDefault(x => x.ItemName == item.ItemName).Buff.Id;
                            if (id != 0)
                                Edit.OpenUrl("https://buff.163.com/goods/" + id + "#tab=buying");
                            else
                                Edit.OpenUrl("https://buff.163.com/market/csgo#tab=buying&page_num=1&search=" + market_hash_name);
                            break;
                        case 5:
                            id = ItemsBase.List.FirstOrDefault(x => x.ItemName == item.ItemName).Buff.Id;
                            if (id != 0)
                                Edit.OpenUrl("https://buff.163.com/goods/" + id);
                            else
                                Edit.OpenUrl("https://buff.163.com/market/csgo#tab=selling&page_num=1&search=" + market_hash_name);
                            break;
                    }
                    break;
                default:
                    Clipboard.SetText(itemName);
                    break;
            }
        }
    }
    public class ParserFilter : ObservableObject
    {
        //category
        public bool Normal { get; set; }
        public bool Stattrak { get; set; }
        public bool Souvenir { get; set; }
        public bool KnifeGlove { get; set; }
        public bool KnifeGloveStattrak { get; set; }
        //other
        public List<string> Weapons => new()
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
        public bool Agent { get; set; }
        public bool Sticker { get; set; }
        public bool Patch { get; set; }
        public bool Collectible { get; set; }
        public bool Key { get; set; }
        public bool Pass { get; set; }
        public bool MusicKit { get; set; }
        public bool Graffiti { get; set; }
        public bool Container { get; set; }
        public bool Gift { get; set; }
        public bool Tool { get; set; }
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

        public bool ApplyFilter(DataParser item)
        {
            var baseItem = ItemsBase.List.FirstOrDefault(x => x.ItemName == item.ItemName);
            //category
            bool category = true;
            if (Normal || Stattrak || Souvenir || KnifeGlove || KnifeGloveStattrak)
            {
                category = false;
                if (baseItem.Type == Type.Weapon || baseItem.Type == Type.Knife || baseItem.Type == Type.Gloves)
                {
                    if (Normal)
                        category = baseItem.Type == Type.Weapon && !item.ItemName.Contains("Souvenir") && !item.ItemName.Contains("StatTrak™");
                    if (Stattrak && !category)
                        category = baseItem.Type == Type.Weapon && item.ItemName.Contains("StatTrak™");
                    if (Souvenir && !category)
                        category = baseItem.Type == Type.Weapon && item.ItemName.Contains("Souvenir");
                    if (KnifeGlove && !category)
                        category = (baseItem.Type == Type.Knife || baseItem.Type == Type.Gloves) && !item.ItemName.Contains("★ StatTrak™");
                    if (KnifeGloveStattrak && !category)
                        category = (baseItem.Type == Type.Knife || baseItem.Type == Type.Gloves) && item.ItemName.Contains("★ StatTrak™");
                }
            }
            //other
            bool other = true;
            if (SelectedWeapon != "Any" || HidePlaced)
            {
                other = false;
                if (SelectedWeapon != "Any")
                    other = item.ItemName.Contains(SelectedWeapon);
                if (HidePlaced && !other)
                    other = !SteamAccount.Orders.Any(x => x.ItemName == item.ItemName);
            }
            //exterior
            bool exterior = true;
            if (NotPainted || BattleScarred || WellWorn || FieldTested || MinimalWear || FactoryNew)
            {
                exterior = false;
                if (NotPainted)
                    exterior = !item.ItemName.Contains("Battle-Scarred")
                        && !item.ItemName.Contains("Well-Worn")
                        && !item.ItemName.Contains("Field-Tested")
                        && !item.ItemName.Contains("Minimal Wear")
                        && !item.ItemName.Contains("Factory New")
                        && (baseItem.Type == Type.Knife || baseItem.Type == Type.Gloves);
                if (BattleScarred && !exterior)
                    exterior = item.ItemName.Contains("Battle-Scarred");
                if (WellWorn && !exterior)
                    exterior = item.ItemName.Contains("Well-Worn");
                if (FieldTested && !exterior)
                    exterior = item.ItemName.Contains("Field-Tested");
                if (MinimalWear && !exterior)
                    exterior = item.ItemName.Contains("Minimal Wear");
                if (FactoryNew && !exterior)
                    exterior = item.ItemName.Contains("Factory New");
            }
            //Quality
            bool quality = true;
            if (Industrial || MilSpec || Restricted || Classified || Covert || Contraband)
            {
                quality = false;
                if (Industrial)
                    quality = baseItem.Quality == Quality.IndustrialGrade;
                if (MilSpec && !quality)
                    quality = baseItem.Quality == Quality.MilSpec;
                if (Restricted && !quality)
                    quality = baseItem.Quality == Quality.Restricted;
                if (Classified && !quality)
                    quality = baseItem.Quality == Quality.Classified;
                if (Covert && !quality)
                    quality = baseItem.Quality == Quality.Covert;
                if (Contraband && !quality)
                    quality = baseItem.Quality == Quality.Contraband;
            }
            //types
            bool types = true;
            if (Weapon || Knife || Gloves || Sticker || Patch || Collectible || Key || Pass || MusicKit || Graffiti || Container || Gift || Tool)
            {
                types = false;
                if (Weapon)
                    types = baseItem.Type == Type.Weapon;
                if (Knife && !types)
                    types = baseItem.Type == Type.Knife;
                if (Gloves && !types)
                    types = baseItem.Type == Type.Gloves;
                if (Agent && !types)
                    types = baseItem.Type == Type.Agent;
                if (Sticker && !types)
                    types = baseItem.Type == Type.Sticker;
                if (Patch && !types)
                    types = baseItem.Type == Type.Patch;
                if (Collectible && !types)
                    types = baseItem.Type == Type.Collectable;
                if (Key && !types)
                    types = baseItem.Type == Type.Key;
                if (Pass && !types)
                    types = baseItem.Type == Type.Pass;
                if (MusicKit && !types)
                    types = baseItem.Type == Type.MusicKit;
                if (Graffiti && !types)
                    types = baseItem.Type == Type.Graffiti;
                if (Container && !types)
                    types = baseItem.Type == Type.Container;
                if (Gift && !types)
                    types = baseItem.Type == Type.Gift;
                if (Tool && !types)
                    types = baseItem.Type == Type.Tool;
            }
            //Prices
            bool prices = true;
            if (Price1 || Price2 || Price3)
            {
                if (Price1)
                    prices = Price1From < item.Purchase && Price1To > item.Purchase;
                if (Price2 && prices)
                    prices = Price2From < item.Price && Price2To > item.Price;
                if (Price3 && prices)
                    prices = Price3From < item.Get && Price3To > item.Get;
            }
            //profit
            bool profit = true;
            if (PrecentFrom != 0 || PrecentTo != 0 || DifferenceFrom != 0 || DifferenceTo != 0)
            {
                if (PrecentFrom != 0)
                    profit = PrecentFrom < item.Precent;
                if (PrecentTo != 0 && profit)
                    profit = PrecentTo > item.Precent;
                if (DifferenceFrom != 0 && profit)
                    profit = DifferenceFrom < item.Difference;
                if (DifferenceTo != 0 && profit)
                    profit = DifferenceTo > item.Difference;
            }

            bool isShow = category && other && exterior && quality && types && prices && profit;
            return isShow;
        }
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
}
