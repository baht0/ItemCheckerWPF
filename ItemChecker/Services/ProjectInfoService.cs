using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using ItemChecker.Net;
using System.Diagnostics;
using System.Windows;
using System.Xml.Linq;
using System.IO;
using OpenQA.Selenium.Chrome;

namespace ItemChecker.MVVM.Model
{
    public class ProjectInfoService : Main
    {
        static void CheckVersion()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            assembly = Assembly.LoadFrom(BaseModel.AppPath + "ItemChecker.Net.dll");
            string ItemCheckerNet = assembly.GetName().Version.ToString();

            assembly = Assembly.LoadFrom(BaseModel.AppPath + "ItemChecker.Support.dll");
            string ItemCheckerSupport = assembly.GetName().Version.ToString();

            assembly = Assembly.LoadFrom(BaseModel.AppPath + "Newtonsoft.Json.dll");
            string NewtonsoftJson = assembly.GetName().Version.ToString();

            assembly = Assembly.LoadFrom(BaseModel.AppPath + "WebDriver.dll");
            string WebDriver = assembly.GetName().Version.ToString();

            assembly = Assembly.LoadFrom(BaseModel.AppPath + "WebDriver.Support.dll");
            string WebDriverSupport = assembly.GetName().Version.ToString();

            assembly = Assembly.LoadFrom(BaseModel.AppPath + "MaterialDesignColors.dll");
            string MaterialDesignColors = assembly.GetName().Version.ToString();

            assembly = Assembly.LoadFrom(BaseModel.AppPath + "MaterialDesignThemes.Wpf.dll");
            string MaterialDesignThemesWpf = assembly.GetName().Version.ToString();

            ICapabilities capabilities = ((ChromeDriver)Browser).Capabilities;
            var chromedriver = (capabilities.GetCapability("chrome") as Dictionary<string, object>)["chromedriverVersion"].ToString().Split(' ');
            string DriverVersion = chromedriver[0];

            ProjectInfo.CurrentVersion = new ProjectInfo(BaseModel.Version, ItemCheckerNet, ItemCheckerSupport, NewtonsoftJson, WebDriver, WebDriverSupport, MaterialDesignColors, MaterialDesignThemesWpf, DriverVersion);
        }
        public static void CheckUpdate()
        {
            try
            {
                CheckVersion();
                XmlDocument xDoc = new();
                xDoc.LoadXml(Post.DropboxRead("info.xml"));
                XmlElement xRoot = xDoc.DocumentElement;

                string Version = null;
                string ItemCheckerNet = null;
                string ItemCheckerSupport = null;
                string NewtonsoftJson = null;
                string WebDriver = null;
                string WebDriverSupport = null;
                string MaterialDesignColors = null;
                string MaterialDesignThemesWpf = null;
                string ChromeDriver = null;

                foreach (XmlNode xnode in xRoot)
                {
                    if (xnode.Name == "PropertyGroup")
                        foreach (XmlNode childnode in xnode.ChildNodes)
                        {
                            if (childnode.Name == "Version")
                                Version = childnode.InnerText;
                            else if (childnode.Name == "Net.Version")
                                ItemCheckerNet = childnode.InnerText;
                            else if (childnode.Name == "Support.Version")
                                ItemCheckerSupport = childnode.InnerText;
                        }
                    if (xnode.Name == "ItemGroup")
                        foreach (XmlNode childnode in xnode.ChildNodes)
                        {
                            XmlNode Include = childnode.Attributes.GetNamedItem("Include");
                            if (Include.Value == "Newtonsoft.Json")
                                NewtonsoftJson = childnode.Attributes.GetNamedItem("Version").Value;
                            else if (Include.Value == "Selenium.WebDriver")
                                WebDriver = childnode.Attributes.GetNamedItem("Version").Value;
                            else if (Include.Value == "Selenium.Support")
                                WebDriverSupport = childnode.Attributes.GetNamedItem("Version").Value;
                            else if (Include.Value == "MaterialDesign.Colors")
                                MaterialDesignColors = childnode.Attributes.GetNamedItem("Version").Value;
                            else if (Include.Value == "MaterialDesign.Themes.Wpf")
                                MaterialDesignThemesWpf = childnode.Attributes.GetNamedItem("Version").Value;
                            else if (Include.Value == "ChromeDriver")
                                ChromeDriver = childnode.Attributes.GetNamedItem("Version").Value;
                        }
                }
                ProjectInfo.LatestVersion = new ProjectInfo(Version, ItemCheckerNet, ItemCheckerSupport, NewtonsoftJson, WebDriver, WebDriverSupport, MaterialDesignColors, MaterialDesignThemesWpf, ChromeDriver);

                if (ProjectInfo.LatestVersion.Version != ProjectInfo.CurrentVersion.Version)
                    ProjectInfo.IsUpdate = true;
            }
            catch (Exception exp)
            {
                errorLog(exp);
                errorMessage(exp);
                ProjectInfo.IsUpdate = false;
            }
        }

