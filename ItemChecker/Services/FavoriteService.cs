using ItemChecker.MVVM.Model;
using ItemChecker.Properties;
using ItemChecker.Support;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace ItemChecker.Services
{
    public class FavoriteService
    {
        public static void Check()
        {
            ItemBaseService baseService = new();
            switch (HomeProperties.Default.ServiceId)
            {
                case 3:
                    baseService.UpdateLfm();
                    break;
                case 4 or 5:
                    if (SteamBase.ItemList.Count / 80 < SteamMarket.Orders.Count)
                    {
                        var currency = SteamAccount.Currency.Id;
                        decimal valueUsd = Currency.ConverterToUsd(SteamMarket.Orders.Min(x => x.OrderPrice), currency);
                        int min = (int)(valueUsd * 0.5m);
                        valueUsd = Currency.ConverterToUsd(SteamAccount.Balance, currency);
                        int max = (int)(valueUsd * 2.0m);
                        baseService.UpdateBuff(HomeProperties.Default.ServiceId == 4, min, max);
                    }
                    break;
            }
            var compareList = Compare().OrderByDescending(x => x.Precent);

            int count = 0;
            decimal sum = 0m;
            foreach (DataQueue item in compareList)
            {
                try
                {
                    sum += item.OrderPrice;
                    if (sum > SteamMarket.Orders.GetAvailableAmount())
                        continue;
                    OrderService.PlaceOrder(item.ItemName);
                    count++;
                }
                catch (Exception exp)
                {
                    BaseService.errorLog(exp, false);
                    continue;
                }
                finally
                {
                    if (item.Precent < HomeProperties.Default.MinPrecent)
                    {
                        DataItem data = ItemsList.Favorite.FirstOrDefault(x => x.ItemName == item.ItemName);
                        ItemsList.Favorite.Remove(data);
                    }
                }
                if (HomePush.token.IsCancellationRequested)
                    break;
            }
            Main.Notifications.Add(new()
            {
                Title = "BuyOrderPush",
                Message = $"{count} orders were placed in the last push.",
            });
        }

        static List<DataQueue> Compare()
        {
            ParserCheckService checkService = new();
            List<DataQueue> checkedList = new();

            foreach (DataItem item in ItemsList.Favorite.Where(x => x.ServiceId == HomeProperties.Default.ServiceId))
            {
                try
                {
                    DataParser parseredItem = checkService.Check(item.ItemName, 0, HomeProperties.Default.ServiceId);
                    if (parseredItem.Precent >= HomeProperties.Default.MinPrecent + HomeProperties.Default.Reserve && !OrderService.IsAllow(parseredItem))
                        checkedList.Add(new(parseredItem.ItemName, parseredItem.Purchase, parseredItem.Precent));
                }
                catch (Exception exp)
                {
                    HomePush.cts.Cancel();
                    if (!exp.Message.Contains("429"))
                        BaseService.errorLog(exp, true);
                    else
                        MessageBox.Show(exp.Message, "PlaceOrder stoped!", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                if (HomePush.token.IsCancellationRequested)
                    break;
            }
            return checkedList;
        }
    }
}
