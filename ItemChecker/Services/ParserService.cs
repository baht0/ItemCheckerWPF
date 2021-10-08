using ItemChecker.Net;
using ItemChecker.Properties;
using ItemChecker.Services;
using ItemChecker.Support;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;

namespace ItemChecker.MVVM.Model
{
    public class ParserService : BaseService
    {
        //CHECk
        protected ParserData CheckItems(string itemName)
        {
            Tuple<String, Boolean> response = Tuple.Create("", false);
            do
            {
                response = Get.MrinkaRequest(Edit.MarketHashName(itemName));
                if (!response.Item2)
                {
                    Thread.Sleep(30000);
                }
            }
            while (!response.Item2);

            return parseMrinka(response.Item1, itemName);
        }
        private ParserData parseMrinka(string response, string itemName)
        {
            decimal _price1 = 0;
            decimal _price2 = 0;
            decimal _price3 = 0;
            decimal _price4 = 0;
            string _status = "Tradable";

            if (ParserProperties.Default.serviceOne == 0)
            {
                _price1 = Convert.ToDecimal(JObject.Parse(response)["steam"]["sellOrder"].ToString());
                _price2 = Convert.ToDecimal(JObject.Parse(response)["steam"]["buyOrder"].ToString());
            }
            else if (ParserProperties.Default.serviceTwo == 0)
            {
                _price3 = Convert.ToDecimal(JObject.Parse(response)["steam"]["sellOrder"].ToString());
                decimal buyOrder = Convert.ToDecimal(JObject.Parse(response)["steam"]["buyOrder"].ToString()) * 0.8696m;
                _price4 = Math.Round(buyOrder, 2);
            }
            if (ParserProperties.Default.serviceOne == 1)
            {
                _price1 = Convert.ToDecimal(JObject.Parse(response)["csm"]["buy"]["0"].ToString());
                _price2 = Convert.ToDecimal(JObject.Parse(response)["csm"]["sell"].ToString());
            }
            else if (ParserProperties.Default.serviceTwo == 1)
            {
                _price3 = Convert.ToDecimal(JObject.Parse(response)["csm"]["buy"]["0"].ToString());
                _price4 = Convert.ToDecimal(JObject.Parse(response)["csm"]["sell"].ToString());

                if (Main.Unavailable.Contains(itemName))
                    _status = "Unavailable";
                else if (Main.Overstock.Contains(itemName))
                    _status = "Overstock";
            }
            //loot.farm
            if (ParserProperties.Default.serviceOne == 2 | ParserProperties.Default.serviceTwo == 2)
            {
                string json = Get.Request("https://loot.farm/fullprice.json");
                JArray jArray = JArray.Parse(json);
                List<string> listLF = new();

                for (int i = 0; i < jArray.Count; i++)
                    listLF.Add(jArray[i]["name"].ToString());

                if (listLF.Contains(itemName))
                {
                    int id = listLF.IndexOf(itemName);
                    if (ParserProperties.Default.serviceOne == 2)
                    {
                        decimal price = Convert.ToDecimal(jArray[id]["price"]) / 100;
                        _price1 = price;
                        _price2 = Math.Round(price * 0.95m, 2);
                        int have = Convert.ToInt32(jArray[id]["have"]);
                    }
                    else if (ParserProperties.Default.serviceTwo == 2)
                    {
                        decimal price = Convert.ToDecimal(jArray[id]["price"]) / 100;
                        _price3 = price;
                        _price4 = Math.Round(price * 0.95m, 2);

                        int have = Convert.ToInt32(jArray[id]["have"]);
                        int max = Convert.ToInt32(jArray[id]["max"]);
                        int count = max - have;
                        if (count > 0)
                            _status = "Tradable";
                        else if (count <= 0)
                            _status = "Overstock";
                    }
                }
                else if (ParserProperties.Default.serviceTwo == 2)
                    _status = "Unknown";
            }
            //precent & diff
            decimal _precent;
            decimal _difference;
            if (ParserProperties.Default.serviceOne == 0) //steam -> (any)
            {
                _precent = Edit.Precent(_price2, _price4);
                _difference = Edit.Difference(_price4, _price2);
            }
            else //(any) -> (any)
            {
                _precent = Edit.Precent(_price1, _price4);
                _difference = Edit.Difference(_price4, _price1);
            }

            string _itemType = "Normal";
            if (itemName.Contains("Souvenir"))
                _itemType = "Souvenir";
            if (itemName.Contains("StatTrak"))
                _itemType = "Stattrak";
            if (itemName.Contains("★"))
                _itemType = "KnifeGlove";
            if (itemName.Contains("★ StatTrak"))
                _itemType = "KnifeGloveStattrak";

            Parser.DataCurrency = "USD";
            if (GeneralProperties.Default.Currency == 1)
            {
                _price1 = Edit.Converter(_price1, GeneralProperties.Default.CurrencyValue);
                _price2 = Edit.Converter(_price2, GeneralProperties.Default.CurrencyValue);
                _price3 = Edit.Converter(_price3, GeneralProperties.Default.CurrencyValue);
                _price4 = Edit.Converter(_price4, GeneralProperties.Default.CurrencyValue);
                _difference = Edit.Converter(_difference, GeneralProperties.Default.CurrencyValue);
                Parser.DataCurrency = "RUB";
            }
            return new ParserData(_itemType, itemName, _price1, _price2, _price3, _price4, _precent, _difference, _status);
        }

