using ItemChecker.MVVM.Model;
using ItemChecker.Net;
using ItemChecker.Properties;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;
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
        public static void GetCurrency()
        {
            decimal rub = Get.Currency(SettingsProperties.Default.CurrencyApiKey, "RUB");
            SettingsProperties.Default.RUB = rub != 0 ? rub : SettingsProperties.Default.RUB;
            decimal cny = Get.Currency(SettingsProperties.Default.CurrencyApiKey, "CNY");
            SettingsProperties.Default.CNY = cny != 0 ? cny : SettingsProperties.Default.CNY;

            SettingsProperties.Default.Save();
        }
        public static String StatusSteam()
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

        public static void OpenBrowser()
        {
            string profilesDir = DocumentPath + "profile";

            if (!Directory.Exists(profilesDir))
                Directory.CreateDirectory(profilesDir);

            EdgeDriverService edgeDriverService = EdgeDriverService.CreateDefaultService();
            edgeDriverService.HideCommandPromptWindow = true;
            EdgeOptions option = new();
            option.AddArguments(
                "--headless",
                "--disable-gpu",
                "no-sandbox",
                "--window-size=1920,2160",
                "--disable-extensions",
                "--disable-blink-features=AutomationControlled",
                "ignore-certificate-errors");

            option.AddArguments($"--user-data-dir={profilesDir}", "profile-directory=Default");
            option.Proxy = null;

            Browser = new EdgeDriver(edgeDriverService, option, TimeSpan.FromSeconds(30));
            Browser.Manage().Window.Maximize();
            WebDriverWait = new WebDriverWait(Browser, TimeSpan.FromSeconds(10));
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

        public static void errorLog(Exception exp)
        {
            string message = null;
            message += exp.Message + "\n";
            message += exp.StackTrace;
            if (!File.Exists("errorsLog.txt"))
                File.WriteAllText(DocumentPath + "ErrorsLog.txt", "v." + DataProjectInfo.CurrentVersion + " [" + DateTime.Now + "]\n" + message + "\n");
            else
                File.WriteAllText(DocumentPath + "ErrorsLog.txt", string.Format("{0}{1}", "v." + DataProjectInfo.CurrentVersion + " [" + DateTime.Now + "]\n" + message + "\n", File.ReadAllText("ErrorsLog.txt")));
        }
        public static void errorMessage(Exception exp)
        {
            Application.Current.Dispatcher.Invoke(() => { MessageBox.Show("Something went wrong :(", "Error", MessageBoxButton.OK, MessageBoxImage.Error); });
        }

        #region file
        public static List<string> OpenFileDialog(string filter)
        {
            List<string> itemList = new();
            OpenFileDialog dialog = new()
            {
                InitialDirectory = DocumentPath,
                RestoreDirectory = true,
                Filter = $"ItemsList ({filter}) | *.{filter}"
            };

            if (dialog.ShowDialog() == true)
                itemList = File.ReadAllLines(dialog.FileName).ToList();

            return itemList;
        }
        public static List<string> ReadList(string name)
        {
            try
            {
                string path = DocumentPath + name;
                List<string> list = new();

                if (File.Exists(path))
                    list = File.ReadAllLines(path).ToList();
                return list;
            }
            catch (Exception ex)
            {
                errorLog(ex);
                return new();
            }
        }
        public static void SaveList(string name, List<string> list)
        {
            string str = string.Empty;
            foreach (string item in list)
                str += item + "\r\n";

            File.WriteAllText(DocumentPath + name, str);
        }
        #endregion
    }
}
