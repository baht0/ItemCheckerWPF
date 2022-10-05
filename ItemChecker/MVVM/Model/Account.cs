using HtmlAgilityPack;
using ItemChecker.MVVM.ViewModel;
using ItemChecker.Net;
using ItemChecker.Properties;
using ItemChecker.Services;
using ItemChecker.Support;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows;

namespace ItemChecker.MVVM.Model
{
    public class SteamSignUp
    {
        public bool IsLoggedIn { get; set; } = false;
        public string Login { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool Remember { get; set; } = false;
        public string Code2AF { get; set; } = string.Empty;

        public static SteamSignUp SignUp { get; set; } = new();

        public static Boolean AllowUser(string login)
        {
            JArray users = JArray.Parse(Get.DropboxRead("Users.json"));
            JObject user = (JObject)users.FirstOrDefault(x => x["Login"].ToString() == login);
            if (user != null)
            {
                int id = users.IndexOf(user);
                users[id]["LastLoggedIn"] = DateTime.Now;
                users[id]["Version"] = DataProjectInfo.CurrentVersion;

                Post.DropboxDelete("Users.json");
                Thread.Sleep(200);
                Post.DropboxUpload("Users.json", users.ToString());
                return Convert.ToBoolean(user["Allowed"]);
            }
            return false;
        }
    }
    public class SteamAccount : BaseModel
    {
        static decimal _balance = -1;
        static string _apiKey = string.Empty;
        public static CookieContainer Cookies { get; set; } = new();
        public static string Id64 { get; set; } = string.Empty;
        public static string AccountName { get; set; } = string.Empty;
        public static string UserName { get; set; } = string.Empty;
        public static string StatusMarket { get; set; } = "Enabled";
        public static int CurrencyId { get; set; } = 5;
        public static decimal Balance
        {
            get
            {
                return _balance;
            }
            set
            {
                if (_balance > value && _balance != -1)
                {
                    Main.Notifications.Add(new()
                    {
                        Title = "Balance",
                        Message = $"Your balance has decreased\n-{_balance - value}."
                    });
                }
                else if (_balance < value && _balance != -1)
                {
                    Main.Notifications.Add(new()
                    {
                        Title = "Balance",
                        Message = $"Your balance has increased\n+{value - _balance}."
                    });
                }
                _balance = value;
            }
        }
        public static decimal MaxAmount
        {
            get
            {
                return Balance * 10;
            }
        }
        public static string ApiKey
        {
            get
            {
                if (String.IsNullOrEmpty(_apiKey))
                {
                    var html = Get.Request(SteamAccount.Cookies, "https://steamcommunity.com/dev/apikey");
                    var htmlDoc = new HtmlDocument();
                    htmlDoc.LoadHtml(html);
                    _apiKey = htmlDoc.DocumentNode.SelectSingleNode("//div[@id='bodyContents_ex']/p").InnerText;

                    if (_apiKey.Contains("Key: "))
                        _apiKey = _apiKey.Replace("Key: ", string.Empty);
                    else
                        Main.Notifications.Add(new()
                        {
                            Title = "Steam Account",
                            Message = "Failed to get your API Key!\nSome features will not be available to you."
                        });
                }
                return _apiKey;
            }
            set
            {
                _apiKey = value;
            }
        }

