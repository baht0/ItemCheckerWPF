using ItemChecker.Net;
using ItemChecker.Properties;
using ItemChecker.Support;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ItemChecker.MVVM.Model
{
    internal class SteamMarket
    {
        public static string StatusCommunity
        {
            get
            {
                try
                {
                    JObject res = SteamRequest.Get.GameServersStatus();
                    return res["result"]["services"]["SteamCommunity"].ToString();
                }
                catch
                {
                    return "error";
                }
            }
        }
        public static decimal MaxAmount
        {
            get
            {
                return SteamAccount.Balance * 10;
            }
        }
        public static Order<DataOrder> Orders { get; set; } = new();
    }
    public class DataOrder
    {
        public string ItemName { get; set; }
        public string Id { get; set; }
        public decimal OrderPrice { get; set; }
        public decimal ServicePrice { get; set; }
        public decimal ServiceGive { get; set; }
        public decimal Precent { get; set; }
        public decimal Difference { get; set; }
    }
    public class Order<T> : List<T>
    {
        public bool CancelMinPrecent()
        {
            var currentList = this as Order<DataOrder>;
            decimal availableAmount = GetAvailableAmount();

            if (availableAmount < (SteamMarket.MaxAmount * 0.01m))
            {
                currentList.Cancel(currentList.FirstOrDefault(x => x.Precent == currentList.Min(x => x.Precent)));
                return true;
            }
            return false;
        }
        public decimal GetAvailableAmount()
        {
            var currentList = this as Order<DataOrder>;

            if (currentList.Any())
                return Math.Round(SteamMarket.MaxAmount - currentList.Sum(s => s.OrderPrice), 2);
            return SteamMarket.MaxAmount;
        }
        public void Cancel(T order)
        {
            var item = order as DataOrder;
            SteamRequest.Post.CancelBuyOrder(item.ItemName, item.Id);
            base.Remove(order);
            var reservItem = SavedItems.Reserve.FirstOrDefault(x => x.ItemName == item.ItemName && x.ServiceId == HomeProperties.Default.ServiceId);
            if (reservItem != null)
                SavedItems.Reserve.Remove(reservItem);
        }
        public new bool Add(T order)
        {
            if (IsAllow(order))
            {
                base.Add(order);
                var item = order as DataOrder;
                SavedItems.Reserve.Add(new(item.ItemName, HomeProperties.Default.ServiceId));
                return true;
            }
            Cancel(order);
            return false;
        }
        bool IsAllow(T order)
        {
            var itemOrder = order as DataOrder;
            var item = ItemsBase.List.FirstOrDefault(x => x.ItemName == itemOrder.ItemName);

            bool isAllow = item != null;
            if (isAllow)
                isAllow = SteamAccount.Balance > itemOrder.OrderPrice;
            if (isAllow)
                isAllow = HomeProperties.Default.ServiceId == 0 || (HomeProperties.Default.ServiceId != 0 && HomeProperties.Default.MinPrecent < itemOrder.Precent);
            if (isAllow)
            {
                switch (HomeProperties.Default.ServiceId)
                {
                    case 2:
                        isAllow = !item.Csm.Overstock && !item.Csm.Unavailable;
                        break;
                    case 3:
                        isAllow = !item.Lfm.Overstock && !item.Lfm.Unavailable;
                        break;
                }
            }

            return isAllow;
        }
    }

    public class DataQueue
    {
        public string ItemName { get; set; }
        public decimal OrderPrice
        {
            get
            {
                return _orderPrice;
            }
            set
            {
                var currency = SteamAccount.Currency.Id;
                if (ParserTable.CurectCurrency.Id != 1)
                    value = Currency.ConverterToUsd(value, ParserTable.CurectCurrency.Id);
                value = Currency.ConverterFromUsd(value, currency);
                _orderPrice = value;
            }
        }
        decimal _orderPrice;
        public decimal Precent { get; set; }

        public DataQueue(string itemName, decimal orderPrice, decimal precent)
        {
            ItemName = itemName;
            OrderPrice = orderPrice;
            Precent = precent;
        }
    }
    public class Queue<T> : List<T>
    {
        public new bool Add(T item)
        {
            if (IsAllow(item))
            {
                base.Add(item);
                return true;
            }
            return false;
        }
        bool IsAllow(T item)
        {
            var currentItem = item as DataQueue;
            var currentList = this as Queue<DataQueue>;

            decimal sum = currentList.Select(x => x.OrderPrice).Sum();

            bool isAllow = !currentList.Any(x => x.ItemName == currentItem.ItemName);
            if (isAllow)
                isAllow = !SteamMarket.Orders.Any(n => n.ItemName == currentItem.ItemName);
            if (isAllow)
                isAllow = (sum + currentItem.OrderPrice) < SteamMarket.Orders.GetAvailableAmount();
            if (isAllow)
                isAllow = currentItem.OrderPrice < SteamAccount.Balance;
            if (isAllow)
                isAllow = currentItem.Precent > HomeProperties.Default.MinPrecent;

            return isAllow;
        }
    }
}
