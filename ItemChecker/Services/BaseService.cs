using ItemChecker.MVVM.Model;
using ItemChecker.Net;
using ItemChecker.Properties;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;

namespace ItemChecker.Services
{
    public class BaseService : BaseModel
    {
        public static void GetBase()
        {
            if (BaseModel.token.IsCancellationRequested)
                return;
            decimal course = Get.Course(SettingsProperties.Default.CurrencyApiKey);
            if (course != 0)
            {
                SettingsProperties.Default.CurrencyValue = course;
                SettingsProperties.Default.Save();
            }

            ItemBaseService get = new();
            get.Overstock();
            get.Unavailable();
            get.ItemsBase();
        }
        public static void StatusSteam()
        {
            try
            {
                if (String.IsNullOrEmpty(SteamAccount.ApiKey))
                    return;
                string json = Get.GameServersStatus(SteamAccount.ApiKey);
                StatusCommunity = JObject.Parse(json)["result"]["services"]["SteamCommunity"].ToString();
            }
            catch
            {
                StatusCommunity = "error";
            }
        }

        public static void BrowserExit()
        {
            try
            {
                if (Browser != null)
                {
                    Browser.Quit();
                    Browser = null;
                }
            }
            catch
            {
                foreach (Process proc in Process.GetProcessesByName("chrome")) proc.Kill();
                foreach (Process proc in Process.GetProcessesByName("chromedriver")) proc.Kill();
                foreach (Process proc in Process.GetProcessesByName("conhost"))
                {
                    try
                    {
                        proc.Kill();
                    }
                    catch
                    {
                        continue;
                    }
                }
            }
        }

        public static void Log(string text)
        {
            if (!File.Exists("Log.txt"))
                File.WriteAllText("Log.txt", "v." + DataProjectInfo.CurrentVersion + " [" + DateTime.Now + "]\n" + text + "\n");
            else
                File.WriteAllText("Log.txt", string.Format("{0}{1}", "v." + DataProjectInfo.CurrentVersion + " [" + DateTime.Now + "]\n" + text + "\n", File.ReadAllText("Log.txt")));
        }
        public static void errorLog(Exception exp)
        {
            string message = null;
            message += exp.Message + "\n";
            message += exp.StackTrace;
            if (!File.Exists("errorsLog.txt"))
                File.WriteAllText("errorsLog.txt", "v." + DataProjectInfo.CurrentVersion + " [" + DateTime.Now + "]\n" + message + "\n");
            else
                File.WriteAllText("errorsLog.txt", string.Format("{0}{1}", "v." + DataProjectInfo.CurrentVersion + " [" + DateTime.Now + "]\n" + message + "\n", File.ReadAllText("errorsLog.txt")));
        }
        public static void errorMessage(Exception exp)
        {
            MessageBox.Show("Something went wrong :(", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        protected List<string> OpenFileDialog(string filter)
        {
            List<string> itemList = new();
            OpenFileDialog dialog = new()
            {
                InitialDirectory = AppDomain.CurrentDomain.BaseDirectory,
                RestoreDirectory = true,
                Filter = $"ItemsList ({filter}) | *.{filter}"
            };

            if (dialog.ShowDialog() == true)
                itemList = File.ReadAllLines(dialog.FileName).ToList();

            return itemList;
        }
        protected List<string> clearPrices(List<string> itemList)
        {
            List<string> adjustedList = new();
            foreach (string item in itemList)
            {
                if (item.Contains(";"))
                {
                    int id = item.LastIndexOf(';');
                    adjustedList.Add(item.Substring(0, id));
                }
                else
                    adjustedList.Add(item);
            }
            return adjustedList;
        }
    }
}
