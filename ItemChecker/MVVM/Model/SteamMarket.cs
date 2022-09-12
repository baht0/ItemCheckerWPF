using ItemChecker.Net;
using ItemChecker.Properties;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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
                    if (String.IsNullOrEmpty(SteamAccount.ApiKey))
                        return string.Empty;
                    JObject res = Get.GameServersStatus(SteamAccount.ApiKey);
                    return res["result"]["services"]["SteamCommunity"].ToString();
                }
                catch
                {
                    return "error";
                }
            }
        }
        public static Order<DataOrder> Orders { get; set; } = new();
    }
    public class DataOrder
    {
        public string ItemName { get; set; }
        public string Id { get; set; }
        public decimal SteamPrice { get; set; }
        public decimal OrderPrice { get; set; }
        public decimal ServicePrice { get; set; }
        public decimal ServiceGive { get; set; }
        public decimal Precent { get; set; }
        public decimal Difference { get; set; }
    }
    public class Order<T> : List<T>
    {
        public Boolean CancelMinPrecent()
        {
            var currentList = this as Order<DataOrder>;
            decimal availableAmount = GetAvailableAmount();

            if (availableAmount < (SteamAccount.MaxAmount * 0.01m))
            {
                currentList.Cancel(currentList.FirstOrDefault(x => x.Precent == currentList.Min(x => x.Precent)));
                return true;
            }
            return false;
        }
        public Decimal GetAvailableAmount()
        {
            var currentList = this as Order<DataOrder>;

            if (currentList.Any())
                return Math.Round(SteamAccount.MaxAmount - currentList.Sum(s => s.OrderPrice), 2);
            return SteamAccount.MaxAmount;
        }
        public void Cancel(T order)
        {
            var itemOrder = order as DataOrder;
            string market_hash_name = HttpUtility.UrlEncode(itemOrder.ItemName);
            Post.CancelBuyOrder(SteamAccount.Cookies, market_hash_name, itemOrder.Id);
            base.Remove(order);
        }
        public new Boolean Add(T item)
        {
            if (IsAllow(item))
            {
                base.Add(item);
                return true;
            }
            Cancel(item);
            return false;
        }
        Boolean IsAllow(T order)
        {
            var itemOrder = order as DataOrder;
            var item = SteamBase.ItemList.FirstOrDefault(x => x.ItemName == itemOrder.ItemName);

            bool isAllow = item != null;
            if (isAllow)
                isAllow = SteamAccount.Balance > itemOrder.OrderPrice;
            if (isAllow)
                isAllow = SettingsProperties.Default.ServiceId != 0 && SettingsProperties.Default.MinPrecent < itemOrder.Precent;
            if (isAllow)
            {
                switch (SettingsProperties.Default.ServiceId)
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
}
