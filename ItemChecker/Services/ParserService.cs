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
using System.Windows;

namespace ItemChecker.MVVM.Model
{
    public class ParserService : ItemBaseService
    {
        string _itemType { get; set; }
        string _itemName { get; set; }
        decimal _price1 { get; set; }
        decimal _price2 { get; set; }
        decimal _price3 { get; set; }
        decimal _price4 { get; set; }
        decimal _precent { get; set; }
        decimal _difference { get; set; }
        string _status { get; set; }
        bool _have { get; set; }

        void DefaultValue()
        {
            this._itemType = "Normal";
            this._itemName = String.Empty;
            this._price1 = 0;
            this._price2 = 0;
            this._price3 = 0;
            this._price4 = 0;
            this._precent = 0;
            this._difference = 0;
            this._status = "Tradable";
            this._have = false;
        }
        protected DataParser CheckList(string itemName)
        {
            DefaultValue();
            _itemName = itemName;
            ItemType();

            JObject json = null;
            if (ParserProperties.Default.ServiceOne == 0 | ParserProperties.Default.ServiceTwo == 0)
            {
                ItemBase Item = ItemBase.SkinsBase.Where(x => x.ItemName == itemName).First();
                json = Get.ItemOrdersHistogram(Item.SteamId);
            }

            ServiceOne(json);
            ServiceTwo(json);
            Calculate();

            return new DataParser(_itemType, _itemName, _price1, _price2, _price3, _price4, _precent, _difference, _status, _have);
        }
        void ServiceOne(JObject json)
        {
            switch (ParserProperties.Default.ServiceOne)
            {
                case 0: //st
                    {
                        DataSteamMarket steamItem = MarketItems(json);
                        DataSteamMarket.MarketItems.Add(steamItem);

                        _price1 = steamItem.LowestSellOrder;
                        _price2 = steamItem.HighestBuyOrder;
                        if (GeneralProperties.Default.Currency == 0)
                        {
                            _price1 = Edit.ConverterToUsd(_price1, GeneralProperties.Default.CurrencyValue);
                            _price2 = Edit.ConverterToUsd(_price2, GeneralProperties.Default.CurrencyValue);
                        }
                        if (DataOrder.Orders.Any(n => n.ItemName.Contains(_itemName)))
                            _status = "Ordered";
                        _have = _price1 != 0;
                        break;
                    }
                case 1: //csm
                    {
                        List<DataInventoryCsm> CsmItems = DataInventoryCsm.Inventory.Where(x => x.ItemName == _itemName).ToList();
                        _price1 = CsmItems.Select(x => x.Price).Min();
                        _price2 = Math.Round(_price1 * 0.95m, 2);
                        if (GeneralProperties.Default.Currency == 1)
                        {
                            _price1 = Edit.ConverterToRub(_price1, GeneralProperties.Default.CurrencyValue);
                            _price2 = Edit.ConverterToRub(_price2, GeneralProperties.Default.CurrencyValue);
                        }
                        _have = _price1 != 0;
                        break;
                    }
                case 2: //lf
                    {
                        DataInventoryLf LfItem = DataInventoryLf.Inventory.Where(x => x.ItemName == _itemName).First();
                        _price1 = LfItem.DefaultPrice;
                        _price2 = Math.Round(_price1 * 0.95m, 2);
                        if (GeneralProperties.Default.Currency == 1)
                        {
                            _price1 = Edit.ConverterToRub(_price1, GeneralProperties.Default.CurrencyValue);
                            _price2 = Edit.ConverterToRub(_price2, GeneralProperties.Default.CurrencyValue);
                        }
                        _have = LfItem.Have > 0;
                        break;
                    }
            }
        }
        void ServiceTwo(JObject json)
        {
            switch (ParserProperties.Default.ServiceTwo)
            {
                case 0:
                    {
                        if (!String.IsNullOrEmpty(json["lowest_sell_order"].ToString()))
                            _price3 = Convert.ToDecimal(json["lowest_sell_order"]) / 100;
                        if (!String.IsNullOrEmpty(json["highest_buy_order"].ToString()))
                            _price4 = Math.Round(Convert.ToDecimal(json["highest_buy_order"]) / 100 * 0.8696m, 2);
                        if (GeneralProperties.Default.Currency == 0)
                        {
                            _price3 = Edit.ConverterToUsd(_price3, GeneralProperties.Default.CurrencyValue);
                            _price4 = Edit.ConverterToUsd(_price4, GeneralProperties.Default.CurrencyValue);
                        }
                        break;
                    }
                case 1:
                    {
                        ItemBase Item = ItemBase.SkinsBase.Where(x => x.ItemName == _itemName).First();
                        _price3 = Item.PriceCsm;
                        _price4 = Math.Round(_price3 * 0.95m, 2); 
                        if (GeneralProperties.Default.Currency == 1)
                        {
                            _price3 = Edit.ConverterToRub(_price3, GeneralProperties.Default.CurrencyValue);
                            _price4 = Edit.ConverterToRub(_price4, GeneralProperties.Default.CurrencyValue);
                        }

                        if (ItemBase.Unavailable.Any(x => x.ItemName == _itemName))
                            _status = "Unavailable";
                        else if (ItemBase.Overstock.Any(x => x.ItemName == _itemName))
                            _status = "Overstock";
                        break;
                    }
                case 2:
                    {
                        if (!DataInventoryLf.Inventory.Any(x=> x.ItemName == _itemName))
                        {
                            _status = "Unknown";
                            break;
                        }
                        DataInventoryLf LfItem = DataInventoryLf.Inventory.Where(x => x.ItemName == _itemName).First();
                        _price3 = LfItem.DefaultPrice;
                        _price4 = Math.Round(_price3 * 0.95m, 2);
                        if(GeneralProperties.Default.Currency == 1)
                        {
                            _price3 = Edit.ConverterToRub(_price3, GeneralProperties.Default.CurrencyValue);
                            _price4 = Edit.ConverterToRub(_price4, GeneralProperties.Default.CurrencyValue);
                        }
                        if (LfItem.IsOverstock)
                            _status = "Overstock";
                        break;
                    }
            }
        }
        void ItemType()
        {
            if (_itemName.Contains("Souvenir"))
                _itemType = "Souvenir";
            if (_itemName.Contains("StatTrak"))
                _itemType = "Stattrak";
            if (_itemName.Contains("★ "))
                _itemType = "KnifeGlove";
            if (_itemName.Contains("★ StatTrak"))
                _itemType = "KnifeGloveStattrak";
        }
        void Calculate()
        {
            if (ParserProperties.Default.ServiceOne == 0) //steam -> (any)
            {
                _precent = Edit.Precent(_price2, _price4);
                _difference = Edit.Difference(_price4, _price2);
            }
            else //(any) -> (any)
            {
                _precent = Edit.Precent(_price1, _price4);
                _difference = Edit.Difference(_price4, _price1);
            }
            switch (GeneralProperties.Default.Currency)
            {
                case 0:
                    {
                        ParserList.DataCurrency = "USD";
                        break;
                    }
                case 1:
                    {
                        ParserList.DataCurrency = "RUB";
                        break;
                    }
            }
        }
        DataSteamMarket MarketItems(JObject json)
        {
            decimal high = 0;
            decimal low = 0;
            List<decimal> buyOrderGraph = new();
            List<decimal> sellOrderGraph = new();
            if (!String.IsNullOrEmpty(json["highest_buy_order"].ToString()))
                high = Convert.ToDecimal(json["highest_buy_order"]) / 100;
            if (!String.IsNullOrEmpty(json["lowest_sell_order"].ToString()))
                low = Convert.ToDecimal(json["lowest_sell_order"]) / 100;
            int i = 1;
            if (((JArray)json["buy_order_graph"]).Any())
            {
                foreach (var order in json["buy_order_graph"])
                {
                    if (i > 6)
                        break;
                    buyOrderGraph.Add(Convert.ToDecimal(order[0]));
                    i++;
                }
                buyOrderGraph.RemoveAt(0);
            }
            i = 1;
            if (((JArray)json["sell_order_graph"]).Any())
            {
                foreach (var order in json["sell_order_graph"])
                {
                    if (i > 6)
                        break;
                    sellOrderGraph.Add(Convert.ToDecimal(order[0]));
                    i++;
                }
                sellOrderGraph.RemoveAt(0);
            }
            return new DataSteamMarket()
            {
                ItemName = _itemName,
                HighestBuyOrder = high,
                LowestSellOrder = low,
                BuyOrderGraph = buyOrderGraph,
                SellOrderGraph = sellOrderGraph
            };
        }

