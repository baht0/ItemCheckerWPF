using ItemChecker.Core;
using ItemChecker.Net;
using ItemChecker.Support;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ItemChecker.MVVM.Model
{
    public class DataGridRare : BaseMainTable<DataRare>
    {
        public static bool CanBeUpdated { get; set; }

        public void OpenItem(int columnId)
        {
            var item = SelectedItem;
            string market_hash_name = Uri.EscapeDataString(item.ItemName);
            switch (columnId)
            {
                case 1 or 2 or 3:
                    MessageBoxResult result = MessageBox.Show("Inspect in Game?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                        Edit.OpenUrl(item.Link);
                    break;
                case 4:
                    Edit.OpenUrl("https://steamcommunity.com/market/listings/730/" + market_hash_name);
                    break;
                default:
                    Clipboard.SetText(item.ItemName);
                    break;
            }
        }
    }
    public class RareFilter
    {
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
        public List<int> CountStickers => new()
                {
                    0, 1, 2, 3, 4, 5
                };
        public int MinStickerCount { get; set; } = 0;
        public bool SameQuality { get; set; }

        public bool ApplyFilter(DataRare item)
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
                        category = !item.ItemName.Contains("Souvenir") && !item.ItemName.Contains("StatTrak™") && !item.ItemName.Contains("★");
                    if (Stattrak && !category)
                        category = item.ItemName.Contains("StatTrak™");
                    if (Souvenir && !category)
                        category = item.ItemName.Contains("Souvenir");
                    if (KnifeGlove && !category)
                        category = item.ItemName.Contains("★");
                    if (KnifeGloveStattrak && !category)
                        category = item.ItemName.Contains("★ StatTrak™");
                }
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
            //Quality sticker
            bool qualityS = true;
            if (NormalS || Holo || Glitter || Foil || Gold || ContrabandS)
            {
                qualityS = false;
                if (!SameQuality)
                {
                    if (NormalS)
                        qualityS = !item.Stickers.Any(x => x.Contains("(Holo)")) &&
                            !item.Stickers.Any(x => x.Contains("(Glitter)")) &&
                            !item.Stickers.Any(x => x.Contains("(Foil)")) &&
                            !item.Stickers.Any(x => x.Contains("(Gold)")) &&
                            !item.Stickers.Any(x => x == "Sticker | Howling Dawn");
                    if (Holo && !qualityS)
                        qualityS = item.Stickers.Any(x => x.Contains("(Holo)"));
                    if (Glitter && !qualityS)
                        qualityS = item.Stickers.Any(x => x.Contains("(Glitter)"));
                    if (Foil && !qualityS)
                        qualityS = item.Stickers.Any(x => x.Contains("(Foil)"));
                    if (Lenticular && !qualityS)
                        qualityS = item.Stickers.Any(x => x.Contains("(Lenticular)"));
                    if (Gold && !qualityS)
                        qualityS = item.Stickers.Any(x => x.Contains("(Gold)"));
                    if (ContrabandS && !qualityS)
                        qualityS = item.Stickers.Any(x => x == "Sticker | Howling Dawn");
                }
                else if (SameQuality)
                {
                    if (NormalS)
                        qualityS = !item.Stickers.All(x => x.Contains("(Holo)")) &&
                            !item.Stickers.All(x => x.Contains("(Glitter)")) &&
                            !item.Stickers.All(x => x.Contains("(Foil)")) &&
                            !item.Stickers.All(x => x.Contains("(Gold)")) &&
                            !item.Stickers.All(x => x == "Sticker | Howling Dawn");
                    if (Holo && !qualityS)
                        qualityS = item.Stickers.All(x => x.Contains("(Holo)"));
                    if (Glitter && !qualityS)
                        qualityS = item.Stickers.All(x => x.Contains("(Glitter)"));
                    if (Foil && !qualityS)
                        qualityS = item.Stickers.All(x => x.Contains("(Foil)"));
                    if (Lenticular && !qualityS)
                        qualityS = item.Stickers.All(x => x.Contains("(Lenticular)"));
                    if (Gold && !qualityS)
                        qualityS = item.Stickers.All(x => x.Contains("(Gold)"));
                    if (ContrabandS && !qualityS)
                        qualityS = item.Stickers.All(x => x == "Sticker | Howling Dawn");
                }
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
            //phase
            bool phase = true;
            if (Phase1 || Phase2 || Phase3 || Phase4 || Ruby || Sapphire || BlackPearl || Emerald)
            {
                phase = false;
                if (Phase1)
                    phase = item.Phase == Doppler.Phase1;
                if (Phase2 && !phase)
                    phase = item.Phase == Doppler.Phase2;
                if (Phase3 && !phase)
                    phase = item.Phase == Doppler.Phase3;
                if (Phase4 && !phase)
                    phase = item.Phase == Doppler.Phase4;
                if (Ruby && !phase)
                    phase = item.Phase == Doppler.Ruby;
                if (Sapphire && !phase)
                    phase = item.Phase == Doppler.Sapphire;
                if (BlackPearl && !phase)
                    phase = item.Phase == Doppler.BlackPearl;
                if (Emerald && !phase)
                    phase = item.Phase == Doppler.Emerald;
            }
            //Prices
            bool prices = true;
            if (Price || Compare)
            {
                if (Price)
                    prices = PriceFrom < item.Price && PriceTo > item.Price;
                if (Compare && prices)
                    prices = CompareFrom < item.PriceCompare && CompareTo > item.PriceCompare;
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
            //other
            bool other = true;
            if (item.Stickers.Any() && MinStickerCount != 0)
            {
                other = false;
                if (item.Stickers.Any())
                    other = MinStickerCount <= item.Stickers.Count;
            }

            bool isShow = category && exterior && quality && qualityS && prices && profit && other;
            return isShow;
        }
    }
    public class DataRare : ObservableObject
    {
        public int ParameterId { get; set; }
        public int CompareId { get; set; }
        public string ItemName { get; set; } = "Unknown";
        public decimal Price { get; set; }
        public decimal PriceCompare { get; set; }
        public decimal Precent { get; set; }
        public decimal Difference { get; set; }
        public string Link { get; set; }

        public decimal FloatValue { get; set; }
        public List<string> Stickers { get; set; } = new();
        public Doppler Phase { get; set; } = Doppler.None;
        public DateTime Checked { get; set; } = DateTime.Now;

        public DataListingItem DataBuy { get; set; } = new();

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
        public void BuyItem()
        {
            MessageBoxResult result = MessageBox.Show(
                        "Are you sure you want to buy this item?", "Question",
                        MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.No)
                return;

            IsBusy = true;
            Task.Run(() =>
            {
                try
                {
                    var response = SteamRequest.Post.BuyListing(ItemName, DataBuy.ListingId, DataBuy.Fee, DataBuy.Subtotal, DataBuy.Total, SteamAccount.Currency.Id);
                    string message = response.StatusCode == System.Net.HttpStatusCode.OK ? $"{ItemName}\nWas bought." : "Something went wrong...";
                    Main.Message.Enqueue(message);
                }
                catch (Exception exp)
                {
                    BaseModel.ErrorLog(exp, true);
                }
                finally
                {
                    IsBusy = false;
                }
            });
        }
    }
    public enum Doppler
    {
        None,
        Phase1,
        Phase2,
        Phase3,
        Phase4,
        Ruby,
        Sapphire,
        BlackPearl,
        Emerald
    }
}
