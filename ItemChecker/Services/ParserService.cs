using ItemChecker.Properties;
using ItemChecker.Services;
using ItemChecker.Support;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;

namespace ItemChecker.MVVM.Model
{
    public class ParserService : ItemBaseService
    {
        public static bool ApplyFilter(ParserFilter filterConfig, DataParser item)
        {
            //category
            bool category = true;
            if (filterConfig.Normal | filterConfig.Stattrak | filterConfig.Souvenir | filterConfig.KnifeGlove | filterConfig.KnifeGloveStattrak)
            {
                category = false;
                if (filterConfig.Normal)
                    category = item.ItemType.Contains("Normal");
                if (filterConfig.Stattrak & !category)
                    category = item.ItemType.Contains("Souvenir");
                if (filterConfig.Souvenir & !category)
                    category = item.ItemType.Contains("Stattrak");
                if (filterConfig.KnifeGlove & !category)
                    category = item.ItemType.Contains("KnifeGlove");
                if (filterConfig.KnifeGloveStattrak & !category)
                    category = item.ItemType.Contains("KnifeGloveStattrak");
            }
            //status
            bool status = true;
            if (filterConfig.Tradable | filterConfig.Ordered | filterConfig.Overstock | filterConfig.Unavailable | filterConfig.Unknown)
            {
                status = false;
                if (filterConfig.Tradable)
                    status = item.Status.Contains("Tradable");
                if (filterConfig.Ordered & !status)
                    status = item.Status.Contains("Ordered");
                if (filterConfig.Overstock & !status)
                    status = item.Status.Contains("Overstock");
                if (filterConfig.Unavailable & !status)
                    status = item.Status.Contains("Unavailable");
                if (filterConfig.Unknown & !status)
                    status = item.Status.Contains("Unknown");
            }
            //exterior
            bool exterior = true;
            if (filterConfig.NotPainted | filterConfig.BattleScarred | filterConfig.WellWorn | filterConfig.FieldTested | filterConfig.MinimalWear | filterConfig.FactoryNew)
            {
                exterior = false;
                if (filterConfig.NotPainted)
                    exterior = !item.ItemName.Contains("Battle-Scarred") &
                        !item.ItemName.Contains("Well-Worn") &
                        !item.ItemName.Contains("Field-Tested") &
                        !item.ItemName.Contains("Minimal Wear") &
                        !item.ItemName.Contains("Factory New") &
                        item.ItemType.Contains("KnifeGlove");
                if (filterConfig.BattleScarred & !exterior)
                    exterior = item.ItemName.Contains("Battle-Scarred");
                if (filterConfig.WellWorn & !exterior)
                    exterior = item.ItemName.Contains("Well-Worn");
                if (filterConfig.FieldTested & !exterior)
                    exterior = item.ItemName.Contains("Field-Tested");
                if (filterConfig.MinimalWear & !exterior)
                    exterior = item.ItemName.Contains("Minimal Wear");
                if (filterConfig.FactoryNew & !exterior)
                    exterior = item.ItemName.Contains("Factory New");
            }
            //types
            bool types = true;
            if (filterConfig.Weapon | filterConfig.Knife | filterConfig.Gloves | filterConfig.Sticker | filterConfig.Patch | filterConfig.Pin | filterConfig.Key | filterConfig.Pass | filterConfig.MusicKit | filterConfig.Graffiti | filterConfig.Case | filterConfig.Package)
            {
                types = false;
                if (filterConfig.Weapon)
                    types = !item.ItemName.Contains("Sticker ") &
                        !item.ItemName.Contains("Patch ") &
                        !item.ItemName.Contains(" Pin") &
                        !item.ItemName.Contains(" Key") &
                        !item.ItemName.Contains(" Pass") &
                        !item.ItemName.Contains("Music Kit") &
                        !item.ItemName.Contains("Sealed Graffiti") &
                        !item.ItemName.Contains("Case") &
                        !item.ItemName.Contains(" Package");
                if (filterConfig.Knife & !types)
                    types = item.ItemName.Contains("Knife");
                if (filterConfig.Gloves & !types)
                    types = item.ItemName.Contains("Gloves |") | item.ItemName.Contains("★ Hand");
                if (filterConfig.Sticker & !types)
                    types = item.ItemName.Contains("Sticker ");
                if (filterConfig.Patch & !types)
                    types = item.ItemName.Contains("Patch ");
                if (filterConfig.Pin & !types)
                    types = item.ItemName.Contains(" Pin");
                if (filterConfig.Key & !types)
                    types = item.ItemName.Contains(" Key");
                if (filterConfig.Pass & !types)
                    types = item.ItemName.Contains(" Pass");
                if (filterConfig.MusicKit & !types)
                    types = item.ItemName.Contains("Music Kit");
                if (filterConfig.Graffiti & !types)
                    types = item.ItemName.Contains("Sealed Graffiti");
                if (filterConfig.Case & !types)
                    types = item.ItemName.Contains("Case");
                if (filterConfig.Package & !types)
                    types = item.ItemName.Contains(" Package");
            }
            //Prices
            bool prices = true;
            if (filterConfig.Price1 | filterConfig.Price2 | filterConfig.Price3 | filterConfig.Price4)
            {
                if (filterConfig.Price1)
                    prices = filterConfig.Price1From < item.Price1 & filterConfig.Price1To > item.Price1;
                if (filterConfig.Price2 & prices)
                    prices = filterConfig.Price2From < item.Price2 & filterConfig.Price2To > item.Price2;
                if (filterConfig.Price3 & prices)
                    prices = filterConfig.Price3From < item.Price3 & filterConfig.Price3To > item.Price3;
                if (filterConfig.Price4 & prices)
                    prices = filterConfig.Price4From < item.Price4 & filterConfig.Price4To > item.Price4;
            }
            //other
            bool other = true;
            if (filterConfig.PrecentFrom != 0 | filterConfig.PrecentTo != 0 | filterConfig.DifferenceFrom != 0 | filterConfig.DifferenceTo != 0 | filterConfig.Hide100 | filterConfig.Hide0 | filterConfig.Have)
            {
                if (filterConfig.PrecentFrom != 0)
                    other = filterConfig.PrecentFrom < item.Precent;
                if (filterConfig.PrecentTo != 0 & other)
                    other = filterConfig.PrecentTo > item.Precent;
                if (filterConfig.DifferenceFrom != 0 & other)
                    other = filterConfig.DifferenceFrom < item.Difference;
                if (filterConfig.DifferenceTo != 0 & other)
                    other = filterConfig.DifferenceTo > item.Difference;

                if (filterConfig.Hide100 & other)
                    other = item.Precent != -100;
                if (filterConfig.Hide0 & other)
                    other = item.Precent != 0;
                if (filterConfig.Have & other)
                    other = item.Have;
            }

            bool isShow = category & status & exterior & types & prices & other;
            return isShow;
        }
        //CSV
        public void ExportCsv(ObservableCollection<DataParser> parserGrid, ICollectionView collectionView, string mode)
        {
            dynamic source = parserGrid;
            if (collectionView.Cast<DataParser>().Count() != parserGrid.Count)
            {
                MessageBoxResult result = MessageBox.Show($"Export with filter applied?\n\nClick \"No\" to export the entire list.", "Question", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (result == MessageBoxResult.Cancel)
                    return;
                else if (result == MessageBoxResult.Yes)
                    source = collectionView;
            }

            string csv = $"{ParserStatistics.DataCurrency},{ParserProperties.Default.ServiceOne},{ParserProperties.Default.ServiceTwo}\r\n";
            foreach (var item in source)
            {
                string itemName = item.ItemName;
                if (itemName.Contains(","))
                    itemName = itemName.Replace(",", ";");
                csv += $"{item.ItemType},{itemName},{item.Price1},{item.Price2},{item.Price3},{item.Price4},{item.Precent},{item.Difference},{item.Status},{item.Have}\r\n";
            }

            //write
            string path = $"{BaseModel.AppPath}\\extract";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            File.WriteAllText(path + $"\\Parser_{mode}_{ParserStatistics.DataCurrency}_{DateTime.Now:dd.MM.yyyy_hh.mm}.csv", Edit.replaceSymbols(csv));
        }
        public ObservableCollection<DataParser> ImportCsv()
        {
            List<string> list = OpenFileDialog("csv");
            ObservableCollection<DataParser> datas = new();
            if (!list.Any())
                return datas;

            foreach (string info in list)
            {
                string[] line = info.Split(',');
                if (line.Length == 3)
                {
                    ParserStatistics.DataCurrency = line[0].ToString();
                    ParserProperties.Default.ServiceOne = int.Parse(line[1]);
                    ParserProperties.Default.ServiceTwo = int.Parse(line[2]);
                    continue;
                }

                string itemName = line[1];
                if (itemName.Contains(','))
                    itemName = itemName.Replace(";", ",");

                datas.Add(
                    new DataParser(
                        line[0],
                        itemName,
                        decimal.Parse(line[2]),
                        decimal.Parse(line[3]),
                        decimal.Parse(line[4]),
                        decimal.Parse(line[5]),
                        decimal.Parse(line[6]),
                        decimal.Parse(line[7]),
                        line[8],
                        bool.Parse(line[9])
                        )
                    );
            }

            return datas;
        }
        public void ExportTxt(ICollectionView collectionView, string HeaderText, string mode)
        {
            MessageBoxResult result = MessageBox.Show($"Do need to add \"{HeaderText}\" to the list?", "Question", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
            if (result == MessageBoxResult.Cancel)
                return;

            string txt = null;
            if (result == MessageBoxResult.Yes)
            {
                if (ParserStatistics.DataCurrency == "RUB")
                    MessageBox.Show("Attention, prices in \"RUB\" after conversion may not be accurate.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                foreach (DataParser item in collectionView)
                {
                    decimal price = item.Price1;
                    if (ParserStatistics.DataCurrency == "RUB")
                        price = Math.Round(item.Price1 / SettingsProperties.Default.CurrencyValue, 2);

                    txt += $"{item.ItemName};{price}\r\n";
                }
            }

            if (result == MessageBoxResult.No)
            {
                foreach (DataParser item in collectionView)
                    txt += $"{item.ItemName}\r\n";
            }
            //write
            string path = $"{BaseModel.AppPath}\\extract";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            File.WriteAllText(path + $"\\Parser_{mode}_{ParserStatistics.DataCurrency}_{result}_{DateTime.Now:dd.MM.yyyy_hh.mm}.txt", Edit.replaceSymbols(txt));
        }
    }
}