        //CSV
        public void ExportCsv(ICollectionView collectionView, string mode)
        {
            dynamic source = DataParser.ParserItems;
            if (collectionView.Cast<DataParser>().Count() != DataParser.ParserItems.Count)
            {
                MessageBoxResult result = MessageBox.Show($"Export with filter applied?\n\nClick \"No\" to export the entire list.", "Question", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (result == MessageBoxResult.Cancel)
                    return;
                else if (result == MessageBoxResult.Yes)
                    source = collectionView;
            }

            string csv = $"{ParserList.DataCurrency},{ParserProperties.Default.ServiceOne},{ParserProperties.Default.ServiceTwo}\r\n";
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
            File.WriteAllText(path + $"\\Parser_{mode}_{ParserList.DataCurrency}_{DateTime.Now:dd.MM.yyyy_hh.mm}.csv", Edit.replaceSymbols(csv));
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
                    ParserList.DataCurrency = line[0].ToString();
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
                if (ParserList.DataCurrency == "RUB")
                    MessageBox.Show("Attention, prices in \"RUB\" after conversion may not be accurate.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                foreach (DataParser item in collectionView)
                {
                    decimal price = item.Price1;
                    if (ParserList.DataCurrency == "RUB")
                        price = Math.Round(item.Price1 / GeneralProperties.Default.CurrencyValue, 2);

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
            File.WriteAllText(path + $"\\Parser_{mode}_{ParserList.DataCurrency}_{result}_{DateTime.Now:dd.MM.yyyy_hh.mm}.txt", Edit.replaceSymbols(txt));
        }
    }
}