using ItemChecker.MVVM.Model;
using ItemChecker.Net;
using ItemChecker.Properties;
using ItemChecker.Support;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Web;
using System.Windows;

namespace ItemChecker.Services
{
    public class FloatCheckService : BaseService
    {
        DataFloat floatData = new();
        public Int32 checkFloat(string itemName)
        {
            int purchasesMade = 0;
            string market_hash_name = HttpUtility.UrlEncode(itemName);
            getPrice(market_hash_name, itemName);

            var json = Get.Request("https://steamcommunity.com/market/listings/730/" + market_hash_name + "/render?start=0&count=50&currency=5&language=english&format=json");

            JObject obj = JObject.Parse(json);
            var attributes = obj["listinginfo"].ToList<JToken>();

            foreach (JToken attribute in attributes)
            {
                try
                {
                    JProperty jProperty = attribute.ToObject<JProperty>();
                    string listing_id = jProperty.Name;

                    decimal subtotal = Convert.ToDecimal(JObject.Parse(json)["listinginfo"][listing_id]["converted_price"].ToString());
                    decimal fee_steam = Convert.ToDecimal(JObject.Parse(json)["listinginfo"][listing_id]["converted_steam_fee"].ToString());
                    decimal fee_csgo = Convert.ToDecimal(JObject.Parse(json)["listinginfo"][listing_id]["converted_publisher_fee"].ToString());
                    decimal fee = fee_steam + fee_csgo;
                    decimal total = subtotal + fee;
                    decimal price = total / 100;

                    floatData.Precent = Edit.Precent(floatData.PriceCompare, price);
                    if (floatData.Precent > HomeProperties.Default.MaxPrecent)
                        break;

                    string ass_id = JObject.Parse(json)["listinginfo"][listing_id]["asset"]["id"].ToString();

                    string link = JObject.Parse(json)["listinginfo"][listing_id]["asset"]["market_actions"][0]["link"].ToString();
                    link = link.Replace("%listingid%", listing_id);
                    link = link.Replace("%assetid%", ass_id);

                    if (itemName.Contains("Factory New")) floatData.MaxFloat = FloatProperties.Default.maxFloatValue_FN;
                    else if (itemName.Contains("Minimal Wear")) floatData.MaxFloat = FloatProperties.Default.maxFloatValue_MW;
                    else if (itemName.Contains("Field-Tested")) floatData.MaxFloat = FloatProperties.Default.maxFloatValue_FT;
                    else if (itemName.Contains("Well-Worn")) floatData.MaxFloat = FloatProperties.Default.maxFloatValue_WW;
                    else if (itemName.Contains("Battle-Scarred")) floatData.MaxFloat = FloatProperties.Default.maxFloatValue_BS;

                    if (getFloatValue(link) < floatData.MaxFloat)
                        purchasesMade += buyItem(itemName, price, listing_id, fee, subtotal, total);
                }
                catch
                {
                    continue;
                }
                if (BaseModel.token.IsCancellationRequested)
                    break;
            }
            return purchasesMade;
        }

        void getPrice(string market_hash_name, string itemName)
        {
            try
            {
                var prices = Get.PriceOverview(market_hash_name);
                floatData.LowestPrice = prices.Item1;
                floatData.MedianPrice = prices.Item2;
                var item = ItemBase.SkinsBase.FirstOrDefault(x => x.ItemName == itemName);
                if (item != null)
                    floatData.CsmPrice = Math.Round(item.CsmInfo.Price * SettingsProperties.Default.CurrencyValue, 2);

                switch (HomeProperties.Default.Compare)
                {
                    case 0:
                        floatData.PriceCompare = floatData.LowestPrice;
                        break;
                    case 1:
                        floatData.PriceCompare = floatData.MedianPrice;
                        break;
                    case 2:
                        floatData.PriceCompare = floatData.CsmPrice;
                        break;
                }
            }
            catch (Exception exp)
            {
                errorLog(exp);
            }
        }
        Decimal getFloatValue(string link)
        {
            try
            {
                var json = Get.Request(@"https://api.csgofloat.com/?url=" + link);
                floatData.FloatValue = Convert.ToDecimal(JObject.Parse(json)["iteminfo"]["floatvalue"].ToString());
                return floatData.FloatValue;
            }
            catch (Exception exp)
            {
                errorLog(exp);
                return 1;
            }

        }
        Int32 buyItem(string itemName, decimal price, string listing_id, decimal fee, decimal subtotal, decimal total)
        {
            string message = $"Found item:\n{itemName}\n{floatData.LowestPrice}₽ (Lowest price) | {floatData.MedianPrice}₽ (Median price)\n\nFloat: {floatData.FloatValue}\nPrice ST: {price}₽ ({Math.Round(floatData.LowestPrice - price, 2)}₽)\nPrice CSM: {floatData.CsmPrice}₽ ({Math.Round(floatData.CsmPrice - price, 2)}₽)\n\nClick YES if you want to buy the item";

            SystemSounds.Asterisk.Play();
            MessageBoxResult result = MessageBox.Show(message, "Buy item", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                string market_hash_name = HttpUtility.UrlEncode(itemName);
                Post.BuyListing(SteamCookies, market_hash_name, listing_id, fee, subtotal, total);
                return 1;
            }
            return 0;
        }
    }
}
