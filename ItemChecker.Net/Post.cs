using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;

namespace ItemChecker.Net
{
    public class Post
    {
        //main
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
        public static String BuyListing(string listing_id, decimal fee, decimal subtotal, decimal total, string sessionid)
        {
            string body = $"sessionid={sessionid}&currency=5&fee={(int)fee}&subtotal={(int)subtotal}&total={(int)total}&quantity=1&first_name=&last_name=&billing_address=&billing_address_two=&billing_country=&billing_city=&billing_state=&billing_postal_code=&save_my_address=1";
            string url = "https://steamcommunity.com/market/buylisting/" + listing_id;

            return FetchRequest("application/x-www-form-urlencoded", body, url);
        }
        public static String CreateBuyOrder(string market_hash_name, decimal last_order, string sessionid)
        {
            int price_total = (int)(last_order * 100 + 1);
            string body = $"sessionid={sessionid}&currency=5&appid=730&market_hash_name={market_hash_name}&price_total={price_total}&quantity=1&billing_state=&save_my_address=0";
            string url = "https://steamcommunity.com/market/createbuyorder/";

            return FetchRequest("application/x-www-form-urlencoded", body, url);
        }
        public static String CancelBuyOrder(string buy_orderid, string sessionid)
        {
            string body = $"sessionid={sessionid}&buy_orderid={buy_orderid}";
            string url = "https://steamcommunity.com/market/cancelbuyorder/";

            return FetchRequest("application/x-www-form-urlencoded", body, url);
        }
        public static String AcceptTrade(string tradeofferid, string partner_id, string sessionid)
        {
            string body = "sessionid=" + sessionid + @"&serverid=1&tradeofferid=" + tradeofferid + @"&partner=" + partner_id + @"&captcha=";
            string url = "https://steamcommunity.com/tradeoffer/" + tradeofferid + "/accept";

            return FetchRequest("application/x-www-form-urlencoded", body, url);
        }
        public static String SellItem(string assetid, string price, string sessionid)
        {
            string body = $"sessionid={sessionid}&appid=730&contextid=2&assetid={assetid}&amount=1&price={price}";
            string url = "https://steamcommunity.com/market/sellitem/";

            return FetchRequest("application/x-www-form-urlencoded", body, url);
        }
        //dropbox
        public static String DropboxRead(string file)
        {
            HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create("https://content.dropboxapi.com/2/files/download");

            httpRequest.Headers["Dropbox-API-Arg"] = "{\"path\": \"/" + file + "\"}";
            httpRequest.Headers["Authorization"] = "Bearer a94CSH6hwyUAAAAAAAAAAf3zRyhyZknI9J8KM3VZihWEILAuv6Vr3ht_-4RQcJxs";

            HttpWebResponse httpResponse = (HttpWebResponse)httpRequest.GetResponse();
            Stream stream = httpResponse.GetResponseStream();
            StreamReader streamReader = new(stream);
            string s = httpResponse.StatusCode.ToString();

            return streamReader.ReadToEnd();
        }
        public static String DropboxListFolder(string path)
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

            return streamReader.ReadToEnd();
        }
    }
}
