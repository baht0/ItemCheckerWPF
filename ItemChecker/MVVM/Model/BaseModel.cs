using HtmlAgilityPack;
using ItemChecker.Core;
using ItemChecker.Net;
using ItemChecker.Properties;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.IO;
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
        public static string StatusCommunity { get; set; }

        public static SteamLogin LoginSteam { get; set; } = new();

        public static Boolean GetCookies()
        {
            try
            {
                string url = "https://steamcommunity.com/login/home/?goto=my/profile";
                if (SettingsProperties.Default.SteamCookies != null)
                {
                    string html = Get.Request(SettingsProperties.Default.SteamCookies, url);
                    HtmlDocument htmlDoc = new();
                    htmlDoc.LoadHtml(html);
                    string title = htmlDoc.DocumentNode.SelectSingleNode("html/head/title").InnerText;
                    if (!title.Contains("Sign In"))
                        return true;
                }
                if (Browser == null)
                    OpenBrowser();
                Browser.Navigate().GoToUrl(url);

                while (!Browser.Url.Contains("id") & !Browser.Url.Contains("profiles"))
                {
                    LoginSteam.IsLoggedIn = false;
                    Steam();
                }
                LoginSteam.IsLoggedIn = true;
                ICookieJar cookies = Browser.Manage().Cookies;
                string steamLoginSecure = cookies.GetCookieNamed("steamLoginSecure").Value.ToString();

                SettingsProperties.Default.SteamCookies = new();
                SettingsProperties.Default.SteamCookies.Add(Get.GetSteamSessionId());
                SettingsProperties.Default.SteamCookies.Add(new System.Net.Cookie("steamLoginSecure", steamLoginSecure, "/", "steamcommunity.com"));
                SettingsProperties.Default.Save();

                if (SettingsProperties.Default.Quit)
                {
                    Browser.Quit();
                    Browser = null;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        static void OpenBrowser()
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
        static void Steam()
        {
            IWebElement username = Browser.FindElement(By.XPath("//input[@name='username']"));
            IWebElement password = Browser.FindElement(By.XPath("//input[@name='password']"));
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

            Thread.Sleep(4000);
        }        
    }
}
