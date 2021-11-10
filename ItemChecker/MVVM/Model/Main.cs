using ItemChecker.Net;
using ItemChecker.Properties;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Timers;

namespace ItemChecker.MVVM.Model
{
    public class Main : BaseModel
    {
        public static bool IsLoading { get; set; }

        public static string Theme { get; set; } = "Light";
        public static string StatusCommunity { get; set; }

        public static List<string> Overstock = new();
        public static List<string> Unavailable = new();

        public static Timer Timer = new(1000);
        public static int TimerTick { get; set; }

        public static void StatusSteam()
        {
            try
            {
                if (String.IsNullOrEmpty(Account.ApiKey))
                    return;
                string json = Get.GameServersStatus(Account.ApiKey);
                StatusCommunity = JObject.Parse(json)["result"]["services"]["SteamCommunity"].ToString();
            }
            catch
            {
                StatusCommunity = "error";
            }
        }
    }
}