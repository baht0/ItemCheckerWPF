using ItemChecker.MVVM.Model;
using ItemChecker.Properties;
using ItemChecker.Support;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace ItemChecker.Services
{
    public class FavoriteService
    {
        public void Check()
        {
            ItemBaseService baseService = new();
            switch (SettingsProperties.Default.ServiceId)
            {
                case 2:
                    baseService.UpdateCsm();
                    break;
                case 3:
                    baseService.UpdateLfm();
                    break;
                case 4:
                    var currency = SteamBase.CurrencyList.FirstOrDefault(x => x.Id == SteamAccount.CurrencyId).Value;
                    decimal valueUsd = Edit.ConverterToUsd(SteamMarket.Orders.Min(x => x.OrderPrice), currency);
                    int min = (int)(valueUsd * 0.5m);
                    valueUsd = Edit.ConverterToUsd(SteamAccount.Balance, SteamBase.CurrencyList.FirstOrDefault(x => x.Id == 1).Value);
                    int max = (int)(valueUsd * 2.0m);
                    baseService.UpdateBuff(true, min, max);
                    break;
                case 5:
                    currency = SteamBase.CurrencyList.FirstOrDefault(x => x.Id == SteamAccount.CurrencyId).Value;
                    valueUsd = Edit.ConverterToUsd(SteamMarket.Orders.Min(x => x.OrderPrice), currency);
                    min = (int)(valueUsd * 0.5m);
                    valueUsd = Edit.ConverterToUsd(SteamAccount.Balance, SteamBase.CurrencyList.FirstOrDefault(x => x.Id == 1).Value);
                    max = (int)(valueUsd * 2.0m);
                    baseService.UpdateBuff(false, min, max);
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
                    if (item.Precent < SettingsProperties.Default.MinPrecent && HomeProperties.Default.Unwanted)
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
        List<DataQueue> Compare()
        {
            ParserCheckService checkService = new();
            List<DataQueue> checkedList = new();

            foreach (DataItem item in ItemsList.Favorite.Where(x => x.ServiceId == SettingsProperties.Default.ServiceId))
            {
                try
                {
                    DataParser parseredItem = checkService.Check(item.ItemName, 0, SettingsProperties.Default.ServiceId);
                    if (parseredItem.Precent >= SettingsProperties.Default.MinPrecent + HomeProperties.Default.Reserve && !OrderService.IsAllow(parseredItem))
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
