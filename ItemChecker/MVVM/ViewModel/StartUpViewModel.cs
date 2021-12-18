﻿using ItemChecker.Core;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Newtonsoft.Json.Linq;
using ItemChecker.MVVM.Model;
using System.Windows;
using System.Diagnostics;
using System.Linq;
using ItemChecker.Properties;
using System.Globalization;

namespace ItemChecker.MVVM.ViewModel
{
    public class StartUpViewModel : ObservableObject
    {
        IView _view;
        Task startTask;
        string _version;
        string _status;
        bool _isLogin;
        private Account _login;

        public bool LoginSuccessful { get; set; }
        public string Version
        {
            get { return _version; }
            set
            {
                _version = value;
                OnPropertyChanged();
            }
        }
        public string Status
        {
            get { return _status; }
            set
            {
                _status = value;
                OnPropertyChanged();
            }
        }
        public bool IsLogin
        {
            get { return _isLogin; }
            set
            {
                _isLogin = value;
                OnPropertyChanged();
            }
        }
        public Account Login
        {
            get { return _login; }
            set
            {
                _login = value;
                OnPropertyChanged();
            }
        }

        public StartUpViewModel(IView view)
        {
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.CreateSpecificCulture("en-Us");
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.CreateSpecificCulture("en-Us");
            Login = new Account()
            {
                Login = string.Empty,
                Remember = false,
                Code2AF = string.Empty,
            };
            _view = view;
            Version = BaseModel.Version;
            BaseModel.IsLoading = true;
            startTask = Task.Run(() => { Starting(); });
        }
        public ICommand ExitCommand =>
            new RelayCommand((obj) =>
            {
                Task.Run(() =>
                {
                    BaseModel.cts.Cancel();
                    Status = "Exit...";
                    startTask.Wait(5000);
                    BaseModel.BrowserExit();
                }).Wait();
                Application.Current.Shutdown();
            });
        void Hide()
        {
            _view.Close();
        }
        void Starting()
        {
            try
            {
                KillProccess();
                CompletionUpdate();
                LaunchBrowser();
                CheckUpdate();
                Status = "Get Base...";
                Account.GetBase();

                SteamAccount();
                LoginCsm();
                Status = "Status Steam...";
                BaseModel.StatusSteam();
                Status = "Check Steam Orders...";
                OrderCheckService order = new();
                order.SteamOrders();

                if (BaseModel.token.IsCancellationRequested)
                    return;
                BaseModel.IsLoading = false;
                LoginSuccessful = true;
                Application.Current.Dispatcher.Invoke(() => { Hide(); });
            }
            catch (Exception exp)
            {
                BaseModel.errorLog(exp);
                BaseModel.errorMessage(exp);
            }
        }
        void KillProccess()
        {
            if (Process.GetProcessesByName(Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location)).Count() > 1)
            {
                MessageBox.Show(
                    "The program is already running!",
                    "Warning",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Process.GetCurrentProcess().Kill();
            }
            if (GeneralProperties.Default.ExitChrome)
                foreach (Process proc in Process.GetProcessesByName("chrome")) proc.Kill();
            foreach (Process proc in Process.GetProcessesByName("chromedriver")) proc.Kill();
            foreach (Process proc in Process.GetProcessesByName("conhost"))
            {
                try
                {
                    proc.Kill();
                }
                catch
                {
                    continue;
                }
            }
        }
        void CompletionUpdate()
        {
            if (BaseModel.token.IsCancellationRequested)
                return;
            if (StartUpProperties.Default.completionUpdate)
            {
                Status = "Completion Update...";
                string update = BaseModel.AppPath + @"\update";
                if (Directory.Exists(update))
                {
                    string path = BaseModel.AppPath;
                    string updaterExe = "ItemChecker.Updater.exe";
                    string updaterDll = "ItemChecker.Updater.dll";
                    File.Move($"{update}\\{updaterExe}", path + updaterExe, true);
                    File.Move($"{update}\\{updaterDll}", path + updaterDll, true);
                    Directory.Delete(update, true);
                }
                GeneralProperties.Default.Upgrade();
                HomeProperties.Default.Upgrade();
                ParserProperties.Default.Upgrade();
                FloatProperties.Default.Upgrade();

                StartUpProperties.Default.completionUpdate = false;
                StartUpProperties.Default.Save();
            }
        }
        void LaunchBrowser()
        {
            if (BaseModel.token.IsCancellationRequested)
                return;

            string profilesDir = BaseModel.AppPath + "Profiles";

            if (!Directory.Exists(profilesDir))
                Directory.CreateDirectory(profilesDir);

            DirectoryInfo dirInfo = new(profilesDir);
            dirInfo.Attributes = FileAttributes.Hidden;

            Status = "Launch Browser...";
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

            option.AddArguments($"--user-data-dir={profilesDir}\\{GeneralProperties.Default.Profile}", "profile-directory=Default");
            option.Proxy = null;


            BaseModel.Browser = new ChromeDriver(chromeDriverService, option, TimeSpan.FromSeconds(30));
            BaseModel.Browser.Manage().Window.Maximize();
            BaseModel.WebDriverWait = new WebDriverWait(BaseModel.Browser, TimeSpan.FromSeconds(10));
        }
        void CheckUpdate()
        {
            if (BaseModel.token.IsCancellationRequested)
                return;
            Status = "Check Update...";
            ProjectInfoService.CheckUpdate();
        }

