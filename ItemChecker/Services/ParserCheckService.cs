using ItemChecker.Net;
using ItemChecker.Properties;
using ItemChecker.Support;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ItemChecker.MVVM.Model
{
    public class ParserCheckService : ParserService
    {
        string _itemType { get; set; } = "Normal";
        string _itemName { get; set; } = String.Empty;
        decimal _price1 { get; set; } = 0;
        decimal _price2 { get; set; } = 0;
        decimal _price3 { get; set; } = 0;
        decimal _price4 { get; set; } = 0;
        decimal _precent { get; set; } = 0;
        decimal _difference { get; set; } = 0;
        string _status { get; set; } = "Tradable";
        bool _have { get; set; } = false;

        public DataParser Check(string itemName)
        {
            _itemName = itemName;
            ItemType();

            JObject json = new();
            if (ParserProperties.Default.ServiceOne == 0 | ParserProperties.Default.ServiceTwo == 0 | ParserProperties.Default.ServiceOne == 1 | ParserProperties.Default.ServiceTwo == 1)
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
                        if (SettingsProperties.Default.Currency == 0)
                        {
                            _price1 = Edit.ConverterToUsd(_price1, SettingsProperties.Default.CurrencyValue);
                            _price2 = Edit.ConverterToUsd(_price2, SettingsProperties.Default.CurrencyValue);
                        }
                        if (DataOrder.Orders.Any(n => n.ItemName == _itemName))
                            _status = "Ordered";
                        _have = _price1 != 0;
                        break;
                    }
                case 1: //sta
                    {
                        DataSteamMarket steamItem = MarketItems(json);
                        DataSteamMarket.MarketItems.Add(steamItem);

                        _price1 = steamItem.LowestSellOrder;
                        _price2 = steamItem.HighestBuyOrder;
                        if (SettingsProperties.Default.Currency == 0)
                        {
                            _price1 = Edit.ConverterToUsd(_price1, SettingsProperties.Default.CurrencyValue);
                            _price2 = Edit.ConverterToUsd(_price2, SettingsProperties.Default.CurrencyValue);
                        }
                        if (DataOrder.Orders.Any(n => n.ItemName == _itemName))
                            _status = "Ordered";
                        _have = _price1 != 0;
                        break;
                    }
                case 2:
                    {
                        _price1 = DataInventoryCsm.Inventory.Where(x => x.ItemName == _itemName).Select(x => x.Price).DefaultIfEmpty().Min();
                        _price2 = Math.Round(_price1 * Calculator.CommissionCsm, 2);
                        if (SettingsProperties.Default.Currency == 1)
                        {
                            _price1 = Edit.ConverterToRub(_price1, SettingsProperties.Default.CurrencyValue);
                            _price2 = Edit.ConverterToRub(_price2, SettingsProperties.Default.CurrencyValue);
                        }
                        _have = _price1 != 0;
                        break;
                    }
                case 3:
                    {
                        _price1 = DataInventoryLf.Inventory.Where(x => x.ItemName == _itemName).Select(x => x.DefaultPrice).FirstOrDefault();
                        _price2 = Math.Round(_price1 * Calculator.CommissionLf, 2);
                        if (SettingsProperties.Default.Currency == 1)
                        {
                            _price1 = Edit.ConverterToRub(_price1, SettingsProperties.Default.CurrencyValue);
                            _price2 = Edit.ConverterToRub(_price2, SettingsProperties.Default.CurrencyValue);
                        }
                        _have = _price1 != 0;
                        break;
                    }
            }
        }
        void ServiceTwo(JObject json)
        {
            switch (ParserProperties.Default.ServiceTwo)
            {
                case 0: //sta
                    {
                        DataSteamMarket steamItem = MarketItems(json);
                        DataSteamMarket.MarketItems.Add(steamItem);

                        _price3 = steamItem.LowestSellOrder;
                        _price4 = Math.Round(steamItem.HighestBuyOrder * Calculator.CommissionSteam, 2);
                        if (SettingsProperties.Default.Currency == 0)
                        {
                            _price3 = Edit.ConverterToUsd(_price3, SettingsProperties.Default.CurrencyValue);
                            _price4 = Edit.ConverterToUsd(_price4, SettingsProperties.Default.CurrencyValue);
                        }
                        break;
                    }
                case 1: //sta
                    {
                        DataSteamMarket steamItem = MarketItems(json);
                        DataSteamMarket.MarketItems.Add(steamItem);

                        _price3 = steamItem.LowestSellOrder;
                        _price4 = Math.Round(steamItem.HighestBuyOrder * Calculator.CommissionSteam, 2);
                        if (SettingsProperties.Default.Currency == 0)
                        {
                            _price3 = Edit.ConverterToUsd(_price3, SettingsProperties.Default.CurrencyValue);
                            _price4 = Edit.ConverterToUsd(_price4, SettingsProperties.Default.CurrencyValue);
                        }
                        break;
                    }
                case 2:
                    {
                        ItemBase Item = ItemBase.SkinsBase.Where(x => x.ItemName == _itemName).FirstOrDefault();
                        _price3 = Item.PriceCsm;
                        _price4 = Math.Round(_price3 * Calculator.CommissionCsm, 2);
                        if (SettingsProperties.Default.Currency == 1)
                        {
                            _price3 = Edit.ConverterToRub(_price3, SettingsProperties.Default.CurrencyValue);
                            _price4 = Edit.ConverterToRub(_price4, SettingsProperties.Default.CurrencyValue);
                        }

                        if (ItemBase.Unavailable.Any(x => x.ItemName == _itemName))
                            _status = "Unavailable";
                        else if (ItemBase.Overstock.Any(x => x.ItemName == _itemName))
                            _status = "Overstock";
                        break;
                    }
                case 3:
                    {
                        if (!DataInventoryLf.Inventory.Any(x => x.ItemName == _itemName))
                        {
                            _status = "Unknown";
                            break;
                        }
                        DataInventoryLf LfItem = DataInventoryLf.Inventory.Where(x => x.ItemName == _itemName).FirstOrDefault();
                        _price3 = LfItem.DefaultPrice;
                        _price4 = Math.Round(_price3 * Calculator.CommissionLf, 2);
                        if (SettingsProperties.Default.Currency == 1)
                        {
                            _price3 = Edit.ConverterToRub(_price3, SettingsProperties.Default.CurrencyValue);
                            _price4 = Edit.ConverterToRub(_price4, SettingsProperties.Default.CurrencyValue);
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
            if (ParserProperties.Default.ServiceOne == 1) //sta -> (any)
            {
                _precent = Edit.Precent(_price2, _price4);
                _difference = Edit.Difference(_price4, _price2);
            }
            else //(any) -> (any)
            {
                _precent = Edit.Precent(_price1, _price4);
                _difference = Edit.Difference(_price4, _price1);
            }
            switch (SettingsProperties.Default.Currency)
            {
                case 0:
                    {
                        ParserStatistics.DataCurrency = "USD";
                        break;
                    }
                case 1:
                    {
                        ParserStatistics.DataCurrency = "RUB";
                        break;
                    }
            }
        }
        DataSteamMarket MarketItems(JObject json)
        {
            decimal high = 0;
            decimal low = 0;
            if (!String.IsNullOrEmpty(json["highest_buy_order"].ToString()))
                high = Convert.ToDecimal(json["highest_buy_order"]) / 100;
            if (!String.IsNullOrEmpty(json["lowest_sell_order"].ToString()))
                low = Convert.ToDecimal(json["lowest_sell_order"]) / 100;

            return new DataSteamMarket()
            {
                ItemName = _itemName,
                HighestBuyOrder = high,
                LowestSellOrder = low,
            };
        }

        //List
        public List<string> SelectFile()
        {
            List<string> list = OpenFileDialog("txt");
            if (list.Any())
                list = clearPrices(list);

            return list;
        }
        public static List<string> ApplyConfig(ParserConfig config)
        {
            List<string> list = new();

            foreach (string item in ParserProperties.Default.CheckList)
            {
                //Dopplers
                if (config.OnlyDopplers & !item.Contains("Doppler"))
                    continue;
                if (!config.Dopplers & item.Contains("Doppler"))
                    continue;
                //Unavailable
                if (config.ServiceTwo == 2 & ItemBase.Unavailable.Any(x => x.ItemName == item))
                    continue;
                if (config.ServiceTwo == 3 & !DataInventoryLf.Inventory.Any(x => x.ItemName == item))
                    continue;
                //Overstock
                if (!config.Overstock)
                {
                    if (config.ServiceTwo == 2 & ItemBase.Overstock.Any(x => x.ItemName == item))
                        continue;
                    else if (config.ServiceTwo == 3 & DataInventoryLf.Inventory.Where(x => x.ItemName == item).Select(x => x.IsOverstock).FirstOrDefault())
                        continue;
                }
                if (!config.Ordered & config.ServiceOne == 1 & DataOrder.Orders.Any(x => x.ItemName == item))
                    continue;
                //Price
                if (config.MinPrice != 0)
                {
                    if (config.ServiceOne == 2 & DataInventoryCsm.Inventory.Where(x => x.ItemName == item).Select(x => x.Price).DefaultIfEmpty().Min() < config.MinPrice)
                        continue;
                    else if (config.ServiceOne == 3 & DataInventoryLf.Inventory.Where(x => x.ItemName == item).Select(x => x.DefaultPrice).FirstOrDefault() < config.MinPrice)
                        continue;
                }
                if (config.MaxPrice != 0)
                {
                    if (config.ServiceOne == 2 & DataInventoryCsm.Inventory.Where(x => x.ItemName == item).Select(x => x.Price).DefaultIfEmpty().Min() > config.MaxPrice)
                        continue;
                    else if (config.ServiceOne == 3 & DataInventoryLf.Inventory.Where(x => x.ItemName == item).Select(x => x.DefaultPrice).FirstOrDefault() > config.MaxPrice)
                        continue;
                }
                //add
                if (config.Souvenir & item.Contains("Souvenir"))
                    list.Add(item);
                else if (config.Stattrak & item.Contains("StatTrak™"))
                    list.Add(item);
                else if (config.KnifeGlove & item.Contains("★ "))
                    list.Add(item);
                else if (config.KnifeGloveStattrak & item.Contains("★ StatTrak™"))
                    list.Add(item);
                else if (!config.Souvenir & !config.Stattrak & !config.KnifeGlove & !config.KnifeGloveStattrak)
                    list.Add(item);
            }
            return list;
        }
    }
}