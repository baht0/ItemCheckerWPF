using ItemChecker.Net;
using ItemChecker.Properties;
using ItemChecker.Support;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ItemChecker.MVVM.Model
{
    public class ParserCheckService : ParserService
    {
        #region prop
        string _itemType { get; set; } = String.Empty;
        string _itemName { get; set; } = String.Empty;
        decimal _price1 { get; set; } = 0;
        decimal _price2 { get; set; } = 0;
        decimal _price3 { get; set; } = 0;
        decimal _price4 { get; set; } = 0;
        decimal _precent { get; set; } = 0;
        decimal _difference { get; set; } = 0;
        string _status { get; set; } = "Tradable";
        bool _have { get; set; } = false;
        #endregion

        #region check
        public DataParser Check(string itemName, int serOneId, int serTwoId)
        {
            _itemName = itemName;
            _itemType = ItemBase.SkinsBase.FirstOrDefault( x=> x.ItemName == itemName).Type;

            JObject json = new();
            if (ParserProperties.Default.ServiceOne == 0 | ParserProperties.Default.ServiceTwo == 0 | ParserProperties.Default.ServiceOne == 1 | ParserProperties.Default.ServiceTwo == 1)
            {
                ItemBase Item = ItemBase.SkinsBase.FirstOrDefault(x => x.ItemName == itemName);
                json = Item != null ? Get.ItemOrdersHistogram(Item.SteamInfo.Id) : null;
            }

            ServiceOne(json, serOneId);
            ServiceTwo(json, serTwoId);
            Calculate(serOneId);

            return new DataParser(_itemType, _itemName, _price1, _price2, _price3, _price4, _precent, _difference, _status, _have);
        }
        void ServiceOne(JObject json, int serOneId)
        {
            switch (serOneId)
            {
                case 0 or 1:
                    {
                        if (json == null)
                            break;
                        DataSteamMarket steamItem = MarketItems(json);
                        DataSteamMarket.MarketItems.Add(steamItem);

                        _price1 = steamItem.LowestSellOrder;
                        _price2 = steamItem.HighestBuyOrder;
                        if (SettingsProperties.Default.CurrencyId == 0)
                        {
                            _price1 = Edit.ConverterToUsd(_price1, SettingsProperties.Default.CurrencyValue);
                            _price2 = Edit.ConverterToUsd(_price2, SettingsProperties.Default.CurrencyValue);
                        }
                        if (DataOrder.Orders.Any(n => n.ItemName == _itemName))
                            _status = "Ordered";
                        _have = serOneId == 0 || _price1 != 0;
                        break;
                    }
                case 2:
                    {
                        _price1 = DataInventoryCsm.Inventory.Where(x => x.ItemName == _itemName).Select(x => x.Price).DefaultIfEmpty().Min();
                        _price2 = Math.Round(_price1 * Calculator.CommissionCsm, 2);
                        if (SettingsProperties.Default.CurrencyId == 1)
                        {
                            _price1 = Edit.ConverterToRub(_price1, SettingsProperties.Default.CurrencyValue);
                            _price2 = Edit.ConverterToRub(_price2, SettingsProperties.Default.CurrencyValue);
                        }
                        _have = _price1 != 0;
                        break;
                    }
                case 3:
                    {
                        _price1 = ItemBase.SkinsBase.FirstOrDefault(x => x.ItemName == _itemName).LfmInfo.Price;
                        _price2 = Math.Round(_price1 * Calculator.CommissionLf, 2);
                        if (SettingsProperties.Default.CurrencyId == 1)
                        {
                            _price1 = Edit.ConverterToRub(_price1, SettingsProperties.Default.CurrencyValue);
                            _price2 = Edit.ConverterToRub(_price2, SettingsProperties.Default.CurrencyValue);
                        }
                        _have = ItemBase.SkinsBase.FirstOrDefault(x => x.ItemName == _itemName).LfmInfo.Have > 0;
                        break;
                    }
            }
        }
        void ServiceTwo(JObject json, int serTwoId)
        {
            switch (serTwoId)
            {
                case 0 or 1: //st
                    {
                        if (json == null)
                            break;
                        DataSteamMarket steamItem = MarketItems(json);
                        DataSteamMarket.MarketItems.Add(steamItem);

                        _price3 = steamItem.LowestSellOrder;
                        _price4 = Math.Round(steamItem.HighestBuyOrder * Calculator.CommissionSteam, 2);
                        if (SettingsProperties.Default.CurrencyId == 0)
                        {
                            _price3 = Edit.ConverterToUsd(_price3, SettingsProperties.Default.CurrencyValue);
                            _price4 = Edit.ConverterToUsd(_price4, SettingsProperties.Default.CurrencyValue);
                        }
                        break;
                    }
                case 2:
                    {
                        ItemBase Item = ItemBase.SkinsBase.Where(x => x.ItemName == _itemName).FirstOrDefault();
                        _price3 = Item.CsmInfo.Price;
                        _price4 = Math.Round(_price3 * Calculator.CommissionCsm, 2);
                        if (SettingsProperties.Default.CurrencyId == 1)
                        {
                            _price3 = Edit.ConverterToRub(_price3, SettingsProperties.Default.CurrencyValue);
                            _price4 = Edit.ConverterToRub(_price4, SettingsProperties.Default.CurrencyValue);
                        }

                        if (ItemBase.SkinsBase.FirstOrDefault(x => x.ItemName == _itemName).CsmInfo.Unavailable)
                            _status = "Unavailable";
                        else if (ItemBase.SkinsBase.FirstOrDefault(x => x.ItemName == _itemName).CsmInfo.Overstock)
                            _status = "Overstock";
                        break;
                    }
                case 3:
                    {
                        Lfm LfItem = ItemBase.SkinsBase.FirstOrDefault(x => x.ItemName == _itemName).LfmInfo;
                        if (LfItem.Overstock)
                            _status = "Overstock";
                        else if (LfItem.Unavailable)
                        {
                            _status = "Unavailable";
                            break;
                        }
                        _price3 = LfItem.Price;
                        _price4 = Math.Round(_price3 * Calculator.CommissionLf, 2);
                        if (SettingsProperties.Default.CurrencyId == 1)
                        {
                            _price3 = Edit.ConverterToRub(_price3, SettingsProperties.Default.CurrencyValue);
                            _price4 = Edit.ConverterToRub(_price4, SettingsProperties.Default.CurrencyValue);
                        }
                        break;
                    }
            }
        }
        void Calculate(int serOneId)
        {
            if (serOneId == 0) //sta -> (any)
            {
                _precent = Edit.Precent(_price2, _price4);
                _difference = Edit.Difference(_price4, _price2);
            }
            else //(any) -> (any)
            {
                _precent = Edit.Precent(_price1, _price4);
                _difference = Edit.Difference(_price4, _price1);
            }
        }
        DataSteamMarket MarketItems(JObject json)
        {
            decimal high = !String.IsNullOrEmpty(json["highest_buy_order"].ToString()) ? Convert.ToDecimal(json["highest_buy_order"]) / 100 : 0;
            decimal low = !String.IsNullOrEmpty(json["lowest_sell_order"].ToString()) ? Convert.ToDecimal(json["lowest_sell_order"]) / 100 : 0;
            return new DataSteamMarket()
            {
                ItemName = _itemName,
                HighestBuyOrder = high,
                LowestSellOrder = low,
            };
        }
        public List<PriceHistory> PriceHistory(string itemName)
        {
            string json = Get.Request(SteamCookies, "https://steamcommunity.com/market/pricehistory/?country=RU&currency=5&appid=730&market_hash_name=" + HttpUtility.UrlEncode(itemName));
            JArray sales = JArray.Parse(JObject.Parse(json)["prices"].ToString());
            List<PriceHistory> history = new();
            foreach (var sale in sales.Reverse())
            {
                DateTime date = DateTime.Parse(sale[0].ToString()[..11]);
                decimal price = Decimal.Parse(sale[1].ToString());
                int count = Convert.ToInt32(sale[2]);

                history.Add(new PriceHistory()
                {
                    Date = date,
                    Price = price,
                    Count = count,
                });
            }
            return history;
        }
        #endregion

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
                if (list.Any(x => x == item))
                    continue;
                //Dopplers
                if (config.OnlyDopplers && !item.Contains("Doppler"))
                    continue;
                if (!config.Dopplers && item.Contains("Doppler"))
                    continue;
                //Unavailable
                if (config.ServiceTwo == 2 && ItemBase.SkinsBase.FirstOrDefault(x => x.ItemName == item).CsmInfo.Unavailable)
                    continue;
                if (config.ServiceTwo == 3 && ItemBase.SkinsBase.FirstOrDefault(x => x.ItemName == item).LfmInfo.Unavailable)
                    continue;
                //Overstock
                if (!config.Overstock)
                {
                    if (config.ServiceTwo == 2 && ItemBase.SkinsBase.FirstOrDefault(x => x.ItemName == item).CsmInfo.Overstock)
                        continue;
                    else if (config.ServiceTwo == 3 && ItemBase.SkinsBase.FirstOrDefault(x => x.ItemName == item).LfmInfo.Overstock)
                        continue;
                }
                if (!config.Ordered && config.ServiceOne == 0 && DataOrder.Orders.Any(x => x.ItemName == item))
                    continue;
                //Price
                if (config.MinPrice != 0)
                {
                    if (config.ServiceOne < 2 && ItemBase.SkinsBase.FirstOrDefault(x => x.ItemName == item).SteamInfo.Price < config.MinPrice)
                        continue;
                    else if (config.ServiceOne == 2 && DataInventoryCsm.Inventory.Where(x => x.ItemName == item).Select(x => x.Price).DefaultIfEmpty().Min() < config.MinPrice)
                        continue;
                    else if (config.ServiceOne == 3 && ItemBase.SkinsBase.FirstOrDefault(x => x.ItemName == item).LfmInfo.Price < config.MinPrice)
                        continue;
                }
                if (config.MaxPrice != 0)
                {
                    if (config.ServiceOne < 2 && ItemBase.SkinsBase.FirstOrDefault(x => x.ItemName == item).SteamInfo.Price > config.MaxPrice)
                        continue;
                    else if (config.ServiceOne == 2 && DataInventoryCsm.Inventory.Where(x => x.ItemName == item).Select(x => x.Price).DefaultIfEmpty().Min() > config.MaxPrice)
                        continue;
                    else if (config.ServiceOne == 3 && ItemBase.SkinsBase.FirstOrDefault(x => x.ItemName == item).LfmInfo.Price > config.MaxPrice)
                        continue;
                }
                //add
                string type = ItemBase.SkinsBase.FirstOrDefault(x => x.ItemName == item).Type;
                if (config.Normal && type == "Weapon" && !item.Contains("Souvenir") && !item.Contains("StatTrak™"))
                    list.Add(item);
                else if (config.Souvenir && type == "Weapon" && item.Contains("Souvenir"))
                    list.Add(item);
                else if (config.Stattrak && type == "Weapon" && item.Contains("StatTrak™"))
                    list.Add(item);
                else if (config.KnifeGlove && (type == "Knife" | type == "Gloves"))
                    list.Add(item);
                else if (config.KnifeGloveStattrak && (type == "Knife" | type == "Gloves") && item.Contains("StatTrak™"))
                    list.Add(item);
                else if (!config.Normal && !config.Souvenir && !config.Stattrak && !config.KnifeGlove && !config.KnifeGloveStattrak)
                    list.Add(item);
            }
            return list;
        }
    }
}