        void SteamAccount()
        {
            if (BaseModel.token.IsCancellationRequested)
                return;
            Status = "Steam Account...";

            BaseModel.Browser.Navigate().GoToUrl("https://steamcommunity.com/login/");

            if (BaseModel.Browser.Url.Contains("id") | BaseModel.Browser.Url.Contains("profiles"))
            {
                Account.GetSteamAccount();
                Account.GetSteamApiKey();
                return;
            }

            IWebElement username = BaseModel.Browser.FindElement(By.XPath("//input[@name='username']"));
            IWebElement password = BaseModel.Browser.FindElement(By.XPath("//input[@name='password']"));
            try
            {
                IWebElement remember = BaseModel.WebDriverWait.Until(e => e.FindElement(By.XPath("//input[@name='remember_login']")));
                remember.Click();
            } catch { }
            IWebElement signin = BaseModel.WebDriverWait.Until(e => e.FindElement(By.XPath("//button[@class='btn_blue_steamui btn_medium login_btn']")));

            IsLogin = true;
            while (IsLogin)
                Thread.Sleep(500);
            username.SendKeys(Login.Login);
            password.SendKeys(Login.Password);
            signin.Click();

            Thread.Sleep(2000);
            IWebElement code = BaseModel.WebDriverWait.Until(e => e.FindElement(By.XPath("//input[@id='twofactorcode_entry']")));
            code.SendKeys(Login.Code2AF);
            code.SendKeys(OpenQA.Selenium.Keys.Enter);

            Thread.Sleep(4000);
            SteamAccount();
        }
        public ICommand LoginCommand =>
            new RelayCommand((obj) =>
            {
                var propertyInfo = obj.GetType().GetProperty("Password");
                Login.Password = (string)propertyInfo.GetValue(obj, null);
                if (Login.Password != "")
                    IsLogin = false;

            }, (obj) => IsLogin & !String.IsNullOrEmpty(Login.Login) & Login.Code2AF.Length == 5);
        void LoginCsm()
        {
            if (BaseModel.token.IsCancellationRequested | GeneralProperties.Default.Guard)
                return;
            Status = "Cs.Money...";

            BaseModel.Browser.Navigate().GoToUrl("https://cs.money/pending-trades");
            IWebElement html = BaseModel.WebDriverWait.Until(e => e.FindElement(By.XPath("//pre")));
            string json = html.Text;

            if (json.Contains("error"))
            {
                string code_error = JObject.Parse(json)["error"].ToString();
                if (code_error == "6")
                {
                    BaseModel.Browser.Navigate().GoToUrl("https://auth.dota.trade/login?redirectUrl=https://cs.money/&callbackUrl=https://cs.money/login");

                    IWebElement signins = BaseModel.WebDriverWait.Until(e => e.FindElement(By.XPath("//input[@class='btn_green_white_innerfade']")));
                    signins.Click();
                    Thread.Sleep(500);
                }
                LoginCsm();
            }
            Account.GetCsmBalance();
        }
    }
}