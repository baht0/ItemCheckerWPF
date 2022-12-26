using ItemChecker.Core;
using ItemChecker.MVVM.Model;
using ItemChecker.Net;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace ItemChecker.Services
{
    public class BaseService
    {
        static decimal price_dol;
        public static void GetCurrencyList()
        {
            JObject json = JObject.Parse(DropboxRequest.Get.Read("steamBase.json"));
            SteamBase.CurrencyList = JArray.Parse(json["Currency"].ToString()).ToObject<List<Currency>>();

            foreach (var currency in SteamBase.CurrencyList.Where(x => x.Id == 1 || x.Id == 5 || x.Id == 23).ToList())
                currency.Value = GetCurrency(currency.Id);
        }
        public static Decimal GetCurrency(int id)
        {
            int item_nameid = 1548540;
            string itemName = "StatTrak™ AK-47 | Fire Serpent (Field-Tested)";

            var json = SteamRequest.Get.ItemOrdersHistogram(itemName, item_nameid, id);
            decimal price = Convert.ToDecimal(json["highest_buy_order"]);
            price_dol = id == 1 ? price : price_dol;

            return price / price_dol;
        }

        public static void errorLog(Exception exp, bool isShow)
        {
            try
            {
                string info = $"v.{DataProjectInfo.CurrentVersion} [{DateTime.Now}] {exp.Message}\n{exp.StackTrace}\n";
                string file = String.IsNullOrEmpty(SteamAccount.AccountName) ? "NoAccountName.txt" : $"{SteamAccount.AccountName}.txt";
                JObject json = DropboxRequest.Post.ListFolder($"ErrorLogs");
                JArray usersLog = JArray.Parse(json["entries"].ToString());
                if (usersLog.Any(x => x["name"].ToString() == file))
                {
                    string read = DropboxRequest.Get.Read($"ErrorLogs/{file}");
                    info = string.Format("{0}{1}", info, read);
                    DropboxRequest.Post.Delete($"ErrorLogs/{file}");
                }
                DropboxRequest.Post.Upload($"ErrorLogs/{file}", info);
            }
            finally
            {
                if (isShow)
                    MessageBox.Show(exp.Message, "Something went wrong :(", MessageBoxButton.OK, MessageBoxImage.Error);
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
