using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;

namespace ItemChecker.Support
{
    public class Edit
    {
        public static void OpenUrl(string url)
        {
            var psi = new ProcessStartInfo(url)
            {
                UseShellExecute = true,
                Verb = "open"
            };
            Process.Start(psi);
        }
        public static void OpenCsm(string itemName)
        {
            string market_hash_name = Uri.EscapeDataString(itemName);
            string stattrak = "false";
            string souvenir = "false";
            if (market_hash_name.Contains("StatTrak"))
                stattrak = "true";
            if (market_hash_name.Contains("Souvenir"))
                souvenir = "true";

            string url = "https://cs.money/csgo/trade/?search=" + market_hash_name + "&sort=price&order=asc&hasRareFloat=false&hasRareStickers=false&hasRarePattern=false&hasTradeLock=false&hasTradeLock=true&isStatTrak=" + stattrak + "&isSouvenir=" + souvenir;

            OpenUrl(url);
        }

        //remove
        public static Decimal GetDecimal(string str)
        {
            var mat = Regex.Match(str, @"(\d+(\.\d+)?)|(\.\d+)").Value;
            return Convert.ToDecimal(mat, CultureInfo.InvariantCulture);
        }
        public static Decimal SteamAvgPrice(string itemName, JObject items)
        {
            try
            {
                itemName = itemName.Replace("'", "&#39");
                var item = items[itemName] as JObject;
                if (item != null && item["price"] != null)
                {
                    if (item["price"]["24_hours"] != null)
                        return Convert.ToDecimal(item["price"]["24_hours"]["average"]);
                    else if (item["price"]["7_days"] != null)
                        return Convert.ToDecimal(item["price"]["7_days"]["average"]);
                    else if (item["price"]["30_days"] != null)
                        return Convert.ToDecimal(item["price"]["30_days"]["average"]);
                    else if (item["price"]["all_time"] != null)
                        return Convert.ToDecimal(item["price"]["all_time"]["average"]);
                }
                return 0;
            }
            catch
            {
                return 0;
            }
        }
        public static String RemoveDoppler(string itemName)
        {
            if (itemName.Contains("★") & itemName.Contains("Doppler"))
            {
                itemName = itemName.Replace(" Phase 1", string.Empty);
                itemName = itemName.Replace(" Phase 2", string.Empty);
                itemName = itemName.Replace(" Phase 3", string.Empty);
                itemName = itemName.Replace(" Phase 4", string.Empty);
                itemName = itemName.Replace(" Ruby", string.Empty);
                itemName = itemName.Replace(" Sapphire", string.Empty);
                itemName = itemName.Replace(" Black Pearl", string.Empty);
                itemName = itemName.Replace(" Emerald", string.Empty);
            }
            return itemName;
        }

        public static Decimal Precent(decimal a, decimal b) //from A to B
        {
            if (a != 0)
                return Math.Round((b - a) / a * 100, 2);
            else
                return 0;
        }
        public static Decimal Difference(decimal a, decimal b)
        {
            return Math.Round(a - b, 2);
        }

        //time
        public static DateTime ConvertFromUnixTimestamp(double timestamp)
        {
            DateTime origin = new(1970, 1, 1, 0, 0, 0, 0);
            return origin.AddSeconds(timestamp).ToLocalTime();
        }
        public static DateTime ConvertFromUnixTimestampJava(double timestamp)
        {
            DateTime origin = new(1970, 1, 1, 0, 0, 0, 0);
            return origin.AddMilliseconds(timestamp).ToLocalTime();
        }
        public static String calcTimeLeft(DateTime start, int count, int i)
        {
            double min = (count - ++i) / calcTimeLeftSpeed(start, i);
            TimeSpan time = TimeSpan.FromMinutes(min);
            if (min > 60)
                return time.ToString("hh'h 'mm'min'");
            else if (min > 1)
                return time.ToString("mm'min 'ss'sec.'");

            return time.ToString("ss'sec.'");
        }
        static Double calcTimeLeftSpeed(DateTime start, int i)
        {
            var time_passed = DateTime.Now.Subtract(start).TotalMinutes;
            return Math.Round(++i / time_passed, 2);
        }
    }
}