        public static Boolean NeedLogin()
        {
            bool showLogin = true;
            string url = "https://steamcommunity.com/login/home/?goto=my/profile";
            string steamLoginSecure = MainProperties.Default.SteamLoginSecure.Replace("\r\n", "").Trim();

            if (!String.IsNullOrEmpty(steamLoginSecure))
            {
                System.Net.Cookie steamSessionId = Get.SteamSessionId();
                SteamAccount.Cookies.Add(steamSessionId);
                SteamAccount.Cookies.Add(new System.Net.Cookie("steamLoginSecure", steamLoginSecure, "/", "steamcommunity.com"));
                string html = Get.Request(SteamAccount.Cookies, url);
                HtmlDocument htmlDoc = new();
                htmlDoc.LoadHtml(html);
                string title = htmlDoc.DocumentNode.SelectSingleNode("html/head/title").InnerText;
                showLogin = title.Contains("Sign In");
            }
            if (showLogin)
            {
                if (Browser == null)
                    BaseService.OpenBrowser();
                Browser.Navigate().GoToUrl(url);
                showLogin = !Browser.Url.Contains("id") & !Browser.Url.Contains("profiles") ? true : GetCookies();
            }
            return showLogin;
        }
        public static Boolean Login()
        {
            try
            {
                Browser.Navigate().GoToUrl("https://steamcommunity.com/login/home/?goto=my/profile");

                IWebElement username = WebDriverWait.Until(e => e.FindElement(By.XPath("//input[@class='newlogindialog_TextInput_2eKVn'][1]")));
                IWebElement password = WebDriverWait.Until(e => e.FindElement(By.XPath("//*[@id='responsive_page_template_content']/div[1]/div[1]/div/div/div/div[2]/div/form/div[2]/input")));

                IWebElement signin = WebDriverWait.Until(e => e.FindElement(By.XPath("//*[@id='responsive_page_template_content']/div[1]/div[1]/div/div/div/div[2]/div/form/div[4]/button")));

                while (!SteamSignUp.SignUp.IsLoggedIn)
                    Thread.Sleep(500);
                username.SendKeys(SteamSignUp.SignUp.Login);
                password.SendKeys(SteamSignUp.SignUp.Password);
                signin.Click();

                Thread.Sleep(2000);
                for (int i = 1; i <= 5; i++)
                {
                    IWebElement code = WebDriverWait.Until(e => e.FindElement(By.XPath($"//*[@id='responsive_page_template_content']/div[1]/div[1]/div/div/div/div[2]/form/div/div[2]/div/input[{i}]")));
                    code.SendKeys(SteamSignUp.SignUp.Code2AF[i-1].ToString());
                }

                MainProperties.Default.Save();
                Thread.Sleep(4000);

                SteamSignUp.SignUp.IsLoggedIn = false;
                return !Browser.Url.Contains("id") & !Browser.Url.Contains("profiles") ? true : GetCookies();
            }
            catch
            {
                SteamSignUp.SignUp.IsLoggedIn = false;
                return true;
            }
        }
        static Boolean GetCookies()
        {
            try
            {
                System.Net.Cookie steamSessionId = Get.SteamSessionId();
                string steamLoginSecure = Browser.Manage().Cookies.GetCookieNamed("steamLoginSecure").Value.ToString();

                CurrencyId = MainProperties.Default.SteamCurrencyId;
                if (CurrencyId == 0)
                {
                    string country = Browser.Manage().Cookies.GetCookieNamed("steamCountry").Value.ToString()[..2];
                    var currency = SteamBase.CurrencyList.FirstOrDefault(x => x.Country == country);
                    CurrencyId = currency != null ? currency.Id : 1;
                }

                MainProperties.Default.SteamCurrencyId = CurrencyId;
                SteamAccount.Cookies = new();
                SteamAccount.Cookies.Add(steamSessionId);
                SteamAccount.Cookies.Add(new System.Net.Cookie("steamLoginSecure", steamLoginSecure, "/", "steamcommunity.com"));
                
                MainProperties.Default.SteamLoginSecure = steamLoginSecure;
                MainProperties.Default.Save();
                Browser.Quit();
                Browser = null;

                return false;
            }
            catch (Exception ex)
            {
                BaseService.errorLog(ex, false);
                return true;
            }
        }

        public static void GetAccount()
        {
            System.Net.Cookie steamLoginSecure = SteamAccount.Cookies.GetAllCookies().FirstOrDefault(x => x.Name == "steamLoginSecure");
            Id64 = steamLoginSecure.Value[..17];
            string html = Get.Request(SteamAccount.Cookies, "https://steamcommunity.com/market/");
            HtmlDocument htmlDoc = new();
            htmlDoc.LoadHtml(html);
            UserName = htmlDoc.DocumentNode.SelectSingleNode("//span[@id='account_pulldown']").InnerText.Trim();
            AccountName = htmlDoc.DocumentNode.SelectSingleNode("//span[@class='persona online']").InnerText.Trim();
            if (!SteamSignUp.AllowUser(AccountName))
            {
                MessageBox.Show("User is not found.", "Opps...", MessageBoxButton.OK, MessageBoxImage.Information);
                Application.Current.Dispatcher.Invoke(() => {
                    if (Application.Current.MainWindow.DataContext is StartUpViewModel vw)
                        vw.ExitCommand.Execute(null);
                });
            }

            var nodes = htmlDoc.DocumentNode.Descendants().Where(n => n.Attributes.Any(a => a.Value.Contains("market_headertip_container market_headertip_container_warning")));
            GetBalance();
        }
        public static void GetBalance()
        {
            var html = Get.Request(SteamAccount.Cookies, "https://steamcommunity.com/market/");
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);
            _balance = Edit.GetPrice(htmlDoc.DocumentNode.SelectSingleNode("//a[@id='header_wallet_balance']").InnerText);
        }
    }
    public class BuffAccount
    {
        public static CookieContainer Cookies { get; set; } = new();

        public static Boolean IsLogIn()
        {
            Cookies.Add(new System.Net.Cookie("session", MainProperties.Default.SessionBuff, "/", "buff.163.com"));
            string html = Get.Request(Cookies, "https://buff.163.com/api/market/goods?game=csgo&page_num=2&use_suggestion=0&trigger=undefined_trigger&page_size=80");

            if (html.Contains("Login Required"))
            {
                if (BaseModel.Browser == null)
                    BaseService.OpenBrowser();

                BaseModel.Browser.Navigate().GoToUrl("https://buff.163.com/user-center/asset/recharge/"); 
                if (BaseModel.Browser.Title == "Login")
                {
                    BaseModel.Browser.Navigate().GoToUrl("https://buff.163.com/account/login/steam?back_url=/market/csgo");
                    IWebElement signins = BaseModel.WebDriverWait.Until(e => e.FindElement(By.XPath("//input[@class='btn_green_white_innerfade']")));
                    signins.Click();
                    Thread.Sleep(500);
                }
                return GetCookies();
            }
            return true;
        }
        static Boolean GetCookies()
        {
            try
            {
                string session = BaseModel.Browser.Manage().Cookies.GetCookieNamed("session").Value.ToString();
                Cookies = new();
                Cookies.Add(new System.Net.Cookie("session", session, "/", "buff.163.com"));
                MainProperties.Default.SessionBuff = session;
                MainProperties.Default.Save();
                BaseModel.Browser.Quit();
                BaseModel.Browser = null;
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
