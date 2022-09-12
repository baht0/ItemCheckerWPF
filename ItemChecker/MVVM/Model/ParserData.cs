using ItemChecker.Properties;
using ItemChecker.Support;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ItemChecker.MVVM.Model
{
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
    public class DataQueue
    {
        decimal _orderPrice;
        public string ItemName { get; set; }
        public decimal OrderPrice
        {
            get
            {
                return _orderPrice;
            }
            set
            {
                var currency = SteamBase.CurrencyList.FirstOrDefault(x => x.Id == SteamAccount.CurrencyId).Value;
                if (ParserTable.CurectCurrency.Id != 1)
                    value = Edit.ConverterToUsd(value, ParserTable.CurectCurrency.Value);
                value = Edit.ConverterFromUsd(value, currency);
                _orderPrice = value;
            }
        }
        public decimal Precent { get; set;}

        public DataQueue(string itemName, decimal orderPrice, decimal precent)
        {
            ItemName = itemName;
            OrderPrice = orderPrice;
            Precent = precent;
        }
    }
    public class Queue<T> : List<T>
    {
        public new Boolean Add(T item)
        {
            var currentItem = item as DataQueue;
            if (IsAllow(currentItem))
            {
                base.Add(item);
                return true;
            }
            return false;
        }
        Boolean IsAllow(DataQueue item)
        {
            var currentList = this as Queue<DataQueue>;

            decimal sum = currentList.Select(x => x.OrderPrice).Sum();

            bool isAllow = !currentList.Any(x => x.ItemName == item.ItemName);
            if (isAllow)
                isAllow = !SteamMarket.Orders.Any(n => n.ItemName == item.ItemName);
            if (isAllow)
                isAllow = (sum + item.OrderPrice) < SteamMarket.Orders.GetAvailableAmount();
            if (isAllow)
                isAllow = item.OrderPrice < SteamAccount.Balance;
            if (isAllow)
                isAllow = item.Precent > SettingsProperties.Default.MinPrecent;

            return isAllow;
        }
    }
}