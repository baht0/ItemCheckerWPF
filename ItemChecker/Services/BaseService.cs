using ItemChecker.MVVM.Model;
using ItemChecker.Net;
using ItemChecker.Properties;
using ItemChecker.Support;
using Microsoft.Win32;
using OpenQA.Selenium.Support.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ItemChecker.Services
{
    public class BaseService : Main
    {
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
