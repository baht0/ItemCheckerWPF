using ItemChecker.MVVM.Model;
using ItemChecker.Net;
using ItemChecker.Support;
using Microsoft.Win32;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace ItemChecker.Services
{
    public class BaseService : BaseModel
    {
        protected String ParseMrinka(string itemName)
        {
            Tuple<String, Boolean> response = Tuple.Create(string.Empty, false);
            do
            {
                response = Get.MrinkaRequest(Edit.MarketHashName(itemName));
                if (!response.Item2)
                {
                    Thread.Sleep(30000);
                }
            }
            while (!response.Item2);

            return response.Item1;
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
