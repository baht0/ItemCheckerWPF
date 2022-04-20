using System;
using System.Collections.Generic;
using System.Reflection;

namespace ItemChecker.MVVM.Model
{
    public class DataNotification
    {
        public bool IsRead { get; set; } = false;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime Date { get; set; } = DateTime.Now;
    }
    public class DataProjectInfo
    {
        public static List<string> FilesList{ get; set; } = new()
        {
            "ItemChecker.exe",
            "ItemChecker.dll",
            "ItemChecker.runtimeconfig.json",
            "ItemChecker.Net.dll",
            "ItemChecker.Support.dll",
            "ItemChecker.Updater.exe",
            "ItemChecker.Updater.dll",
            "ItemChecker.Updater.runtimeconfig.json",
            "icon.ico",
            "chromedriver.exe",
            "WebDriver.dll",
            "WebDriver.Support.dll",
            "Newtonsoft.Json.dll",
            "HtmlAgilityPack.dll",
            "MaterialDesignColors.dll",
            "MaterialDesignThemes.Wpf.dll"
        };
        public static string CurrentVersion { get; set; } = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public static string LatestVersion { get; set; } = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public static bool IsUpdate { get; set; } = false;
    }
}
