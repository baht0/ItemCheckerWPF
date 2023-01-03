using ItemChecker.Net;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ItemChecker.Support
{
    public class Currencies
    {
        public static List<DataCurrency> Steam { get; set; } = new();
        public static List<DataCurrency> Allow
        {
            get
            {
                return Steam.Where(x => x.Value > 0).ToList();
            }
        }

        public static void GetSteamCurrencies()
        {
            var json = JArray.Parse(DropboxRequest.Get.Read("SteamCurrencies.json"));
            Steam = JArray.Parse(json.ToString()).ToObject<List<DataCurrency>>();

            foreach (var currency in Steam.Where(x => x.Id == 1 || x.Id == 23).ToList())
                currency.Value = Currency.GetCurrency(currency.Id);
        }
    }
    public class Currency
    {
        static decimal DollarValue;
        public static decimal GetCurrency(int id)
        {
            int item_nameid = 1548540;
            string itemName = "StatTrak™ AK-47 | Fire Serpent (Field-Tested)";

            var json = SteamRequest.Get.ItemOrdersHistogram(itemName, item_nameid, id);
            decimal price = Convert.ToDecimal(json["highest_buy_order"]);
            DollarValue = id == 1 ? price : DollarValue;

            return price / DollarValue;
        }

        public static decimal ConverterFromUsd(decimal value, int valueCurrencyId)
        {
            return Math.Round(value * Currencies.Allow.FirstOrDefault(x => x.Id == valueCurrencyId).Value, 2);
        }
        public static decimal ConverterToUsd(decimal value, int valueCurrencyId)
        {
            return Math.Round(value / Currencies.Allow.FirstOrDefault(x => x.Id == valueCurrencyId).Value, 2);
        }
    }
    public class DataCurrency
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string Symbol { get; set; } = string.Empty;
        public string Name
        {
            get
            {
                return $"{Code} ({Symbol})";
            }
        }
        public decimal Value { get; set; }
    }
}
