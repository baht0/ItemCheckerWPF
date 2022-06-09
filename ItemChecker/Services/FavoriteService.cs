using ItemChecker.MVVM.Model;
using ItemChecker.Properties;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace ItemChecker.Services
{
    public class FavoriteService : BaseService
    {
        public void PlaceOrderFav(decimal availableAmount)
        {
            ObservableCollection<DataQueue> checkedList = new(Check().OrderByDescending(x => x.Precent));
            int count = 0;
            decimal sum = 0m;
            foreach (DataQueue item in checkedList)
            {
                try
                {
                    sum += item.OrderPrice;
                    if (sum > availableAmount)
                        continue;
                    ParserQueue.PlaceOrder(item.ItemName);
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
                        DataSavedList data = DataSavedList.Items.FirstOrDefault(x => x.ItemName == item.ItemName);
                        DataSavedList.Items.Remove(data);
                        DataSavedList.Save();
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
        ObservableCollection<DataQueue> Check()
        {
            ParserCheckService checkService = new();
            ObservableCollection<DataQueue> checkedList = new();

            foreach (DataSavedList item in DataSavedList.Items.Where(x => x.ListName == "favorite" && x.ServiceId == SettingsProperties.Default.ServiceId))
            {
                try
                {
                    DataParser parseredItem = checkService.Check(item.ItemName, 0, SettingsProperties.Default.ServiceId);
                    if (parseredItem.Precent >= SettingsProperties.Default.MinPrecent + HomeProperties.Default.Reserve && !ParserQueue.CheckConditions(checkedList, parseredItem))
                    {
                        checkedList.Add(new()
                        {
                            ItemName = parseredItem.ItemName,
                            OrderPrice = parseredItem.Price2,
                            Precent = parseredItem.Precent
                        });
                    }
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
