using ItemChecker.Net;
using ItemChecker.Properties;
using ItemChecker.Support;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ItemChecker.MVVM.Model
{
    public class Main : Start
    {
        public static bool IsLoading { get; set; }
        public static string StatusCommunity { get; set; }
        public static string StatusSessions { get; set; }        

        public static List<string> Overstock = new();
        public static List<string> Unavailable = new();

        public static void StatusSteam()
        {
            if (String.IsNullOrEmpty(SteamProperties.Default.steamApiKey))
                return;
            string json = Get.GameServersStatus(SteamProperties.Default.steamApiKey);
            StatusCommunity = JObject.Parse(json)["result"]["services"]["SteamCommunity"].ToString();
            StatusSessions = JObject.Parse(json)["result"]["services"]["SessionsLogon"].ToString();
        }
    }
}
