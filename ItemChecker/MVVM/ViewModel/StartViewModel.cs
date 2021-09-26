using ItemChecker.Core;
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
using ItemChecker.MVVM.View;
using System.Diagnostics;
using System.Linq;
using ItemChecker.Properties;

namespace ItemChecker.MVVM.ViewModel
{
    public class StartViewModel : ObservableObject
    {
        IView _view;
        Task startTask;
        string _version;
        string _status;
        bool _isLogin;
        private Account _account;

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
        public Account Account
        {
            get { return _account; }
            set
            {
                _account = value;
                OnPropertyChanged();
            }
        }

        public StartViewModel(IView view)
        {
            _view = view;
            Version = Start.Version;
            Account = Account.GetUser();
            startTask = Task.Run(() => { Starting(); });
        }
        public ICommand CloseCommand
        {
            get
            {
                return new RelayCommand((obj) =>
                {
                    MessageBoxResult result = MessageBox.Show("Are you sure you want to close?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        Task.Run(() => {
                            Start.cts.Cancel();
                            Status = "Exit...";
                            startTask.Wait();
                            Start.BrowserExit();
                        }).Wait();
                        Application.Current.Shutdown();
                    }
                });
            }
        }
        private void Hide()
        {
            _view.Hide();
        }
        void Starting()
        {
            try
            {
                KillProccess();
                CompletionUpdate();
                LaunchBrowser();
                CheckUpdate();

                LoginSteam();
                LoginTryskins();
                LoginCsm();
                Status = "Get Informations...";
                Account.GetInformations();
                Status = "Check Steam Orders...";
                BuyOrder order = new();
                order.SteamOrders();
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Hide();
                    MainWindow mainWindow = new();
                    mainWindow.Show();
                });
            }
            catch (Exception exp)
            {
                Start.errorLog(exp, Start.Version);
                Start.errorMessage(exp);
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
            //if (GeneralProperties.Default.exitChrome)
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
            if (Start.token.IsCancellationRequested)
                return;
            if (StartProperties.Default.completionUpdate)
            {
                Status = "Completion Update...";
                string update = Start.AppPath + @"\update";
                if (Directory.Exists(update))
                {
                    string path = Start.AppPath;
                    string updaterExe = "ItemChecker.Updater.exe";
                    string updaterDll = "ItemChecker.Updater.dll";
                    File.Move($"{update}\\{updaterExe}", path + updaterExe, true);
                    File.Move($"{update}\\{updaterDll}", path + updaterDll, true);
                    Directory.Delete(update, true);
                }
                GeneralProperties.Default.Upgrade();
                SteamProperties.Default.Upgrade();
                ParserProperties.Default.Upgrade();
                FloatProperties.Default.Upgrade();

                StartProperties.Default.completionUpdate = false;
                StartProperties.Default.Save();
            }
        }
        void LaunchBrowser()
        {
            if (Start.token.IsCancellationRequested)
                return;
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
            if (GeneralProperties.Default.profileUse)
                option.AddArguments($"--user-data-dir={Start.AppPath}\\profiles\\{GeneralProperties.Default.profile}", "profile-directory=Default");
            else
                Directory.Delete($"{Start.AppPath}\\profile", true);
            option.Proxy = null;

            Start.Browser = new ChromeDriver(chromeDriverService, option, TimeSpan.FromSeconds(30));
            Start.Browser.Manage().Window.Maximize();
            Start.webDriverWait = new WebDriverWait(Start.Browser, TimeSpan.FromSeconds(GeneralProperties.Default.wait));
        }
        void CheckUpdate()
        {
            if (Start.token.IsCancellationRequested)
                return;
            Status = "Check Update...";
            ProjectInfo project = new();
            project.CheckUpdate();
        }

        void LoginSteam()
        {
            if (Start.token.IsCancellationRequested)
                return;
            Status = "Login Steam...";
            Start.Browser.Navigate().GoToUrl("https://steamcommunity.com/login/home/?goto=");
            string cookie = Start.Browser.Manage().Cookies.GetCookieNamed("sessionid").ToString();
            Start.SessionId = cookie.Substring(10, 24);

            if (Start.Browser.Url.Contains("id") | Start.Browser.Url.Contains("profiles"))
            {
                IWebElement account = Start.webDriverWait.Until(ExpectedConditions.ElementExists(By.XPath("//span[@class='persona online']")));
                Account.AccountName = account.GetAttribute("textContent");
                return;
            }

            IWebElement username = Start.Browser.FindElement(By.XPath("//input[@name='username']"));
            IWebElement password = Start.Browser.FindElement(By.XPath("//input[@name='password']"));
            IWebElement signin = Start.webDriverWait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//button[@class='btn_blue_steamui btn_medium login_btn']")));

            IsLogin = true;

            while (IsLogin)
                Task.Delay(500);
            username.SendKeys(Account.Login);
            password.SendKeys(Account.Password);
            signin.Click();

            IWebElement code = Start.webDriverWait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//input[@id='twofactorcode_entry']")));
            code.SendKeys(Account.Code2AF);
            code.SendKeys(OpenQA.Selenium.Keys.Enter);

            Thread.Sleep(4000);
            LoginSteam();
        }
        public ICommand LoginCommand
        {
            get
            {
                return new RelayCommand((obj) =>
                {
                    var propertyInfo = obj.GetType().GetProperty("Password");
                    Account.Password = (string)propertyInfo.GetValue(obj, null);
                    if (Account.Password != "")
                        IsLogin = false;
                }, (obj) => IsLogin & !String.IsNullOrEmpty(Account.Login) & Account.Code2AF.Length == 5);
            }
        }
        void LoginTryskins()
        {
            if (Start.token.IsCancellationRequested)
                return;
            Status = "Login TrySkins...";

            Start.Browser.Navigate().GoToUrl("https://table.altskins.com/site/items");
            if (Start.Browser.Url.Contains("items"))
                return;

            Start.Browser.Navigate().GoToUrl("https://table.altskins.com/login/steam");

            IWebElement account = Start.webDriverWait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//div[@class='OpenID_loggedInAccount']")));
            IWebElement signins = Start.webDriverWait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//input[@class='btn_green_white_innerfade']")));
            signins.Click();
            Thread.Sleep(300);

            LoginTryskins();
        }
        void LoginCsm()
        {
            if (Start.token.IsCancellationRequested)
                return;
            Status = "Login Cs.Money...";

            Start.Browser.Navigate().GoToUrl("https://cs.money/pending-trades");
            IWebElement html = Start.webDriverWait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//pre")));
            string json = html.Text;

            //string json = MainPresenter.JsonSelenium("https://cs.money/pending-trades");
            if (json.Contains("error"))
            {
                var code_error = JObject.Parse(json)["error"].ToString();
                if (code_error == "6")
                {
                    Main.Browser.Navigate().GoToUrl("https://auth.dota.trade/login?redirectUrl=https://cs.money/&callbackUrl=https://cs.money/login");

                    IWebElement signins = Start.webDriverWait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//input[@class='btn_green_white_innerfade']")));
                    signins.Click();
                    Thread.Sleep(500);
                }
                LoginCsm();
            }
        }
    }
}
