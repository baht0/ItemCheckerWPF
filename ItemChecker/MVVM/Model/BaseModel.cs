using HtmlAgilityPack;
using ItemChecker.Core;
using ItemChecker.Net;
using ItemChecker.Properties;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
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
        public static CookieContainer SteamCookies { get; set; } = new();
        public static SteamLogin LoginSteam { get; set; } = new();
        public static string StatusCommunity { get; set; } = string.Empty;

        public static void OpenBrowser()
        {
            string profilesDir = AppPath + "profile";

            if (!Directory.Exists(profilesDir))
                Directory.CreateDirectory(profilesDir);

            DirectoryInfo dirInfo = new(profilesDir);
            dirInfo.Attributes = FileAttributes.Hidden;

            ChromeDriverService chromeDriverService = ChromeDriverService.CreateDefaultService();
            chromeDriverService.HideCommandPromptWindow = true;
            ChromeOptions option = new();
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

            Browser = new ChromeDriver(chromeDriverService, option, TimeSpan.FromSeconds(30));
            Browser.Manage().Window.Maximize();
            WebDriverWait = new WebDriverWait(Browser, TimeSpan.FromSeconds(10));
        }

        public static Boolean IsLogIn()
        {
            bool isLogin = true;
            string url = "https://steamcommunity.com/login/home/?goto=my/profile";
            System.Net.Cookie steamSessionId = Get.SteamSessionId();
            string steamLoginSecure = SettingsProperties.Default.SteamLoginSecure.Replace("\r\n", "").Trim();
            if (!String.IsNullOrEmpty(steamLoginSecure))
            {
                SteamCookies.Add(steamSessionId);
                SteamCookies.Add(new System.Net.Cookie("steamLoginSecure", steamLoginSecure, "/", "steamcommunity.com"));
                string html = Get.Request(SteamCookies, url);
                HtmlDocument htmlDoc = new();
                htmlDoc.LoadHtml(html);
                string title = htmlDoc.DocumentNode.SelectSingleNode("html/head/title").InnerText;
                isLogin = title.Contains("Sign In");
            }
            if (isLogin)
            {
                if (Browser == null)
                    OpenBrowser();
                Browser.Navigate().GoToUrl(url);
                isLogin = !Browser.Url.Contains("id") & !Browser.Url.Contains("profiles") ? true : GetCookies();
            }
            return isLogin;
        }
        public static Boolean GetCookies()
        {
            try
            {
                System.Net.Cookie steamSessionId = Get.SteamSessionId();
                ICookieJar cookies = Browser.Manage().Cookies;
                string steamLoginSecure = cookies.GetCookieNamed("steamLoginSecure").Value.ToString();
                SteamCookies = new();
                SteamCookies.Add(steamSessionId);
                SteamCookies.Add(new System.Net.Cookie("steamLoginSecure", steamLoginSecure, "/", "steamcommunity.com"));

                if (StartUpProperties.Default.Remember)
                {
                    SettingsProperties.Default.SteamLoginSecure = steamLoginSecure;
                    SettingsProperties.Default.Save();
                }
                if (SettingsProperties.Default.Quit)
                {
                    Browser.Quit();
                    Browser = null;
                }
                return false;
            }
            catch
            {
                return true;
            }
        }
        public static Boolean Steam()
        {
            try
            {
                Browser.Navigate().GoToUrl("https://steamcommunity.com/login/home/?goto=my/profile");

                IWebElement username = WebDriverWait.Until(e => e.FindElement(By.XPath("//input[@name='username']")));
                IWebElement password = WebDriverWait.Until(e => e.FindElement(By.XPath("//input[@name='password']")));
                try
                {
                    IWebElement remember = WebDriverWait.Until(e => e.FindElement(By.XPath("//input[@name='remember_login']")));
                    remember.Click();
                }
                catch { }
                IWebElement signin = WebDriverWait.Until(e => e.FindElement(By.XPath("//button[@class='btn_blue_steamui btn_medium login_btn']")));

                while (!LoginSteam.IsLoggedIn)
                    Thread.Sleep(500);
                username.SendKeys(LoginSteam.Login);
                password.SendKeys(LoginSteam.Password);
                signin.Click();

                Thread.Sleep(2000);
                IWebElement code = WebDriverWait.Until(e => e.FindElement(By.XPath("//input[@id='twofactorcode_entry']")));
                code.SendKeys(LoginSteam.Code2AF);
                code.SendKeys(Keys.Enter);

                StartUpProperties.Default.Remember = LoginSteam.Remember;
                StartUpProperties.Default.Save();
                Thread.Sleep(4000);

                LoginSteam.IsLoggedIn = false;
                return !Browser.Url.Contains("id") & !Browser.Url.Contains("profiles") ? true : GetCookies();
            }
            catch
            {
                LoginSteam.IsLoggedIn = false;
                return true;
            }
        }        
    }
}