        public ObservableCollection<string> GetCheckList(string url)
        {
            var json = Get.Request(url);
            if (url.Contains("csmoney"))
                json = JObject.Parse(json)["data"].ToString();
            JArray jArray = JArray.Parse(json);

            ObservableCollection<string> list = new();
            for (int i = 0; i < jArray.Count; i++)
            {
                string item = jArray[i]["name"].ToString();
                if (url.Contains("loot.farm"))
                    list.Add(item);
                else if (!Main.Unavailable.Contains(item))
                    list.Add(item);
            }
            return list;
        }

        //selectFile
        public ObservableCollection<string> SelectFile()
        {
            List<string> list = OpenFileDialog("txt");
            if (list.Any())
                list = clearPrices(list);

            return new ObservableCollection<string>(list);
        }

        //CSV
        public void ExportCsv(ICollectionView collectionView, string mode)
        {
            dynamic source = ParserData.ParserItems;
            if (collectionView.Cast<ParserData>().Count() != ParserData.ParserItems.Count)
            {
                MessageBoxResult result = MessageBox.Show($"Export with filter applied?\n\nClick \"No\" to export the entire list.", "Question", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (result == MessageBoxResult.Cancel)
                    return;
                else if (result == MessageBoxResult.Yes)
                    source = collectionView;
            }

            string csv = $"{Parser.DataCurrency},{ParserProperties.Default.serviceOne},{ParserProperties.Default.serviceTwo}\r\n";
            foreach (var item in source)
            {
                string itemName = item.ItemName;
                if (itemName.Contains(","))
                    itemName = itemName.Replace(",", ";");
                csv += $"{item.ItemType},{itemName},{item.Price1},{item.Price2},{item.Price3},{item.Price4},{item.Precent},{item.Difference},{item.Status}\r\n";
            }

            //write
            string path = $"{BaseModel.AppPath}\\extract";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            File.WriteAllText(path + $"\\Parser_{mode}_{Parser.DataCurrency}_{DateTime.Now:dd.MM.yyyy_hh.mm}.csv", Edit.replaceSymbols(csv));
        }
        public ObservableCollection<ParserData> ImportCsv()
        {
            List<string> list = OpenFileDialog("csv");
            ObservableCollection<ParserData> datas = new();
            if (!list.Any())
                return datas;

            foreach (string info in list)
            {
                string[] line = info.Split(',');
                if (line.Length == 3)
                {
                    Parser.DataCurrency = line[0].ToString();
                    ParserProperties.Default.serviceOne = int.Parse(line[1]);
                    ParserProperties.Default.serviceTwo = int.Parse(line[2]);
                    continue;
                }

                string itemName = line[1];
                if (itemName.Contains(","))
                    itemName = itemName.Replace(";", ",");

                datas.Add(new ParserData(
                    line[0],
                    itemName,
                    decimal.Parse(line[2]),
                    decimal.Parse(line[3]),
                    decimal.Parse(line[4]),
                    decimal.Parse(line[5]),
                    decimal.Parse(line[6]),
                    decimal.Parse(line[7]),
                    line[8]));
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
                if (Parser.DataCurrency == "RUB")
                    MessageBox.Show("Attention, prices in \"RUB\" after conversion may not be accurate.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                foreach (ParserData item in collectionView)
                {
                    decimal price = item.Price1;
                    if (Parser.DataCurrency == "RUB")
                        price = Math.Round(item.Price1 / GeneralProperties.Default.CurrencyValue, 2);

                    txt += $"{item.ItemName};{price}\r\n";
                }
            }

            if (result == MessageBoxResult.No)
            {
                foreach (ParserData item in collectionView)
                    txt += $"{item.ItemName}\r\n";
            }
            //write
            string path = $"{BaseModel.AppPath}\\extract";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            File.WriteAllText(path + $"\\Parser_{mode}_{Parser.DataCurrency}_{result}_{DateTime.Now:dd.MM.yyyy_hh.mm}.txt", Edit.replaceSymbols(txt));
        }
    }
}