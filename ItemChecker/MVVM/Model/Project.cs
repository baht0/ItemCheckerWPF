using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

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
        public static string UpdateFolder
        {
            get
            {
                return AppDomain.CurrentDomain.BaseDirectory + @"\update";
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
        public static string Theme
        {
            get
            {

                RegistryKey registry = Registry.CurrentUser.OpenSubKey(
                        @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
                if ((int)registry.GetValue("SystemUsesLightTheme") == 1)
                    return "Light";
                else
                    return "Dark";
            }
        }
    }
    public class ProjectUpdates
    {
        public string Version { get; set; }
        public string Text { get; set; }
        public DateTime Date { get; set; }
    }
    public class DataProjectInfo
    {
        public static List<string> FilesList
        {
            get
            {
                return new()
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
                    "Newtonsoft.Json.dll",
                    "HtmlAgilityPack.dll",
                    "MaterialDesignColors.dll",
                    "MaterialDesignThemes.Wpf.dll",
                };
            }
        }
        public static string CurrentVersion
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }
        public static string LatestVersion { get; set; } = string.Empty;
    }
}
