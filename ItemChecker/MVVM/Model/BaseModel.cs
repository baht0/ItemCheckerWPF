using HtmlAgilityPack;
using ItemChecker.Core;
using ItemChecker.Net;
using ItemChecker.Properties;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;
using System;
using System.IO;
using System.Net;
using System.Threading;

namespace ItemChecker.MVVM.Model
{
    public class BaseModel : ObservableObject
    {
        //app
        public static string AppPath { get; set; } = AppDomain.CurrentDomain.BaseDirectory;
        public static string DocumentPath{
            get
            {
                string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\ItemChecker\\";
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                return path;
            }
        }
        public static string Theme { get; set; } = "Light";
        //loading
        public static bool IsParsing { get; set; }
        public static bool IsWorking { get; set; }
        public static bool IsBrowser { get; set; }
        public static CancellationTokenSource cts { get; set; } = new();
        public static CancellationToken token { get; set; } = cts.Token;
        //selenium
        public static IWebDriver Browser { get; set; }
        public static WebDriverWait WebDriverWait { get; set; }
        //steam
        public static SteamLogin LoginSteam { get; set; } = new();
        public static string StatusCommunity { get; set; } = string.Empty;
    }
}
