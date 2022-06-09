using ItemChecker.Properties;
using ItemChecker.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ItemChecker.MVVM.Model
{
    public class ParserService : ItemBaseService
    {
        public void Export(List<DataParser> parserGrid, ParserCheckConfig parserConfig)
        {
            string items = JsonConvert.SerializeObject(parserGrid, Formatting.Indented);
            JObject json = new(
                new JProperty("Size", parserGrid.Count),
                new JProperty("ParserCheckConfig", JObject.FromObject(parserConfig)),
                new JProperty("Items", JArray.Parse(items)));

            string path = ProjectInfo.DocumentPath + "extract\\";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            File.WriteAllText(path + $"Parser_{parserConfig.CheckedTime:dd.MM_hh.mm}_{parserConfig.ServiceOne}{parserConfig.ServiceTwo}.json", json.ToString());
        }
        public List<DataParser> Import()
        {
            string file = OpenFileDialog("json");
            if (String.IsNullOrEmpty(file))
                return new();
            JObject json = JObject.Parse(file);

            ParserProperties.Default.ServiceOne = Convert.ToInt32(json["Service1"]);
            ParserProperties.Default.ServiceTwo = Convert.ToInt32(json["Service2"]);
            ParserCheckConfig.CheckedConfig = json["ParserCheckConfig"].ToObject<ParserCheckConfig>();

            List<DataParser> data = JsonConvert.DeserializeObject<List<DataParser>>(json["Items"].ToString());

            if (ParserCheckConfig.CheckedConfig.ServiceOne < 2 || ParserCheckConfig.CheckedConfig.ServiceTwo < 2)// with comision
                foreach (var item in data)
                {
                    var itemBase = SteamBase.ItemList.FirstOrDefault(x => x.ItemName == item.ItemName).Steam;
                    if (ParserCheckConfig.CheckedConfig.ServiceOne < 2)
                    {
                        itemBase.LowestSellOrder = item.Price1;
                        itemBase.HighestBuyOrder = item.Price2;
                    }
                    if (ParserCheckConfig.CheckedConfig.ServiceTwo < 2)
                    {
                        itemBase.LowestSellOrder = item.Price3;
                        itemBase.HighestBuyOrder = item.Price4;
                    }
                }

            return data;
        }

        public static bool ApplyFilter(ParserFilter filterConfig, DataParser item)
        {
            var baseItem = SteamBase.ItemList.FirstOrDefault(x => x.ItemName == item.ItemName);
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
            //other
            bool other = true;
            if (filterConfig.Have || filterConfig.SelectedWeapon != "Any")
            {
                if (filterConfig.Have)
                    other = item.Have;
                if (filterConfig.SelectedWeapon != "Any" && other)
                    other = item.ItemName.Contains(filterConfig.SelectedWeapon);
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
            //Quality
            bool quality = true;
            if (filterConfig.Industrial || filterConfig.MilSpec || filterConfig.Restricted || filterConfig.Classified || filterConfig.Covert || filterConfig.Contraband)
            {
                quality = false;
                string Quality = SteamBase.ItemList.FirstOrDefault(x => x.ItemName == item.ItemName).Quality;
                if (filterConfig.Industrial)
                    quality = Quality == "Industrial Grade";
                if (filterConfig.MilSpec && !quality)
                    quality = Quality == "Mil-Spec";
                if (filterConfig.Restricted && !quality)
                    quality = Quality == "Restricted";
                if (filterConfig.Classified && !quality)
                    quality = Quality == "Classified";
                if (filterConfig.Covert && !quality)
                    quality = Quality == "Covert";
                if (filterConfig.Contraband && !quality)
                    quality = Quality == "Contraband";
            }
            //types
            bool types = true;
            if (filterConfig.Weapon || filterConfig.Knife || filterConfig.Gloves || filterConfig.Sticker || filterConfig.Patch || filterConfig.Collectible || filterConfig.Key || filterConfig.Pass || filterConfig.MusicKit || filterConfig.Graffiti || filterConfig.Case || filterConfig.Package)
            {
                types = false;
                if (filterConfig.Weapon)
                    types = baseItem.Type == "Weapon";
                if (filterConfig.Knife && !types)
                    types = baseItem.Type == "Knife";
                if (filterConfig.Gloves && !types)
                    types = baseItem.Type == "Gloves";
                if (filterConfig.Agent && !types)
                    types = baseItem.Type == "Agent";
                if (filterConfig.Capsule && !types)
                    types = baseItem.Type.Contains("Capsule");
                if (filterConfig.Sticker && !types)
                    types = baseItem.Type == "Sticker";
                if (filterConfig.Patch && !types)
                    types = baseItem.Type == "Patch";
                if (filterConfig.Collectible && !types)
                    types = baseItem.Type == "Collectable";
                if (filterConfig.Key && !types)
                    types = baseItem.Type == "Key";
                if (filterConfig.Pass && !types)
                    types = baseItem.Type == "Pass";
                if (filterConfig.MusicKit && !types)
                    types = baseItem.Type == "Music Kit";
                if (filterConfig.Graffiti && !types)
                    types = baseItem.Type == "Graffiti";
                if (filterConfig.Case && !types)
                    types = baseItem.Type == "Skin Case";
                if (filterConfig.Package && !types)
                    types = baseItem.Type.Contains("Package");
            }
            //Prices
            bool prices = true;
            if (filterConfig.Price1 || filterConfig.Price2 || filterConfig.Price3 || filterConfig.Price4)
            {
                if (filterConfig.Price1)
                    prices = filterConfig.Price1From < item.Price1 && filterConfig.Price1To > item.Price1;
                if (filterConfig.Price2 && prices)
                    prices = filterConfig.Price2From < item.Price2 && filterConfig.Price2To > item.Price2;
                if (filterConfig.Price3 && prices)
                    prices = filterConfig.Price3From < item.Price3 && filterConfig.Price3To > item.Price3;
                if (filterConfig.Price4 && prices)
                    prices = filterConfig.Price4From < item.Price4 && filterConfig.Price4To > item.Price4;
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

            bool isShow = category && other && exterior && quality && types && prices && profit;
            return isShow;
        }
    }
}