using System.Collections.Generic;
using System.Reflection;

namespace ItemChecker.MVVM.Model
{
    public class DataProjectInfo
    {
        public static List<string> FilesList { get; set; } = new()
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
