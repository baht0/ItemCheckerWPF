using System;
using System.Diagnostics;
using System.Globalization;

namespace ItemChecker.Support
{
    public class Edit
    {
        //url
        public static String MarketHashName(string name)
        {
            name = name.Replace("★", "%E2%98%85");
            name = name.Replace("™", "%E2%84%A2");
            name = name.Replace(" ", "%20");
            name = name.Replace("|", "%7C");
            name = name.Replace("(", "%28");
            name = name.Replace(")", "%29");
            name = name.Replace("\r\n", "\n");

            return name;
        }
        public static void openUrl(string url)
        {
            var psi = new ProcessStartInfo(url)
            {
                UseShellExecute = true,
                Verb = "open"
            };
            Process.Start(psi);
        }
        public static void openCsm(string market_hash_name)
        {
            string url;
            string stattrak = "false";
            string souvenir = "false";
            if (market_hash_name.Contains("StatTrak"))
                stattrak = "true";
            if (market_hash_name.Contains("Souvenir"))
                souvenir = "true";

            url = "https://cs.money/csgo/trade/?search=" + market_hash_name + "&sort=price&order=asc&hasRareFloat=false&hasRareStickers=false&hasRarePattern=false&hasTradeLock=true&isStatTrak=" + stattrak + "&isMarket=false&isSouvenir=" + souvenir;

            openUrl(url);
        }

        //remove
        public static Decimal removeRub(string str)
        {
            str = str.Replace(",", ".");
            decimal rub = Convert.ToDecimal(str[0..^5], CultureInfo.InvariantCulture);
            return rub;
        }
        public static Decimal removeDol(string str)
        {
            str = str.Replace("$", null);
            decimal dol = Convert.ToDecimal(str.Replace(" USD", ""), CultureInfo.InvariantCulture);
            return dol;
        }
        public static Decimal removeSymbol(string str)
        {
            str = str.Replace(",", ".");
            return Convert.ToDecimal(str.Substring(0, str.Length - 1), CultureInfo.InvariantCulture);
        }
        public static String buyOrderId(string idOrder)
        {
            return idOrder.Replace("mybuyorder_", "");
        }
        public static Int32 ItemNameId(string html)
        {
            html = html.Substring(html.IndexOf("Market_LoadOrderSpread"));
            var a = html.IndexOf("(");
            var b = html.IndexOf(")");
            string str = html.Substring(a, b);

            int id = Convert.ToInt32(System.Text.RegularExpressions.Regex.Replace(str, @"[^\d]+", ""));
            return id;
        }
        public static String removeDoppler(string itemName)
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

        public static String replaceSymbols(string str)
        {
            str = str.Replace("â„¢", "™");
            str = str.Replace("â˜…", "★");
            return str;
        }
        public static Decimal ConverterToRub(decimal value, decimal currency)
        {
            return Math.Round(value * currency, 2);
        }
        public static Decimal ConverterToUsd(decimal value, decimal currency)
        {
            return Math.Round(value / currency, 2);
        }
        public static Decimal Precent(decimal a, decimal b) //from A to B
        {
            if (a != 0)
                return Math.Round(((b - a) / a) * 100, 2);
            else
                return 0;
        }
        public static Decimal Difference(decimal a, decimal b)
        {
            return Math.Round((a - b), 2);
        }
        public static Decimal CommissionSteam(decimal price)
        {
            return Math.Round(price - price * 0.130435m, 2);
        }

        //time
        public static DateTime ConvertFromUnixTimestamp(double timestamp)
        {
            DateTime origin = new(1970, 1, 1, 0, 0, 0, 0);
            return origin.AddMilliseconds(timestamp).ToLocalTime();
        }
        public static DateTime UnixTimeToDateTime(double unixtime)
        {
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddMilliseconds(unixtime).ToLocalTime();
            return dtDateTime;
        }
        public static String calcTimeLeft(DateTime start, int count, int i)
        {
            double min = (count - ++i) / calcTimeLeftSpeed(start, i);
            TimeSpan time = TimeSpan.FromMinutes(min);
            if (min > 60)
                return time.ToString("hh'h 'mm'min'");
            else if (min > 1)
                return time.ToString("mm'min 'ss'sec.'");
            else
                return time.ToString("ss'sec.'");
        }
        public static Double calcTimeLeftSpeed(DateTime start, int i)
        {
            DateTime now = DateTime.Now;
            var time_passed = now.Subtract(start).TotalMinutes;
            return Math.Round(++i / time_passed, 2);
        }
    }
}
