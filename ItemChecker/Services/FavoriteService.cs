using ItemChecker.MVVM.Model;
using ItemChecker.Properties;
using ItemChecker.Support;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;

namespace ItemChecker.Services
{
    public class FavoriteService : BaseService
    {
        #region TXT
        public static void ExportTxt(ObservableCollection<string> FavoriteList)
        {
            string txt = string.Empty;
            foreach (string item in FavoriteList)
                txt += $"{item}\r\n";

            string path = $"{BaseModel.AppPath}\\extract";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            File.WriteAllText(path + $"\\Home_Favorite_{DateTime.Now:dd.MM.yyyy_hh.mm}.txt", Edit.replaceSymbols(txt));
        }
        public ObservableCollection<string> ImportTxt()
        {
            List<string> items = OpenFileDialog("txt");

            return new ObservableCollection<string>(items);
        }
        #endregion TXT
        public Int32 PlaceOrderFav(decimal availableAmount)
        {
            ObservableCollection<QueueData> checkedList = Check();
            checkedList = new ObservableCollection<QueueData>(checkedList.OrderByDescending(x => x.Precent));
            int count = 0;
            decimal sum = 0m;
            foreach (var item in checkedList)
            {
                if (HomeConfig.tokenPush.IsCancellationRequested)
                    break;
                try
                {
                    sum += item.OrderPrice;
                    if (sum > availableAmount)
                        break;
                    Queue.PlaceOrder(item.ItemName);
                    count++;
                }
                catch (Exception exp)
                {
                    BaseService.errorLog(exp);
                    continue;
                }
            }
            return count;
        }
        ObservableCollection<QueueData> Check()
        {
            ParserCheckService checkService = new();
            ObservableCollection<QueueData> checkedList = new();

            foreach (string itemName in HomeProperties.Default.FavoriteList)
            {
                if (HomeConfig.tokenPush.IsCancellationRequested)
                    break;
                try
                {
                    DataParser data = checkService.Check(itemName, 0, SettingsProperties.Default.ServiceId);
                    if (data.Precent >= SettingsProperties.Default.MinPrecent + HomeProperties.Default.Reserve)
                        checkedList = Queue.AddQueue(checkedList, data);
                    else if (data.Precent < SettingsProperties.Default.MinPrecent && HomeProperties.Default.Unwanted)
                        HomeProperties.Default.FavoriteList.Remove(itemName);
                }
                catch (Exception exp)
                {
                    HomeConfig.ctsPush.Cancel();
                    if (!exp.Message.Contains("429"))
                    {
                        BaseService.errorLog(exp);
                        BaseService.errorMessage(exp);
                    }
                    else
                        MessageBox.Show(exp.Message, "Parser stoped!", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            return checkedList;
        }
    }
}
