using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ItemChecker.MVVM.Model
{
    public class ProjectInfo
    {
        public static string AppPath
        {
            get
            {
                return AppDomain.CurrentDomain.BaseDirectory;
            }
        }
        public static string DocumentPath
        {
            get
            {
                string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\ItemChecker\\";
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                return path;
            }
        }
        public static string Theme { get; set; } = "Light";
    }
    public class ProjectUpdates
    {
        public string Version { get; set; }
        public string Text { get; set; }
        public DateTime Date { get; set; }

        public static List<ProjectUpdates> Updates { get; set; } = new();
    }
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
            "msedgedriver.exe",
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