        public static void Update()
        {
            MessageBoxResult result = MessageBox.Show($"Want to upgrade from {ProjectInfo.CurrentVersion.Version} to {ProjectInfo.LatestVersion.Version}?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.No)
                return;
            
            string args = null;
            List<bool> updates = new();
            updates.Add(ProjectInfo.LatestVersion.ItemCheckerNet != ProjectInfo.CurrentVersion.ItemCheckerNet);
            updates.Add(ProjectInfo.LatestVersion.ItemCheckerSupport != ProjectInfo.CurrentVersion.ItemCheckerSupport);
            updates.Add(ProjectInfo.LatestVersion.NewtonsoftJson != ProjectInfo.CurrentVersion.NewtonsoftJson);
            updates.Add(ProjectInfo.LatestVersion.WebDriver != ProjectInfo.CurrentVersion.WebDriver);
            updates.Add(ProjectInfo.LatestVersion.WebDriverSupport != ProjectInfo.CurrentVersion.WebDriverSupport);
            updates.Add(ProjectInfo.LatestVersion.MaterialDesignColors != ProjectInfo.CurrentVersion.MaterialDesignColors);
            updates.Add(ProjectInfo.LatestVersion.MaterialDesignThemesWpf != ProjectInfo.CurrentVersion.MaterialDesignThemesWpf);
            updates.Add(ProjectInfo.LatestVersion.ChromeDriver != ProjectInfo.CurrentVersion.ChromeDriver);

            foreach (bool update in updates)
                args += $"{update} ";

            ProcessStartInfo updater = new();
            updater.FileName = AppPath + "ItemChecker.Updater.exe";
            updater.Arguments = args;
            Process.Start(updater);

            BrowserExit();
            Application.Current.Shutdown();
        }

        public static void CreateCurrentVersion()
        {
            if (!File.Exists("info.xml"))
                File.Delete(BaseModel.AppPath + "info.xml");

            new XDocument(
                new XElement("Project",
                    new XElement("PropertyGroup",
                        new XElement("Version", ProjectInfo.CurrentVersion.Version),
                        new XElement("Net.Version", ProjectInfo.CurrentVersion.ItemCheckerNet),
                        new XElement("Support.Version", ProjectInfo.CurrentVersion.ItemCheckerSupport)
                    ),
                    new XElement("ItemGroup",
                        new XElement("PackageReference", new XAttribute("Include", "Newtonsoft.Json"), new XAttribute("Version", ProjectInfo.CurrentVersion.NewtonsoftJson)),
                        new XElement("PackageReference", new XAttribute("Include", "Selenium.WebDriver"), new XAttribute("Version", ProjectInfo.CurrentVersion.WebDriver)),
                        new XElement("PackageReference", new XAttribute("Include", "Selenium.Support"), new XAttribute("Version", ProjectInfo.CurrentVersion.WebDriverSupport)),
                        new XElement("PackageReference", new XAttribute("Include", "MaterialDesign.Colors"), new XAttribute("Version", ProjectInfo.CurrentVersion.MaterialDesignColors)),
                        new XElement("PackageReference", new XAttribute("Include", "MaterialDesign.Themes.Wpf"), new XAttribute("Version", ProjectInfo.CurrentVersion.MaterialDesignThemesWpf)),
                        new XElement("PackageReference", new XAttribute("Include", "ChromeDriver"), new XAttribute("Version", ProjectInfo.CurrentVersion.ChromeDriver))
                    )
                )
            ).Save("info.xml");
        }
    }
}
