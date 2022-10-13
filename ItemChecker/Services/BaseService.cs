﻿using ItemChecker.MVVM.Model;
using ItemChecker.Net;
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
            int item_nameid = 1548540; //StatTrak™ AK-47 | Fire Serpent (Field-Tested)
            string url = "https://steamcommunity.com/market/itemordershistogram?country=RU&language=english&currency=1&item_nameid=" + item_nameid + "&two_factor=0";
            decimal price_usd = Convert.ToDecimal(JObject.Parse(Get.Request(url))["highest_buy_order"]);

            JObject json = JObject.Parse(Get.DropboxRead("steamBase.json"));
            List<Currency> currencies = JArray.Parse(json["Currency"].ToString()).ToObject<List<Currency>>();

            List<int> allowCurrencyId = new() { 1, 5, 23 };//steamCurrencies ID
            foreach (var currency in currencies)
            {
                currency.Value = Get.CurrencySteam(currency.Id, item_nameid, price_usd);
                currency.IsAllow = allowCurrencyId.Any(x => x == currency.Id);
                SteamBase.CurrencyList.Add(currency);
            }
        }

        public static void OpenBrowser()
        {
            string profilesDir = ProjectInfo.DocumentPath + "profile";

            if (!Directory.Exists(profilesDir))
                Directory.CreateDirectory(profilesDir);

            EdgeDriverService edgeDriverService = EdgeDriverService.CreateDefaultService();
            edgeDriverService.HideCommandPromptWindow = true;
            EdgeOptions option = new();
            option.AddArguments(
                $"--user-data-dir={profilesDir}",
                "profile-directory=Default",
                "--headless",
                "--disable-gpu",
                "no-sandbox",
                "--window-size=1920,2160",
                "--disable-extensions",
                "--disable-blink-features=AutomationControlled",
                "ignore-certificate-errors");

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
                foreach (Process proc in Process.GetProcessesByName("msedge")) proc.Kill();
                foreach (Process proc in Process.GetProcessesByName("msedgedriver")) proc.Kill();
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

        public static void errorLog(Exception exp, bool isShow)
        {
            try
            {
                string info = $"v.{DataProjectInfo.CurrentVersion} [{DateTime.Now}] {exp.Message}\n{exp.StackTrace}\n";
                string file = String.IsNullOrEmpty(SteamAccount.AccountName) ? "NoAccountName.txt" : $"{SteamAccount.AccountName}.txt";
                JObject json = Post.DropboxListFolder($"ErrorLogs");
                JArray usersLog = JArray.Parse(json["entries"].ToString());
                if (usersLog.Any(x => x["name"].ToString() == file))
                {
                    string read = Get.DropboxRead($"ErrorLogs/{file}");
                    info = string.Format("{0}{1}", info, read);
                    Post.DropboxDelete($"ErrorLogs/{file}");
                }
                Post.DropboxUpload($"ErrorLogs/{file}", info);

                if (isShow)
                    MessageBox.Show(exp.Message, "Something went wrong :(", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch
            {
                MessageBox.Show($"Error saving logs.\n\n{exp.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static String OpenFileDialog(string filter)
        {
            OpenFileDialog dialog = new()
            {
                InitialDirectory = ProjectInfo.DocumentPath,
                RestoreDirectory = true,
                Filter = $"File | *.{filter}"
            };

            return dialog.ShowDialog() == true ? File.ReadAllText(dialog.FileName) : string.Empty;
        }
    }
}
