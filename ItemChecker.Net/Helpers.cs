using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ItemChecker.Net
{
    public class Helpers
    {
        internal static string UserAgent
        {
            get
            {
                return "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/108.0.0.0 Safari/537.36";
            }
        }
        internal static string DocumentPath
        {
            get
            {
                string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\ItemChecker\\net\\";
                var info = new DirectoryInfo(path);
                if (!info.Exists)
                    info.Create();
                info.Attributes = FileAttributes.Hidden;
                return path;
            }
        }

        internal static Cookie CreateSessionId()
        {
            var rnd = new Random();
            Byte[] bytes = new Byte[12];
            rnd.NextBytes(bytes);
            string sessionId = Convert.ToHexString(bytes).ToLower();

            var cookie = new Cookie("sessionid", sessionId, "/", "steamcommunity.com")
            {
                Secure = true,
                HttpOnly = false,
            };

            return cookie;
        }
        internal static CookieContainer GetCookieContainer(string cookieHost, HttpResponseMessage response)
        {
            Uri uri = new(cookieHost);
            var initialContainer = new CookieContainer();
            foreach (var header in response.Headers.GetValues("Set-Cookie"))
            {
                var cookie = header.Replace("CET", "GMT");
                initialContainer.SetCookies(uri, cookie);
            }
            var collection = initialContainer.GetAllCookies();
            foreach (Cookie cookie in collection.Cast<Cookie>())
            {
                cookie.Secure = true;
                cookie.Expires = cookie.Expires != new DateTime() ? cookie.Expires.AddYears(1) : DateTime.Now.AddYears(1);
            }
            var cookies = new CookieContainer();
            cookies.Add(collection);
            return cookies;
        }
        internal static async Task<string> ToResponseStringAsync(HttpResponseMessage response)
        {
            return await response.Content.ReadAsStringAsync();
        }

        static string GetHexString(string str)
        {
            byte[] bytes = Encoding.Default.GetBytes(str);
            return Convert.ToHexString(bytes).ToLower();
        }
        internal static async Task SerializeCookieAsync(CookieContainer cookies, string fileName)
        {
            string filePath = $"{DocumentPath}{GetHexString(fileName)}.json";

            string json = JsonSerializer.Serialize(cookies.GetAllCookies());
            await File.WriteAllTextAsync(filePath, json);
        }
        internal static async Task<CookieContainer> DeserializeCookieAsync(string fileName)
        {
            string filePath = $"{DocumentPath}{GetHexString(fileName)}.json";
            if (!File.Exists(filePath))
                return null;

            var cookieContainer = new CookieContainer();
            var json =  await File.ReadAllTextAsync(filePath);
            var cookieCollection = JsonSerializer.Deserialize<CookieCollection>(json);
            cookieContainer.Add(cookieCollection);

            return cookieContainer;
        }

        internal static decimal GetDecimal(string str)
        {
            var mat = Regex.Match(str, @"(\d+(\.\d+)?)|(\.\d+)").Value;
            return Convert.ToDecimal(mat, CultureInfo.InvariantCulture);
        }
    }
}