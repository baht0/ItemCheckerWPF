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
using System.Linq;

namespace ItemChecker.MVVM.Model
{
    public class ProjectInfo : Main
    {
        public static bool IsUpdate { get; set; }
        private List<string> info = new();
        private List<string> latest = new();
        private List<bool> updates = new();

        public void CheckUpdate()
        {
            try
            {
                getCurrentVersion();
                XmlDocument xDoc = new();
                xDoc.LoadXml(Post.DropboxRead("info.xml"));
                XmlElement xRoot = xDoc.DocumentElement;

                foreach (XmlNode xnode in xRoot)
                {
                    if (xnode.Name == "PropertyGroup")
                        foreach (XmlNode childnode in xnode.ChildNodes)
                        {
                            if (childnode.Name == "Version")
                                latest.Add(childnode.InnerText);
                            else if (childnode.Name == "Net.Version")
                                latest.Add(childnode.InnerText);
                            else if (childnode.Name == "Support.Version")
                                latest.Add(childnode.InnerText);
                        }
                    if (xnode.Name == "ItemGroup")
                        foreach (XmlNode childnode in xnode.ChildNodes)
                        {
                            XmlNode Include = childnode.Attributes.GetNamedItem("Include");
                            if (Include.Value == "Newtonsoft.Json")
                                latest.Add(childnode.Attributes.GetNamedItem("Version").Value);
                            else if (Include.Value == "Selenium.WebDriver")
                                latest.Add(childnode.Attributes.GetNamedItem("Version").Value);
                            else if (Include.Value == "Selenium.Support")
                                latest.Add(childnode.Attributes.GetNamedItem("Version").Value);
                            else if (Include.Value == "Selenium.WebDriver.ChromeDriver")
                                latest.Add(childnode.Attributes.GetNamedItem("Version").Value);
                        }
                }
                if (latest[0] != info[0])
                {
                    updates.Add(true);
                    for (int i = 1; i < 7; i++)
                    {
                        if (latest[i] != info[i])
                            updates.Add(true);
                        else
                            updates.Add(false);
                    }
                }
                if (updates.Any())
                    IsUpdate = true;
                else
                    IsUpdate = false;
            }
            catch (Exception exp)
            {
                errorLog(exp, Start.Version);
                errorMessage(exp);
                IsUpdate = false;
            }
        }
        private void getCurrentVersion()
        {
            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();

                info.Add(assembly.GetName().Version.ToString());

                assembly = Assembly.LoadFrom(Start.AppPath + "ItemChecker.Net.dll");
                info.Add(assembly.GetName().Version.ToString());

                assembly = Assembly.LoadFrom(Start.AppPath + "ItemChecker.Support.dll");
                info.Add(assembly.GetName().Version.ToString());

                assembly = Assembly.LoadFrom(Start.AppPath + "Newtonsoft.Json.dll");
                info.Add(assembly.GetName().Version.ToString());

                assembly = Assembly.LoadFrom(Start.AppPath + "WebDriver.dll");
                info.Add(assembly.GetName().Version.ToString());

                assembly = Assembly.LoadFrom(Start.AppPath + "WebDriver.Support.dll");
                info.Add(assembly.GetName().Version.ToString());

                ICapabilities capabilities = ((RemoteWebDriver)Main.Browser).Capabilities;
                var chromedriver = (capabilities.GetCapability("chrome") as Dictionary<string, object>)["chromedriverVersion"].ToString().Split(' ');
                info.Add(chromedriver[0]);
            }
            catch (Exception exp)
            {
                errorLog(exp, Start.Version);
                errorMessage(exp);
            }
        }

        public void update()
        {
            string args = null;
            foreach (bool update in updates)
                args += $"{update} ";

            ProcessStartInfo updater = new();
            updater.FileName = AppPath + "ItemChecker.Updater.exe";
            updater.Arguments = args;
            Process.Start(updater);

            BrowserExit();
            Application.Current.Shutdown();
        }

        public void createCurrentVersion()
        {
            if (!File.Exists("info.xml"))
                File.Delete(Start.AppPath + "info.xml");

            new XDocument(
                new XElement("Project",
                    new XElement("PropertyGroup",
                        new XElement("Version", info[0]),
                        new XElement("Net.Version", info[1]),
                        new XElement("Support.Version", info[2])
                    ),
                    new XElement("ItemGroup",
                        new XElement("PackageReference", new XAttribute("Include", "Newtonsoft.Json"), new XAttribute("Version", info[3])),
                        new XElement("PackageReference", new XAttribute("Include", "Selenium.WebDriver"), new XAttribute("Version", info[4])),
                        new XElement("PackageReference", new XAttribute("Include", "Selenium.Support"), new XAttribute("Version", info[5])),
                        new XElement("PackageReference", new XAttribute("Include", "Selenium.WebDriver.ChromeDriver"), new XAttribute("Version", info[6]))
                    )
                )
            ).Save("info.xml");
        }
    }
}
