using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace ItemChecker.Net
{
    public class Post
    {
        public static String FetchRequest(string contentType, string body, string url)
        {
            string js_fetch = @"
                async function postReq(url) {
                    const response = await fetch(url, {
                        method: 'POST',
                        headers: { 'Content-Type': '" + contentType + @"; charset=UTF-8' },
                        body: '" + body + @"',
                        credentials: 'include' });
                return response.json(); }
                postReq('" + url + @"').then(data => console.log(data));";

            return js_fetch;
        }
        public static String FetchRequestWithResponse(string contentType, string body, string url)
        {
            string js_fetch = @"
                async function postReq(url) {
                    const response = await fetch(url, {
                        method: 'POST',
                        headers: { 'Content-Type': '" + contentType + @"; charset=UTF-8' },
                        body: '" + body + @"',
                        credentials: 'include' });
                return response.json(); }
                postReq('" + url + @"').then(data => {
                        var json = JSON.stringify(data);
                        document.open();
                        document.write('<html><body><pre>' + json + '</pre></body></html>');
                        document.close();
                        });";

            return js_fetch;
        }
        //steam
        static HttpWebResponse SteamRequest(CookieContainer cookies, string body, string url, string referer)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(body);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.CookieContainer = cookies;
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            request.Host = "steamcommunity.com";
            request.Accept = "*/*";
            request.Referer = referer;
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/96.0.4664.110 Safari/537.36";
            request.ContentLength = bytes.Length;
            Stream stream = request.GetRequestStream();
            stream.Write(bytes, 0, bytes.Length);
            stream.Close();
            WebResponse response = request.GetResponse();

            return (HttpWebResponse)response;
        }
        public static HttpWebResponse CreateBuyOrder(CookieContainer cookies, string market_hash_name, decimal highest_buy_order, int currencyId)
        {
            CookieCollection cookieCollection = cookies.GetAllCookies();
            Cookie sessionId = cookieCollection.FirstOrDefault(x => x.Name == "sessionid");
            string body = $"sessionid={sessionId.Value}&currency={currencyId}&appid=730&market_hash_name={market_hash_name}&price_total={(int)(highest_buy_order * 100 + 1)}&quantity=1&billing_state=&save_my_address=0";
            string url = "https://steamcommunity.com/market/createbuyorder/";
            string referer = "https://steamcommunity.com/market/listings/730/" + market_hash_name;

            return SteamRequest(cookies, body, url, referer);
        }
        public static HttpWebResponse CancelBuyOrder(CookieContainer cookies, string market_hash_name, string buyOrderId)
        {
            CookieCollection cookieCollection = cookies.GetAllCookies();
            Cookie sessionId = cookieCollection.FirstOrDefault(x => x.Name == "sessionid");
            string body = $"sessionid={sessionId.Value}&buy_orderid={buyOrderId}";
            string url = "https://steamcommunity.com/market/cancelbuyorder/";
            string referer = "https://steamcommunity.com/market/listings/730/" + market_hash_name;

            return SteamRequest(cookies, body, url, referer);
        }
        public static HttpWebResponse BuyListing(CookieContainer cookies, string market_hash_name, string listingId, decimal fee, decimal subtotal, decimal total, int currencyId)
        {
            CookieCollection cookieCollection = cookies.GetAllCookies();
            Cookie sessionId = cookieCollection.FirstOrDefault(x => x.Name == "sessionid");
            string body = $"sessionid={sessionId.Value}&currency={currencyId}&fee={(int)fee}&subtotal={(int)subtotal}&total={(int)total}&quantity=1&first_name=&last_name=&billing_address=&billing_address_two=&billing_country=&billing_city=&billing_state=&billing_postal_code=&save_my_address=1";
            string url = "https://steamcommunity.com/market/buylisting/" + listingId;
            string referer = "https://steamcommunity.com/market/listings/730/" + market_hash_name;

            return SteamRequest(cookies, body, url, referer);
        }
        public static HttpWebResponse SellItem(CookieContainer cookies, string user, string assetId, int price)
        {
            CookieCollection cookieCollection = cookies.GetAllCookies();
            Cookie sessionId = cookieCollection.FirstOrDefault(x => x.Name == "sessionid");
            string body = $"sessionid={sessionId.Value}&appid=730&contextid=2&assetid={assetId}&amount=1&price={price}";
            string url = "https://steamcommunity.com/market/sellitem/";
            string referer = "https://steamcommunity.com/id/" + user + "/inventory/";

            return SteamRequest(cookies, body, url, referer);
        }
        public static HttpWebResponse AcceptTrade(CookieContainer cookies, string tradeOfferId, string partnerId)
        {
            CookieCollection cookieCollection = cookies.GetAllCookies();
            Cookie sessionId = cookieCollection.FirstOrDefault(x => x.Name == "sessionid");
            string body = "sessionid=" + sessionId.Value + "&serverid=1&tradeofferid=" + tradeOfferId + @"&partner=" + partnerId + @"&captcha=";
            string url = "https://steamcommunity.com/tradeoffer/" + tradeOfferId + "/accept";
            string referer = "https://steamcommunity.com/id/baht0/inventory/";

            return SteamRequest(cookies, body, url, referer);
        }

        //dropbox
        public static JObject DropboxListFolder(string path)
        {
            HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create("https://api.dropboxapi.com/2/files/list_folder");

            httpRequest.Method = "POST";
            httpRequest.ContentType = "application/json";
            httpRequest.Headers["Authorization"] = "Bearer a94CSH6hwyUAAAAAAAAAAf3zRyhyZknI9J8KM3VZihWEILAuv6Vr3ht_-4RQcJxs";

            JObject json = new(
                        new JProperty("path", $"/{path}"),
                        new JProperty("recursive", false),
                        new JProperty("include_media_info", false),
                        new JProperty("include_deleted", false),
                        new JProperty("include_has_explicit_shared_members", false),
                        new JProperty("include_mounted_folders", true),
                        new JProperty("include_non_downloadable_files", true));
            string data = json.ToString(Formatting.None);

            using (var streamWriter = new StreamWriter(httpRequest.GetRequestStream()))
            {
                streamWriter.Write(data.ToLower());
            }
            var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
            var streamReader = new StreamReader(httpResponse.GetResponseStream());

            return JObject.Parse(streamReader.ReadToEnd());
        }
        public static String DropboxDelete(string path)
        {
            JObject json = new(
                           new JProperty("path", $"/{path}"));
            string data = json.ToString(Formatting.None);
            var httpRequest = (HttpWebRequest)WebRequest.Create("https://api.dropboxapi.com/2/files/delete_v2");

            httpRequest.Method = "POST";
            httpRequest.Headers["Authorization"] = "Bearer a94CSH6hwyUAAAAAAAAAAf3zRyhyZknI9J8KM3VZihWEILAuv6Vr3ht_-4RQcJxs";
            httpRequest.ContentType = "application/json";

            using (var streamWriter = new StreamWriter(httpRequest.GetRequestStream()))
            {
                streamWriter.Write(data);
            }

            var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
                return result;
            }
        }
        public static String DropboxFolder(string path)
        {
            JObject json = new(
                           new JProperty("path", $"/{path}"),
                           new JProperty("autorename", false));
            string data = json.ToString(Formatting.None);
            var httpRequest = (HttpWebRequest)WebRequest.Create("https://api.dropboxapi.com/2/files/create_folder_v2");

            httpRequest.Method = "POST";
            httpRequest.Headers["Authorization"] = "Bearer a94CSH6hwyUAAAAAAAAAAf3zRyhyZknI9J8KM3VZihWEILAuv6Vr3ht_-4RQcJxs";
            httpRequest.ContentType = "application/json";

            using (var streamWriter = new StreamWriter(httpRequest.GetRequestStream()))
            {
                streamWriter.Write(data);
            }

            var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
                return result;
            }
        }
        public static String DropboxUpload(string path, string data)
        {
            JObject json = new(
                           new JProperty("path", $"/{path}"),
                           new JProperty("mode", "add"),
                           new JProperty("autorename", false),
                           new JProperty("mute", false),
                           new JProperty("strict_conflict", false));
            string args = json.ToString(Formatting.None);

            var httpRequest = (HttpWebRequest)WebRequest.Create("https://content.dropboxapi.com/2/files/upload");

            httpRequest.Method = "POST";
            httpRequest.ContentType = "application/octet-stream";
            httpRequest.Headers["Dropbox-API-Arg"] = args;
            httpRequest.Headers["Authorization"] = "Bearer a94CSH6hwyUAAAAAAAAAAf3zRyhyZknI9J8KM3VZihWEILAuv6Vr3ht_-4RQcJxs";

            using (var streamWriter = new StreamWriter(httpRequest.GetRequestStream()))
            {
                streamWriter.Write(data);
            }

            var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
                return result;
            }
        }
        public static void DropboxUploadFile(string path, string filePath)
        {
            JObject json = new(
                           new JProperty("path", $"/{path}"),
                           new JProperty("mode", "add"),
                           new JProperty("autorename", false),
                           new JProperty("mute", false),
                           new JProperty("strict_conflict", false));
            string args = json.ToString(Formatting.None);

            using (WebClient client = new())
            {
                client.Headers.Add("Content-Type", "application/octet-stream");
                client.Headers.Add("Dropbox-API-Arg", args);
                client.Headers.Add("Authorization", "Bearer a94CSH6hwyUAAAAAAAAAAf3zRyhyZknI9J8KM3VZihWEILAuv6Vr3ht_-4RQcJxs");
                using (Stream fileStream = File.OpenRead(filePath))
                using (Stream requestStream = client.OpenWrite(new Uri("https://content.dropboxapi.com/2/files/upload"), "POST"))
                {
                    fileStream.CopyTo(requestStream);
                }
            }
        }
    }
}
