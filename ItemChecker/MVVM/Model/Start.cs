using ItemChecker.Properties;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows;

namespace ItemChecker.MVVM.Model
{
    public class Start
    {
        public static string Version { get; set; } = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public static IWebDriver Browser { get; set; }
        public static WebDriverWait webDriverWait { get; set; }
        public static string SessionId { get; set; }

        public static CancellationTokenSource cts = new();
        public static CancellationToken token = cts.Token;

        public static string AppPath = AppDomain.CurrentDomain.BaseDirectory;

        public static void BrowserExit()
        {
            try
            {
                Browser.Quit();
            }
            catch
            {
                if (GeneralProperties.Default.exitChrome)
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
        }

        public static void errorLog(Exception exp, string ver)
        {
            string message = null;
            message += exp.Message + "\n";
            message += exp.StackTrace;
            if (!File.Exists("errorsLog.txt"))
                File.WriteAllText("errorsLog.txt", "v." + ver + " [" + DateTime.Now + "]\n" + message + "\n");
            else
                File.WriteAllText("errorsLog.txt", string.Format("{0}{1}", "v." + ver + " [" + DateTime.Now + "]\n" + message + "\n", File.ReadAllText("errorsLog.txt")));
        }
        public static void errorMessage(Exception exp)
        {
            MessageBox.Show("Something went wrong :(", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
