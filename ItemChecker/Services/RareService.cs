using ItemChecker.MVVM.Model;
using System.Linq;

namespace ItemChecker.Services
{
    public class RareService
    {
        public static bool ApplyFilter(RareFilter filterConfig, DataRare item)
        {
            var baseItem = ItemsBase.List.FirstOrDefault(x => x.ItemName == item.ItemName);
            //category
            bool category = true;
            if (filterConfig.Normal || filterConfig.Stattrak || filterConfig.Souvenir || filterConfig.KnifeGlove || filterConfig.KnifeGloveStattrak)
            {
                category = false;
                if (baseItem.Type == "Weapon" || baseItem.Type == "Knife" || baseItem.Type == "Gloves")
                {
                    if (filterConfig.Normal)
                        category = !item.ItemName.Contains("Souvenir") && !item.ItemName.Contains("StatTrak™") && !item.ItemName.Contains("★");
                    if (filterConfig.Stattrak && !category)
                        category = item.ItemName.Contains("StatTrak™");
                    if (filterConfig.Souvenir && !category)
                        category = item.ItemName.Contains("Souvenir");
                    if (filterConfig.KnifeGlove && !category)
                        category = item.ItemName.Contains("★");
                    if (filterConfig.KnifeGloveStattrak && !category)
                        category = item.ItemName.Contains("★ StatTrak™");
                }
            }
            //exterior
            bool exterior = true;
            if (filterConfig.NotPainted || filterConfig.BattleScarred || filterConfig.WellWorn || filterConfig.FieldTested || filterConfig.MinimalWear || filterConfig.FactoryNew)
            {
                exterior = false;
                if (filterConfig.NotPainted)
                    exterior = !item.ItemName.Contains("Battle-Scarred") &&
                        !item.ItemName.Contains("Well-Worn") &&
                        !item.ItemName.Contains("Field-Tested") &&
                        !item.ItemName.Contains("Minimal Wear") &&
                        !item.ItemName.Contains("Factory New") &&
                        baseItem.Type.Contains("KnifeGlove");
                if (filterConfig.BattleScarred && !exterior)
                    exterior = item.ItemName.Contains("Battle-Scarred");
                if (filterConfig.WellWorn && !exterior)
                    exterior = item.ItemName.Contains("Well-Worn");
                if (filterConfig.FieldTested && !exterior)
                    exterior = item.ItemName.Contains("Field-Tested");
                if (filterConfig.MinimalWear && !exterior)
                    exterior = item.ItemName.Contains("Minimal Wear");
                if (filterConfig.FactoryNew && !exterior)
                    exterior = item.ItemName.Contains("Factory New");
            }
            //Quality sticker
            bool qualityS = true;
            if (filterConfig.NormalS || filterConfig.Holo || filterConfig.Glitter || filterConfig.Foil || filterConfig.Gold || filterConfig.ContrabandS)
            {
                qualityS = false;
                if (!filterConfig.Quality)
                {
                    if (filterConfig.NormalS)
                        qualityS = !item.Stickers.Any(x => x.Contains("(Holo)")) &&
                            !item.Stickers.Any(x => x.Contains("(Glitter)")) &&
                            !item.Stickers.Any(x => x.Contains("(Foil)")) &&
                            !item.Stickers.Any(x => x.Contains("(Gold)")) &&
                            !item.Stickers.Any(x => x == "Sticker | Howling Dawn");
                    if (filterConfig.Holo && !qualityS)
                        qualityS = item.Stickers.Any(x => x.Contains("(Holo)"));
                    if (filterConfig.Glitter && !qualityS)
                        qualityS = item.Stickers.Any(x => x.Contains("(Glitter)"));
                    if (filterConfig.Foil && !qualityS)
                        qualityS = item.Stickers.Any(x => x.Contains("(Foil)"));
                    if (filterConfig.Lenticular && !qualityS)
                        qualityS = item.Stickers.Any(x => x.Contains("(Lenticular)"));
                    if (filterConfig.Gold && !qualityS)
                        qualityS = item.Stickers.Any(x => x.Contains("(Gold)"));
                    if (filterConfig.ContrabandS && !qualityS)
                        qualityS = item.Stickers.Any(x => x == "Sticker | Howling Dawn");
                }
                else if (filterConfig.Quality)
                {
                    if (filterConfig.NormalS)
                        qualityS = !item.Stickers.All(x => x.Contains("(Holo)")) &&
                            !item.Stickers.All(x => x.Contains("(Glitter)")) &&
                            !item.Stickers.All(x => x.Contains("(Foil)")) &&
                            !item.Stickers.All(x => x.Contains("(Gold)")) &&
                            !item.Stickers.All(x => x == "Sticker | Howling Dawn");
                    if (filterConfig.Holo && !qualityS)
                        qualityS = item.Stickers.All(x => x.Contains("(Holo)"));
                    if (filterConfig.Glitter && !qualityS)
                        qualityS = item.Stickers.All(x => x.Contains("(Glitter)"));
                    if (filterConfig.Foil && !qualityS)
                        qualityS = item.Stickers.All(x => x.Contains("(Foil)"));
                    if (filterConfig.Lenticular && !qualityS)
                        qualityS = item.Stickers.All(x => x.Contains("(Lenticular)"));
                    if (filterConfig.Gold && !qualityS)
                        qualityS = item.Stickers.All(x => x.Contains("(Gold)"));
                    if (filterConfig.ContrabandS && !qualityS)
                        qualityS = item.Stickers.All(x => x == "Sticker | Howling Dawn");
                }
            }
            //Quality
            bool quality = true;
            if (filterConfig.Industrial || filterConfig.MilSpec || filterConfig.Restricted || filterConfig.Classified || filterConfig.Covert || filterConfig.Contraband)
            {
                quality = false;
                if (filterConfig.Industrial)
                    quality = baseItem.Quality == "Industrial Grade";
                if (filterConfig.MilSpec && !quality)
                    quality = baseItem.Quality == "Mil-Spec";
                if (filterConfig.Restricted && !quality)
                    quality = baseItem.Quality == "Restricted";
                if (filterConfig.Classified && !quality)
                    quality = baseItem.Quality == "Classified";
                if (filterConfig.Covert && !quality)
                    quality = baseItem.Quality == "Covert";
                if (filterConfig.Contraband && !quality)
                    quality = baseItem.Quality == "Contraband";
            }
            //phase
            bool phase = true;
            if (filterConfig.Phase1 || filterConfig.Phase2 || filterConfig.Phase3 || filterConfig.Phase4 || filterConfig.Ruby || filterConfig.Sapphire || filterConfig.BlackPearl || filterConfig.Emerald)
            {
                phase = false;
                if (filterConfig.Phase1)
                    phase = item.Phase == "Phase 1";
                if (filterConfig.Phase2 && !phase)
                    phase = item.Phase == "Phase 2";
                if (filterConfig.Phase3 && !phase)
                    phase = item.Phase == "Phase 3";
                if (filterConfig.Phase4 && !phase)
                    phase = item.Phase == "Phase 4";
                if (filterConfig.Ruby && !phase)
                    phase = item.Phase == "Ruby";
                if (filterConfig.Sapphire && !phase)
                    phase = item.Phase == "Sapphire";
                if (filterConfig.BlackPearl && !phase)
                    phase = item.Phase == "Black Pearl";
                if (filterConfig.Emerald && !phase)
                    phase = item.Phase == "Emerald";
            }
            //Prices
            bool prices = true;
            if (filterConfig.Price || filterConfig.Compare)
            {
                if (filterConfig.Price)
                    prices = filterConfig.PriceFrom < item.Price && filterConfig.PriceTo > item.Price;
                if (filterConfig.Compare && prices)
                    prices = filterConfig.CompareFrom < item.PriceCompare && filterConfig.CompareTo > item.PriceCompare;
            }
            //profit
            bool profit = true;
            if (filterConfig.PrecentFrom != 0 || filterConfig.PrecentTo != 0 || filterConfig.DifferenceFrom != 0 || filterConfig.DifferenceTo != 0)
            {
                if (filterConfig.PrecentFrom != 0)
                    profit = filterConfig.PrecentFrom < item.Precent;
                if (filterConfig.PrecentTo != 0 && profit)
                    profit = filterConfig.PrecentTo > item.Precent;
                if (filterConfig.DifferenceFrom != 0 && profit)
                    profit = filterConfig.DifferenceFrom < item.Difference;
                if (filterConfig.DifferenceTo != 0 && profit)
                    profit = filterConfig.DifferenceTo > item.Difference;
            }
            //other
            bool other = true;
            if (item.Stickers.Any() && filterConfig.MinStickerCount != 0)
            {
                other = false;
                if (item.Stickers.Any())
                    other = filterConfig.MinStickerCount <= item.Stickers.Count;
            }

            bool isShow = category && exterior && quality && qualityS && prices && profit && other;
            return isShow;
        }
    }
}
